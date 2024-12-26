using System.Windows;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.Commands;

//TODO: Move to separate service
[UsedImplicitly]
public class TranslateSelectionCommand : ExternalCommand
{
    public override void Execute()
    {
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