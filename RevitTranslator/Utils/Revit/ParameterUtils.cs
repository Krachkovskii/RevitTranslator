using RevitTranslator.Utils.App;

namespace RevitTranslator.Utils.Revit;

public static class ParameterUtils
{
    public static bool CanUseParameter(Parameter param)
    {
        return param is { StorageType: StorageType.String, HasValue: true, IsReadOnly: false }
               && !param.Definition.Name.Contains("GUID")
               && !param.Definition.Name.Equals("Text Font");
    }
    
    public static string GetText(this Parameter parameter)
    {
        var value = parameter.AsString();
        return !value.HasText() ? string.Empty : value;
    }
}