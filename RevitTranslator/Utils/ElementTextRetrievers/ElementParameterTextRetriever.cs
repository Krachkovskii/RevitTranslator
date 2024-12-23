using RevitTranslator.Utils.App;

namespace RevitTranslator.Utils.ElementTextRetrievers;
public class ElementParameterTextRetriever : BaseParameterTextRetriever
{
    private readonly Element _parentElement;
    public ElementParameterTextRetriever()
    {
    }
    public ElementParameterTextRetriever(Element element)
    {
        _parentElement = element;
        Process(element);
    }

    /// <summary>
    /// Retrieves parameters and processes each individual parameter
    /// </summary>
    /// <param name="Object">
    /// Object to process
    /// </param>
    protected override void Process(object Object)
    {
        // Making sure that we're processing only elements, since can have parameters.
        // Other non-Element objects, e.g. ScheduleFields, don't have parameters.
        if (Object is not Element element)
        {
            return;
        }

        var parameters = GetElementParameters(element);
        foreach (var parameter in parameters)
        {
            ProcessParameter(parameter);
        }
    }

    /// <summary>
    /// Retrieves all editable text parameters from an element
    /// </summary>
    /// <param name="element">Element to retrieve parameters from</param>
    /// <returns>List with valid parameter objects</returns>
    private static List<Parameter> GetElementParameters(Element element)
    {
        var paramList = new List<Parameter>();

        foreach (Parameter param in element.Parameters)
        {
            if (CanUseParameter(param))
            {
                paramList.Add(param);
            }
        }

        return paramList;
    }

    /// <summary>
    /// Retrieves text contents of parameter and adds them to the list
    /// </summary>
    /// <param name="param">
    /// Parameter object to process
    /// </param>
    private void ProcessParameter(Parameter param)
    {
        if (param == null)
        {
            return;
        }
        
        var text = base.GetText(param);
        
        if (!ValidationUtils.HasText(text))
        {
            return;
        }

        var unit = new RevitTranslationUnit(param, text);
        unit.ParentElement = param.Element;

        AddUnitToList(unit);
    }
}
