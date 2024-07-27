using System.Windows.Threading;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.ViewModels;
using RevitTranslatorAddin.Views;

namespace RevitTranslatorAddin.Utils.Revit;

/// <summary>
/// This class handles updates, initiation and closing of a progress window.
/// </summary>
internal class ProgressWindowUtils
{
    internal static ProgressWindowViewModel VM { get; set; } = null;
    internal static ProgressWindow PW { get; set; } = null;
    internal static AutoResetEvent WindowClosedEvent { get; set; } = new AutoResetEvent(false);
    internal static ManualResetEvent WindowReadyEvent { get; set; } = new ManualResetEvent(false);

    /// <summary>
    /// Sets up a new instance of a ProgressWindow
    /// </summary>
    internal static void ShowProgressWindow()
    {
        if (VM != null || PW != null) 
        {
            PW = null;
            VM = null;
        }
        VM = new ProgressWindowViewModel();
        PW = new ProgressWindow(VM);

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
    internal static void Start()
    {
        var windowThread = new Thread(ShowProgressWindow);
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();

        WindowReadyEvent.WaitOne();
    }

    /// <summary>
    /// Indicates the end of translation process, changing the state of progress window
    /// </summary>
    internal static void End()
    {
        VM.TranslationsFinished();
    }

    /// <summary>
    /// Updates values of a ProgressWindow
    /// </summary>
    /// <param name="num">
    /// Int
    ///     Current number of translations
    /// </param>
    internal static void Update(int num)
    {
        if (ProgressWindowViewModel.Cts != null && !ProgressWindowViewModel.Cts.IsCancellationRequested)
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
}