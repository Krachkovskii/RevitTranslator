namespace RevitTranslator.Ui.Library.Converters;

public static class ConverterManager
{
    public static BoolToVisibilityConverter BoolToVisibilityConverter = new();
    public static BoolToHiddenVisibilityConverter BoolToHiddenVisibilityConverter = new();
    public static BoolToColorConverter BoolToColorConverter = new();
    public static InverseBoolToVisibilityConverter InverseBoolToVisibilityConverter = new();
}