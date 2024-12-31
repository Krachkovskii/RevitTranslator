namespace RevitTranslator.Utils.Revit;
/// <summary>
/// Retrieve and filter all user-visible and modifiable categories
/// </summary>
public static class CategoryFilter
{
    private static readonly BuiltInCategory[] SpecialCategories =
    [
        BuiltInCategory.OST_Materials
    ];

    public static Category[] ValidCategories { get; } = GetValidCategories();

    private static Category[] GetValidCategories()
    {
        return Context.ActiveDocument!.Settings.Categories
            .Cast<Category>()
            .Where(IsValidCategory)
            .ToArray();
    }

    private static bool IsValidCategory(Category category)
    {
        return IsRegularValidCategory(category) || IsSpecialCategory(category);
    }

    private static bool IsRegularValidCategory(Category category)
    {
        return category.BuiltInCategory != BuiltInCategory.INVALID
            && category.IsVisibleInUI
            && category.Parent == null
            && category.CategoryType != CategoryType.Invalid;
    }

    private static bool IsSpecialCategory(Category category)
    {
        return SpecialCategories.Contains(category.BuiltInCategory);
    }
}
