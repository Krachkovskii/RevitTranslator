
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace RevitTranslator.UI.Views.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return DependencyProperty.UnsetValue;
        if (value is not Enum enumValue) return value.ToString();

        var name = enumValue.ToString();
        var type = enumValue.GetType();

        var memberInfo = type.GetMember(name).FirstOrDefault();
        if (memberInfo == null) return name;

        var descriptionAttr = memberInfo.GetCustomAttribute<DescriptionAttribute>(false);
        return descriptionAttr != null ? descriptionAttr.Description : name;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException($"{nameof(EnumDescriptionConverter)} cannot convert back.");
    }
}