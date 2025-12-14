using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Services;
using RevitTranslator.Extensions;
using RevitTranslator.UI.Contracts;
using RevitTranslator.UI.Views;

namespace RevitTranslator.Services;

public class CategorySelectionService(ICategoriesWindowViewModel viewModel, CategoriesWindow window)
{
    public Element[] SelectedElements { get; private set; } = [];

    public bool Initialize()
    {
        var result = window.ShowDialog();
        
        SelectedElements = new FilteredElementCollector(Context.ActiveDocument)
            .WherePasses(
                new ElementMulticategoryFilter(viewModel.CategoryTypes
                    .SelectMany(type => type.Categories)
                    .Where(category => category.IsChecked)
                    .Select(category => category.Id.ToElementId())
                    .ToArray()))
            .ToArray();

        return result is true && SelectedElements.Length > 0;
    }
}