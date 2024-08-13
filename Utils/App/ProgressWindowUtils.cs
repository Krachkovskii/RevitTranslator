using System.Windows.Threading;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.ViewModels;
using RevitTranslatorAddin.Views;

namespace RevitTranslatorAddin.Utils.App;

/// <summary>
/// This class handles updates, initiation and closing of a progress window.
/// </summary>
public class ProgressWindowUtils
{
    /// <summary>
    /// Current ViewModel instance.
    /// </summary>
    internal ProgressWindowViewModel VM { get; set; } = null;
    
    /// <summary>
    /// Current View instance.
    /// </summary>
    internal ProgressWindow PW { get; set; } = null;
    
    /// <summary>
    /// Handler for CancellationToken.
    /// </summary>
    internal CancellationTokenHandler TokenHandler { get; set; } = null;
    private AutoResetEvent _windowClosedEvent { get; set; } = new AutoResetEvent(false);
    private ManualResetEvent _windowReadyEvent { get; set; } = new ManualResetEvent(false);

    internal ProgressWindowUtils()
    {
    }

    /// <summary>
    /// Sets up a new instance of a ProgressWindow
    /// </summary>
    private void ShowProgressWindow()
    {
        if (VM != null || PW != null)
        {
            PW = null;
            VM = null;
        }
        VM = new ProgressWindowViewModel();
        PW = new ProgressWindow(VM, this);

        PW.Activate();
        PW.Focus();

        PW.Closed += (s, e) => _windowClosedEvent.Set();
        PW.Loaded += (s, e) => _windowReadyEvent.Set();

        PW.Show();
        Dispatcher.Run();
    }

    /// <summary>
    /// Creates an instance of a ProgressWindow on a separate thread
    /// </summary>
    internal void Start()
    {
        var windowThread = new Thread(ShowProgressWindow);
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();

        _windowReadyEvent.WaitOne();
    }

    /// <summary>
    /// Indicates the end of translation process, changing the state of progress window
    /// </summary>
    internal void End()
    {
        VM.TranslationsFinishedStatus();
    }

    /// <summary>
    /// Updates total number of translations to be performed
    /// </summary>
    /// <param name="num"></param>
    internal void UpdateTotal(int num)
    {
        if (TokenHandler.Cts != null && !TokenHandler.Cts.IsCancellationRequested)
        {
            PW.Dispatcher.Invoke(() =>
            {
                VM.Maximum = num;
            });
        }
    }

    /// <summary>
    /// Updates current number of completed translations
    /// </summary>
    /// <param name="num"></param>
    internal void UpdateCurrent(int num)
    {
        if (TokenHandler.Cts != null && !TokenHandler.Cts.IsCancellationRequested)
        {
            PW.Dispatcher.Invoke(() =>
            {
                VM.Value = num;
                VM.CharacterCount = TranslationUtils.CharacterCount;
                VM.MonthlyUsage = TranslationUtils.Usage + TranslationUtils.CharacterCount;
            });
        }
    }

    /// <summary>
    /// Invokes "Fetching data..." state for Progress window
    /// </summary>
    internal void StartTranslationStatus()
    {
        PW.Dispatcher.Invoke(() =>
        {
            VM.TranslationStartedStatus();
        });
    }
}