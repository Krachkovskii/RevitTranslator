namespace RevitTranslator.Revit.Core.Models;

public class ViewGroupDto
{
    public required IReadOnlyCollection<ViewDto> Views { get; init; }
    public required string Name { get; init; }
}
