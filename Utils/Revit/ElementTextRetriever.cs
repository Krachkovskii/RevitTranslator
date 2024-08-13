﻿using System;
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
/// <summary>
/// Class for extracting all user-visible text properties and parameters from the elements
/// </summary>
internal class ElementTextRetriever
{
    internal List<TranslationUnit> TranslationUnits { get; } = [];
    private HashSet<Element> _elementTypes { get; } = [];
    private readonly ProgressWindowUtils _progressWindowUtils = null;

    internal ElementTextRetriever(ProgressWindowUtils windowUtils, List<Element> elements)
    {
        _progressWindowUtils = windowUtils;
        ProcessElements(elements);
    }

    /// <summary>
    /// Extracts text from all provided elements
    /// </summary>
    /// <param name="elements"></param>
    private void ProcessElements(List<Element> elements)
    {
        foreach (Element element in elements) 
        {
            ProcessElement(element);
        }

        ProcessElementTypes();
    }

    /// <summary>
    /// Extracts text from individual element
    /// </summary>
    /// <param name="element"></param>
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

    /// <summary>
    /// Extracts text from TextNote element
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    private TranslationUnit GetTextFromTextBlock(TextNote note)
    {
        var text = note.Text;
        return new TranslationUnit(note, text);
    }

    /// <summary>
    /// Extracts text from Schedule element (currently only field headers and name)
    /// </summary>
    /// <param name="schedule"></param>
    private void ProcessSchedule(ViewSchedule schedule)
    {
        ProcessScheduleHeaders(schedule);
        ProcessElementName(schedule);
    }

    /// <summary>
    /// Extracts all overrides from Dimension element
    /// </summary>
    /// <param name="dim"></param>
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

    /// <summary>
    /// Extracts text from Dimension element with a single Dimension Segment
    /// </summary>
    /// <param name="dim"></param>
    private void ProcessSingleSegmentDimension(Dimension dim)
    {
        ProcessDimensionAboveOverride(dim, true);
        ProcessDimensionBelowOverride(dim, true);
        ProcessDimensionPrefixOverride(dim, true);
        ProcessDimensionSuffixOverride(dim, true);
        ProcessDimensionValueOverride(dim, true);
    }

    /// <summary>
    /// Extracts text from Dimension element with multiple Dimension Segments
    /// </summary>
    /// <param name="dim"></param>
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

    /// <summary>
    /// Extracts text from a particular override of a dimension
    /// </summary>
    /// <param name="d">Dimension or DimensionSegment</param>
    /// <param name="details">Type of override (e.g. Above)</param>
    /// <param name="isSingleSegment">True if dimension has only one segment</param>
    /// <returns></returns>
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

    /// <summary>
    /// Process Above override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
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

    /// <summary>
    /// Process Below override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
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

    /// <summary>
    /// Process Prefix override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
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

    /// <summary>
    /// Process Suffix override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
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

    /// <summary>
    /// Process Value override of a Dimension element
    /// </summary>
    /// <param name="dim">Dimension or DimensionSegment</param>
    /// <param name="isSingleSegment"></param>
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

    /// <summary>
    /// Process all element's parameters, including name
    /// </summary>
    /// <param name="el"></param>
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

    /// <summary>
    /// Extracts the Name property of an Element
    /// </summary>
    /// <param name="el"></param>
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

    /// <summary>
    /// Processes all available ElementTypes
    /// </summary>
    private void ProcessElementTypes()
    {
        foreach (var type in _elementTypes)
        {
            ProcessElementParameters(type);
        }
    }

    /// <summary>
    /// Extracts text from all Field Headers of a Schedule
    /// </summary>
    /// <param name="s">Schedule to process</param>
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

    /// <summary>
    /// Extracts the name of the field header
    /// </summary>
    /// <param name="sd">Schedule's Definition</param>
    /// <param name="s">The schedule</param>
    /// <param name="fieldIndex">Index of a field to be processed</param>
    /// <returns></returns>
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

    /// <summary>
    /// Adds a Unit to the list of units to be translated
    /// </summary>
    /// <param name="unit"></param>
    private void AddTranslationUnitToList(TranslationUnit unit)
    {
        TranslationUnits.Add(unit);
    }
}
