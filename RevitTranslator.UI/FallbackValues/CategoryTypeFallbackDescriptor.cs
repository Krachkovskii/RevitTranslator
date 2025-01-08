using RevitTranslator.Common.App.Models;

namespace RevitTranslator.UI.FallbackValues;

public static class CategoryTypeFallbackDescriptor
{
    public static ObservableCategoryType[] FilteredCategoryTypes { get; } = new ObservableCategoryType[6];

    static CategoryTypeFallbackDescriptor()
    {
        for (var i = 0; i < 6; i++)
        {
            FilteredCategoryTypes[i] = new ObservableCategoryType
            {
                IsChecked = i % 2 == 0,
                Name = $"Category Type {i + 1}",
                FilteredCategories = CreateCategories()
            };
        }
    }

    private static List<ObservableCategoryDescriptor> CreateCategories()
    {
        var categories = new List<ObservableCategoryDescriptor>();
        for (var i = 0; i < 6; i++)
        {
            var category = new ObservableCategoryDescriptor
            {
                IsChecked = i % 2 == 0,
                Name = $"Category {i + 1}"
            };
            categories.Add(category);
        }
        return categories;
    }
}