using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RevitTranslatorAddin.ViewModels;

public class ProgressWindowViewModel : INotifyPropertyChanged
{
    private int _maximum = 0;
    private int _value = 0;
    private int _characterCount = 0;
    private string _statusTextBlock = string.Empty;
    private bool _isProgressBarIndeterminate = false;
    private bool _isStopEnabled = true;
    private bool _isStopRequested = false;
    private bool _translationFinished = false;
    private string _afterText = string.Empty;
    private Visibility _afterTextVisible = Visibility.Invisible;

    internal static CancellationTokenSource Cts = null;

    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = value;
            OnPropertyChanged(nameof(Maximum));
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    public string StatusTextBlock
    {
        get => _statusTextBlock;
        set
        {
            _statusTextBlock = value;
            OnPropertyChanged(nameof(StatusTextBlock));
        }
    }

    public int CharacterCount
    {
        get => _characterCount;
        set
        {
            _characterCount = value;
            OnPropertyChanged(nameof(CharacterCount));
        }
    }

    public bool IsProgressBarIndeterminate
    {
        get => _isProgressBarIndeterminate;
        set
        {
            _isProgressBarIndeterminate = value;
            OnPropertyChanged(nameof(IsProgressBarIndeterminate));
        }
    }

    public bool IsStopEnabled
    {
        get => _isStopEnabled;
        set
        {
            _isStopEnabled = value;
            OnPropertyChanged(nameof(IsStopEnabled));
        }
    }

    public bool IsStopRequested
    {
        get => _isStopRequested;
        set
        {
            _isStopRequested = value;
            OnPropertyChanged(nameof(IsStopRequested));
        }
    }

    public bool TranslationFinished
    {
        get => _translationFinished;
        set
        {
            _translationFinished = value;
            OnPropertyChanged(nameof(TranslationFinished));
        }
    }

    public string AfterText
    {
        get => _afterText;
        set
        {
            _afterText = value;
            OnPropertyChanged(nameof(AfterText));
        }
    }

    public Visibility AfterTextVisible
    {
        get => _afterTextVisible;
        set
        {
            _afterTextVisible = value;
            OnPropertyChanged(nameof(AfterTextVisible));
        }
    }

    public ICommand StopCommand
    {
        get;
    }

    public ICommand CloseWindowCommand
    {
        get;
    }

    private void OnWindowClosed(object parameter)
    {
    }

    internal void TranslationsFinished()
    {
        if (IsStopEnabled)
        {
            IsStopEnabled = false;
        }

        AfterTextVisible = Visibility.Visible;
        
        if (IsStopRequested)
        {
            AfterText = "Translation was interrupted";
        }
        else
        {
            AfterText = "Translation finished!";
        }
    }

    private void Stop()
    {
        IsStopEnabled = false;
        IsStopRequested = true;
        Cts?.Cancel();
    }

    public ProgressWindowViewModel()
    {
        StopCommand = new RelayCommand(Stop);
        AfterTextVisible = Visibility.Invisible;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}