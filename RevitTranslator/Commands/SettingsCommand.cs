using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Services;
using RevitTranslator.UI.Views;
using SettingsViewModel = RevitTranslator.ViewModels.SettingsViewModel;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class SettingsCommand : ExternalCommand
{
    public override void Execute()
    {
        var parentWindow = UiApplication.MainWindowHandle.ToWindow();
        Host.ServiceProvider.GetRequiredService<ScopedWindowService>().ShowDialog<SettingsWindow>(parentWindow);
    }
}
