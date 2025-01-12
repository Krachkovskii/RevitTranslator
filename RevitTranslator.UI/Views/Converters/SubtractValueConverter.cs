using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace RevitTranslator.UI.Views.Converters;

public class SubtractValueConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue || parameter is not double doubleParameter) return null;
        return doubleValue - doubleParameter;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue || parameter is not double doubleParameter) return null;
        return doubleValue + doubleParameter;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}