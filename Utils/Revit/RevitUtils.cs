using System.Diagnostics;
using System.Runtime.InteropServices;
using Autodesk.Revit.UI;
using Autodesk.Windows;

namespace RevitTranslatorAddin.Utils.Revit;

/// <summary>
/// Represents active Revit context. Updates at the start of command execution.
/// </summary>
internal class RevitUtils
{
    internal static UIApplication UIApp = null;
    internal static Autodesk.Revit.ApplicationServices.Application App = null;
    internal static UIDocument UIDoc = null;
    internal static Document Doc = null;

    internal static ExternalEvent ExEvent = null;
    internal static IExternalEventHandler ExEventHandler = null;

    /// <summary>
    /// Characters that can't be used in certain Revit text properties.
    /// </summary>
    internal static readonly List<char> ForbiddenSymbols = new()
    {
        '\\', ':', '{', '}', '[', ']', '|', ';', '<', '>', '?', '`', '~'
    };
    internal static void SetUtils(UIApplication uiapp)
    {
        UIApp = uiapp;
        App = uiapp.Application;
        UIDoc = uiapp.ActiveUIDocument;
        Doc = UIDoc.Document;
    }

    /// <summary>
    /// Sets focus on a main Revit window momentarily.
    /// Code snippet created by Jeremy Tammik.
    /// </summary>
    //internal static void SetTemporaryFocus()
    //{
    //    /// <summary>
    //    /// The GetForegroundWindow function returns a 
    //    /// handle to the foreground window.
    //    /// </summary>
    //    [DllImport("user32.dll")]
    //    static extern IntPtr GetForegroundWindow();

    //    /// <summary>
    //    /// Move the window associated with the passed 
    //    /// handle to the front.
    //    /// </summary>
    //    [DllImport("user32.dll")]
    //    static extern bool SetForegroundWindow(IntPtr hWnd);

    //    var hBefore = GetForegroundWindow();

    //    SetForegroundWindow(ComponentManager.ApplicationWindow);
    //    SetForegroundWindow(hBefore);
    //}



    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    /// <summary>
    /// Sets focus on the main Revit window momentarily.
    /// </summary>
    internal static void SetTemporaryFocus()
    {
        IntPtr hBefore = GetForegroundWindow();

        // Find the Revit main window
        IntPtr revitWindow = FindRevitMainWindow();
        if (revitWindow != IntPtr.Zero)
        {
            SetForegroundWindow(revitWindow);
            SetForegroundWindow(hBefore);
        }
        else
        {
            // Handle the case where Revit window is not found
            Debug.WriteLine("Revit main window not found.");
        }
    }

    /// <summary>
    /// Finds the main window of the current Revit process.
    /// </summary>
    /// <returns>IntPtr of the Revit main window, or IntPtr.Zero if not found.</returns>
    private static IntPtr FindRevitMainWindow()
    {
        Process currentProcess = Process.GetCurrentProcess();
        IntPtr revitWindow = IntPtr.Zero;

        foreach (Process process in Process.GetProcessesByName("Revit"))
        {
            if (process.Id == currentProcess.Id)
            {
                revitWindow = process.MainWindowHandle;
                break;
            }
        }

        return revitWindow;
    }

}
