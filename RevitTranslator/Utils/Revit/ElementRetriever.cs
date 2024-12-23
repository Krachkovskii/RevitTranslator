using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslator.Utils.Revit;
public class ElementRetriever
{

    /// <summary>
    /// Gets Revit elements that are currently selected in the UI
    /// </summary>
    /// <returns>
    /// List of Elements
    /// </returns>
    public static List<Element> GetCurrentSelection()
    {
        var ids = RevitUtils.UIDoc.Selection.GetElementIds().ToList();
        var elements = GetElementsFromIds(ids);

        return elements;
    }

    /// <summary>
    /// Gets corresponding elements for all provided ElementIds
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public static List<Element> GetElementsFromIds(IEnumerable<ElementId> ids)
    {
        var elements = new List<Element>();

        foreach (var id in ids)
        {
            var el = RevitUtils.Doc.GetElement(id);
            if (el != null)
            {
                elements.Add(el);
            }
        }

        return elements;
    }

    /// <summary>
    /// Gets all unique tagged elements for all tags in provided list of elements.
    /// </summary>
    /// <param name="tags">
    /// Elements to process. Can contain any elements, but only IndependentTags will be processed.
    /// </param>
    /// <returns>
    /// Unique tagged elements.
    /// </returns>
    public static HashSet<Element> GetTaggedElements(IEnumerable<Element> tags)
    {
        var set = new HashSet<Element>();

        foreach (var t in tags)
        {
            if (t is not IndependentTag tag)
            {
                continue;
            }

            var tagElementIds = tag.GetTaggedLocalElementIds();
            var taggedElements = GetElementsFromIds(tagElementIds);
            var tagElements = tag.GetTaggedLocalElements().ToList();
            set.UnionWith(taggedElements);
        }

        set.RemoveWhere(n => n == null);
        return set;
    }
}
