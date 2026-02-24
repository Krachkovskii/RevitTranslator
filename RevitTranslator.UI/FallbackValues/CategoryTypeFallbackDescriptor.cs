using System.Collections.ObjectModel;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Models;
using RevitTranslator.Common.Models.Categories;

namespace RevitTranslator.UI.FallbackValues;

public static class CategoryTypeFallbackDescriptor
{
    public static ObservableCollection<CategoryTypeViewModel> FilteredCategoryTypes { get; } = new CategoryTypeViewModel[6].ToObservableCollection();

    static CategoryTypeFallbackDescriptor()
    {
        for (var i = 0; i < 6; i++)
        {
            FilteredCategoryTypes[i] = new CategoryTypeViewModel
            {
                IsChecked = i % 2 == 0,
                Name = $"Category Type {i + 1}",
                Categories = CreateCategories()
            };
        }
    }

    private static ObservableCollection<CategoryViewModel> CreateCategories()
    {
        var categories = new ObservableCollection<CategoryViewModel>();
        for (var i = 0; i < 6; i++)
        {
            var category = new CategoryViewModel
            {
                IsChecked = i % 2 == 0,
                Name = $"Category {i + 1}"
            };
            categories.Add(category);
        }
        return categories;
    }
}