namespace RevitTranslator.Common.Models.Views;

public class ViewGroupDto
{
    public required IReadOnlyCollection<ViewDto> Views { get; init; }
    public required string Name { get; init; }
}