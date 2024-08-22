using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.ViewModels;
using RevitTranslatorAddin.Views;

[assembly: InternalsVisibleTo("RevitTranslatorAddin.Tests")]
namespace RevitTranslatorAddin.Utils.App;

/// <summary>
/// This class handles updates, initiation and closing of a progress window.
/// </summary>
public class ProgressWindowUtils
{
    /// <summary>
    /// Current ViewModel instance.
    /// </summary>
    public ProgressWindowViewModel VM { get; private set; } = null;
    
    /// <summary>
    /// Current View instance.
    /// </summary>
    public ProgressWindow PW { get; private set; } = null;
    
    /// <summary>
    /// Handler for CancellationToken.
    /// </summary>
    public CancellationTokenHandler TokenHandler { get; set; } = null;
    private AutoResetEvent _windowClosedEvent { get; set; } = new AutoResetEvent(false);
    private ManualResetEvent _windowReadyEvent { get; set; } = new ManualResetEvent(false);

    public ProgressWindowUtils() {}

    /// <summary>
    /// Sets up a new instance of a ProgressWindow
    /// </summary>
    private  void ShowProgressWindow()
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
    public void Start()
    {
        var windowThread = new Thread(ShowProgressWindow);
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();

        _windowReadyEvent.WaitOne();
    }

    /// <summary>
    /// Indicates the end of translation process, changing the state of progress window
    /// </summary>
    public void End()
    {
        if (VM == null)
        {
            Debug.WriteLine("ViewModel of a progressWindow is null");
            return;
        }

        VM.TranslationsFinishedStatus();
    }

    /// <summary>
    /// Updates total number of translations to be performed
    /// </summary>
    /// <param name="num"></param>
    public void UpdateTotal(int num)
    {
        if (PW == null || VM == null)
        {
            Debug.WriteLine("ProgressWindow or its ViewModel is null");
            return;
        }

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
    public void UpdateCurrent(int num)
    {
        if (PW == null || VM == null)
        {
            Debug.WriteLine("ProgressWindow or its ViewModel is null");
            return;
        }

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
    public void StartTranslationStatus()
    {
        if (PW == null || VM == null)
        {
            Debug.WriteLine("ProgressWindow or its ViewModel is null");
            return;
        }

        PW.Dispatcher.Invoke(() =>
        {
            VM.TranslationStartedStatus();
        });
    }
}