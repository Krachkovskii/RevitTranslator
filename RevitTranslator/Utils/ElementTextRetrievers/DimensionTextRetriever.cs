using RevitTranslator.Enums;
using RevitTranslator.Models;
using RevitTranslator.Utils.App;

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

        var hasOneSegment = dim.HasOneSegment();
        if (hasOneSegment)
        {
            ProcessDimension(dim);
        }
        else
        {
            ProcessMultiSegmentDimension();
        }
    }

    private void ProcessMultiSegmentDimension()
    {
        var segments = _dimension.Segments;
        foreach (var segment in segments)
        {
            if (segment is not DimensionSegment seg) continue;
            
            ProcessDimension(seg);
        }
    }

    private void ProcessDimension(object segment)
    {
        ProcessDimensionOverride(segment, false, TranslationDetails.DimensionAbove);
        ProcessDimensionOverride(segment, false, TranslationDetails.DimensionBelow);
        ProcessDimensionOverride(segment, false, TranslationDetails.DimensionPrefix);
        ProcessDimensionOverride(segment, false, TranslationDetails.DimensionSuffix);
        ProcessDimensionOverride(segment, false, TranslationDetails.DimensionOverride);
    }

    private void ProcessDimensionOverride(object dimensionObject, bool isSingleSegment, TranslationDetails details)
    {
        var value = ExtractDimensionText(dimensionObject, details, isSingleSegment);
        if (!ValidationUtils.HasText(value)) return;

        var unit = new TranslationEntity();

        if (isSingleSegment)
        {
            unit.Element = _dimension;
        }
        else
        {
            var dim = (DimensionSegment)dimensionObject;
            unit.Element = dim;
            unit.ParentElement = _dimension;
        }

        unit.ElementId = _dimension.Id;
        unit.Document = _dimension.Document;
        unit.OriginalText = value;
        unit.TranslationDetails = details;

        AddUnitToList(unit);
    }

    private string ExtractDimensionText(object d, TranslationDetails details, bool isSingleSegment)
    {
        string value;
        object dimensionObject;

        if (isSingleSegment)
        {
            dimensionObject = (Dimension)d;

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
