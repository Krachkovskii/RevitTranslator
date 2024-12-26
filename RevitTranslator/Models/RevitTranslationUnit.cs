using RevitTranslator.Utils.Revit;

namespace RevitTranslator.Models;
/// <summary>
/// This class describes a Revit-related unit of translation, 
/// which includes reference to the element, original text, translation etc.
/// </summary>
public class RevitTranslationUnit
{
    private object _element = null;
    private Element _parentElement = null;

    public RevitTranslationUnit()
    {
    }

    public RevitTranslationUnit(object element, string originalText)
    {
        OriginalText = originalText;
        Element = element;
    }

    public RevitTranslationUnit(object element, string originalText, TranslationDetails details)
    {
        OriginalText = originalText;
        Element = element;
        TranslationDetails = details;
    }

    public RevitTranslationUnit(object element, string originalText, ScheduleCellCoordinates cellCoordinates)
    {
        OriginalText = originalText;
        Element = element;
        ScheduleCellCoordinates = cellCoordinates;
    }

    public Document Document { get; private set; } = null;

    /// <summary>
    /// The element to be updated.
    /// </summary>
    public object Element
    {
        get => _element;
        set
        {
            _element = value;
            SetElementDocumentAndId(value);
        }
    }

    /// <summary>
    /// ElementId of the element.
    /// </summary>
    public ElementId ElementId { get; private set; } = null;

    /// <summary>
    /// Original text that will be replaced by translation.
    /// </summary>
    public string OriginalText { get; private set; } = string.Empty;

    /// <summary>
    /// Optional: parent element that is UI-visible. 
    /// e.g.: if translated object is Parameter, you can store the element for this parameter;
    /// if translated object is ScheduleField, store Schedule element here.
    /// </summary>
    public Element ParentElement
    {
        get => _parentElement;
        set
        {
            _parentElement = value;
            SetElementDocumentAndId(value);
        }
    }

    /// <summary>
    /// Optional: TableSectionData coordinates (row, column).
    /// </summary>
    public ScheduleCellCoordinates ScheduleCellCoordinates { get; set; } = null;

    /// <summary>
    /// Translation of the original text.
    /// </summary>
    public string TranslatedText { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional: Additional details of the translation, e.g. Dimension Override or Element Name.
    /// </summary>
    public TranslationDetails TranslationDetails { get; set; } = TranslationDetails.None;
    
    
    private void SetElementDocumentAndId(object element)
    {
        if (element is Element el)
        {
            Document = el.Document;
            ElementId = el.Id;
        }

        else if (ParentElement is Element parentEl)
        {
            Document = parentEl.Document;
            ElementId = parentEl.Id;
        }
    }
}
