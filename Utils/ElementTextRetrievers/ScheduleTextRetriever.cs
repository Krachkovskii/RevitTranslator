using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Utils.ElementTextRetrievers;
internal class ScheduleTextRetriever : BaseElementTextRetriever
{
    internal ScheduleTextRetriever(ViewSchedule schedule)
    {
        Process(schedule);
    }

    internal ScheduleTextRetriever(ScheduleSheetInstance scheduleInstance) 
    {
        var schedule = RevitUtils.Doc.GetElement(scheduleInstance.ScheduleId) as ViewSchedule;
        Process(schedule);
    }

    protected override void Process(object Object)
    {
        if (Object is not ViewSchedule schedule) 
        {
            return;
        }

        ProcessHeaders(schedule);
        // additional methods for extracting other schedule properties go here
    }

    /// <summary>
    /// Retrieves text contents from Schedule field's header.
    /// </summary>
    /// <param name="sd">Schedule's definition</param>
    /// <param name="fieldIndex">Index of the field</param>
    /// <returns></returns>
    private static string GetHeaderText(ScheduleDefinition sd, int fieldIndex)
    {
        var field = sd.GetField(fieldIndex);
        var header = field.ColumnHeading;

        if (!ValidationUtils.HasText(header))
        {
            return string.Empty;
        }

        return header;
    }

    private void ProcessHeaders(ViewSchedule schedule)
    {
        var definition = schedule.Definition;
        var fieldCount = definition.GetFieldCount();

        for (var i = 0; i < fieldCount; i++)
        {
            var headerText = GetHeaderText(definition, i);

            var unit = new TranslationUnit(definition.GetField(i), headerText);
            unit.ParentElement = schedule;

            AddUnitToList(unit);
        }
    }
}
