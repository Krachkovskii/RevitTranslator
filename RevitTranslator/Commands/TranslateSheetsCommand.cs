using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Revit.Core.Services;
using RevitTranslator.Services;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class TranslateSheetsCommand : ExternalCommand
{
    public override async void Execute()
    {
        try
        {
            using var scope = Host.ServiceProvider.CreateScope();
            var sheetService = scope.ServiceProvider.GetRequiredService<SheetSelectionService>();
            var result = sheetService.Initialize();
            if (result is not true) return;

            await scope.ServiceProvider.GetRequiredService<TranslationManager>().ExecuteAsync(sheetService.SelectedElements);
        }
        catch (Exception ex)
        {
            // todo: add logging
            Console.WriteLine(ex);
        }
    }
}
