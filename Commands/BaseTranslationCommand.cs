using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.ElementTextRetrievers;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Commands;
public class BaseTranslationCommand : ExternalCommand
{
    protected ProgressWindowUtils _progressWindowUtils = null;
    protected Models.DeeplSettings _settings = null;
    protected TranslationUtils _translationUtils = null;

    /// <summary>
    /// Creates and sets all necessary utils, i.e. progress window, translation etc.
    /// </summary>
    protected void CreateAndSetUtils()
    {
        _settings = Models.DeeplSettings.LoadFromJson();
        _progressWindowUtils = new ProgressWindowUtils();
        ElementUpdateHandler.ProgressWindowUtils = _progressWindowUtils;
        _translationUtils = new TranslationUtils(_settings, _progressWindowUtils);
    }

    public override void Execute() => throw new NotImplementedException();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elements">Elements to process.</param>
    /// <param name="pwUtils">ProgressWindowUtils object for this run.</param>
    /// <param name="tUtils">TranslationUtils for this run.</param>
    /// <param name="callFromContext">True if called from Command; false if called from UI thread, e.g. ViewModel</param>
    /// <param name="translateProjectParameters">If true, project parameters will be considered.</param>
    internal static void StartCommandTranslation(List<Element> elements, ProgressWindowUtils pwUtils, TranslationUtils tUtils, bool callFromContext, bool translateProjectParameters)
    {
        if (elements == null
            || elements.Count == 0
            || pwUtils == null
            || tUtils == null)
        {
            return;
        }

        if (callFromContext)
        {
            RevitUtils.CreateAndAssignEvents();
        }

        pwUtils.Start();

        var textRetriever = new BatchTextRetriever(elements, translateProjectParameters);
        var taskHandler = new MultiTaskTranslationHandler(tUtils, textRetriever.UnitGroups, pwUtils);

        var result = taskHandler.TranslateUnits();

        if (taskHandler.TotalTranslationCount > 0)
        {

            if (!result.Completed)
            {
                var proceed = TranslationUtils.ProceedWithUpdate();
                if (!proceed)
                {
                    return;
                }
            }

            ElementUpdateHandler.TranslationUnitGroups = textRetriever.UnitGroups;

            RevitUtils.ExEvent.Raise();
            RevitUtils.SetTemporaryFocus();
        }
        pwUtils.Dispose();
    }
}
