﻿using Nice3point.Revit.Toolkit.External;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateSelectionCommand : ExternalCommand
{
    private TranslationUtils _utils = null;
    private Models.Settings _settings = null;

    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetUtils(UiApplication);
        }

        _settings = Models.Settings.LoadFromJson();
        
        if (!TranslationUtils.CanTranslate(_settings))
        {
            return;
        }

        _utils = new TranslationUtils(_settings);
        ProgressWindowUtils.Start();

        RevitUtils.ExEventHandler = new ElementUpdateHandler();
        RevitUtils.ExEvent = ExternalEvent.Create(RevitUtils.ExEventHandler);

        List<ElementId> selection = RevitUtils.UIDoc.Selection.GetElementIds().ToList();
        //var finished = _utils.StartTranslation(selection);
        var finishedTask = Task.Run( async () => await _utils.StartTranslationAsync(selection));
        var finished = finishedTask.GetAwaiter().GetResult();

        if (TranslationUtils.Translations.Count > 0)
        {
            if (!finished)
            {
                var proceed = TranslationUtils.ProceedWithUpdate();
                if (!proceed)
                {
                    return;
                }
            }

            RevitUtils.ExEvent.Raise();
            RevitUtils.SetTemporaryFocus();

        }

        ProgressWindowUtils.End();
    }
}   