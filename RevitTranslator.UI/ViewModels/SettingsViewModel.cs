using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Contracts;
using RevitTranslator.Common.Messages;
using TranslationService.Models;
using TranslationService.Utils;
using TranslationService.Validation;

namespace RevitTranslator.UI.ViewModels;

public partial class SettingsViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    [NotifyPropertyChangedFor(nameof(IsApiKeyValid))]
    [NotifyPropertyChangedFor(nameof(IsPaidPlan))]
    private string _deeplApiKey = string.Empty;

    public bool IsApiKeyValid => ApiKeyValidator.TryValidate(DeeplApiKey, out _, out _);
    public bool? IsPaidPlan => IsApiKeyValid ? !ApiKeyValidator.IsFreePlan(DeeplApiKey) : null;
    public string Version { get; } = "";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    private LanguageDescriptor? _selectedSourceLanguage;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    private LanguageDescriptor? _selectedTargetLanguage;

    [ObservableProperty] private string _buttonText = string.Empty;
    [ObservableProperty] private bool _isAutoDetectChecked;
    [ObservableProperty] private LanguageDescriptor[] _sourceLanguages = [];
    [ObservableProperty] private LanguageDescriptor[] _targetLanguages = [];

    private LanguageDescriptor? _previousLanguage;
    private readonly DeeplTranslationClient _translationClient;
    private readonly ITranslationReportService _reportService;

    public SettingsViewModel(DeeplTranslationClient translationClient, ITranslationReportService reportService)
    {
        _translationClient = translationClient;
        _reportService = reportService;

        if (!DeeplSettingsUtils.Load())
        {
            ButtonText = "Failed to load settings";
            return;
        }

        SetSettingsValues();
        ButtonText = "Save Settings";
        Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public int Usage => _translationClient.Usage;
    public int Limit => _translationClient.Limit;

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        if (IsApiKeyValid)
        {
            await LoadLanguagesAsync();
        }

        await UpdateUsageAsync();
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

    [RelayCommand(CanExecute = nameof(CanExecuteSaveSettings))]
    private async Task SaveSettingsAsync()
    {
        ButtonText = "Validating...";

        if (!ApiKeyValidator.TryValidate(DeeplApiKey, out var sanitizedKey, out var validationError))
        {
            ButtonText = validationError ?? "Invalid API key";
            await Task.Delay(3000);
            ButtonText = "Save Settings";
            return;
        }

        ButtonText = "Saving settings...";

        var oldSettings = DeeplSettingsUtils.CurrentSettings;
        var newSettings = new DeeplSettingsDescriptor
        {
            IsPaidPlan = IsPaidPlan is true,
            DeeplApiKey = sanitizedKey,
            SourceLanguage = SelectedSourceLanguage,
            TargetLanguage = SelectedTargetLanguage
        };
        newSettings.Save();

        DeeplApiKey = sanitizedKey;

        var test = await _translationClient.CanTranslateAsync();
        StrongReferenceMessenger.Default.Send(new SettingsValidityChangedMessage(test));

        if (test)
        {
            await UpdateUsageAsync();
            ButtonText = "Settings saved";
            return;
        }

        ButtonText = "Invalid credentials";
        if (oldSettings is null) return;

        oldSettings.Save();
        SetSettingsValues();

        await Task.Delay(3000);
        ButtonText = "Settings were restored";
    }

    [RelayCommand]
    private void OpenReportsFolder() => _reportService.OpenReportDirectory();

    private void SetSettingsValues()
    {
        if (DeeplSettingsUtils.CurrentSettings is null) return;

        DeeplApiKey = DeeplSettingsUtils.CurrentSettings.DeeplApiKey;
        SelectedSourceLanguage = DeeplSettingsUtils.CurrentSettings.SourceLanguage;
        SelectedTargetLanguage = DeeplSettingsUtils.CurrentSettings.TargetLanguage;

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
            SelectedSourceLanguage = _previousLanguage ??= SourceLanguages[0];
        }
    }

    private bool CanExecuteSaveSettings()
    {
        if (!IsApiKeyValid) return false;

        var savedSettings = DeeplSettingsUtils.CurrentSettings;
        if (savedSettings is null) return true;

        var hasChanges = savedSettings.IsPaidPlan != IsPaidPlan ||
                         savedSettings.DeeplApiKey != DeeplApiKey ||
                         savedSettings.SourceLanguage != SelectedSourceLanguage ||
                         savedSettings.TargetLanguage != SelectedTargetLanguage;

        ButtonText = hasChanges ? "Save Settings" : "Settings saved";

        return hasChanges;
    }

    partial void OnDeeplApiKeyChanged(string value)
    {
        if (!IsApiKeyValid) return;
        if (SourceLanguages.Length > 0) return;

        _ = LoadLanguagesAsync();
    }

    private async Task LoadLanguagesAsync()
    {
        var sourceLanguages = await _translationClient.GetSourceLanguagesAsync(DeeplApiKey);
        var targetLanguages = await _translationClient.GetTargetLanguagesAsync(DeeplApiKey);

        SourceLanguages = sourceLanguages.OrderBy(lang => lang.VisibleName).ToArray();
        TargetLanguages = targetLanguages.OrderBy(lang => lang.VisibleName).ToArray();
        
        SelectedTargetLanguage ??= TargetLanguages.First();
    }

    private async Task UpdateUsageAsync()
    {
        await _translationClient.CheckUsageAsync();
        OnPropertyChanged(nameof(Usage));
        OnPropertyChanged(nameof(Limit));
    }
}
