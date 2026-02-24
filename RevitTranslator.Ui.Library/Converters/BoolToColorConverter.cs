using System.Windows.Data;

namespace RevitTranslator.Ui.Library.Converters;

public class BoolToColorConverter : IMultiValueConverter
{
    // 'values' is an array of the bindings from XAML
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // Check for unbound values
        if (values.Length != 2
            || values[0] is not FrameworkElement source
            || values[1] is not bool value)
        {
            return Brushes.Gray;
        }

        try
        {
            var key = value ? "PaletteGreenBrush" : "PaletteRedBrush";
            return source.TryFindResource(key) as Brush;
        }
        catch (Exception)
        {
            return Brushes.Gray;
        }
    }

    // Usually not needed for one-way bindings
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}