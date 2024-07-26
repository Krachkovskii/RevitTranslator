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
    /// This method retrieves all the user-visible element cateogries from Revit.
    /// </summary>
    /// <returns>A list of valid categories, sorted by category type and then by catgory name.</returns>
    internal static List<Category> GetCategories()
    {
        var validCategories = new List<Category>();
        var allCategories = RevitUtils.Doc.Settings.Categories;

        foreach (Category c in allCategories)
        {
            if (c.BuiltInCategory != BuiltInCategory.INVALID
                && c.IsVisibleInUI
                && c.Parent == null
                && c.CategoryType != CategoryType.Invalid){
            validCategories.Add(c);
            }
        }

        var categories = validCategories.OrderBy(c => c.CategoryType.ToString())
            .ThenBy(c => c.Name).ToList();
        return categories;
    }
}
