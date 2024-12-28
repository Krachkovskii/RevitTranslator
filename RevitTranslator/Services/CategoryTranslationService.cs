using RevitTranslator.Common.App.Models;
using RevitTranslator.Contracts;

namespace RevitTranslator.Services;

public class CategoryTranslationService : IService
{
    public List<ObservableCategoryDescriptor> SelectedCategories { get; set; } = [];
    public void Execute()
    {
        
    }
}