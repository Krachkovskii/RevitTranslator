using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.UI.Views;
using RevitTranslator.ViewModels;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class TranslateCategoriesCommand : ExternalCommand
{
    public override void Execute()
    {
        Host.ServiceProvider.GetRequiredService<CategoriesWindow>().ShowDialog();
    }
}