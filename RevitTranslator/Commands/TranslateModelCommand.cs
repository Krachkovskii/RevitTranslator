using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Services;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class TranslateModelCommand : ExternalCommand
{
    public override void Execute()
    {
        var instances = Document.EnumerateInstances().ToArray();
        if (instances.Length == 0) return;
        
        var service = new BaseTranslationService();
        service.SelectedElements = instances;
        service.Execute();
    }
}
