using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.Revit;
internal class CategoryFilter
{
    private static readonly HashSet<BuiltInCategory> SpecialCategories = new HashSet<BuiltInCategory>
    {
        BuiltInCategory.OST_Materials
    };

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

    private static bool IsValidCategory(Category category)
    {
        return IsRegularValidCategory(category) || IsSpecialCategory(category);
    }

    private static bool IsRegularValidCategory(Category category)
    {
        return category.BuiltInCategory != BuiltInCategory.INVALID
            && category.IsVisibleInUI
            && category.Parent == null
            && category.CategoryType != CategoryType.Invalid
            && !category.Name.Contains("Tag", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSpecialCategory(Category category)
    {
        return SpecialCategories.Contains(category.BuiltInCategory);
    }
}
