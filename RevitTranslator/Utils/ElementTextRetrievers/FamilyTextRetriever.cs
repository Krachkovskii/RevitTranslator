using RevitTranslator.Models;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.Utils.ElementTextRetrievers;
public class FamilyTextRetriever : BaseElementTextRetriever
{
    // this list can be further extended
    private readonly List<Type> _filterTypes = [
        typeof(TextElement)
    ];
    
    private Family _family;
    private Document _familyDoc;
    
    public FamilyTextRetriever(Family family)
    {
        if (family == null || !family.IsEditable) return;

        Process(family);
    }

    /// <summary>
    /// EntityGroup associated with this family document.
    /// </summary>
    public DocumentTranslationEntityGroup EntityGroup { get; private set; } = null;

    protected override sealed void Process(object Object)
    {
        if ( Object is not Family family)
        {
            return;
        }

        var familyDoc = TypeAndFamilyUtils.GetFamilyDocument(family);

        _family = family;
        _familyDoc = familyDoc;

        var elements = RetrieveFamilyElements();

        // family with no elements to process doesn't to be processed any further
        if (elements.Count == 0)
        {
            return;
        }

        EntityGroup = new DocumentTranslationEntityGroup(familyDoc);

        foreach (var element in elements)
        {
            ProcessFamilyElement(element);
        }

        AddUnitsToGroup();
    }
    
    private List<Element> RetrieveFamilyElements()
    {
        var collector = new FilteredElementCollector(_familyDoc);
        collector.WhereElementIsNotElementType();
        collector.WherePasses( new ElementMulticlassFilter(_filterTypes) );
        
        return collector.ToList();
    }

    private void ProcessFamilyElement(Element element)
    {
        switch (element)
        {
            case TextElement note:
                using (var noteRetriever = new TextElementTextRetriever(note))
                {
                    foreach ( var unit in noteRetriever.ElementTranslationUnits)
                    {
                        AddUnitToList(unit);
                    }
                }
                break;
        }
    }

    private void AddUnitsToGroup()
    {
        foreach (var unit in ElementTranslationUnits)
        {
            EntityGroup.TranslationEntities.Add(unit);
        }
    }
}
