namespace RevitTranslator.Revit.Core.Models;

public record ViewDto(long Id, ViewTypeInternal ViewType, string Name, int ElementCount);
