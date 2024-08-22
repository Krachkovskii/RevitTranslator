using System.Windows;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateSelectionCommand : BaseCommand
{
    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetRevitUtils(UiApplication);
        }

        CreateAndSetUtils();

        if (!_translationUtils.CanTranslate(_settings))
        {
            return;
        }

        var selection = RevitUtils.GetCurrentSelection();
        if (selection.Count == 0)
        {
            MessageBox.Show("Nothing was selected.");
            return;
        }

        RevitUtils.StartCommandTranslation(selection, _progressWindowUtils, _translationUtils, true, false);
    }
}