using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Models;
using System.Windows;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;
using Autodesk.Revit.DB;
using RevitTranslatorAddin.Utils.App;
using Autodesk.Revit.UI.Selection;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateModelCommand : ExternalCommand
{
    private TranslationUtils _translationUtils = null;
    private ProgressWindowUtils _progressWindowUtils = null;
    private Models.Settings _settings = null;
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

        // only allow elements of user-visible categories via ElementMulticategoryFilter
        var filter = CreateCategoryFilter();
        var instances = GetModelInstances(filter);
        int types = GetModelTypes(filter);

        var elementCount = CountTotalElements(instances.Count, types);
        var warningResult = ShowElementCountWarning(elementCount);

        if (warningResult.Equals(MessageBoxResult.No))
        {
            return;
        }

        _progressWindowUtils.Start();

        RevitUtils.ExEventHandler = new ElementUpdateHandler();
        RevitUtils.ExEvent = ExternalEvent.Create(RevitUtils.ExEventHandler);

        var textRetriever = new ElementTextRetriever(_progressWindowUtils);
        textRetriever.ProcessElements(instances);
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

    private ElementMulticategoryFilter CreateCategoryFilter()
    {
        List<BuiltInCategory> categoryList = CreateCategoryList();
        var filter = new ElementMulticategoryFilter(categoryList);
        return filter;
    }

    private List<BuiltInCategory> CreateCategoryList()
    {
        List<BuiltInCategory> categoryList = [];

        var categories = CategoriesModel.GetCategories();
        foreach (Category category in categories)
        {
            categoryList.Add(category.BuiltInCategory);
        }

        return categoryList;
    }

    private List<Element> GetModelInstances(ElementMulticategoryFilter filter)
    {
        List<Element> instances = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .ToList();

        return instances;
    }

    private int GetModelTypes(ElementMulticategoryFilter filter)
    {
        int types = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsElementType()
            .GetElementCount();

        return types;
    }

    private int CountTotalElements(int instCount, int typesCount)
    {
        var rounded = (int)Math.Round((instCount + typesCount) / 100d) * 100;
        return rounded;
    }

    private bool ShowElementCountWarning(int count)
    {
        var result = MessageBox.Show($"You're about to translate all text properties of {count}+ elements. " +
                $"It can be time-consuming and you might hit translation limits.\n\n" +
                $"It will also translate elements such as view names or any annotations.\n" +
                $"Consider selecting only necessary categories.\n\n" +
                $"Are you sure you want to translate the whole model?",
                "Large number of translations!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) { return true; }
        else { return false; }
    }
    private void CreateAndSetUtils()
    {
        _settings = Models.Settings.LoadFromJson();
        _progressWindowUtils = new ProgressWindowUtils();
        ElementUpdateHandler.ProgressWindowUtils = _progressWindowUtils;
        _translationUtils = new TranslationUtils(_settings, _progressWindowUtils);
        _progressWindowUtils.TranslationUtils = _translationUtils;
    }
}
