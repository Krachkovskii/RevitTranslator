using RevitTranslator.Revit.Core.Models;

namespace RevitTranslator.UI.Messages;

public record ViewsSelectedMessage(IReadOnlyCollection<ViewDto> SelectedViews);
