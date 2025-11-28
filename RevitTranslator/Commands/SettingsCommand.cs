using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.UI.Views;
using SettingsViewModel = RevitTranslator.ViewModels.SettingsViewModel;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class SettingsCommand : ExternalCommand
{
    public override void Execute()
    {
        Host.ServiceProvider.GetRequiredService<SettingsWindow>().Show();
    }
}
