using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RevitTranslatorAddin.Utils.DeepL;

namespace RevitTranslatorAddin.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private Models.Settings _settings;
    private int _previousSourceIndex = -1;
    // TODO: Checkbox for paid plan, since it requires different API endpoint

    //TODO: saving state of settings to enable or disable update button if nothing has changed (test below)
    //private Tuple<string, int, int> _savedState = new(string.Empty, 0, 0);
    //private Tuple<string, int, int> CurrentState
    //{
    //    set => StateDiffers = !value.Equals(_savedState);
    //}
    //private bool _stateDiffers = false;
    //public bool StateDiffers { 
    //    get => _stateDiffers; 
    //    set 
    //    {
    //        _stateDiffers = value;
    //        OnPropertyChanged(nameof(StateDiffers));
    //    } 
    //}

    private string _deeplApiKey;
    public string DeeplApiKey
    {
        get => _deeplApiKey;
        set
        {
            _deeplApiKey = value;
            OnPropertyChanged(nameof(DeeplApiKey));
            //CurrentState = Tuple.Create(DeeplApiKey, SelectedSourceLanguageIndex, SelectedTargetLanguageIndex);
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
            //CurrentState = Tuple.Create(DeeplApiKey, SelectedSourceLanguageIndex, SelectedTargetLanguageIndex);
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
                //CurrentState = Tuple.Create(DeeplApiKey, SelectedSourceLanguageIndex, SelectedTargetLanguageIndex);
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
        UpdateButtonText = "Save Settings";
    }
        
    private void LoadSettings()
    {
        _settings = Models.Settings.LoadFromJson();
        DeeplApiKey = _settings.DeeplApiKey;
        _languages = _settings.Languages;
        Languages = new ObservableCollection<string>(_languages.Keys);
        IsPaidPlan = _settings.IsPaidPlan;

        // loading source language
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

        // loading target language
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
        _settings.DeeplApiKey = DeeplApiKey;
        _settings.SourceLanguage = SelectedSourceLanguageIndex == -1 ? null : Languages[SelectedSourceLanguageIndex];
        _settings.TargetLanguage = Languages[SelectedTargetLanguageIndex];
        _settings.SaveToJson();
        //_savedState = Tuple.Create(DeeplApiKey, SelectedSourceLanguageIndex, SelectedTargetLanguageIndex);

        if (!TranslationUtils.CanTranslate(Models.Settings.LoadFromJson()))
        {
            return;
        }
        
        UpdateButtonText = "Settings saved!";

        Task.Run(async () =>
        {
            await Task.Delay(3000);
            UpdateButtonText = "Save settings";
        });
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
        SelectedTargetLanguageIndex = index1;
    }

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

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        return false;
    }
}
