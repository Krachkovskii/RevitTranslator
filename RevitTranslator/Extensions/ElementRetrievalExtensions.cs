using Autodesk.Revit.UI;

namespace RevitTranslator.Extensions;

public static class ElementRetrievalExtensions
{
    /// <summary>
    /// Gets Revit elements that are currently selected in the UI
    /// </summary>
    /// <returns>
    /// List of Elements
    /// </returns>
    public static List<Element> GetSelectedElements(this UIDocument document)
    {
        return document.Selection
            .GetElementIds()
            .Select(id => id.ToElement())
            .OfType<Element>()
            .ToList();
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
    public static HashSet<Element> GetUniqueTaggedElements(this IEnumerable<IndependentTag> tags)
    {
        return tags
            .SelectMany(tag => tag.GetTaggedLocalElements())
            .Where(element => element is not null)
            .ToHashSet();
    }
}
