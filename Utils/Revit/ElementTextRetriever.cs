using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Models;
using Newtonsoft.Json.Linq;
using RevitTranslatorAddin.ViewModels;
using RevitTranslatorAddin.Utils.App;
using Autodesk.Revit.DB;
using System.Xml.Linq;

namespace RevitTranslatorAddin.Utils.Revit;
internal class ElementTextRetriever
{
    internal List<TranslationUnit> TranslationUnits { get; } = [];
    private HashSet<Element> _elementTypes { get; } = [];
    private readonly ProgressWindowUtils _progressWindowUtils = null;

    internal ElementTextRetriever(ProgressWindowUtils windowUtils)
    {
        _progressWindowUtils = windowUtils;
    }

    internal void ProcessElements(List<Element> elements)
    {
        foreach (Element element in elements) 
        {
            ProcessElement(element);
        }

        ProcessElementTypes();
    }

    private void ProcessElement(object element)
    {
        switch (element)
        {
            case TextNote note:
                var noteUnit = GetTextFromTextBlock(note);
                AddTranslationUnitToList(noteUnit);
                break;

            case ElementType elementType:
                _elementTypes.Add(elementType);
                break;

            case ScheduleSheetInstance scheduleInstance:
                ProcessSchedule(RevitUtils.Doc.GetElement(scheduleInstance.ScheduleId) as ViewSchedule);
                break;

            case ViewSchedule schedule:
                ProcessSchedule(schedule);
                break;

            case Dimension dim:
                ProcessDimensionOverrides(dim);
                break;

            case Element el:
                ProcessElementParameters(el);
                break;

            case object _:
                return;
        }
    }

    private TranslationUnit GetTextFromTextBlock(TextNote note)
    {
        var text = note.Text;
        return new TranslationUnit(note, text);
    }

    private void ProcessSchedule(ViewSchedule schedule)
    {
        ProcessScheduleHeaders(schedule);
    }

    private void ProcessDimensionOverrides(Dimension dim)
    {
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

    private void ProcessSingleSegmentDimension(Dimension dim)
    {
        ProcessDimensionAboveOverride(dim, true);
        ProcessDimensionBelowOverride(dim, true);
        ProcessDimensionPrefixOverride(dim, true);
        ProcessDimensionSuffixOverride(dim, true);
        ProcessDimensionValueOverride(dim, true);
    }

    private void ProcessMultiSegmentDimension(Dimension dim)
    {
        var segments = dim.Segments;
        
        foreach(var segment in segments)
        {
            ProcessDimensionAboveOverride(segment, false);
            ProcessDimensionBelowOverride(segment, false);
            ProcessDimensionPrefixOverride(segment, false);
            ProcessDimensionSuffixOverride(segment, false);
            ProcessDimensionValueOverride(segment, false);
        }
    }

    private string ExtractDimensionText(object d, TranslationDetails details, bool isSingleSegment)
    {
        string value;
        object DimensionObject;

        if (isSingleSegment)
        {
            DimensionObject = d as Dimension;
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
            return value;
        }

        else
        {
            DimensionObject = d as DimensionSegment;
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
            return value;
        }
    }

    private void ProcessDimensionAboveOverride(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionAbove, isSingleSegment);

        if (!ValidationUtils.IsTextOnly(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionAbove);
        AddTranslationUnitToList(dimensionUnit);
    }

    private void ProcessDimensionBelowOverride(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionBelow, isSingleSegment);

        if (!ValidationUtils.IsTextOnly(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionBelow);
        AddTranslationUnitToList(dimensionUnit);
    }

    private void ProcessDimensionPrefixOverride(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionPrefix, isSingleSegment);

        if (!ValidationUtils.IsTextOnly(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionPrefix);
        AddTranslationUnitToList(dimensionUnit);
    }

    private void ProcessDimensionSuffixOverride(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionPrefix, isSingleSegment);

        if (!ValidationUtils.IsTextOnly(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionSuffix);
        AddTranslationUnitToList(dimensionUnit);
    }

    private void ProcessDimensionValueOverride(object dim, bool isSingleSegment)
    {
        var value = ExtractDimensionText(dim, TranslationDetails.DimensionOverride, isSingleSegment);

        if (!ValidationUtils.IsTextOnly(value))
        {
            return;
        }

        var dimensionUnit = new TranslationUnit(dim, value, TranslationDetails.DimensionOverride);
        AddTranslationUnitToList(dimensionUnit); 
    }

    private void ProcessElementParameters(Element el)
    {
        var parameters = GetElementParameters(el);

        foreach (var parameter in parameters)
        {
            ProcessParameter(parameter);
        }

        ProcessElementName(el);
    }

    /// <summary>
    /// Get list of modifiable parameters for a given Element.
    /// </summary>
    /// <param name="element">
    /// Element to get parameters from.
    /// </param>
    /// <returns>
    /// List of Parameters.
    /// </returns>
    private List<Parameter> GetElementParameters(Element element)
    {
        var parameters = element.Parameters
            .Cast<Parameter>()
            .Where(p => p.StorageType == StorageType.String
                        && !p.IsReadOnly
                        && p.UserModifiable
                        && p.HasValue)
            .ToList();

        return parameters;
    }

    /// <summary>
    /// Translates contents of a single text parameter.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    private void ProcessParameter(Parameter param)
    {
        var value = param.AsString();

        if (!ValidationUtils.IsTextOnly(value))
        {
            return;
        }

        var unit = new TranslationUnit(param, value);
        unit.ParentElement = param.Element;

        AddTranslationUnitToList(unit);
    }


    private void ProcessElementName(Element el)
    {
        var name = el.Name;
        
        if (!ValidationUtils.IsTextOnly(name))
        {
            return;
        }

        var unit = new TranslationUnit(el, name, TranslationDetails.ElementName);
        AddTranslationUnitToList(unit);
    }

    private void ProcessElementTypes()
    {
        foreach (var type in _elementTypes)
        {
            ProcessElementParameters(type);
        }
    }

    private void ProcessScheduleHeaders(ViewSchedule s)
    {
        var sd = s.Definition;
        var fieldCount = sd.GetFieldCount();
        for (var i = 0; i < fieldCount; i++)
        {
            var unit = ProcessScheduleHeader(sd, s, i);
            if (unit != null)
            {
                AddTranslationUnitToList(unit);
            }
        }
    }

    private TranslationUnit ProcessScheduleHeader(ScheduleDefinition sd, ViewSchedule s, int fieldIndex)
    {
        var field = sd.GetField(fieldIndex);
        var header = field.ColumnHeading;

        if (!ValidationUtils.IsTextOnly(header))
        {
            return null;
        }

        var unit = new TranslationUnit(field, header);
        unit.ParentElement = s;

        return unit;
    }

    private void AddTranslationUnitToList(TranslationUnit unit)
    {
        TranslationUnits.Add(unit);
    }
}
