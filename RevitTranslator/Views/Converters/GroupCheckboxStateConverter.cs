using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace RevitTranslatorAddin.Views.Converters;

public class GroupCheckboxStateConverter : MarkupExtension, IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || !(values[0] is bool) || values[1] == null)
            return null;

        bool isChecked = (bool)values[0];
        string name = values[1].ToString();

        return new Tuple<bool, string>(isChecked, name);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
    
}
