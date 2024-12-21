using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.UI.Contracts;

namespace RevitTranslator.Demo.ViewModels;

public partial class MockSettingsViewModel : ObservableValidator, ISettingsViewModel
{
    [ObservableProperty] private string _deeplApiKey = string.Empty;
    [ObservableProperty] private string _buttonText;
    [ObservableProperty] private bool _isAutoDetectChecked;
    [ObservableProperty] private bool _isPaidPlan;
    [ObservableProperty] private int? _selectedSourceLanguageIndex;
    [ObservableProperty] private int _selectedTargetLanguageIndex;
    
    public LanguageDescriptor[] Languages { get; } = DeeplLanguageCodes.LanguageCodes;

    public MockSettingsViewModel()
    {
        var faker = new Faker();
        SelectedSourceLanguageIndex = faker.Random.Int(0, Languages.Length - 1);
        SelectedTargetLanguageIndex = faker.Random.Int(0, Languages.Length - 1);
        
        _isAutoDetectChecked = faker.Random.Bool();
        if (_isAutoDetectChecked) SelectedSourceLanguageIndex = -1;
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
    private async Task SaveSettings()
    {
        
    }

    partial void OnIsAutoDetectCheckedChanged(bool value)
    {
        if (value) SelectedSourceLanguageIndex = -1;
    }
}