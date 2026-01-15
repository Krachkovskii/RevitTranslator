using RevitTranslator.Common.Models.Views;

namespace RevitTranslator.Common.Contracts;

public interface IRevitViewProvider
{
    IReadOnlyCollection<ViewDto> GetAllIterableViews();
    IReadOnlyCollection<ViewDto> GetAllSheets();
    IReadOnlyCollection<ViewGroupDto> GetAllSheetCollections();
}