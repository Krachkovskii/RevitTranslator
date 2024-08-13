using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RevitTranslatorAddin.Utils.DeepL;

namespace RevitTranslatorAddin.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private Models.DeeplSettings _settings;
    private int _previousSourceIndex = -1;

    private string _deeplApiKey;
    public string DeeplApiKey
    {
        get => _deeplApiKey;
        set
        {
            _deeplApiKey = value;
            OnPropertyChanged(nameof(DeeplApiKey));
        }
    }

    private SortedList<string, string> _languages;
    public ObservableCollection<string> Languages { get; private set; } = [];

    private int _selectedSourceLanguageIndex;
    public int SelectedSourceLanguageIndex
    {
        get => _selectedSourceLanguageIndex;
        set
        {
            _selectedSourceLanguageIndex = value;
            OnPropertyChanged(nameof(SelectedSourceLanguageIndex));
        }
    }

    private int _selectedTargetLanguageIndex;
    public int SelectedTargetLanguageIndex
    {
        get => _selectedTargetLanguageIndex;
        set
        {
            if (_selectedTargetLanguageIndex != value)
            {
                _selectedTargetLanguageIndex = value;
                OnPropertyChanged(nameof(SelectedTargetLanguageIndex));
            }
        }
    }

    private bool _isAutoDetectChecked = true;
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

    private string _updateButtonText;
    public string UpdateButtonText
    {
        get => _updateButtonText;
        set
        {
            _updateButtonText = value;
            OnPropertyChanged(nameof(UpdateButtonText));
        }
    }

    private bool _isPaidPlan;
    public bool IsPaidPlan
    {
        get => _isPaidPlan;
        set 
        {
            _isPaidPlan = value;
            OnPropertyChanged(nameof(IsPaidPlan));
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand SwitchLanguagesCommand { get; }
    public ICommand OpenLinkedinCommand { get; }

    public SettingsViewModel()
    {
        LoadSettings();
        SaveCommand = new RelayCommand(SaveSettings);
        SwitchLanguagesCommand = new RelayCommand(SwitchLanguages);
        OpenLinkedinCommand = new RelayCommand<string>(OpenLinkedin);
        UpdateButtonText = "Save settings";
    }

    /// <summary>
    /// Loads the settings file
    /// </summary>
    private void LoadSettings()
    {
        _settings = Models.DeeplSettings.LoadFromJson();
        DeeplApiKey = _settings.DeeplApiKey;
        _languages = _settings.Languages;
        Languages = new ObservableCollection<string>(_languages.Keys);
        IsPaidPlan = _settings.IsPaidPlan;

        SetSourceLanguage();
        SetTargetLanguage();
    }

    /// <summary>
    /// Sets the source language
    /// </summary>
    private void SetSourceLanguage()
    {
        if (_settings.SourceLanguage == null)
        {
            SelectedSourceLanguageIndex = -1;
            IsAutoDetectChecked = true;
        }
        else
        {
            IsAutoDetectChecked = false;
            if (_settings.SourceLanguage == null)
            {
                SelectedSourceLanguageIndex = -1;
            }
            else if (char.IsLower(_settings.SourceLanguage[0]))
            {
                SelectedSourceLanguageIndex = _languages.IndexOfKey(_languages.FirstOrDefault(kvp => kvp.Value == _settings.SourceLanguage).Key);
            }
            else
            {
                SelectedSourceLanguageIndex = _languages.IndexOfKey(_settings.SourceLanguage);
            }
        }
    }

    /// <summary>
    /// Sets the target language
    /// </summary>
    private void SetTargetLanguage()
    {
        if (_settings.TargetLanguage == null)
        {
            SelectedTargetLanguageIndex = -1;
        }
        else if (char.IsLower(_settings.TargetLanguage[0]))
        {
            SelectedTargetLanguageIndex = _languages.IndexOfKey(_languages.FirstOrDefault(kvp => kvp.Value == _settings.TargetLanguage).Key);
        }
        else
        {
            SelectedTargetLanguageIndex = _languages.IndexOfKey(_settings.TargetLanguage);
        }
    }

    /// <summary>
    /// Saves settings file and checks if provided credentials are valid
    /// </summary>
    private void SaveSettings()
    {
        _settings.DeeplApiKey = DeeplApiKey;
        _settings.SourceLanguage = SelectedSourceLanguageIndex == -1 ? null : Languages[SelectedSourceLanguageIndex];
        _settings.TargetLanguage = Languages[SelectedTargetLanguageIndex];
        _settings.SaveToJson();

        var translationUtils = new TranslationUtils(_settings, new Utils.App.ProgressWindowUtils());

        if (!translationUtils.CanTranslate(_settings))
        {
            return;
        }
        
        UpdateButtonText = "Settings saved!";

        Task.Run(async () =>
        {
            await Task.Delay(2000);
            UpdateButtonText = "Save settings";
        });
    }

    /// <summary>
    /// Switches "to" and "from" languages in the UI
    /// </summary>
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

    /// <summary>
    /// Opens the link to my LinkedIn page in default browser
    /// </summary>
    /// <param name="uri"></param>
    private void OpenLinkedin(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var validUri))
        {
            Process.Start(new ProcessStartInfo(validUri.AbsoluteUri) { UseShellExecute = true });
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
