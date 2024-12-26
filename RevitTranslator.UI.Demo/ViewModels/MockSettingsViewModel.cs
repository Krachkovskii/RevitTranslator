using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.UI.Contracts;
using TranslationService.Models;
using TranslationService.Utils;

namespace RevitTranslator.Demo.ViewModels;

public partial class MockSettingsViewModel : ObservableValidator, ISettingsViewModel
{
    [ObservableProperty] private string _deeplApiKey = string.Empty;
    [ObservableProperty] private string _buttonText;
    [ObservableProperty] private bool _isAutoDetectChecked;
    [ObservableProperty] private bool _isPaidPlan;
    [ObservableProperty] private LanguageDescriptor? _selectedSourceLanguage = DeeplLanguageCodes.LanguageCodes[0];
    [ObservableProperty] private LanguageDescriptor _selectedTargetLanguage = DeeplLanguageCodes.LanguageCodes[1];
    
    private LanguageDescriptor? _previousLanguage = null;
    
    public MockSettingsViewModel()
    {
        DeeplSettingsUtils.Load();
        // var faker = new Faker();
        // SelectedSourceLanguageIndex = faker.Random.Int(0, Languages.Length - 1);
        // SelectedTargetLanguageIndex = faker.Random.Int(0, Languages.Length - 1);
        //
        // _isAutoDetectChecked = faker.Random.Bool();
        // if (_isAutoDetectChecked) SelectedSourceLanguageIndex = -1;
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
            SelectedSourceLanguage = _previousLanguage ??= DeeplLanguageCodes.LanguageCodes[0];
        }
    }
}