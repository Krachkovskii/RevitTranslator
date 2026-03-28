using Autodesk.Revit.DB;

namespace RevitTranslator.Revit.Abstractions.Contracts;

public interface IRevitContextProvider
{
    Document ActiveDocument { get; }
}