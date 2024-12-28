using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Services;

namespace RevitTranslator.Commands;

[UsedImplicitly]
public class TranslateSelectionCommand : ExternalCommand
{
    public override void Execute()
    {
        var selection = Context.ActiveUiDocument!.Selection
            .GetElementIds()
            .ToList();
        
        var service = new BaseTranslationService();
        service.SelectedElements = selection;
        service.Execute();
    }
}