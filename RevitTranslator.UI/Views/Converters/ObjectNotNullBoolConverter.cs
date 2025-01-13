using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace RevitTranslator.UI.Views.Converters;

public sealed class ObjectNotNullBoolConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}