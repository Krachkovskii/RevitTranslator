using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Utils.ElementTextRetrievers;
/// <summary>
/// This class handles an element's text attributes (name and parameters).
/// </summary>
internal class GenericElementTextRetriever : BaseElementTextRetriever
{
    internal GenericElementTextRetriever(Element element)
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

        var unit = new TranslationUnit(element, name, TranslationDetails.ElementName);

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
