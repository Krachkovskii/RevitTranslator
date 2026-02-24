namespace RevitTranslator.Utils;

public static class TypeAndFamilyUtils
{
    /// <summary>
    /// Gets Family for this element, if available.
    /// </summary>
    /// <param name="element"></param>
    /// <returns>
    /// Family for this element, null if not available.
    /// </returns>
    private static Family? GetFamily(Element element)
    {
        if (element is not FamilyInstance instance) return null;
        // ReSharper disable once ConvertTypeCheckPatternToNullCheck
        if (instance.Symbol is not FamilySymbol symbol) return null;

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
    private static ElementType GetElementType(Element element)
    {
        return (ElementType)Context.ActiveDocument!.GetElement(element.GetTypeId());
    }

    /// <summary>
    /// Loads modified family document back to host document
    /// </summary>
    /// <param name="familyDoc"></param>
    public static void LoadFamilyToActiveDocument(this Document familyDoc)
    {
        var loadOptions = new FamilyLoadOptions();
        familyDoc.LoadFamily(Context.ActiveDocument, loadOptions);
    }

    public static HashSet<ElementType> GetUniqueTypes(this IEnumerable<Element> elements)
    {
        var set = new HashSet<ElementType>();
        foreach (var element in elements)
        {
            var elementType = GetElementType(element);
            set.Add(elementType);
        }

        return set;
    }

    public static HashSet<Family> GetUniqueFamilies(this IEnumerable<Element> elements)
    {
        var set = new HashSet<Family>();
        
        foreach (var element in elements)
        {
            var family = GetFamily(element);
            if (family is null) continue;
            
            set.Add(family);
        }
        
        return set;
    }
}
