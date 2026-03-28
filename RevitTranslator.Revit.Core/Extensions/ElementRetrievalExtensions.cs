
namespace RevitTranslator.Revit.Core.Extensions;

public static class ElementRetrievalExtensions
{
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
