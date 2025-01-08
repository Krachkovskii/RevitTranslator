using RevitTranslator.Enums;

namespace RevitTranslator.Models;
/// <summary>
/// This class describes a Revit-related unit of translation, 
/// which includes reference to the element, original text, translation etc.
/// </summary>
public class TranslationEntity
{
    /// <summary>
    /// Document that hosts the Element or ParentElement.
    /// </summary>
    public Document Document { get; set; } = null!;

    /// <summary>
    /// The element to be updated.
    /// </summary>
    public object Element { get; set; } = null!;

    /// <summary>
    /// ElementId of the element.
    /// </summary>
    public ElementId ElementId { get; set; } = null!;

    /// <summary>
    /// Original text that will be replaced by translation.
    /// </summary>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Optional: parent element that is UI-visible. 
    /// e.g.: if translated object is Parameter, you can store the element for this parameter;
    /// if translated object is ScheduleField, store Schedule element here.
    /// </summary>
    public Element? ParentElement { get; set; }

    /// <summary>
    /// Optional: TableSectionData coordinates (row, column).
    /// </summary>
    public ScheduleCellCoordinates? ScheduleCellCoordinates { get; set; }

    /// <summary>
    /// Translation of the original text.
    /// </summary>
    public string TranslatedText { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional: Additional details of the translation, e.g. Dimension Override or Element Name.
    /// </summary>
    public TranslationDetails TranslationDetails { get; set; } = TranslationDetails.None;
}
