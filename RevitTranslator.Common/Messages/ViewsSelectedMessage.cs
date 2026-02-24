using RevitTranslator.Common.Models.Views;

namespace RevitTranslator.Common.Messages;

public record ViewsSelectedMessage(IReadOnlyCollection<ViewDto> SelectedViews);
