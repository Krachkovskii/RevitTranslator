using RevitTranslator.Utils.App;

namespace RevitTranslator.Utils.ElementTextRetrievers;
public class BaseParameterTextRetriever : BaseElementTextRetriever
{
    protected static bool CanUseParameter(Parameter param)
    {
        return param.StorageType == StorageType.String
               && !param.IsReadOnly
               && param.HasValue
               && !param.Definition.Name.Contains("GUID")
               && !param.Definition.Name.Equals("Text Font");
    }

    protected override string GetText(object Object)
    {
        if (Object is not Parameter param) return string.Empty;

        var pv = param.AsString();
        return !ValidationUtils.HasText(pv) ? string.Empty : pv;
    }
}
