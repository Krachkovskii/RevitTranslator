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
    internal static void SetTemporaryFocus()
    {
        /// <summary>
        /// The GetForegroundWindow function returns a 
        /// handle to the foreground window.
        /// </summary>
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Move the window associated with the passed 
        /// handle to the front.
        /// </summary>
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        var hBefore = GetForegroundWindow();

        SetForegroundWindow(ComponentManager.ApplicationWindow);
        SetForegroundWindow(hBefore);
    }
}
