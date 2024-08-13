namespace RevitTranslatorAddin.Utils.Revit;

/// <summary>
/// Represents translation attribute for elements that have multiple text properties
/// </summary>
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
