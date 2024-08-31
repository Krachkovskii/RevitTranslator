using RevitTranslatorAddin.Utils.DeepL;

namespace RevitTranslatorAddin.ViewModels;

public partial class ProgressWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _buttonText = string.Empty;

    [ObservableProperty] private int _characterCount = 0;

    [ObservableProperty] private int _currentValue = 0;

    [ObservableProperty] private bool _isProgressBarIndeterminate = false;

    [ObservableProperty] private bool _isStopEnabled = true;

    [ObservableProperty] private int _maximum = 0;

    [ObservableProperty] private int _monthlyLimit = 500000;

    [ObservableProperty] private int _monthlyUsage = 0;

    [ObservableProperty] private double _progressBarOpacity = 1;

    [ObservableProperty] private string _statusTextBlock = string.Empty;

    public ProgressWindowViewModel()
    {
        MonthlyLimit = TranslationUtils.Limit;
        MonthlyUsage = TranslationUtils.Usage;
        IsStopEnabled = true;
        IsProgressBarIndeterminate = true;
        ButtonText = "Extracting model data...";
    }
    public CancellationTokenSource Cts { get; set; } = null;
    public bool IsStopRequested { get; set; } = false;
    public bool TranslationFinished
    {
        get; set;
    } = false;

    /// <summary>
    /// Represents the end of translation process
    /// </summary>
    public void TranslationsFinishedStatus()
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

    /// <summary>
    /// Represents the start of translation process
    /// </summary>
    public void TranslationStartedStatus()
    {
        IsProgressBarIndeterminate = false;
        ButtonText = "Stop translation";
    }
    /// <summary>
    /// Represents the end of Revit model update
    /// </summary>
    public void UpdateFinished()
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

    /// <summary>
    /// Represents the start of Revit model update
    /// </summary>
    public void UpdateStarted()
    {
        ButtonText = "Updating Revit model...";
        IsProgressBarIndeterminate = true;
    }

    /// <summary>
    /// Cancels the translation process
    /// </summary>
    [RelayCommand]
    private void Stop()
    {
        Cts.Cancel();
        ButtonText = "Stopping translation...";
        IsStopEnabled = false;
        IsStopRequested = true;
    }
}