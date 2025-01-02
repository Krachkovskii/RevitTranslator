using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.UI.Contracts;
using TranslationService.Models;
using TranslationService.Utils;

namespace RevitTranslator.Demo.ViewModels;

public partial class MockSettingsViewModel : ObservableValidator, ISettingsViewModel
{
    [ObservableProperty] private string _deeplApiKey = string.Empty;
    [ObservableProperty] private bool _isAutoDetectChecked;
    [ObservableProperty] private bool _isPaidPlan;
    [ObservableProperty] private LanguageDescriptor? _selectedSourceLanguage = DeeplLanguageCodes.TargetLanguages[0];
    [ObservableProperty] private LanguageDescriptor _selectedTargetLanguage = DeeplLanguageCodes.TargetLanguages[1];
    [ObservableProperty] private string _buttonText;
    
    private LanguageDescriptor? _previousLanguage;
    private int _savedHash;
    
    public MockSettingsViewModel()
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
    
    [RelayCommand(CanExecute = nameof(CheckChanges))]
    private void SaveSettings()
    {
        var settings = new DeeplSettingsDescriptor();
        settings.IsPaidPlan = IsPaidPlan;
        settings.DeeplApiKey = DeeplApiKey;
        settings.SourceLanguage = SelectedSourceLanguage;
        settings.TargetLanguage = SelectedTargetLanguage;

        DeeplSettingsUtils.CurrentSettings = settings;
        settings.Save();
        _savedHash = CaluclateHash();
        
        ButtonText = "Settings saved";
    }

    private void SetSettingsValues()
    {
        IsPaidPlan = DeeplSettingsUtils.CurrentSettings.IsPaidPlan;
        DeeplApiKey = DeeplSettingsUtils.CurrentSettings.DeeplApiKey;
        SelectedSourceLanguage = DeeplSettingsUtils.CurrentSettings.SourceLanguage;
        SelectedTargetLanguage = DeeplSettingsUtils.CurrentSettings.TargetLanguage;

        if (SelectedSourceLanguage is null)
        {
            IsAutoDetectChecked = true;
        }
        
        _savedHash = CaluclateHash();
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

    partial void OnDeeplApiKeyChanged(string value)
    {
        CheckChanges();
    }

    partial void OnIsPaidPlanChanged(bool value)
    {
        CheckChanges();
    }

    partial void OnSelectedSourceLanguageChanged(LanguageDescriptor value)
    {
        CheckChanges();
    }


    partial void OnSelectedTargetLanguageChanged(LanguageDescriptor value)
    {
        CheckChanges();
    }

    private bool CheckChanges()
    {
        var hasChanges = _savedHash == CaluclateHash();
        ButtonText =  hasChanges ? "Settings saved" : "Save Settings";

        return hasChanges;
    }

    private int CaluclateHash()
    {
        unchecked
        {
            var hash = 95;
            hash = hash * 31 + SelectedSourceLanguage?.GetHashCode()?? 1;
            hash = hash * 31 + SelectedTargetLanguage.GetHashCode();
            hash = hash * 31 + IsPaidPlan.GetHashCode();
            hash = hash * 31 + DeeplApiKey.GetHashCode();

            return hash;
        }
    }
}