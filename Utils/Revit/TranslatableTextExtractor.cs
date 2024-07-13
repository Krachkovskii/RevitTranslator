﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.Revit;
internal class TranslatableTextExtractor
{
    public int PossibleTranslationCount = 0;
    private bool IsTranslatable(string input)
    {
        return !(string.IsNullOrWhiteSpace(input) || Regex.IsMatch(input, @"^\d+$"));
    }

    private string TextNoteText(TextNote textNote)
    {
        if (IsTranslatable(textNote.Text))
        {
            PossibleTranslationCount++;
            return textNote.Text;
        }

        return null;
    }


    private List<(string, string)> DimensionText(Dimension dim)
    {
        var overrides = new List<(string, string)>();
        if (IsTranslatable(dim.Above))
        {
            overrides.Add((dim.Above, "above"));
            PossibleTranslationCount++;
        }

        if (IsTranslatable(dim.Below))
        {
            overrides.Add((dim.Below, "below"));
            PossibleTranslationCount++;
        }

        if (IsTranslatable(dim.Prefix))
        {
            overrides.Add((dim.Prefix, "prefix"));
            PossibleTranslationCount++;
        }

        if (IsTranslatable(dim.Suffix))
        {
            overrides.Add((dim.Suffix, "suffix"));
            PossibleTranslationCount++;
        }

        if (IsTranslatable(dim.ValueOverride))
        {
            overrides.Add((dim.ValueOverride, "override"));
            PossibleTranslationCount++;
        }

        if (overrides.Count > 0)
        {
            return overrides;
        }

        return null;
    }

    private List<(Parameter, string)> ElementParametersText(Element element)
    {
        var output = new List<(Parameter, string)>();

        var parameters = element.Parameters
            .Cast<Parameter>()
            .Where(p => p.StorageType == StorageType.String && !p.IsReadOnly)
            .ToList();

        foreach (var parameter in parameters)
        {
            var value = parameter.AsString();
            if (IsTranslatable(value))
            {
                output.Add((parameter, value));
                PossibleTranslationCount++;
            }
        }

        if (output.Count > 0)
        {
            return output;
        }

        return null;
    }

    private List<(ScheduleField, string)> ScheduleHeadersText(ViewSchedule s)
    {
        var output = new List<(ScheduleField, string)>();
        //var s = doc.GetElement(scheduleId) as ViewSchedule;

        // Translating field headers
        var sd = s.Definition;
        var fieldCount = sd.GetFieldCount();
        for (var i = 0; i < fieldCount; i++)
        {
            var field = sd.GetField(i);
            var header = field.ColumnHeading;
            if (IsTranslatable(header))
            {
                output.Add((field, header));
                PossibleTranslationCount++;
            }
        }

        if (output.Count > 0)
        {
            return output;
        }

        return null;
    }

    internal void GetTranslatableText(List<Element> elements)
    {
        foreach (var element in elements)
        {
            if (element is TextNote)
            {
                var text = TextNoteText(element as TextNote);
            }
            else if (element is Dimension)
            {
                var text = DimensionText(element as Dimension);
            }
            else if (element is ViewSchedule)
            {
                var text = ScheduleHeadersText(element as ViewSchedule);
            }
            else if (element is ScheduleSheetInstance)
            {
                var si = (ScheduleSheetInstance)element;
                var s = RevitUtils.Doc.GetElement(si.ScheduleId) as ViewSchedule;
                var text = ScheduleHeadersText(s);
            }
            else
            {
                var name = element.Name;
                PossibleTranslationCount++;

                var text = ElementParametersText(element);
            }

        }
    }
}
