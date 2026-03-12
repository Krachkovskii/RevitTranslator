using RevitTranslator.Revit.Core.Enums;

namespace RevitTranslator.Revit.Core.Models;

/// <summary>
/// This class describes a Revit-related unit of translation,
/// which includes reference to the element, original text, translation etc.
/// </summary>
public sealed class TranslationEntity
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
    /// Optional: ElementId of the ParentElement, stored as a safe value that remains valid
    /// after the parent element's document is closed or its transaction is rolled back.
    /// </summary>
    public ElementId? ParentElementId { get; set; }

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

    /// <summary>
    /// Indicates whether the translated text was successfully written back to the Revit model.
    /// </summary>
    public bool UpdatedInModel { get; set; }

    /// <summary>
    /// The illegal character that prevented the entity from being updated in the model, if any.
    /// </summary>
    public char? IllegalCharacter { get; set; }
}
