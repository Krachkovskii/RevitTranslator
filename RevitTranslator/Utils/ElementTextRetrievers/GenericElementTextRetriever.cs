using RevitTranslator.Utils.App;
using RevitTranslator.Utils.Revit;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslator.Utils.ElementTextRetrievers;
/// <summary>
/// This class handles an element's text attributes (name and parameters).
/// </summary>
public class GenericElementTextRetriever : BaseElementTextRetriever
{
    public GenericElementTextRetriever(Element element)
    {
        Process(element);
    }

    protected override void Process(object Object)
    {
        if (Object is not Element element)
        {
            return;
        }

        ProcessElementParameters(element);

        if (element is ElementType)
        {
            ProcessElementName(element);
        }
        else if (element is ViewSchedule schedule)
        {
            ProcessElementName(schedule);
        }
        else if (element is ScheduleSheetInstance scheduleInstance)
        {
            ProcessElementName( RevitUtils.Doc.GetElement( scheduleInstance.ScheduleId ));
        }
    }

    /// <summary>
    /// Retrieves element's Name property.
    /// </summary>
    /// <param name="element"></param>
    private void ProcessElementName(Element element)
    {
        var name = element.Name;

        if (!ValidationUtils.HasText(name))
        {
            return;
        }

        var unit = new RevitTranslationUnit(element, name, TranslationDetails.ElementName);

        AddUnitToList(unit);
    }

    /// <summary>
    /// Processes all valid parameters of an Element.
    /// </summary>
    /// <param name="element"></param>
    private void ProcessElementParameters(Element element)
    {
        var parameterRetriever = new ElementParameterTextRetriever(element);
        var parameterUnits = parameterRetriever.TranslationUnits;

        foreach (var unit in parameterUnits)
        {
            AddUnitToList(unit);
        }
    }
}
