using System.Windows;
using System.Windows.Interop;

namespace RevitTranslator.Common.Extensions;

public static class WindowExtensions
{
    public static Window? ToWindow(this IntPtr hWnd)
    {
        var source = HwndSource.FromHwnd(hWnd);
        return source?.RootVisual as Window;
    }

    public static bool? ShowDialog(this Window window, Window? parentWindow)
    {
        window.Owner = parentWindow;
        return window.ShowDialog();
    }

    public static void Show(this Window window, Window? parentWindow)
    {
        window.Owner = parentWindow;
        window.Show();
    }
}