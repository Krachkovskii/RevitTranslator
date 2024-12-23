using System.Windows;
using RevitTranslator.Utils.Revit;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslator.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateSelectionCommand : BaseTranslationCommand
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

        var selection = ElementRetriever.GetCurrentSelection();
        if (selection.Count == 0)
        {
            MessageBox.Show("Nothing was selected.");
            return;
        }

        StartCommandTranslation(selection, _progressWindowUtils, _translationUtils, true, false);
    }
}