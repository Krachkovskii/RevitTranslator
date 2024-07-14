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

    internal static void ShowProgressWindow()
    {
        VM = new ProgressWindowViewModel();
        PW = new ProgressWindow(VM);
        PW.Activate();
        PW.Focus();
        VM.Maximum = 100;
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
            VM.Value = num;
            VM.Maximum = TranslationUtils.TranslationsCount;
            VM.CharacterCount = TranslationUtils.CharacterCount;
        });
    }

    internal static void RevitUpdate()
    {
        //PW.Dispatcher.Invoke(() => VM.StatusTextBlock = "Updating Revit Elements");
    }
}