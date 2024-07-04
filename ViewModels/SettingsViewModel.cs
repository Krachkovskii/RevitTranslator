using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;

namespace RevitTranslatorAddin.ViewModels;

public class SettingsViewModel : ObservableObject
{
    private Models.Settings _settings;
    private int _previousSourceIndex = -1;
    // TODO: Checkbox for paid plan, since it requires different API endpoint

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
    //private SortedDictionary<string, string> _languages;
    private SortedList<string, string> _languages;
    public ObservableCollection<string> Languages { get; private set; } = [];

    private string _selectedSourceLanguage;
    public string SelectedSourceLanguage
    {
        get => _selectedSourceLanguage;
        set
        {
            _selectedSourceLanguage = value;
            OnPropertyChanged(nameof(SelectedSourceLanguage));
        }
    }

    private string _selectedTargetLanguage;
    public string SelectedTargetLanguage
    {
        get => _selectedTargetLanguage;
        set
        {
            if (_selectedTargetLanguage != value)
            {
                _selectedTargetLanguage = value;
                OnPropertyChanged(nameof(SelectedTargetLanguage));
            }
        }
    }


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


    public bool IsSourceComboBoxEnabled => !IsAutoDetectChecked;

    private bool _isAutoDetectChecked;
    public bool IsAutoDetectChecked
    {
        get => _isAutoDetectChecked;
        set
        {
            _isAutoDetectChecked = value;
            OnPropertyChanged(nameof(IsAutoDetectChecked));
            if ( IsAutoDetectChecked ) 
            {
                _previousSourceIndex = SelectedSourceLanguageIndex;
                SelectedSourceLanguageIndex = -1;
            }
            else
            {
                SelectedSourceLanguageIndex = _previousSourceIndex;
            }
            OnPropertyChanged(nameof(IsSourceComboBoxEnabled));
        }
    }

    private string _updateButtonText = "Click me";
    public string UpdateButtonText
    {
        get => _updateButtonText;
        set
        {
            _updateButtonText = value;
            OnPropertyChanged(nameof(UpdateButtonText));
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
        UpdateButtonText = "Update Settings";
    }
        
    private void LoadSettings()
    {
        _settings = Models.Settings.LoadFromJson();
        DeeplApiKey = _settings.DeeplApiKey;
        _languages = _settings.Languages;
        Languages = new ObservableCollection<string>(_languages.Keys);

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

    private void SaveSettings()
    {
        UpdateButtonText = "Settings saved!";
        _settings.DeeplApiKey = DeeplApiKey;
        _settings.SourceLanguage = SelectedTargetLanguageIndex == -1 ? null : Languages[SelectedTargetLanguageIndex];
        _settings.TargetLanguage = Languages[SelectedTargetLanguageIndex];
        _settings.SaveToJson();

        Task.Delay(3000);

        UpdateButtonText = "Update Settings";
    }

    private void SwitchLanguages()
    {
        if (IsAutoDetectChecked)
        {
            IsAutoDetectChecked = false;
        }
        var index1 = SelectedSourceLanguageIndex;
        var index2 = SelectedTargetLanguageIndex;

        SelectedSourceLanguageIndex = index2;
        SelectedSourceLanguageIndex = index1;

        //string temp = SelectedSourceLanguage;
        //SelectedSourceLanguage = SelectedTargetLanguage;
        //SelectedTargetLanguage = temp;
    }

    private void OpenLinkedin(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var validUri))
        {
            Process.Start(new ProcessStartInfo(validUri.AbsoluteUri) { UseShellExecute = true });
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
