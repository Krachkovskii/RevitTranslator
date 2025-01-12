using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Enums;
using RevitTranslator.Models;
using RevitTranslator.Utils.App;

namespace RevitTranslator.Utils.Revit;

public class ModelUpdater
{
    private readonly List<string> _nonUpdatableElements = [];

    public void Update(List<DocumentTranslationEntityGroup> documentGroups)
    {
        foreach (var group in documentGroups)
        {
            UpdateDocument(group);
            if (group.Document.IsFamilyDocument)
            {
                group.Document.LoadFamilyToActiveDocument();
            }
        }

        StrongReferenceMessenger.Default.Send(new ModelUpdatedMessage());
    }

    private void UpdateDocument(DocumentTranslationEntityGroup group)
    {
        var title = group.Document.IsFamilyDocument ? "Translate Family Document" : "Translate Document";
        using var transaction = new Transaction(group.Document, title);
        transaction.Start();
        
        try
        {
            foreach (var unit in group.TranslationEntities)
            {
                UpdateUnit(unit);
            }

            if (_nonUpdatableElements.Count > 0)
            {
                ShowCantTranslateMessage(group.Document);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.RollBack();
        }
    }

    private void UpdateUnit(TranslationEntity entity)
    {
        if (entity.TranslatedText == string.Empty 
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
                tsd.SetCellText(entity.ScheduleCellCoordinates!.Row, entity.ScheduleCellCoordinates.Column, entity.TranslatedText);
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
        }
    }

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

    private void AddIllegalTranslation(TranslationEntity entity)
    {
        _nonUpdatableElements.Add($"{entity.TranslatedText} " +
                        $"(Symbol: \"{entity.TranslatedText.FirstOrDefault(c => ValidationUtils.ForbiddenParameterSymbols.Contains(c))}\", " +
                        $"ElementId: {entity.ElementId})");
    }

    private void ShowCantTranslateMessage(Document doc)
    {
        MessageBox.Show($"Some translations in document \"{doc.Title}\" weren't updated due to forbidden symbols: \n{string.Join("\n", _nonUpdatableElements)}.",
                                        "Warning",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
    }
}
