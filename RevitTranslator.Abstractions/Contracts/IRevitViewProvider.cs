using RevitTranslator.Abstractions.Models.Views;

namespace RevitTranslator.Abstractions.Contracts;

public interface IRevitViewProvider
{
    Task<IReadOnlyCollection<ViewDto>> GetAllIterableViewsAsync();
    Task<IReadOnlyCollection<ViewDto>> GetAllSheetsAsync();
    Task<IReadOnlyCollection<ViewGroupDto>> GetAllSheetCollectionsAsync();
}
