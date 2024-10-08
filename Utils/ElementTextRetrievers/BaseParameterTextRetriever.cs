﻿using System.Diagnostics;
using RevitTranslatorAddin.Utils.App;

namespace RevitTranslatorAddin.Utils.ElementTextRetrievers;
internal class BaseParameterTextRetriever : BaseElementTextRetriever
{
    /// <summary>
    /// Checks if parameter is valid for translation. Only string-based, user-modifiable 
    /// parameters that are not empty can be used.
    /// </summary>
    /// <param name="param">
    /// Parameter object to check
    /// </param>
    /// <returns>
    /// true if parameter is valid for translation, false otherwise
    /// </returns>
    internal static bool CanUseParameter(Parameter param)
    {
        if (param.StorageType == StorageType.String
            && !param.IsReadOnly
            && param.HasValue
            && !param.Definition.Name.Contains("GUID")
            && !param.Definition.Name.Equals("Text Font"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the text contents of the parameter value
    /// </summary>
    /// <param name="Object">
    /// Parameter object to process
    /// </param>
    /// <returns>
    /// Parameter value's text contents
    /// </returns>
    protected override string GetText(object Object)
    {
        if (Object is not Parameter param)
        {
            return string.Empty;
        }

        var pv = param.AsString();
        if (!ValidationUtils.HasText(pv))
        {
            return string.Empty;
        }

        return pv;
    }
}
