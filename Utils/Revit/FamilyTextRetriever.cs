using RevitTranslatorAddin.Utils.App;

namespace RevitTranslatorAddin.Utils.Revit;
internal class FamilyTextRetriever
{
    private  Family _family;
    private Document _familyDoc;
    internal List<Element> ExtractedElements { get; private set; } = [];
    internal TranslationUnitGroup UnitGroup { get; private set; } = null;

    internal FamilyTextRetriever(Family family)
    {
        if (family.IsEditable)
        {
            ProcessFamily(family);
        }
    }

    private void ProcessFamily(Family family)
    {
        _family = family;
        _familyDoc = GetFamilyDocument();

        CreateTranslationUnitGroup();

        ProcessFamilyElements();
    }

    internal static Family GetFamilyFromInstance(FamilyInstance element)
    {
        if (element == null) { return null; }

        if (element.Symbol is FamilySymbol symbol)
        {
            var family = symbol.Family;
            return family;
        }
        else
        {
            return null;
        }
    }

    private Document GetFamilyDocument()
    {
        return RevitUtils.Doc.EditFamily(_family);
    }

    internal static void LoadFamilyDocument(Document familyDoc)
    {
        var loadOptions = new FamilyLoadOptions();
        familyDoc.LoadFamily(RevitUtils.Doc, loadOptions);
    }

    private void ProcessFamilyElements()
    {
        ProcessTextNotes();
    }

    /// <summary>
    /// Gets all text notes in family document
    /// </summary>
    /// <returns></returns>
    private void ProcessTextNotes()
    {
        var collector = new FilteredElementCollector(_familyDoc);
        collector.OfClass(typeof(TextNote));

        foreach (var note in collector)
        {
            AddElementToList((TextNote)note);
        }
    }

    private void AddElementToList(Element element)
    {
        ExtractedElements.Add(element);
    }

    private void CreateTranslationUnitGroup()
    {
        UnitGroup = new TranslationUnitGroup(_familyDoc);
    }
}
