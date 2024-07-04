using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils;
using Nice3point.Revit.Toolkit.External;
using System.Windows.Controls;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateModelCommand : ExternalCommand
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

        // only allow elements of user-visible categories
        List<BuiltInCategory> categoryList = [];
        var allCategories = RevitUtils.Doc.Settings.Categories;
        foreach (Category category in allCategories)
        {
            if (category.BuiltInCategory != BuiltInCategory.INVALID
            && category.IsVisibleInUI
            && (category.CategoryType != CategoryType.Invalid
                && category.CategoryType != CategoryType.Internal))
            {
                categoryList.Add(category.BuiltInCategory);
            }
        }
        var filter = new ElementMulticategoryFilter(categoryList);

        List<ElementId> instances = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .ToElementIds()
            .ToList();
        int types = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsElementType()
            .GetElementCount();

        var rounded = (int)Math.Round((instances.Count + types) / 100d) * 100;

        var td = new TaskDialog("Warning");
        td.MainInstruction = "Large number of translations!";
        td.MainContent = $"You're about to translate all text properties of {rounded}+ elements. " +
            $"It will be time-consuming and you might hit translation limits.\n\n" +
            $"Consider selecting only necessary categories.\n\n" +
            $"Are you sure you want to translate the whole model?";
        td.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
        td.DefaultButton = TaskDialogResult.No;

        var result = td.Show();
        if (result == TaskDialogResult.No)
        {
            return;
        }

        _utils = new TranslationUtils(_settings);
        ProgressWindowUtils.Start();

        IExternalEventHandler handler = new RevitElementUpdateHandler();
        _exEvent = ExternalEvent.Create(handler);
        _utils.StartTranslation(instances);

        if (TranslationUtils.Translations.Count > 0)
        {
            _exEvent.Raise();
        }
        ProgressWindowUtils.End();
    }
}
