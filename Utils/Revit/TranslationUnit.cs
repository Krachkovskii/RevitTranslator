using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.Revit;
public class TranslationUnit
{
    /// <summary>
    /// Original text that will be replaced by translation.
    /// </summary>
    public string OriginalText { get; internal set; } = string.Empty;

    /// <summary>
    /// Translation of the original text.
    /// </summary>
    public string TranslatedText { get; internal set; } = string.Empty;

    /// <summary>
    /// ElementId of the element.
    /// </summary>
    public ElementId ElementId { get; internal set; } = null;

    /// <summary>
    /// The element to be updated.
    /// </summary>
    public object Element { get; internal set; } = null;

    /// <summary>
    /// Optional: parent element that is UI-visible. 
    /// e.g.: if translated object is Parameter, you can store the element for this parameter;
    /// if translated object is ScheduleField, store Schedule element here.
    /// </summary>
    public Element ParentElement { get; internal set; } = null;

    /// <summary>
    /// Optional: TableSectionData coordinates (row, column).
    /// </summary>
    public TableSectionCoordinates TableSectionCoordinates { get; internal set; } = null;

    /// <summary>
    /// Optional: Additional details of the translation, e.g. Dimension Override or Element Name.
    /// </summary>
    public TranslationDetails TranslationDetails { get; internal set; } = TranslationDetails.None;

    public TranslationUnit() { }
}
