using Nice3point.Revit.Toolkit.External;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RevitTranslatorAddin.Utils.App;
using System.Collections.Generic;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateSelectionCommand : ExternalCommand
{
    private TranslationUtils _translationUtils = null;
    private Models.Settings _settings = null;
    private ProgressWindowUtils _progressWindowUtils = null;

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

        CreateUtils();
        CreateAndAssignEvents();

        _progressWindowUtils.Start();

        List<Element> selection = GetCurrentSelection();

        var textRetriever = new ElementTextRetriever(_progressWindowUtils);
        textRetriever.ProcessElements(selection);
        var taskHandler = new MultiTaskTranslationHandler(_translationUtils, textRetriever.TranslationUnits, _progressWindowUtils);
        var result = taskHandler.StartTranslation();

        if (textRetriever.TranslationUnits.Count > 0)
        {

            if (!result.Completed)
            {
                var proceed = TranslationUtils.ProceedWithUpdate();
                if (!proceed)
                {
                    return;
                } 
            }

            ElementUpdateHandler.TranslationUnits = textRetriever.TranslationUnits;

            RevitUtils.ExEvent.Raise();
            RevitUtils.SetTemporaryFocus();
        }
        _progressWindowUtils.End();
    }

    private List<Element> GetCurrentSelection()
    {
        var ids = RevitUtils.UIDoc.Selection.GetElementIds().ToList();
        var elements = RevitUtils.GetElementsFromIds(ids);
        
        return elements;
    }

    private void CreateUtils()
    {
        _progressWindowUtils = new ProgressWindowUtils();
        ElementUpdateHandler.ProgressWindowUtils = _progressWindowUtils;
        _translationUtils = new TranslationUtils(_settings, _progressWindowUtils);
    }

    private void CreateAndAssignEvents()
    {
        RevitUtils.ExEventHandler = new ElementUpdateHandler();
        RevitUtils.ExEvent = ExternalEvent.Create(RevitUtils.ExEventHandler);
    }
}