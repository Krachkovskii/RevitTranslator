using RevitTranslator.Common.Models.Views;

namespace RevitTranslator.Common.Contracts;

public interface IRevitViewProvider
{
    IReadOnlyCollection<ViewDto> GetAllIterableViews();
    IReadOnlyCollection<ViewDto> GetAllSheets();
#if NET8_0_OR_GREATER
    public IReadOnlyCollection<ViewGroupDto> GetAllSheetCollections()
#endif
}