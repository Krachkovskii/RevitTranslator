using RevitTranslator.Revit.Abstractions.Contracts;

namespace RevitTranslator.Services;

internal class RevitContextProvider : IRevitContextProvider
{
    public Document ActiveDocument => Context.ActiveDocument!;
}
