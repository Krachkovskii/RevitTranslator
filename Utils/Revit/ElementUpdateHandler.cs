using System.Diagnostics;
using System.Windows;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.App;

namespace RevitTranslatorAddin.Utils.Revit;

/// <summary>
/// This class handles update of elements in the model using Transaction
/// </summary>
public class ElementUpdateHandler : IExternalEventHandler, IDisposable
{
    /// <summary>
    /// List of translations that can't be applied due to illegal characters
    /// </summary>
    private readonly List<string> _cantUpdate = [];

    /// <summary>
    /// Utils for the Progress window of current command
    /// </summary>
    internal static ProgressWindowUtils ProgressWindowUtils { get; set; } = null;

    internal static List<TranslationUnitGroup> TranslationUnitGroups { get; set; } = [];

    /// <summary>
    /// Clears all necessary values
    /// </summary>
    public void Dispose()
    {
        ProgressWindowUtils.PW.Dispatcher.Invoke(() => ProgressWindowUtils.VM.UpdateFinished());

        _cantUpdate.Clear();
        ProgressWindowUtils = null;
        TranslationUnitGroups = null;

        RevitUtils.ExEventHandler = null;
    }

    public void Execute(UIApplication app)
    {
        ProgressWindowUtils.PW.Dispatcher.Invoke(() => ProgressWindowUtils.VM.UpdateStarted());

        foreach (var group in TranslationUnitGroups)
        {
            RunTranslationUpdate(group);
            if (group.Document.IsFamilyDocument)
            {
                TypeAndFamilyManager.LoadFamilyToActiveDocument(group.Document);
            }
        }

        Dispose();
    }

    public string GetName()
    {
        return "Element Updater";
    }

    /// <summary>
    /// Updates the Name property of the element
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="element"></param>
    private static void SetElementName(TranslationUnit unit, Element element)
    {
        element.Name = unit.TranslatedText;
    }

    /// <summary>
    /// Shows error message about a faulty transaction
    /// </summary>
    /// <param name="ex"></param>
    private static void ShowTransactionErrorMessage(Exception ex)
    {
        MessageBox.Show(ex.Message,
                        "Error updating elements",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
    }

    /// <summary>
    /// Adds report about translation with illegal characters
    /// </summary>
    /// <param name="unit"></param>
    private void AddIllegalTranslation(TranslationUnit unit)
    {
        _cantUpdate.Add($"{unit.TranslatedText} " +
            $"(Symbol: \"{unit.TranslatedText.FirstOrDefault(c => ValidationUtils.ForbiddenParameterSymbols.Contains(c))}\", " +
            $"ElementId: {unit.ElementId})");
    }

    private void RunTranslationUpdate(TranslationUnitGroup group)
    {
        using (var t = new Transaction(group.Document, $"Update Document {group.Document.Title}"))
        {
            t.Start();
            try
            {
                foreach (var unit in group.TranslationUnits)
                {
                    UpdateElement(unit);
                }

                if (_cantUpdate.Count > 0)
                {
                    ShowCantTranslateMessage(group.Document);
                }

                t.Commit();
            }
            catch (Exception ex)
            {
                ShowTransactionErrorMessage(ex);
                t.RollBack();
            }
        };
    }
    
    /// <summary>
    /// Updates overrides of DimensionSegment element
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="dim"></param>
    private void SetDimensionSegmentText(TranslationUnit unit, DimensionSegment dim)
    {
        switch (unit.TranslationDetails)
        {
            case TranslationDetails.DimensionAbove:
                dim.Above = unit.TranslatedText;
                break;
            case TranslationDetails.DimensionBelow:
                dim.Below = unit.TranslatedText;
                break;
            case TranslationDetails.DimensionPrefix:
                dim.Prefix = unit.TranslatedText;
                break;
            case TranslationDetails.DimensionSuffix:
                dim.Suffix = unit.TranslatedText;
                break;
            case TranslationDetails.DimensionOverride:
                dim.ValueOverride = unit.TranslatedText;
                break;
        }
    }

    /// <summary>
    /// Updates overrides of Dimension element
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="dim"></param>
    private void SetDimensionText(TranslationUnit unit, Dimension dim)
    {
        switch (unit.TranslationDetails)
        {
            case TranslationDetails.DimensionAbove:
                dim.Above = unit.TranslatedText;
                break;
            case TranslationDetails.DimensionBelow:
                dim.Below = unit.TranslatedText;
                break;
            case TranslationDetails.DimensionPrefix:
                dim.Prefix = unit.TranslatedText;
                break;
            case TranslationDetails.DimensionSuffix:
                dim.Suffix = unit.TranslatedText;
                break;
            case TranslationDetails.DimensionOverride:
                dim.ValueOverride = unit.TranslatedText;
                break;
        }
    }

    /// <summary>
    /// Updates the value of the Schedule field
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="field"></param>
    private void SetScheduleField(TranslationUnit unit, ScheduleField field)
    {
        field.ColumnHeading = unit.TranslatedText;
    }

    /// <summary>
    /// Updates the text in Text note
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="textElement"></param>
    private void SetTextElementText(TranslationUnit unit, TextElement textElement)
    {
        textElement.Text = unit.TranslatedText;
    }

    /// <summary>
    /// Shows message with all translations that can't be updated due to illegal characters
    /// </summary>
    private void ShowCantTranslateMessage(Document doc)
    {
        MessageBox.Show($"These translations in document \"{doc.Title}\" weren't updated due to forbidden symbols: \n{string.Join("\n", _cantUpdate)}.",
                                        "Warning",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
    }

    /// <summary>
    /// Updates individual element, performing a set of validation measures
    /// </summary>
    /// <param name="unit"></param>
    private void UpdateElement(TranslationUnit unit)
    {
        try
        {
            if (unit.Element == null 
                || unit.TranslatedText == string.Empty 
                || unit.TranslatedText == unit.OriginalText)
            {
                return;
            }

            if (ValidationUtils.NameHasIllegalCharacters(unit))
            {
                AddIllegalTranslation(unit);
                return;
            }

            UpdateElementTranslation(unit);
        }

        catch (Exception ex)
        {
            Debug.WriteLine($"Error while updating the value: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the text value of the element
    /// </summary>
    /// <param name="unit"></param>
    private void UpdateElementTranslation(TranslationUnit unit)
    {
        switch (unit.Element)
        {
            case Parameter param:
                param.Set(unit.TranslatedText);
                break;

            case ScheduleField field:
                SetScheduleField(unit, field);
                break;

            case TableSectionData tsd:
                tsd.SetCellText(unit.ScheduleCellCoordinates.Row, unit.ScheduleCellCoordinates.Column, unit.TranslatedText);
                break;

            case TextElement textElement:
                SetTextElementText(unit, textElement);
                break;

            case Dimension dim:
                SetDimensionText(unit, dim);
                break;

            case DimensionSegment dimSegment:
                SetDimensionSegmentText(unit, dimSegment);
                break;

            case Element element:
                if (unit.TranslationDetails == TranslationDetails.ElementName)
                {
                    SetElementName(unit, element);
                }
                break;

            case object _:
                break;
        }
    }
}
