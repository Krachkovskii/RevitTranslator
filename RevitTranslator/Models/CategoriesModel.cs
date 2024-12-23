using RevitTranslator.Utils.Revit;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Models;
/// <summary>
/// This class provides access to user-visible and user-modifiable categories
/// </summary>
public class CategoriesModel
{
    public List<Category> AllValidCategories { get; private set; } = [];
    public CategoriesModel()
    {
        AllValidCategories = GetCategories();
    }

    /// <summary>
    /// This method retrieves all the user-visible element categories from Revit.
    /// </summary>
    /// <returns>A list of valid categories, sorted by category type and then by category name.</returns>
    public List<Category> GetCategories()
    {
        var validCategories = CategoryFilter.GetValidCategories(RevitUtils.Doc.Settings.Categories);

        var categories = validCategories.OrderBy(c => c.CategoryType.ToString())
            .ThenBy(c => c.Name).ToList();
        return categories;
    }
}
