using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.UI.Contracts;
using TranslationService.Models;
using TranslationService.Utils;

namespace RevitTranslator.Demo.ViewModels;

public partial class MockSettingsViewModel : ObservableValidator, ISettingsViewModel
{
    //TODO: Add validation properties
    [ObservableProperty] private string _deeplApiKey = string.Empty;
    [ObservableProperty] private string _buttonText;
    [ObservableProperty] private bool _isAutoDetectChecked;
    [ObservableProperty] private bool _isPaidPlan;
    [ObservableProperty] private LanguageDescriptor? _selectedSourceLanguage = DeeplLanguageCodes.TargetLanguages[0];
    [ObservableProperty] private LanguageDescriptor _selectedTargetLanguage = DeeplLanguageCodes.TargetLanguages[1];
    
    private LanguageDescriptor? _previousLanguage;
    
    public MockSettingsViewModel()
    {
        DeeplSettingsUtils.Load();
    }
    
    [RelayCommand]
    private void SwitchLanguages()
    {
        
    }
    
    [RelayCommand]
    private void OpenLinkedin()
    {
        
    }
    
    [RelayCommand]
    private void SaveSettings()
    {
        var settings = new DeeplSettingsDescriptor();
        settings.IsPaidPlan = IsPaidPlan;
        settings.DeeplApiKey = DeeplApiKey;
        settings.SourceLanguage = SelectedSourceLanguage;
        settings.TargetLanguage = SelectedTargetLanguage;

        DeeplSettingsUtils.CurrentSettings = settings;
        settings.Save();
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
}