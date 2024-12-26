using System.Diagnostics;
using RevitTranslator.UI.Contracts;
using TranslationService.Models;
using TranslationService.Utils;

namespace RevitTranslator.ViewModels;

public partial class SettingsViewModel : ObservableObject, ISettingsViewModel
{
    private readonly DeeplSettingsDescriptor _settings = DeeplSettingsUtils.CurrentSettings;
    private bool _isAutoDetectChecked = true;
    private int _previousSourceIndex = -1;

    [ObservableProperty] private string _deeplApiKey;
    [ObservableProperty] private bool _isPaidPlan;
    [ObservableProperty] private int _selectedSourceLanguageIndex;
    [ObservableProperty] private int _selectedTargetLanguageIndex;
    [ObservableProperty] private string _buttonText;

    public LanguageDescriptor[] Languages { get; } = DeeplLanguageCodes.TargetLanguages;

    public SettingsViewModel()
    {
        LoadSettings();
        ButtonText = "Save settings";
    }

    public bool IsAutoDetectChecked
    {
        get => _isAutoDetectChecked;
        set
        {
            if (_isAutoDetectChecked != value)
            {
                _isAutoDetectChecked = value;
                OnPropertyChanged(nameof(IsSourceComboBoxEnabled));
                OnPropertyChanged(nameof(IsAutoDetectChecked));

                if (value)
                {
                    _previousSourceIndex = SelectedSourceLanguageIndex;
                    SelectedSourceLanguageIndex = -1;
                }
                else
                {
                    SelectedSourceLanguageIndex = _previousSourceIndex;
                }
            }
        }
    }

    public bool IsSourceComboBoxEnabled => !IsAutoDetectChecked;

    /// <summary>
    /// Loads the settings file
    /// </summary>
    private void LoadSettings()
    {
        DeeplSettingsUtils.Load();
        
        DeeplApiKey = _settings.DeeplApiKey;
        IsPaidPlan = _settings.IsPaidPlan;

        // SetSourceLanguage();
        // SetTargetLanguage();
    }

    /// <summary>
    /// Opens the link to my LinkedIn page in default browser
    /// </summary>
    /// <param name="uri"></param>
    [RelayCommand]
    private void OpenLinkedin(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var validUri))
        {
            Process.Start(new ProcessStartInfo(validUri.AbsoluteUri) { UseShellExecute = true });
        }
    }

    /// <summary>
    /// Saves settings file and checks if provided credentials are valid
    /// </summary>
    [RelayCommand]
    private void SaveSettings()
    {
        _settings.DeeplApiKey = DeeplApiKey;
        _settings.SourceLanguage = SelectedSourceLanguageIndex == -1 ? null : Languages[SelectedSourceLanguageIndex];
        _settings.TargetLanguage = Languages[SelectedTargetLanguageIndex];
        _settings.Save();
        
        if (!TranslationUtils.TryTestTranslate())
        {
            return;
        }

        ButtonText = "Settings saved!";

        Task.Run((Func<Task>)(async () =>
        {
            await Task.Delay(2000);
            ButtonText = "Save settings";
        }));
    }

    /// <summary>
    /// Sets the source language
    /// </summary>
    // private void SetSourceLanguage()
    // {
    //     if (_settings.SourceLanguage == null)
    //     {
    //         SelectedSourceLanguageIndex = -1;
    //         IsAutoDetectChecked = true;
    //     }
    //     else
    //     {
    //         IsAutoDetectChecked = false;
    //         if (_settings.SourceLanguage == null)
    //         {
    //             SelectedSourceLanguageIndex = -1;
    //         }
    //         else if (char.IsLower(_settings.SourceLanguage[0]))
    //         {
    //             SelectedSourceLanguageIndex = _languages.IndexOfKey(_languages.FirstOrDefault(kvp => kvp.Value == _settings.SourceLanguage).Key);
    //         }
    //         else
    //         {
    //             SelectedSourceLanguageIndex = _languages.IndexOfKey(_settings.SourceLanguage);
    //         }
    //     }
    // }

    /// <summary>
    /// Sets the target language
    /// </summary>
    // private void SetTargetLanguage()
    // {
    //     if (_settings.TargetLanguage == null)
    //     {
    //         SelectedTargetLanguageIndex = -1;
    //         IsAutoDetectChecked = true;
    //     }
    //     else if (char.IsLower(_settings.TargetLanguage[0]))
    //     {
    //         SelectedTargetLanguageIndex = _languages.IndexOfKey(_languages.FirstOrDefault(kvp => kvp.Value == _settings.TargetLanguage).Key);
    //     }
    //     else
    //     {
    //         SelectedTargetLanguageIndex = _languages.IndexOfKey(_settings.TargetLanguage);
    //     }
    // }
    
    /// <summary>
    /// Switches "to" and "from" languages in the UI
    /// </summary>
    [RelayCommand]
    private void SwitchLanguages()
    {
        if (IsAutoDetectChecked)
        {
            IsAutoDetectChecked = false;
        }
        var index1 = SelectedSourceLanguageIndex;
        var index2 = SelectedTargetLanguageIndex;

        SelectedSourceLanguageIndex = index2;
        SelectedTargetLanguageIndex = index1;
    }
}
