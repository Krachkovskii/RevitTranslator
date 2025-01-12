using RevitTranslator.Enums;
using RevitTranslator.Models;
using RevitTranslator.Utils.App;

namespace RevitTranslator.ElementTextRetrievers;

public class DimensionTextRetriever : BaseElementTextRetriever
{
    private readonly Dimension _dimension;
    private readonly bool _isSingleSegment;
    
    public DimensionTextRetriever(Dimension dimension)
    {
        _dimension = dimension;
        _isSingleSegment = _dimension.HasOneSegment();
        Process(dimension);
    }

    protected override sealed void Process(object Object)
    {
        if (Object is not Dimension dim) return;

        if (_isSingleSegment)
        {
            ProcessDimension(dim);
        }
        else
        {
            foreach (var segment in _dimension.Segments.OfType<DimensionSegment>())
            {
                ProcessDimension(segment);
            }
        }
    }

    private void ProcessDimension(object dimensionObject)
    {
        ProcessDimensionOverride(dimensionObject, TranslationDetails.DimensionAbove);
        ProcessDimensionOverride(dimensionObject, TranslationDetails.DimensionBelow);
        ProcessDimensionOverride(dimensionObject, TranslationDetails.DimensionPrefix);
        ProcessDimensionOverride(dimensionObject, TranslationDetails.DimensionSuffix);
        ProcessDimensionOverride(dimensionObject, TranslationDetails.DimensionOverride);
    }

    private void ProcessDimensionOverride(object dimensionObject, TranslationDetails details)
    {
        var value = ExtractDimensionText(dimensionObject, details);
        if (!ValidationUtils.HasText(value)) return;

        var unit = new TranslationEntity();

        if (_isSingleSegment)
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

    private string ExtractDimensionText(object dimension, TranslationDetails details)
    {
        string value;
        object dimensionObject;

        if (_isSingleSegment)
        {
            dimensionObject = (Dimension)dimension;

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
            dimensionObject = (DimensionSegment)dimension;

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
