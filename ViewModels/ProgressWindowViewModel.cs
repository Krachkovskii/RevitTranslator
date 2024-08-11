using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.DeepL;

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
    private string _buttonText = string.Empty;
    private double _progressBarOpacity = 1;
    private int _monthlyUsage = 0;
    private int _monthlyLimit = 0;
    internal CancellationTokenSource Cts { get; set; } = null;

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

    public string ButtonText
    {
        get => _buttonText;
        set
        {
            _buttonText = value;
            OnPropertyChanged(nameof(ButtonText));
        }
    }

    public double ProgressBarOpacity
    {
        get => _progressBarOpacity;
        set
        {
            _progressBarOpacity = value;
            OnPropertyChanged(nameof(ProgressBarOpacity));
        }
    }

    public int MonthlyUsage
    {
        get => _monthlyUsage;
        set
        {
            _monthlyUsage = value;
            OnPropertyChanged(nameof(MonthlyUsage));
        }
    }

    public int MonthlyLimit
    {
        get => _monthlyLimit;
        set
        {
            _monthlyLimit = value;
            OnPropertyChanged(nameof(MonthlyLimit));
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

    public ProgressWindowViewModel()
    {
        StopCommand = new RelayCommand(Stop);
        MonthlyLimit = TranslationUtils.Limit;
        MonthlyUsage = TranslationUtils.Usage;
        IsStopEnabled = true;
        ButtonText = "Stop translation";
    }

    internal void TranslationsFinished()
    {
        if (IsStopEnabled)
        {
            IsStopEnabled = false;
        }

        if (IsStopRequested)
        {
            ButtonText = "Translation was interrupted";
        }
        else
        {
            ButtonText = "Translation finished!";
        }
    }

    internal void UpdateStarted()
    {
        ButtonText = "Updating Revit model...";
        IsProgressBarIndeterminate = true;
    }

    internal void UpdateFinished()
    {
        TranslationUtils.ClearTranslationCount();

        if (IsStopRequested)
        {
            ButtonText = "Translation interrupted | Elements updated";
        }
        else
        {
            ButtonText = "Elements translated!";
        }

        ProgressBarOpacity = 0.5;
        IsProgressBarIndeterminate = false;
    }

    private void Stop()
    {
        Cts.Cancel();
        ButtonText = "Stopping translation...";
        IsStopEnabled = false;
        IsStopRequested = true;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}