using System.Threading;
using System.Windows.Threading;
using RevitTranslatorAddin.Views;

namespace RevitTranslatorAddin.Utils;

internal class ProgressWindowUtils
{
    internal static ProgressWindow PW { get; set; } = null;
    internal static AutoResetEvent WindowClosedEvent { get; set; } = new AutoResetEvent(false);
    internal static ManualResetEvent WindowReadyEvent { get; set; } = new ManualResetEvent(false);

    internal static void ShowProgressWindow()
    {
        PW = new ProgressWindow();
        PW.ProgressBar.Minimum = 0;
        PW.ProgressBar.Maximum = 100;
        PW.Closed += (s, e) => WindowClosedEvent.Set();
        PW.Loaded += (s, e) => WindowReadyEvent.Set();
        PW.Show();
        Dispatcher.Run();
    }

    internal static void Start()
    {
        var windowThread = new Thread(ShowProgressWindow);
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();

        WindowReadyEvent.WaitOne();
    }

    internal static void End()
    {
        PW.Dispatcher.Invoke(() => PW.Close());
    }

    internal static void Update(int num, string source)
    {
        PW.Dispatcher.Invoke(() =>
        {
            PW.ProgressBar.Value = num;
            PW.ProgressBar.Maximum = TranslationUtils.TranslationsCount;
            PW.StatusTextBlock.Text = $"Finished {num} out of {TranslationUtils.TranslationsCount} translations to {source}";
        });
    }
}
