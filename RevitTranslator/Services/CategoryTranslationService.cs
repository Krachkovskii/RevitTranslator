using RevitTranslator.Common.App.Models;
using RevitTranslator.Contracts;

namespace RevitTranslator.Services;

public class CategoryTranslationService : IService
{
    public List<ObservableCategoryDescriptor> SelectedCategories { get; set; } = [];
    
    public void Execute()
    {
        var elements = new FilteredElementCollector(Context.ActiveDocument)
            .WherePasses(
                new ElementMulticategoryFilter(SelectedCategories
                    .Select(category => new ElementId(category.Id))
                    .ToArray()))
            .ToArray();

        var service = new BaseTranslationService();
        service.SelectedElements = elements;
        
        service.Execute();
    }
}