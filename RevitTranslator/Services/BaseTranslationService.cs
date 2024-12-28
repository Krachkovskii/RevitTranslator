using RevitTranslator.Contracts;

namespace RevitTranslator.Services;

public class BaseTranslationService : IService
{
    public List<ElementId> SelectedElements { get; set; } = [];
    public void Execute()
    {
        
    }
}