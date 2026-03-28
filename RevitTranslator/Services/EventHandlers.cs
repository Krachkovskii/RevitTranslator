using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitTranslator.Revit.Abstractions.Contracts;

namespace RevitTranslator.Services;

public sealed class EventHandlers : IRevitHandler
{
    private readonly AsyncEventHandler _asyncHandler = new();

    public Task RaiseAsync(Action handler) => _asyncHandler.RaiseAsync(_ => handler());
}
