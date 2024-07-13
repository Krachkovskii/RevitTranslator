using System.Diagnostics;
using System.Windows.Threading;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Views;

namespace RevitTranslatorAddin.Utils.Revit;

/// <summary>
/// This class handles updates, initiation and closing of a progress window.
/// </summary>
internal class ProgressWindowUtils
{
    internal static ViewModels.ProgressWindowViewModel ProgressViewModel { get; set; } = null;
    private static Views.NewProgressWindow _window = null;
    internal static AutoResetEvent WindowClosedEvent { get; set; } = new AutoResetEvent(false);
    internal static ManualResetEvent WindowReadyEvent { get; set; } = new ManualResetEvent(false);

    internal static void ShowProgressWindow()
    {
        ProgressViewModel = new ViewModels.ProgressWindowViewModel();
        _window = new NewProgressWindow(ProgressViewModel);

        ProgressViewModel.ProgressBarValue = 0;
        ProgressViewModel.ProgressBarMaximum = 100;

        _window.Closed += (s, e) =>
        {
            WindowClosedEvent.Set();
            Dispatcher.ExitAllFrames();
        };
        _window.Loaded += (s, e) => WindowReadyEvent.Set();
        _window.Show();

        Dispatcher.Run();
    }

    internal static void Start()
    {
        var windowThread = new Thread(ShowProgressWindow);
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.IsBackground = true;
        windowThread.Start();

        WindowReadyEvent.WaitOne();
    }

    internal static void End()
    {
        _window.Dispatcher.Invoke(() => _window.Close());
    }

    internal static void Update(int num, string source)
    {
        _window.Dispatcher.Invoke(() =>
        {
            ProgressViewModel.ProgressBarValue = num;
            ProgressViewModel.ProgressBarMaximum = TranslationUtils.TranslationsCount;
            ProgressViewModel.ProgressWindowCharacters = TranslationUtils.CharacterCount;
        });
    }

    internal static void RevitUpdate()
    {
        _window.Dispatcher.Invoke(() => ProgressViewModel.IsProgressBarIndeterminate = true);
    }
}
