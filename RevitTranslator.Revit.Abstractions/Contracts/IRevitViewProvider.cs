using RevitTranslator.Revit.Core.Models;

namespace RevitTranslator.Revit.Core.Contracts;

public interface IRevitViewProvider
{
    Task<IReadOnlyCollection<ViewDto>> GetAllIterableViewsAsync();
    Task<IReadOnlyCollection<ViewDto>> GetAllSheetsAsync();
    Task<IReadOnlyCollection<ViewGroupDto>> GetAllSheetCollectionsAsync();
}
