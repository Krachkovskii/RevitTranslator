using RevitTranslator.Models;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.Utils.ElementTextRetrievers;
public class DimensionTextRetriever : BaseElementTextRetriever
{
    private readonly Dimension _dimension;
    
    public DimensionTextRetriever(Dimension dimension)
    {
        _dimension = dimension;
        Process(dimension);
    }

    protected override sealed void Process(object Object)
    {
        if (Object is not Dimension dim) return;

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

    private void ProcessSingleSegmentDimension(Dimension dim)
    {
        ProcessDimensionOverrideAbove(dim, true);
        ProcessDimensionOverrideBelow(dim, true);
        ProcessDimensionOverridePrefix(dim, true);
        ProcessDimensionOverrideSuffix(dim, true);
        ProcessDimensionOverrideValue(dim, true);
    }

    private void ProcessDimensionSegment(DimensionSegment segment)
    {
        ProcessDimensionOverrideAbove(segment, false);
        ProcessDimensionOverrideBelow(segment, false);
        ProcessDimensionOverridePrefix(segment, false);
        ProcessDimensionOverrideSuffix(segment, false);
        ProcessDimensionOverrideValue(segment, false);
    }

    private void ProcessDimensionOverrideAbove(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionAbove, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationEntity(dim, value, TranslationDetails.DimensionAbove);
        AddUnitToList(dimensionUnit);
    }

    private void ProcessDimensionOverrideBelow(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionBelow, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationEntity(dim, value, TranslationDetails.DimensionBelow);
        AddUnitToList(dimensionUnit);
    }

    private void ProcessDimensionOverridePrefix(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionPrefix, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationEntity(dim, value, TranslationDetails.DimensionPrefix);
        AddUnitToList(dimensionUnit);
    }

    private void ProcessDimensionOverrideSuffix(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionSuffix, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationEntity(dim, value, TranslationDetails.DimensionSuffix);
        AddUnitToList(dimensionUnit);
    }
    
    private void ProcessDimensionOverrideValue(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionOverride, isSingleSegment);

        if (!ValidationUtils.HasText(value))
        {
            return;
        }

        var dimensionUnit = new TranslationEntity(dim, value, TranslationDetails.DimensionOverride);
        AddUnitToList(dimensionUnit);
    }

    private string ExtractDimensionText(object d, TranslationDetails details, bool isSingleSegment)
    {
        string value;
        object dimensionObject;

        if (isSingleSegment)
        {
            dimensionObject = (Dimension)d;
            if (dimensionObject is null) return string.Empty;

            value = details switch
            {
                TranslationDetails.DimensionAbove => ((Dimension)dimensionObject).Above,
                TranslationDetails.DimensionBelow => ((Dimension)dimensionObject).Below,
                TranslationDetails.DimensionPrefix => ((Dimension)dimensionObject).Prefix,
                TranslationDetails.DimensionSuffix => ((Dimension)dimensionObject).Suffix,
                TranslationDetails.DimensionOverride => ((Dimension)dimensionObject).ValueOverride,
                _ => string.Empty
            };
        }

        else
        {
            dimensionObject = (DimensionSegment)d;
            if (dimensionObject is null) return string.Empty;

            value = details switch
            {
                TranslationDetails.DimensionAbove => ((DimensionSegment)dimensionObject).Above,
                TranslationDetails.DimensionBelow => ((DimensionSegment)dimensionObject).Below,
                TranslationDetails.DimensionPrefix => ((DimensionSegment)dimensionObject).Prefix,
                TranslationDetails.DimensionSuffix => ((DimensionSegment)dimensionObject).Suffix,
                TranslationDetails.DimensionOverride => ((DimensionSegment)dimensionObject).ValueOverride,
                _ => string.Empty
            };
        }

        return value;
    }

    protected override void AddUnitToList(TranslationEntity entity)
    {
        if (entity.Element is DimensionSegment)
        {
            entity.ParentElement = _dimension;
        }

        base.AddUnitToList(entity);
    }
}
