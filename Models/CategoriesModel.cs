using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Models;
public class CategoriesModel
{
    public static List<Category> AllCategories = [];
    public CategoriesModel()
    {
        AllCategories = GetCategories();
    }

    /// <summary>
    /// This method retrieves all the user-visible element categories from Revit.
    /// </summary>
    /// <returns>A list of valid categories, sorted by category type and then by category name.</returns>
    internal static List<Category> GetCategories()
    {
        var validCategories = CategoryFilter.GetValidCategories(RevitUtils.Doc.Settings.Categories);

        var categories = validCategories.OrderBy(c => c.CategoryType.ToString())
            .ThenBy(c => c.Name).ToList();
        return categories;
    }
}
