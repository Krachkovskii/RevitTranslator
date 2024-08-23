using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Utils.ElementTextRetrievers;
internal class DimensionTextRetriever : BaseElementTextRetriever
{
    /// <summary>
    /// Dimension associated with this retriever.
    /// </summary>
    private readonly Dimension _dimension;
    internal DimensionTextRetriever(Dimension dimension)
    {
        _dimension = dimension;
        Process(dimension);
    }

    /// <summary>
    /// Extracts text from a particular override of a Dimension or DimensionSegment
    /// </summary>
    /// <param name="d">Dimension or DimensionSegment</param>
    /// <param name="details">Type of override (e.g. Above)</param>
    /// <param name="isSingleSegment">True if dimension has only one segment</param>
    /// <returns></returns>
    internal string ExtractDimensionText(object d, TranslationDetails details, bool isSingleSegment)
    {
        string value;
        object DimensionObject;

        if (isSingleSegment)
        {
            DimensionObject = d as Dimension;

            if (DimensionObject == null)
            {
                return string.Empty;
            }

            switch (details)
            {
                case TranslationDetails.DimensionAbove:
                    value = ((Dimension)DimensionObject).Above;
                    break;
                case TranslationDetails.DimensionBelow:
                    value = ((Dimension)DimensionObject).Below;
                    break;
                case TranslationDetails.DimensionPrefix:
                    value = ((Dimension)DimensionObject).Prefix;
                    break;
                case TranslationDetails.DimensionSuffix:
                    value = ((Dimension)DimensionObject).Suffix;
                    break;
                case TranslationDetails.DimensionOverride:
                    value = ((Dimension)DimensionObject).ValueOverride;
                    break;
                default:
                    value = string.Empty;
                    break;
            }
        }

        else
        {
            DimensionObject = d as DimensionSegment;

            if (DimensionObject == null)
            {
                return string.Empty;
            }

            switch (details)
            {
                case TranslationDetails.DimensionAbove:
                    value = ((DimensionSegment)DimensionObject).Above;
                    break;
                case TranslationDetails.DimensionBelow:
                    value = ((DimensionSegment)DimensionObject).Below;
                    break;
                case TranslationDetails.DimensionPrefix:
                    value = ((DimensionSegment)DimensionObject).Prefix;
                    break;
                case TranslationDetails.DimensionSuffix:
                    value = ((DimensionSegment)DimensionObject).Suffix;
                    break;
                case TranslationDetails.DimensionOverride:
                    value = ((DimensionSegment)DimensionObject).ValueOverride;
                    break;
                default:
                    value = string.Empty;
                    break;
            }
        }

        return value;
    }

    internal void ProcessDimensionSegment(DimensionSegment segment)
    {
        ProcessDimensionOverrideAbove(segment, false);
        ProcessDimensionOverrideBelow(segment, false);
        ProcessDimensionOverridePrefix(segment, false);
        ProcessDimensionOverrideSuffix(segment, false);
        ProcessDimensionOverrideValue(segment, false);
    }

    protected override void AddUnitToList(TranslationUnit unit)
    {
        if (unit.Element is DimensionSegment)
        {
            unit.ParentElement = _dimension;
        }

        base.AddUnitToList(unit);
    }

    /// <summary>
    /// Extracts all overrides from Dimension element
    /// </summary>
    /// <param name="dim"></param>
    protected override void Process(object Object)
    {
        if (Object is not Dimension dim)
        {
            return;
        }

        var single = dim.HasOneSegment();

        if (single)
        {
            ProcessSingleSegmentDimension(dim);
        }
        else
        {
            ProcessMultiSegmentDimension(dim);
        }
    }
    /// <summary>
    /// Process Above override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
    private void ProcessDimensionOverrideAbove(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionAbove, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionAbove);
        AddUnitToList(dimensionUnit);
    }

    /// <summary>
    /// Process Below override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
    private void ProcessDimensionOverrideBelow(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionBelow, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionBelow);
        AddUnitToList(dimensionUnit);
    }

    /// <summary>
    /// Process Prefix override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
    private void ProcessDimensionOverridePrefix(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionPrefix, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionPrefix);
        AddUnitToList(dimensionUnit);
    }

    /// <summary>
    /// Process Suffix override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
    private void ProcessDimensionOverrideSuffix(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionSuffix, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionSuffix);
        AddUnitToList(dimensionUnit);
    }

    /// <summary>
    /// Process Value override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
    private void ProcessDimensionOverrideValue(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionOverride, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionOverride);
        AddUnitToList(dimensionUnit);
    }

    /// <summary>
    /// Extracts text from Dimension element with multiple Dimension Segments
    /// </summary>
    /// <param name="dim"></param>
    private void ProcessMultiSegmentDimension(Dimension dim)
    {
        var segments = dim.Segments;

        foreach (var segment in segments)
        {
            if (segment is not DimensionSegment seg)
            {
                continue;
            }
            ProcessDimensionSegment(seg);
        }
    }

    /// <summary>
    /// Extracts text from Dimension element with a single Dimension Segment
    /// </summary>
    /// <param name="dim"></param>
    private void ProcessSingleSegmentDimension(Dimension dim)
    {
        ProcessDimensionOverrideAbove(dim, true);
        ProcessDimensionOverrideBelow(dim, true);
        ProcessDimensionOverridePrefix(dim, true);
        ProcessDimensionOverrideSuffix(dim, true);
        ProcessDimensionOverrideValue(dim, true);
    }
}
