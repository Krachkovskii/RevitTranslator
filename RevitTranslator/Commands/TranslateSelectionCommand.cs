using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Extensions;
using RevitTranslator.Services;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class TranslateSelectionCommand : ExternalCommand
{
    public override async void Execute()
    {
        try
        {
            using var scope = Host.ServiceProvider.CreateScope();
            var selection = UiDocument.GetSelectedElements().ToArray();
            if (selection.Length == 0) return;
        
            await scope.ServiceProvider.GetRequiredService<TranslationManager>().ExecuteAsync(selection);
        }
        catch (Exception ex)
        {
            // todo: add logging
            Debug.WriteLine(ex);
        }
    }
}