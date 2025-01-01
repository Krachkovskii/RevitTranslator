using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Services;

namespace RevitTranslator.Commands;

[UsedImplicitly]
public class TranslateModelCommand : ExternalCommand
{
    public override void Execute()
    {
        var instances = Document.EnumerateInstances().ToArray();
        
        var service = new BaseTranslationService();
        service.SelectedElements = instances;
        service.Execute();
    }
}
