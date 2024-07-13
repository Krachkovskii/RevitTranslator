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
    private static ExternalEvent _exEvent = null;
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

        IExternalEventHandler handler = new ElementUpdateHandler();
        _exEvent = ExternalEvent.Create(handler);

        List<ElementId> selection = RevitUtils.UIDoc.Selection.GetElementIds().ToList();
        var finished = _utils.StartTranslation(selection);

        if (finished && TranslationUtils.Translations.Count > 0)
        {
            _exEvent.Raise();
            RevitUtils.SetTemporaryFocus();
        }

        ProgressWindowUtils.End();
    }
}