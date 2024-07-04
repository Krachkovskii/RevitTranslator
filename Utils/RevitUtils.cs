using Autodesk.Revit.UI;

namespace RevitTranslatorAddin.Utils;
internal class RevitUtils
{
    internal static UIApplication UIApp = null;
    internal static Autodesk.Revit.ApplicationServices.Application App = null;
    internal static UIDocument UIDoc = null;
    internal static Document Doc = null;
    internal static void SetUtils(UIApplication uiapp)
    {
        UIApp = uiapp;
        App = uiapp.Application;
        UIDoc = uiapp.ActiveUIDocument;
        Doc = UIDoc.Document;
    }
}
