using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Extensions;
using RevitTranslator.Services;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class TranslateSelectionCommand : ExternalCommand
{
    public override void Execute()
    {
        var selection = Context.ActiveUiDocument!
            .GetSelectedElements()
            .ToArray();
        
        if (selection.Length == 0) return;
        
        var service = new BaseTranslationService();
        service.SelectedElements = selection;
        service.Execute();
    }
}