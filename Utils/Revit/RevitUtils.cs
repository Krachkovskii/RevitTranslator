using System.Diagnostics;
using System.Runtime.InteropServices;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.ElementTextRetrievers;

namespace RevitTranslatorAddin.Utils.Revit;

/// <summary>
/// Represents active Revit context and general Revit-related methods. 
/// Context is updated every time a command is being executed.
/// </summary>
internal class RevitUtils
{
    internal static Autodesk.Revit.ApplicationServices.Application App = null;
    internal static Document Doc = null;

    /// <summary>
    /// ExternalEvent to be associated with 
    /// </summary>
    internal static ExternalEvent ExEvent { get; private set; } = null;
    internal static IExternalEventHandler ExEventHandler = null;
    internal static UIApplication UIApp = null;
    internal static UIDocument UIDoc = null;
    internal static void CreateAndAssignEvents()
    {
        ExEventHandler = new ElementUpdateHandler();
        ExEvent = ExternalEvent.Create(ExEventHandler);
    }

    /// <summary>
    /// Gets Revit elements that are currently selected in the UI
    /// </summary>
    /// <returns>
    /// List of Elements
    /// </returns>
    internal static List<Element> GetCurrentSelection()
    {
        var ids = UIDoc.Selection.GetElementIds().ToList();
        var elements = GetElementsFromIds(ids);

        return elements;
    }

    /// <summary>
    /// Gets corresponding elements for all provided ElementIds
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    internal static List<Element> GetElementsFromIds(IEnumerable<ElementId> ids)
    {
        var elements = new List<Element>();

        foreach (var id in ids)
        {
            var el = Doc.GetElement(id);
            if (el != null)
            {
                elements.Add(el);
            }
        }

        return elements;
    }

    /// <summary>
    /// Gets all unique tagged elements for all tags in provided list of elements.
    /// </summary>
    /// <param name="tags">
    /// Elements to process. Can contain any elements, but only IndependentTags will be processed.
    /// </param>
    /// <returns>
    /// Unique tagged elements.
    /// </returns>
    internal static HashSet<Element> GetTaggedElements(IEnumerable<Element> tags)
    {
        var set = new HashSet<Element>();

        foreach (var t in tags)
        {
            if (t is not IndependentTag tag)
            {
                continue;
            }

            var tagElementIds = tag.GetTaggedLocalElementIds();
            var taggedElements = GetElementsFromIds(tagElementIds);
            var tagElements = tag.GetTaggedLocalElements().ToList();
            set.UnionWith(taggedElements);
        }

        set.RemoveWhere(n => n == null);
        return set;
    }

    internal static void SetRevitUtils(UIApplication uiapp)
    {
        UIApp = uiapp;
        App = uiapp.Application;
        UIDoc = uiapp.ActiveUIDocument;
        Doc = UIDoc.Document;
    }

    internal static void StartCommandTranslation(List<Element> elements, ProgressWindowUtils pwUtils, TranslationUtils tUtils, bool callFromContext, bool translateProjectParameters)
    {
        if (elements == null 
            || elements.Count == 0
            || pwUtils == null
            || tUtils == null)
        {
            return;
        }

        if (callFromContext)
        {
            CreateAndAssignEvents();
        }

        pwUtils.Start();

        var textRetriever = new BatchTextRetriever(elements, translateProjectParameters);
        var taskHandler = new MultiTaskTranslationHandler(tUtils, textRetriever.UnitGroups, pwUtils);

        var result = taskHandler.PerformTranslation();

        if (taskHandler.TotalTranslationCount > 0)
        {

            if (!result.Completed)
            {
                var proceed = TranslationUtils.ProceedWithUpdate();
                if (!proceed)
                {
                    return;
                }
            }

            ElementUpdateHandler.TranslationUnitGroups = textRetriever.UnitGroups;

            ExEvent.Raise();
            SetTemporaryFocus();
        }
        pwUtils.End();
    }

    #region Revit Window activator
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
    /// <returns>
    /// IntPtr of the Revit main window, or IntPtr.Zero if not found.
    /// </returns>
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

    // This method was generated by Claude, since Jeremy's approach doesn't seem to work in R25.
    // This approach seems to work well in both R23 and R25.
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);
    #endregion
}
