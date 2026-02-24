namespace RevitTranslator.Utils;

/// <summary>
/// Retrieve and filter all user-visible and modifiable categories
/// </summary>
public static class CategoryManager
{
    private static readonly BuiltInCategory[] SpecialCategories =
    [
        BuiltInCategory.OST_Materials
    ];

    /// <summary>
    /// Valid Revit categories to be used with this plug-in.
    /// </summary>
    public static Category[] ValidCategories { get; } = GetValidCategories();

    private static Category[] GetValidCategories()
    {
        return Context.ActiveDocument!.Settings.Categories
            .Cast<Category>()
            .Where(category => IsRegularValidCategory(category) || IsSpecialCategory(category))
            .ToArray();
    }

    private static bool IsRegularValidCategory(Category category)
    {
        return category.BuiltInCategory != BuiltInCategory.INVALID
               && category is { IsVisibleInUI: true, Parent: null }
               && category.CategoryType != CategoryType.Invalid;
    }

    private static bool IsSpecialCategory(Category category)
    {
        return SpecialCategories.Contains(category.BuiltInCategory);
    }
}
