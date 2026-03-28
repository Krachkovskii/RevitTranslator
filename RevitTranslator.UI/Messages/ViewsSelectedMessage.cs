using RevitTranslator.Abstractions.Models;
using RevitTranslator.Abstractions.Models.Views;

namespace RevitTranslator.UI.Messages;

public record ViewsSelectedMessage(IReadOnlyCollection<ViewDto> SelectedViews);
