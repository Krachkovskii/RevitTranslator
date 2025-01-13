using RevitTranslator.Extensions;
using RevitTranslator.Models;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.ElementTextRetrievers;

/// <summary>
/// Use this class to retrieve text from a list of elements.
/// This is the primary class to process elements in a document.
/// </summary>
public class MultiElementTextRetriever : BaseElementTextRetriever
{
    private BaseElementTextRetriever? _currentRetriever;
    private HashSet<ElementType> _elementTypes = [];
    private HashSet<Family> _families = [];
    private HashSet<Element> _taggedElements = [];
    private readonly List<DocumentTranslationEntityGroup> _unitGroups = [];

    public List<DocumentTranslationEntityGroup> CreateEntities(Element[] elements, bool translateProjectParameters, out int unitCount)
    {
        _unitGroups.Add(new DocumentTranslationEntityGroup(Context.ActiveDocument!));
        
        _taggedElements = elements.OfType<IndependentTag>().GetUniqueTaggedElements();
        var instancesTypesFamilies = elements.ToHashSet();
        instancesTypesFamilies.UnionWith(_taggedElements);

        _elementTypes = instancesTypesFamilies.GetUniqueTypes();
        _families = instancesTypesFamilies.GetUniqueFamilies();

        instancesTypesFamilies.UnionWith(_elementTypes);
        instancesTypesFamilies.UnionWith(_families);

        foreach (var element in instancesTypesFamilies)
        {
            Process(element);
        }

        if (translateProjectParameters)
        {
            //TODO: Implement project parameter handling
            // ReSharper disable once UnusedVariable
            var paramRetriever = new ProjectParameterTextRetriever();
        }

        unitCount = _unitGroups.Sum(group => group.TranslationEntities.Count);
        return _unitGroups;
    }

    protected override void Process(object Object)
    {
        switch (Object)
        {
            case TextElement textElement:
                _currentRetriever = new TextElementTextRetriever(textElement);
                break;

            case ViewSchedule schedule:
                _currentRetriever = new ScheduleTextRetriever(schedule);
                break;

            case ScheduleSheetInstance scheduleInstance:
                _currentRetriever = new ScheduleTextRetriever(scheduleInstance);
                break;

            case Dimension dimension:
                _currentRetriever = new DimensionTextRetriever(dimension);
                break;

            case Family family:
                var familyRetriever = new FamilyTextRetriever(family);
                AddFamilyUnitGroupToList(familyRetriever);

                // exiting the function, since families are treated differently and don't
                // need to be processed as regular elements
                return;

            // this case will hit if we're processing anything that doesn't require any special treatment
            // besides one for a generic element
            case not null:
                _currentRetriever = null;
                break;
        }

        if (_currentRetriever is not null)
        {
            AddUnitsToGroup(_currentRetriever.ElementTranslationUnits);
        }
        _currentRetriever?.Dispose();
        _currentRetriever = null;

        if (Object is not Element element) return;

        /* Every element still gets processed as a generic element
        * in case of any instance or type parameters.
        * This function also processes ElementTypes, which are classified as Elements.*/

        var elementRetriever = new ElementTextRetriever(element);
        AddUnitsToGroup(elementRetriever.ElementTranslationUnits);
    }

    private void AddFamilyUnitGroupToList(FamilyTextRetriever retriever)
    {
        if (retriever.EntityGroup is null) return;
        
        _unitGroups.Add(retriever.EntityGroup);
    }
    
    private void AddUnitsToGroup(List<TranslationEntity> units)
    {
        if (units.Count == 0) return;
        
        var document = units[0].Document;
        var documentGroup = _unitGroups.First(group => group.Document.Equals(document));

        foreach (var unit in units)
        {
            documentGroup?.TranslationEntities.Add(unit);
        }
    }
}
