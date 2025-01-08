using Autodesk.Revit.Attributes;
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
        var viewModel = new SettingsViewModel();
        var settingsWindow = new SettingsWindow(viewModel);
        settingsWindow.Show();
    }
}
