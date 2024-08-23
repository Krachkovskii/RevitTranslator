using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.ElementTextRetrievers;

namespace RevitTranslatorAddin.Utils.Revit;
internal class FamilyTextRetriever : GenericElementTextRetriever
{
    private  Family _family;
    private Document _familyDoc;
    internal FamilyTextRetriever(Family family)
    {
        if (family.IsEditable)
        {
            ProcessFamily(family);
        }
    }

    /// <summary>
    /// List of Elements inside family document that are valid for text extraction
    /// </summary>
    internal List<Element> ExtractedElements { get; private set; } = [];

    /// <summary>
    /// Contains TranslationUnits for all valid elements in this family document
    /// </summary>
    internal TranslationUnitGroup UnitGroup { get; private set; } = null;

    /// <summary>
    /// Retrieves Family object for this element's instance.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    internal static Family GetFamilyFromInstance(FamilyInstance element)
    {
        if (element == null) 
        { 
            return null; 
        }

        if (element.Symbol is FamilySymbol symbol)
        {
            var family = symbol.Family;
            return family;
        }
            
        return null;
    }

    /// <summary>
    /// Loads modified family document back to host document
    /// </summary>
    /// <param name="familyDoc"></param>
    internal static void LoadFamilyDocument(Document familyDoc)
    {
        var loadOptions = new FamilyLoadOptions();
        familyDoc.LoadFamily(RevitUtils.Doc, loadOptions);
    }

    /// <summary>
    /// Adds valid element to the list of elements inside this family document
    /// </summary>
    /// <param name="element"></param>
    private void AddElementToList(Element element)
    {
        ExtractedElements.Add(element);
    }

    /// <summary>
    /// Creates new TranslationUnitGroup for this family document
    /// </summary>
    private void CreateTranslationUnitGroup(Document familyDoc)
    {
        UnitGroup = new TranslationUnitGroup(familyDoc);
    }

    /// <summary>
    /// Gets the family document for this family
    /// </summary>
    /// <returns></returns>
    private Document GetFamilyDocument(Family family)
    {
        return RevitUtils.Doc.EditFamily(family);
    }

    /// <summary>
    /// Extracts all valid elements from the family
    /// </summary>
    /// <param name="family"></param>
    private void ProcessFamily(Family family)
    {
        _family = family;
        _familyDoc = GetFamilyDocument(family);

        CreateTranslationUnitGroup(_familyDoc);

        ProcessFamilyElements();
    }

    /// <summary>
    /// Calls all available methods for text extraction for various types of elements in this family
    /// </summary>
    private void ProcessFamilyElements()
    {
        RetrieveTextElements();
    }

    /// <summary>
    /// Gets all text notes in family document
    /// </summary>
    /// <returns></returns>
    private void RetrieveTextElements()
    {
        var collector = new FilteredElementCollector(_familyDoc);
        collector.OfClass(typeof(TextElement));

        foreach (var note in collector)
        {
            AddElementToList((TextElement)note);
        }
    }
}
