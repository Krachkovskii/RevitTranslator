using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using RevitTranslatorAddin.Utils;
using RevitTranslatorAddin.Models;
using CommunityToolkit.Mvvm.DependencyInjection;

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
            TaskDialog.Show("Error", "Set up API key and target language.");
            return;
        }

        _utils = new TranslationUtils(_settings);
        ProgressWindowUtils.Start();

        IExternalEventHandler handler = new RevitElementUpdateHandler();
        _exEvent = ExternalEvent.Create(handler);

        List<ElementId> selection = RevitUtils.UIDoc.Selection.GetElementIds().ToList();
        _utils.StartTranslation(selection);

        if (TranslationUtils.Translations.Count > 0)
        {
            _exEvent.Raise();
        }
        ProgressWindowUtils.End();
    }
}