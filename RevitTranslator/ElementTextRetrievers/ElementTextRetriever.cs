using RevitTranslator.Enums;
using RevitTranslator.Models;
using RevitTranslator.Utils;

namespace RevitTranslator.ElementTextRetrievers;

/// <summary>
/// This class handles an element's text attributes (name and parameters).
/// </summary>
public class ElementTextRetriever : BaseElementTextRetriever
{
    public ElementTextRetriever(Element element)
    {
        Process(element);
    }

    protected override sealed void Process(object Object)
    {
        if (Object is not Element element) return;

        ProcessElementParameters(element);
        switch (element)
        {
            case ElementType:
                ProcessElementName(element);
                break;
            case ViewSchedule schedule:
                ProcessElementName(schedule);
                break;
            case ScheduleSheetInstance scheduleInstance:
                ProcessElementName(Context.ActiveDocument!.GetElement(scheduleInstance.ScheduleId));
                break;
        }
    }

    private void ProcessElementParameters(Element element)
    {
        var parameterRetriever = new ElementParameterTextRetriever(element);
        var parameterUnits = parameterRetriever.ElementTranslationUnits;

        foreach (var unit in parameterUnits)
        {
            AddUnitToList(unit);
        }
    }

    private void ProcessElementName(Element element)
    {
        var name = element.Name;
        if (!name.HasText()) return;

        var unit = new TranslationEntity
        {
            Element = element,
            ElementId = element.Id,
            Document = element.Document,
            OriginalText = name,
            TranslationDetails = TranslationDetails.ElementName
        };
        
        AddUnitToList(unit);
    }
}
