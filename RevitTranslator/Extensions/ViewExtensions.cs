using RevitTranslator.Common.Models.Views;

namespace RevitTranslator.Extensions;

public static class ViewExtensions
{
    public static ViewTypeInternal ToInternal(this ViewType viewType)
    {
        return viewType switch
        {
            ViewType.Elevation => ViewTypeInternal.Elevation,
            ViewType.Section => ViewTypeInternal.Section,
            ViewType.FloorPlan => ViewTypeInternal.FloorPlan,
            ViewType.CeilingPlan => ViewTypeInternal.CeilingPlan,
            ViewType.DrawingSheet => ViewTypeInternal.Sheet,
            ViewType.ThreeD => ViewTypeInternal.ThreeD,
            ViewType.Detail or ViewType.DraftingView => ViewTypeInternal.Legend,
            _ => ViewTypeInternal.Undefined
        };
    }
}