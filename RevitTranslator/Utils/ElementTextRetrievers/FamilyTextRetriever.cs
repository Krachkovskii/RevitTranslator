using RevitTranslator.Utils.App;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.Utils.ElementTextRetrievers;
public class FamilyTextRetriever : BaseElementTextRetriever
{
    // this list can be further extended
    private readonly List<Type> _filterTypes = [typeof(TextElement)];
    private Family _family;
    private Document _familyDoc;
    public FamilyTextRetriever(Family family)
    {
        if (family == null || !family.IsEditable)
        {
            return;
        }

        Process(family);
    }

    /// <summary>
    /// UnitGroup associated with this family document.
    /// </summary>
    public RevitTranslationUnitGroup UnitGroup { get; private set; } = null;

    protected override void Process(object Object)
    {
        if ( Object is not Family family)
        {
            return;
        }

        var familyDoc = TypeAndFamilyManager.GetFamilyDocument(family);

        _family = family;
        _familyDoc = familyDoc;

        var elements = RetrieveFamilyElements(family);

        // family with no elements to process doesn't to be processed any further
        if (elements.Count == 0)
        {
            return;
        }

        UnitGroup = new RevitTranslationUnitGroup(familyDoc);

        foreach (var element in elements)
        {
            ProcessFamilyElement(element);
        }

        AddUnitsToGroup();
    }

    private void AddUnitsToGroup()
    {
        foreach (var unit in TranslationUnits)
        {
            UnitGroup.TranslationUnits.Add(unit);
        }
    }

    /// <summary>
    /// Retrieves text from a given Family element.
    /// </summary>
    /// <param name="element"></param>
    private void ProcessFamilyElement(Element element)
    {
        switch (element)
        {
            case TextElement note:
                using (var noteRetriever = new TextElementTextRetriever(note))
                {
                    foreach ( var unit in noteRetriever.TranslationUnits)
                    {
                        AddUnitToList(unit);
                    }
                }
                break;
        }
    }
    /// <summary>
    /// Calls all available methods for text extraction for various types of elements in this family
    /// </summary>
    private List<Element> RetrieveFamilyElements(Family family)
    {
        var collector = new FilteredElementCollector(_familyDoc);
        collector.WhereElementIsNotElementType();
        collector.WherePasses( new ElementMulticlassFilter(_filterTypes) );
        
        return collector.ToList();
    }
}
