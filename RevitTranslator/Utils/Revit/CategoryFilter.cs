namespace RevitTranslator.Utils.Revit;
/// <summary>
/// Retrieve and filter all user-visible and modifiable categories
/// </summary>
public class CategoryFilter
{
    /// <summary>
    /// Special categories that still need to be used even if they don't pass main filters.
    /// </summary>
    private static readonly HashSet<BuiltInCategory> SpecialCategories =
    [
        BuiltInCategory.OST_Materials
    ];

    /// <summary>
    /// Gets all valid categories that can be used to extract user-modifiable elements
    /// </summary>
    /// <param name="allCategories"></param>
    /// <returns></returns>
    public static List<Category> GetValidCategories(Categories allCategories)
    {
        var validCategories = new List<Category>();

        foreach (Category category in allCategories)
        {
            if (IsValidCategory(category))
            {
                validCategories.Add(category);
            }
        }

        return validCategories;
    }

    /// <summary>
    /// Applies filters to see if category is valid for use
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    private static bool IsRegularValidCategory(Category category)
    {
        return category.BuiltInCategory != BuiltInCategory.INVALID
            && category.IsVisibleInUI
            && category.Parent == null
            && category.CategoryType != CategoryType.Invalid;
    }

    /// <summary>
    /// Determines if category is listed in Special categories hashset
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    private static bool IsSpecialCategory(Category category)
    {
        return SpecialCategories.Contains(category.BuiltInCategory);
    }

    /// <summary>
    /// Checks if a category is valid or special category
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    private static bool IsValidCategory(Category category)
    {
        return IsRegularValidCategory(category) || IsSpecialCategory(category);
    }
}
