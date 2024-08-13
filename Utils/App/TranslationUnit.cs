﻿using System;
using System.Collections.Generic;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Utils.App;
/// <summary>
/// This class describes a Revit-related unit of translation, 
/// which includes reference to the element, original text, translation etc.
/// </summary>
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
    public ScheduleCellCoordinates ScheduleCellCoordinates { get; internal set; } = null;

    /// <summary>
    /// Optional: Additional details of the translation, e.g. Dimension Override or Element Name.
    /// </summary>
    public TranslationDetails TranslationDetails { get; internal set; } = TranslationDetails.None;

    public TranslationUnit()
    {
    }

    public TranslationUnit(object element, string originalText)
    {
        OriginalText = originalText;
        Element = element;
    }

    public TranslationUnit(object element, string originalText, TranslationDetails details)
    {
        OriginalText = originalText;
        Element = element;
        TranslationDetails = details;
    }

    public TranslationUnit(object element, string originalText, ScheduleCellCoordinates cellCoordinates)
    {
        OriginalText = originalText;
        Element = element;
        ScheduleCellCoordinates = cellCoordinates;
    }
}
