using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitTranslatorAddin.Utils;

namespace RevitTranslatorAddin.Models;
public class CategoriesModel
{
    public static List<Category> AllCategories = [];
    public CategoriesModel()
    {
        AllCategories = GetCategories();
    }
    internal static List<Category> GetCategories()
    {
        var validCategories = new List<Category>();
        var allCategories = RevitUtils.Doc.Settings.Categories;
        foreach (Category c in allCategories)
        {
            if (c.BuiltInCategory != BuiltInCategory.INVALID
                && c.IsVisibleInUI
                && c.Parent == null
                && (c.CategoryType != CategoryType.Invalid
                    || c.CategoryType != CategoryType.Internal)) {
            validCategories.Add(c);
            }
        }
        var categories = validCategories.OrderBy(c => c.CategoryType.ToString())
            .ThenBy(c => c.Name).ToList();
        return categories;
    }
}
