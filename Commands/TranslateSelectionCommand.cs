using Nice3point.Revit.Toolkit.External;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.Utils.App;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateSelectionCommand : ExternalCommand
{
    private TranslationUtils _translationUtils = null;
    private Models.DeeplSettings _settings = null;
    private ProgressWindowUtils _progressWindowUtils = null;

    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetUtils(UiApplication);
        }

        CreateAndSetUtils();

        if (!_translationUtils.CanTranslate(_settings))
        {
            return;
        }

        CreateAndAssignEvents();

        _progressWindowUtils.Start();

        List<Element> selection = GetCurrentSelection();

        var textRetriever = new ElementTextRetriever(_progressWindowUtils, selection);
        var taskHandler = new MultiTaskTranslationHandler(_translationUtils, textRetriever.TranslationUnits, _progressWindowUtils);
        var result = taskHandler.PerformTranslation();

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

    /// <summary>
    /// Gets Revit elements that are currently selected in the UI
    /// </summary>
    /// <returns>
    /// List of Elements
    /// </returns>
    private List<Element> GetCurrentSelection()
    {
        var ids = RevitUtils.UIDoc.Selection.GetElementIds().ToList();
        var elements = RevitUtils.GetElementsFromIds(ids);
        
        return elements;
    }

    /// <summary>
    /// Creates and sets all necessary utils, i.e. progress window, translation etc.
    /// </summary>
    private void CreateAndSetUtils()
    {
        _settings = Models.DeeplSettings.LoadFromJson();
        _progressWindowUtils = new ProgressWindowUtils();
        ElementUpdateHandler.ProgressWindowUtils = _progressWindowUtils;
        _translationUtils = new TranslationUtils(_settings, _progressWindowUtils);
    }

    /// <summary>
    /// Creates and assigns External Event and its Handler
    /// </summary>
    private void CreateAndAssignEvents()
    {
        RevitUtils.ExEventHandler = new ElementUpdateHandler();
        RevitUtils.ExEvent = ExternalEvent.Create(RevitUtils.ExEventHandler);
    }
}