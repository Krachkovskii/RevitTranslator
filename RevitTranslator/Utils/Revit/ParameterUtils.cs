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
    
    public static string GetText(object Object)
    {
        if (Object is not Parameter param) return string.Empty;

        var value = param.AsString();
        return !ValidationUtils.HasText(value) ? string.Empty : value;
    }
}