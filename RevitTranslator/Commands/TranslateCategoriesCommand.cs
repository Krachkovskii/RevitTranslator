using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Services;
using RevitTranslator.UI.Views;
using RevitTranslator.ViewModels;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class TranslateCategoriesCommand : ExternalCommand
{
    public override void Execute()
    {
        var parentWindow = UiApplication.MainWindowHandle.ToWindow();
        Host.ServiceProvider.GetRequiredService<ScopedWindowService>().Show<CategoriesWindow>(parentWindow);
    }
}