using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslator.Utils.Revit;
public class TypeAndFamilyManager
{
    /// <summary>
    /// Gets Family for this element, if available.
    /// </summary>
    /// <param name="element"></param>
    /// <returns>
    /// Family for this element, null if not available.
    /// </returns>
    public static Family GetFamily(Element element)
    {
        if (element is not FamilyInstance instance)
        {
            return null;
        }

        if (instance.Symbol is not FamilySymbol symbol)
        {
            return null;
        }

        var family = symbol.Family;
        return family;
    }

    /// <summary>
    /// As per Revit API, this method gets an independent and modifiable copy of the Element's own document.
    /// Not to be confused with Family.Document property, which returns a document that hosts 
    /// a family object, i.e. host document.
    /// </summary>
    /// <returns>
    /// Document that represents this family.
    /// </returns>
    public static Document GetFamilyDocument(Family family)
    {
        return Context.ActiveDocument!.EditFamily(family);
    }

    /// <summary>
    /// Gets FamilyType for this element, if available.
    /// </summary>
    /// <param name="element"></param>
    /// <returns>
    /// FamilyType for this element, null if not available.
    /// </returns>
    public static ElementType GetElementType(Element element)
    {
        return Context.ActiveDocument!.GetElement(element.GetTypeId()) as ElementType;
    }

    /// <summary>
    /// Loads modified family document back to host document
    /// </summary>
    /// <param name="familyDoc"></param>
    public static void LoadFamilyToActiveDocument(Document familyDoc)
    {
        var loadOptions = new FamilyLoadOptions();
        familyDoc.LoadFamily(RevitUtils.Doc, loadOptions);
    }

    public static HashSet<ElementType> GetUniqueTypesFromElements(IEnumerable<Element> elements)
    {
        var set = new HashSet<ElementType>();
        foreach (var element in elements)
        {
            var elementType = GetElementType(element);
            set.Add(elementType);
        }

        return set;
    }

    public static HashSet<Family> GetUniqueFamiliesFromElements(IEnumerable<Element> elements)
    {
        var set = new HashSet<Family>();
        
        foreach (var element in elements)
        {
            var family = GetFamily(element);
            set.Add(family);
        }
        
        return set;
    }
}
