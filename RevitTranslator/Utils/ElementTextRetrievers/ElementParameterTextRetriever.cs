using RevitTranslator.Models;

namespace RevitTranslator.Utils.ElementTextRetrievers;
public class ElementParameterTextRetriever : BaseParameterTextRetriever
{
    private readonly Element _parentElement;

    public ElementParameterTextRetriever(Element element)
    {
        _parentElement = element;
        Process(element);
    }

    protected override sealed void Process(object Object)
    {
        // Making sure that we're processing only elements - only they can have parameters.
        // Other non-Element objects, e.g. ScheduleFields, don't have parameters.
        if (Object is not Element element) return;

        var parameters = GetElementParameters(element);
        foreach (var parameter in parameters)
        {
            ProcessParameter(parameter);
        }
    }

    private static List<Parameter> GetElementParameters(Element element)
    {
        return element.Parameters
            .Cast<Parameter>()
            .Where(CanUseParameter)
            .ToList();
    }
    
    private void ProcessParameter(Parameter param)
    {
        if (param == null) return;
        
        var text = base.GetText(param);
        if (!ValidationUtils.HasText(text)) return;

        var unit = new TranslationEntity(param, text);
        unit.ParentElement = param.Element;

        AddUnitToList(unit);
    }
}
