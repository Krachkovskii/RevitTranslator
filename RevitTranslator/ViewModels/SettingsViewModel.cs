using System.Diagnostics;
using RevitTranslator.UI.Contracts;
using TranslationService.Models;
using TranslationService.Utils;
using TranslationService.Validation;

namespace RevitTranslator.ViewModels;

public partial class SettingsViewModel : ObservableValidator, ISettingsViewModel
{
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))] 
    private string _deeplApiKey = string.Empty;
    
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))] 
    private bool _isPaidPlan;
    
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))] 
    private LanguageDescriptor? _selectedSourceLanguage = DeeplLanguageCodes.TargetLanguages[0];
    
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))] 
    private LanguageDescriptor _selectedTargetLanguage = DeeplLanguageCodes.TargetLanguages[1];
    
    [ObservableProperty] private string _buttonText = string.Empty;
    [ObservableProperty] private bool _isAutoDetectChecked;
    
    private LanguageDescriptor? _previousLanguage;
    private readonly DeeplTranslationClient _translationClient;

    public SettingsViewModel(DeeplTranslationClient translationClient)
    {
        _translationClient = translationClient;

        if (!DeeplSettingsUtils.Load())
        {
            ButtonText = "Failed to load settings";
            return;
        }
        SetSettingsValues();
        ButtonText = "Save Settings";
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
        if (Uri.TryCreate(uri, UriKind.Absolute, out var validUri))
        {
            Process.Start(new ProcessStartInfo(validUri.AbsoluteUri) { UseShellExecute = true });
        }
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

        var detectedFreePlan = ApiKeyValidator.IsFreePlan(sanitizedKey);
        if (detectedFreePlan && IsPaidPlan)
        {
            ButtonText = "Warning: API key appears to be free plan";
            await Task.Delay(2000);
        }

        ButtonText = "Saving settings...";

        var oldSettings = DeeplSettingsUtils.CurrentSettings;
        var newSettings = new DeeplSettingsDescriptor
        {
            IsPaidPlan = IsPaidPlan,
            DeeplApiKey = sanitizedKey,
            SourceLanguage = SelectedSourceLanguage,
            TargetLanguage = SelectedTargetLanguage
        };
        newSettings.Save();

        DeeplApiKey = sanitizedKey;

        var test = await _translationClient.TryTestTranslateAsync();
        if (test)
        {
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

    private void SetSettingsValues()
    {
        if (DeeplSettingsUtils.CurrentSettings is null) return;
        
        IsPaidPlan = DeeplSettingsUtils.CurrentSettings.IsPaidPlan;
        DeeplApiKey = DeeplSettingsUtils.CurrentSettings.DeeplApiKey;
        SelectedSourceLanguage = DeeplSettingsUtils.CurrentSettings.SourceLanguage;
        SelectedTargetLanguage = DeeplSettingsUtils.CurrentSettings.TargetLanguage;

        if (SelectedSourceLanguage is null) return;
            
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
            SelectedSourceLanguage = _previousLanguage ??= DeeplLanguageCodes.TargetLanguages[0];
        }
    }

    private bool CanExecuteSaveSettings()
    {
        var savedSettings = DeeplSettingsUtils.CurrentSettings;
        if (savedSettings is null) return true;
        
        var hasChanges = savedSettings?.IsPaidPlan != IsPaidPlan ||
                         savedSettings.DeeplApiKey != DeeplApiKey ||
                         savedSettings.SourceLanguage != SelectedSourceLanguage ||
                         savedSettings.TargetLanguage != SelectedTargetLanguage;
        
        ButtonText =  hasChanges ? "Save Settings" : "Settings saved";
        
        return hasChanges;
    }
}