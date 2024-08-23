using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace RevitTranslatorAddin.Utils.Revit;
internal class ElementRetriever
{

    /// <summary>
    /// Gets Revit elements that are currently selected in the UI
    /// </summary>
    /// <returns>
    /// List of Elements
    /// </returns>
    internal static List<Element> GetCurrentSelection()
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
    internal static List<Element> GetElementsFromIds(IEnumerable<ElementId> ids)
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
    internal static HashSet<Element> GetTaggedElements(IEnumerable<Element> tags)
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
