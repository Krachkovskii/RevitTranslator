using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Services;
using RevitTranslator.Services;
using RevitTranslator.UI.Views;
using RevitTranslator.ViewModels;

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
            var categoryService = scope.ServiceProvider.GetRequiredService<SheetSelectionService>();
            var result = categoryService.Initialize();
            if (result is not true) return;
            
            await scope.ServiceProvider.GetRequiredService<TranslationManager>().ExecuteAsync(categoryService.SelectedElements);
        }
        catch (Exception ex)
        {
            // todo: add logging
            Console.WriteLine(ex);
        }
    }
}