using RevitTranslator.Models;
using RevitTranslator.Utils;

namespace RevitTranslator.ElementTextRetrievers;

public class FamilyTextRetriever : BaseElementTextRetriever
{
    // this list can be further extended
    private readonly List<Type> _usableTypesInFamily = [
        typeof(TextElement)
    ];
    
    private readonly Document _familyDoc = null!;
    
    public FamilyTextRetriever(Family family)
    {
        if (!family.IsEditable) return;
        
        _familyDoc = TypeAndFamilyUtils.GetFamilyDocument(family);
        Process(family);
    }

    /// <summary>
    /// EntityGroup associated with this family document.
    /// </summary>
    public DocumentTranslationEntityGroup? EntityGroup { get; private set; }

    protected override sealed void Process(object Object)
    {
        if ( Object is not Family) return;

        var elements = RetrieveFamilyElements();
        // family with no elements to process doesn't to be processed any further
        if (elements.Count == 0) return;

        EntityGroup = new DocumentTranslationEntityGroup(_familyDoc);
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
        collector.WherePasses( new ElementMulticlassFilter(_usableTypesInFamily) );
        
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
            EntityGroup?.TranslationEntities.Add(unit);
        }
    }
}
