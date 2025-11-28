namespace RevitTranslator.Models;

/// <summary>
/// Represents grid coordinates (row and column) of a Schedule cell
/// </summary>
public sealed record ScheduleCellCoordinates (int Row, int Column);
