using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.Revit;
public enum TranslationDetails
{
    None,
    ElementName,
    DimensionAbove,
    DimensionBelow,
    DimensionPrefix,
    DimensionSuffix,
    DimensionOverride,
}
