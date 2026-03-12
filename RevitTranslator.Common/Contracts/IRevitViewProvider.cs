using RevitTranslator.Common.Models.Views;

namespace RevitTranslator.Common.Contracts;

public interface IRevitViewProvider
{
    Task<IReadOnlyCollection<ViewDto>> GetAllIterableViewsAsync();
    Task<IReadOnlyCollection<ViewDto>> GetAllSheetsAsync();
    Task<IReadOnlyCollection<ViewGroupDto>> GetAllSheetCollectionsAsync();
}