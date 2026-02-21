using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Services;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class TranslateModelCommand : ExternalCommand
{
    public override async void Execute()
    {
        try
        {
            using var scope = Host.ServiceProvider.CreateScope();
            var instances = Document.EnumerateInstances().ToArray();
            if (instances.Length == 0) return;
        
            await scope.ServiceProvider.GetRequiredService<TranslationManager>().ExecuteAsync(instances);
        }
        catch (Exception ex)
        {
            // todo: add logging
            Console.WriteLine(ex);
        }
    }
}
