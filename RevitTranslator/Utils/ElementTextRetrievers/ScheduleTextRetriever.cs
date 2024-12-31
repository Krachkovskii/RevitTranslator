﻿using RevitTranslator.Models;

namespace RevitTranslator.Utils.ElementTextRetrievers;
public class ScheduleTextRetriever : BaseElementTextRetriever
{
    public ScheduleTextRetriever(ViewSchedule schedule)
    {
        Process(schedule);
    }

    public ScheduleTextRetriever(ScheduleSheetInstance scheduleInstance) 
    {
        var schedule = (ViewSchedule)Context.ActiveDocument!.GetElement(scheduleInstance.ScheduleId);
        Process(schedule);
    }

    protected override sealed void Process(object Object)
    {
        if (Object is not ViewSchedule schedule) return;

        ProcessHeaders(schedule);
        // additional methods for extracting other schedule properties go here
    }

    private void ProcessHeaders(ViewSchedule schedule)
    {
        var definition = schedule.Definition;
        var fieldCount = definition.GetFieldCount();

        for (var i = 0; i < fieldCount; i++)
        {
            var headerText = GetHeaderText(definition, i);

            var unit = new TranslationEntity(definition.GetField(i), headerText);
            unit.ParentElement = schedule;

            AddUnitToList(unit);
        }
    }
    
    private string GetHeaderText(ScheduleDefinition sd, int fieldIndex)
    {
        var field = sd.GetField(fieldIndex);
        var header = field.ColumnHeading;

        return !ValidationUtils.HasText(header) ? string.Empty : header;
    }
}
