using System.Windows;
using Autodesk.Revit.UI;
using RevitTranslator.Models;

namespace RevitTranslator.Utils.Revit;

/// <summary>
/// This class handles update of elements in the model using Transaction
/// </summary>
public class ModelUpdateHandler : IExternalEventHandler
{
    /// <summary>
    /// List of translations that can't be applied due to illegal characters
    /// </summary>
    private readonly List<string> _cantUpdate = [];

    public static List<DocumentTranslationEntityGroup> TranslationUnitGroups { get; set; } = [];

    /// <summary>
    /// Main entry point for Revit event.
    /// </summary>
    /// <param name="app"></param>
    public void Execute(UIApplication app)
    {
        foreach (var group in TranslationUnitGroups)
        {
            UpdateDocumentTranslations(group);
            if (group.Document.IsFamilyDocument)
            {
                group.Document.LoadFamilyToActiveDocument();
            }
        }
    }

    public string GetName()
    {
        return "Element Updater";
    }

    private void UpdateDocumentTranslations(DocumentTranslationEntityGroup group)
    {
        using var transaction = new Transaction(group.Document, $"Update Document {group.Document.Title}");
        transaction.Start();
        
        try
        {
            foreach (var unit in group.TranslationEntities)
            {
                UpdateElement(unit);
            }

            if (_cantUpdate.Count > 0)
            {
                ShowCantTranslateMessage(group.Document);
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            ShowTransactionErrorMessage(ex);
            transaction.RollBack();
        }
    }

    private void UpdateElement(TranslationEntity entity)
    {
        if (entity.Element == null 
            || entity.TranslatedText == string.Empty 
            || entity.TranslatedText == entity.OriginalText) return;

        if (entity.NameHasIllegalCharacters())
        {
            AddIllegalTranslation(entity);
            return;
        }

        UpdateElementTranslation(entity);
    }

    private void UpdateElementTranslation(TranslationEntity entity)
    {
        switch (entity.Element)
        {
            case Parameter param:
                param.Set(entity.TranslatedText);
                break;

            case ScheduleField field:
                field.ColumnHeading = entity.TranslatedText;
                break;

            case TableSectionData tsd:
                tsd.SetCellText(entity.ScheduleCellCoordinates.Row, entity.ScheduleCellCoordinates.Column, entity.TranslatedText);
                break;

            case TextElement textElement:
                textElement.Text = entity.TranslatedText;
                break;

            case Dimension dim:
                SetDimensionText(entity, dim);
                break;

            case DimensionSegment dimSegment:
                SetDimensionSegmentText(entity, dimSegment);
                break;

            case Element element:
                if (entity.TranslationDetails == TranslationDetails.ElementName)
                {
                    element.Name = entity.TranslatedText;
                }
                break;

            case not null:
                break;
        }
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
    /// <param name="entity"></param>
    private void AddIllegalTranslation(TranslationEntity entity)
    {
        _cantUpdate.Add($"{entity.TranslatedText} " +
            $"(Symbol: \"{entity.TranslatedText.FirstOrDefault(c => ValidationUtils.ForbiddenParameterSymbols.Contains(c))}\", " +
            $"ElementId: {entity.ElementId})");
    }

    /// <summary>
    /// Updates overrides of DimensionSegment element
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dim"></param>
    private void SetDimensionSegmentText(TranslationEntity entity, DimensionSegment dim)
    {
        switch (entity.TranslationDetails)
        {
            case TranslationDetails.DimensionAbove:
                dim.Above = entity.TranslatedText;
                break;
            case TranslationDetails.DimensionBelow:
                dim.Below = entity.TranslatedText;
                break;
            case TranslationDetails.DimensionPrefix:
                dim.Prefix = entity.TranslatedText;
                break;
            case TranslationDetails.DimensionSuffix:
                dim.Suffix = entity.TranslatedText;
                break;
            case TranslationDetails.DimensionOverride:
                dim.ValueOverride = entity.TranslatedText;
                break;
        }
    }

    /// <summary>
    /// Updates overrides of Dimension element
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dim"></param>
    private void SetDimensionText(TranslationEntity entity, Dimension dim)
    {
        switch (entity.TranslationDetails)
        {
            case TranslationDetails.DimensionAbove:
                dim.Above = entity.TranslatedText;
                break;
            case TranslationDetails.DimensionBelow:
                dim.Below = entity.TranslatedText;
                break;
            case TranslationDetails.DimensionPrefix:
                dim.Prefix = entity.TranslatedText;
                break;
            case TranslationDetails.DimensionSuffix:
                dim.Suffix = entity.TranslatedText;
                break;
            case TranslationDetails.DimensionOverride:
                dim.ValueOverride = entity.TranslatedText;
                break;
        }
    }


    private void ShowCantTranslateMessage(Document doc)
    {
        MessageBox.Show($"These translations in document \"{doc.Title}\" weren't updated due to forbidden symbols: \n{string.Join("\n", _cantUpdate)}.",
                                        "Warning",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
    }
}
