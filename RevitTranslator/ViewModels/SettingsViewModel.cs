﻿using System.Diagnostics;
using RevitTranslator.UI.Contracts;
using TranslationService.Models;
using TranslationService.Utils;

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

    public SettingsViewModel()
    {
        DeeplSettingsUtils.Load();
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
    private void SaveSettings()
    {
        ButtonText = "Saving settings...";
        
        var oldSettings = DeeplSettingsUtils.CurrentSettings;
        var newSettings = new DeeplSettingsDescriptor
        {
            IsPaidPlan = IsPaidPlan,
            DeeplApiKey = DeeplApiKey,
            SourceLanguage = SelectedSourceLanguage,
            TargetLanguage = SelectedTargetLanguage
        };
        newSettings.Save();

        var test = TranslationUtils.TryTestTranslate();
        if (test)
        {
            ButtonText = "Settings saved";
            return;
        }

        ButtonText = "Invalid credentials";
        if (oldSettings is null) return;
        
        oldSettings.Save();
        SetSettingsValues();
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            ButtonText = "Settings were restored";
        });
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
