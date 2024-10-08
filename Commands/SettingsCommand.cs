﻿using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.ViewModels;
using RevitTranslatorAddin.Views;

namespace RevitTranslatorAddin.Commands;

[UsedImplicitly]
[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

public class SettingsCommand : ExternalCommand
{
    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetRevitUtils(UiApplication);
        }

        var viewModel = new SettingsViewModel();
        var settingsWindow = new SettingsWindow(viewModel);
        settingsWindow.Show();
    }
}
