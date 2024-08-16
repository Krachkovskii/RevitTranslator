using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Models;
using System.Windows;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.Utils.App;
using Autodesk.Revit.UI.Selection;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateModelCommand : ExternalCommand
{
    private TranslationUtils _translationUtils = null;
    private ProgressWindowUtils _progressWindowUtils = null;
    private Models.DeeplSettings _settings = null;
    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetRevitUtils(UiApplication);
        }

        CreateAndSetUtils();

        if (!_translationUtils.CanTranslate(_settings))
        {
            return;
        }

        // only allow elements of user-visible categories via ElementMulticategoryFilter
        var filter = CreateCategoryFilter();
        var instances = GetModelInstances(filter);
        int types = CountModelTypes(filter);

        var elementCount = CountTotalElements(instances.Count, types);
        var warningResult = ShowElementCountWarning(elementCount);

        if (warningResult.Equals(MessageBoxResult.No))
        {
            return;
        }

        RevitUtils.StartCommandTranslation(instances, _progressWindowUtils, _translationUtils, true);

        //RevitUtils.ExEventHandler = new ElementUpdateHandler();
        //RevitUtils.ExEvent = ExternalEvent.Create(RevitUtils.ExEventHandler);

        //_progressWindowUtils.Start();



        //var textRetriever = new ElementTextRetriever(_progressWindowUtils, instances);
        //var taskHandler = new MultiTaskTranslationHandler(_translationUtils, textRetriever.TranslationUnits, _progressWindowUtils);
        //var result = taskHandler.PerformTranslation();

        //if (textRetriever.TranslationUnits.Count > 0)
        //{
        //    if (!result.Completed)
        //    {
        //        var proceed = TranslationUtils.ProceedWithUpdate();
        //        if (!proceed)
        //        {
        //            return;
        //        }
        //    }

        //    ElementUpdateHandler.TranslationUnits = textRetriever.TranslationUnits;

        //    RevitUtils.ExEvent.Raise();
        //    RevitUtils.SetTemporaryFocus();
        //}
        //_progressWindowUtils.End();
    }

    /// <summary>
    /// Creates MulticategoryFilter to get all elements of valid categories
    /// </summary>
    /// <returns>
    /// The filter</returns>
    private ElementMulticategoryFilter CreateCategoryFilter()
    {
        List<BuiltInCategory> categoryList = CreateCategoryList();
        var filter = new ElementMulticategoryFilter(categoryList);
        return filter;
    }

    /// <summary>
    /// Gets BuiltInCategory for all valid categories
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get all element instances of valid categories
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    private List<Element> GetModelInstances(ElementMulticategoryFilter filter)
    {
        List<Element> instances = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .ToList();

        return instances;
    }

    /// <summary>
    /// Counts all Element Types of valuid categories
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    private int CountModelTypes(ElementMulticategoryFilter filter)
    {
        int types = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsElementType()
            .GetElementCount();

        return types;
    }

    /// <summary>
    /// Calculates the approximate number of elements that can be used for translation
    /// </summary>
    /// <param name="instCount"></param>
    /// <param name="typesCount"></param>
    /// <returns></returns>
    private int CountTotalElements(int instCount, int typesCount)
    {
        var rounded = (int)Math.Round((instCount + typesCount) / 100d) * 100;
        return rounded;
    }

    /// <summary>
    /// Shows the warning that contains approximate number of elements. Allows to cancel the operation.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
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
}
