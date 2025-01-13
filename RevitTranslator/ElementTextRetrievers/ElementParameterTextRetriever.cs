using RevitTranslator.Models;
using RevitTranslator.Utils.App;
using RevitTranslator.Utils.Revit;
using ParameterUtils = RevitTranslator.Utils.Revit.ParameterUtils;

namespace RevitTranslator.ElementTextRetrievers;

public class ElementParameterTextRetriever : BaseElementTextRetriever
{
    public ElementParameterTextRetriever(Element element)
    {
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
            .Where(ParameterUtils.CanUseParameter)
            .ToList();
    }
    
    private void ProcessParameter(Parameter param)
    {
        var text = param.GetText();
        if (!text.HasText()) return;

        var unit = new TranslationEntity
        {
            Element = param,
            ElementId = param.Id,
            ParentElement = param.Element,
            Document = param.Element.Document,
            OriginalText = text,
        };

        AddUnitToList(unit);
    }
}
