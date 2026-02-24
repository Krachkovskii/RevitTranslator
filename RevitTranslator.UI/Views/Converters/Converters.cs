namespace RevitTranslator.UI.Views.Converters;

public static class Converters
{
    public static EnumDescriptionConverter EnumDescriptionConverter { get; } = new();
    public static CollapsedBoolVisibilityConverter CollapsedBoolVisibilityConverter { get; } = new();
    public static BoolVisibilityConverter BoolVisibilityConverter { get; } = new();
    public static InverseBoolConverter InverseBoolConverter { get; } = new();
    public static InverseBoolVisibilityConverter InverseBoolVisibilityConverter { get; } = new();
    public static ObjectNotNullBoolConverter ObjectNotNullBoolConverter { get; } = new();
    public static StringToVisibilityConverter StringToVisibilityConverter { get; } = new();
}