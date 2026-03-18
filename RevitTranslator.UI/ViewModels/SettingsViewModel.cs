using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Common.Messages;
using TranslationService.Models;
using TranslationService.Utils;
using TranslationService.Validation;

namespace RevitTranslator.UI.ViewModels;

public partial class SettingsViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsApiKeyValid))]
    [NotifyPropertyChangedFor(nameof(IsPaidPlan))]
    private string _deeplApiKey = string.Empty;

    public bool IsApiKeyValid => ApiKeyValidator.TryValidate(DeeplApiKey, out _, out _);
    public bool? IsPaidPlan => IsApiKeyValid ? !ApiKeyValidator.IsFreePlan(DeeplApiKey) : null;
    public string Version { get; } = "";

    [ObservableProperty] private LanguageDescriptor? _selectedSourceLanguage;
    [ObservableProperty] private LanguageDescriptor? _selectedTargetLanguage;

    [ObservableProperty] private bool _isAutoDetectChecked;
    [ObservableProperty] private bool _isValidatingApiKey;
    [ObservableProperty] private bool _isApiKeyWorking;
    [ObservableProperty] private string _apiKeyValidationMessage = string.Empty;
    [ObservableProperty] private LanguageDescriptor[] _sourceLanguages = [];
    [ObservableProperty] private LanguageDescriptor[] _targetLanguages = [];

    private LanguageDescriptor? _previousLanguage;
    private string _keyBeingValidated = string.Empty;
    private readonly DeeplTranslationClient _translationClient;
    private readonly ITranslationReportService _reportService;

    public SettingsViewModel(DeeplTranslationClient translationClient, ITranslationReportService reportService)
    {
        _translationClient = translationClient;
        _reportService = reportService;

        if (!DeeplSettingsUtils.Load())
        {
            return;
        }

        SetSettingsValues();
        Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public int Usage => _translationClient.Usage;
    public int Limit => _translationClient.Limit;

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        if (!IsApiKeyValid) return;
        if (IsValidatingApiKey) return;

        await ValidateApiKeyAsync(DeeplApiKey);
    }

    [RelayCommand]
    private void SwitchLanguages()
    {
        // this works only when auto-detect is off, so source language would be selected anyway
        var lang1 = SelectedSourceLanguage;
        var lang2 = SelectedTargetLanguage;

        SelectedSourceLanguage = lang2;
        SelectedTargetLanguage = lang1!;
    }

    [RelayCommand]
    private void OpenLinkedin(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var validUri)) return;

        Process.Start(new ProcessStartInfo(validUri.AbsoluteUri) { UseShellExecute = true });
    }

    [RelayCommand]
    private void OpenReportsFolder() => _reportService.OpenReportDirectory();

    public void SaveSettings()
    {
        if (!ApiKeyValidator.TryValidate(DeeplApiKey, out var sanitizedKey, out _)) return;

        var targetLanguage = SelectedTargetLanguage ?? DeeplSettingsUtils.CurrentSettings?.TargetLanguage;
        if (targetLanguage is null) return;

        var settings = new DeeplSettingsDescriptor
        {
            IsPaidPlan = IsPaidPlan is true,
            DeeplApiKey = sanitizedKey,
            SourceLanguage = IsAutoDetectChecked ? null : SelectedSourceLanguage,
            TargetLanguage = targetLanguage
        };
        settings.Save();
    }

    private async Task ValidateApiKeyAsync(string key)
    {
        _keyBeingValidated = key;

        if (!ApiKeyValidator.TryValidate(key, out var sanitizedKey, out _))
        {
            IsApiKeyWorking = false;
            ApiKeyValidationMessage = string.Empty;
            return;
        }

        IsValidatingApiKey = true;
        ApiKeyValidationMessage = "Validating...";

        var tempSettings = new DeeplSettingsDescriptor
        {
            DeeplApiKey = sanitizedKey,
            IsPaidPlan = !ApiKeyValidator.IsFreePlan(sanitizedKey),
            SourceLanguage = IsAutoDetectChecked ? null : SelectedSourceLanguage,
            TargetLanguage = SelectedTargetLanguage ?? DeeplSettingsUtils.CurrentSettings?.TargetLanguage
        };
        DeeplSettingsUtils.UpdateInMemory(tempSettings);

        var isValid = await _translationClient.CanTranslateAsync();

        if (_keyBeingValidated != key)
        {
            IsValidatingApiKey = false;
            return;
        }

        IsApiKeyWorking = isValid;
        IsValidatingApiKey = false;
        ApiKeyValidationMessage = isValid ? string.Empty : "Invalid API key";
        StrongReferenceMessenger.Default.Send(new SettingsValidityChangedMessage(isValid));

        if (!isValid) return;

        await UpdateUsageAsync();

        if (SourceLanguages.Length > 0) return;

        await LoadLanguagesAsync();
    }

    private void SetSettingsValues()
    {
        if (DeeplSettingsUtils.CurrentSettings is null) return;

        SelectedSourceLanguage = DeeplSettingsUtils.CurrentSettings.SourceLanguage;
        SelectedTargetLanguage = DeeplSettingsUtils.CurrentSettings.TargetLanguage;
        DeeplApiKey = DeeplSettingsUtils.CurrentSettings.DeeplApiKey;

        if (SelectedSourceLanguage is not null) return;

        IsAutoDetectChecked = true;
    }

    partial void OnIsAutoDetectCheckedChanged(bool value)
    {
        if (value)
        {
            _previousLanguage = SelectedSourceLanguage;
            SelectedSourceLanguage = null;
        }
        else
        {
            SelectedSourceLanguage = _previousLanguage ??= TargetLanguages[0];
        }
    }

    partial void OnDeeplApiKeyChanged(string value)
    {
        IsApiKeyWorking = false;
        ApiKeyValidationMessage = string.Empty;
        _ = ValidateApiKeyAsync(value);
    }

    private async Task LoadLanguagesAsync()
    {
        var savedTarget = SelectedTargetLanguage ?? DeeplSettingsUtils.CurrentSettings?.TargetLanguage;
        var savedSource = SelectedSourceLanguage ?? DeeplSettingsUtils.CurrentSettings?.SourceLanguage;

        var sourceLanguages = await _translationClient.GetSourceLanguagesAsync(DeeplApiKey);
        var targetLanguages = await _translationClient.GetTargetLanguagesAsync(DeeplApiKey);

        SourceLanguages = sourceLanguages.OrderBy(lang => lang.VisibleName).ToArray();
        TargetLanguages = targetLanguages.OrderBy(lang => lang.VisibleName).ToArray();

        // ComboBox may reset SelectedItem when ItemsSource changes — restore explicitly
        SelectedTargetLanguage = TargetLanguages.FirstOrDefault(l => l == savedTarget)
                                 ?? TargetLanguages.FirstOrDefault();

        if (!IsAutoDetectChecked)
        {
            SelectedSourceLanguage = SourceLanguages.FirstOrDefault(l => l == savedSource)
                                     ?? SourceLanguages.FirstOrDefault();
        }
    }

    private async Task UpdateUsageAsync()
    {
        await _translationClient.CheckUsageAsync();
        OnPropertyChanged(nameof(Usage));
        OnPropertyChanged(nameof(Limit));
    }
}