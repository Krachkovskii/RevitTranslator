using System.Diagnostics;
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
    internal ProgressWindowViewModel VM { get; set; } = null;
    internal ProgressWindow PW { get; set; } = null;
    internal AutoResetEvent WindowClosedEvent { get; set; } = new AutoResetEvent(false);
    internal ManualResetEvent WindowReadyEvent { get; set; } = new ManualResetEvent(false);
    internal CancellationTokenHandler TokenHandler { get; set; } = null;

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

        PW.Closed += (s, e) => WindowClosedEvent.Set();
        PW.Loaded += (s, e) => WindowReadyEvent.Set();

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

        WindowReadyEvent.WaitOne();
    }

    /// <summary>
    /// Indicates the end of translation process, changing the state of progress window
    /// </summary>
    internal void End()
    {
        VM.TranslationsFinished();
    }

    /// <summary>
    /// Updates values of a ProgressWindow
    /// </summary>
    /// <param name="num">
    /// Current number of translations
    /// </param>
    internal void Update(int num)
    {
        if (TokenHandler.Cts != null && !TokenHandler.Cts.IsCancellationRequested)
        {
            PW.Dispatcher.Invoke(() =>
            {
                VM.Value = num;
                VM.Maximum = TranslationUtils.TranslationsCount;
                VM.CharacterCount = TranslationUtils.CharacterCount;
                VM.MonthlyUsage = TranslationUtils.Usage + VM.CharacterCount;
            });
        }
    }

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
}