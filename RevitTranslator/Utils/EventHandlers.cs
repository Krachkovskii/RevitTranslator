using Nice3point.Revit.Toolkit.External.Handlers;

namespace RevitTranslator.Utils;

public sealed class EventHandlers
{
    public ActionEventHandler ActionHandler { get; set; } = new();
    public AsyncEventHandler AsyncHandler { get; set; } = new();
}