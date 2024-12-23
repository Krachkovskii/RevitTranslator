using System.Windows;
using RevitTranslatorAddin.Models;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateModelCommand : BaseTranslationCommand
{
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
        int types = CountElementTypes(filter);

        var elementCount = CountTotalElements(instances.Count, types);
        var warningResult = ShowElementCountWarning(elementCount);

        if (warningResult.Equals(MessageBoxResult.No))
        {
            return;
        }

        StartCommandTranslation(instances, _progressWindowUtils, _translationUtils, true, true);
    }

    /// <summary>
    /// Calculates the approximate number of elements that can be used for translation
    /// </summary>
    /// <param name="instCount"></param>
    /// <param name="typesCount"></param>
    /// <returns></returns>
    private static int CountTotalElements(int instCount, int typesCount)
    {
        var rounded = (int)Math.Round((instCount + typesCount) / 100d) * 100;
        return rounded;
    }

    /// <summary>
    /// Creates MulticategoryFilter to get all elements of valid categories
    /// </summary>
    /// <returns>
    /// The filter</returns>
    private static ElementMulticategoryFilter CreateCategoryFilter()
    {
        List<BuiltInCategory> categoryList = CreateCategoryList();
        var filter = new ElementMulticategoryFilter(categoryList);
        return filter;
    }

    /// <summary>
    /// Gets BuiltInCategory for all valid categories
    /// </summary>
    /// <returns></returns>
    private static List<BuiltInCategory> CreateCategoryList()
    {
        List<BuiltInCategory> categoryList = [];

        var categories = new CategoriesModel().GetCategories();
        foreach (Category category in categories)
        {
            categoryList.Add(category.BuiltInCategory);
        }

        return categoryList;
    }

    /// <summary>
    /// Shows the warning that contains approximate number of elements. Allows to cancel the operation.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private static bool ShowElementCountWarning(int count)
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
    /// Counts all Element Types of valid categories
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    private static int CountElementTypes(ElementMulticategoryFilter filter)
    {
        int types = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsElementType()
            .GetElementCount();

        return types;
    }

    /// <summary>
    /// Get all element instances of valid categories
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    private List<Element> GetModelInstances(ElementMulticategoryFilter filter)
    {
        var instances = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .ToList();

        return instances;
    }
}
