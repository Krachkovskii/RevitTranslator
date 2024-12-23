using RevitTranslator.Utils.App;
using RevitTranslator.Utils.Revit;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslator.Utils.ElementTextRetrievers;
/// <summary>
/// Use this class to retrieve text from a list of elements.
/// This is the primary class to process elements in a document.
/// </summary>
public class BatchTextRetriever : BaseElementTextRetriever
{
    /// <summary>
    /// Current BaseElementTextRetriever for the Process function.
    /// </summary>
    private BaseElementTextRetriever _currentRetriever = null;

    public BatchTextRetriever(List<Element> elements, bool translateProjectParameters)
    {
        TaggedElements = ElementRetriever.GetTaggedElements(elements);
        HashSet<Element> instancesTypesFamilies = elements.ToHashSet();
        instancesTypesFamilies.UnionWith(TaggedElements);

        ElementTypes = TypeAndFamilyManager.GetUniqueTypesFromElements(instancesTypesFamilies);
        Families = TypeAndFamilyManager.GetUniqueFamiliesFromElements(instancesTypesFamilies);

        UnitGroups.Add(new RevitTranslationUnitGroup(RevitUtils.Doc));

        instancesTypesFamilies.UnionWith(ElementTypes);
        instancesTypesFamilies.UnionWith(Families);

        var elementsToTranslate = instancesTypesFamilies.Where(n => n != null).ToList();

        foreach (var element in elementsToTranslate)
        {
            Process(element);
        }

        if (translateProjectParameters)
        {
            var paramRetriever = new ProjectParameterTextRetriever();
        }
    }

    /// <summary>
    /// ElementTypes to be processed.
    /// </summary>
    public HashSet<ElementType> ElementTypes { get; } = [];

    /// <summary>
    /// Families to be processed.
    /// </summary>
    public HashSet<Family> Families { get; } = [];

    /// <summary>
    /// Tagged elements to be processed.
    /// </summary>
    public HashSet<Element> TaggedElements { get; } = [];

    /// <summary>
    /// TranslationUnitGroups associated with provided elements.
    /// </summary>
    public List<RevitTranslationUnitGroup> UnitGroups { get; } = [];
    /// <summary>
    /// Uses class-appropriate methods to retrieve information from a specific element.
    /// </summary>
    /// <param name="Object"></param>
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
            case object:
                _currentRetriever = null;
                break;
        }

        AddUnitsToGroup(_currentRetriever?.TranslationUnits);
        _currentRetriever?.Dispose();
        _currentRetriever = null;

        if (Object is not Element element)
        {
            return;
        }

        /* Every element still gets processed as a generic element
        * in case of any instance or type parameters.
        * This function also processes ElementTypes, which are classified as Elements.*/

        var elementRetriever = new GenericElementTextRetriever(element);
        AddUnitsToGroup(elementRetriever.TranslationUnits);
    }

    /// <summary>
    /// If available, adds the UnitGroup of a family to the list of UnitGroups.
    /// </summary>
    /// <param name="retriever"></param>
    private void AddFamilyUnitGroupToList(FamilyTextRetriever retriever)
    {
        if (retriever == null || retriever.UnitGroup == null)
        {
            return;
        }

        UnitGroups.Add(retriever.UnitGroup);
    }

    /// <summary>
    /// Adds TranslationUnits to the RevitTranslationUnitGroup with matching Document.
    /// </summary>
    /// <param name="units"></param>
    private void AddUnitsToGroup(List<RevitTranslationUnit> units)
    {
        if (units == null || units.Count == 0)
        {
            return;
        }
        
        var u = units[0];
        var group = UnitGroups.FirstOrDefault(g => g.Document.Equals(u.Document));

        foreach (var unit in units)
        {
            group?.TranslationUnits.Add(unit);
        }
    }
}
