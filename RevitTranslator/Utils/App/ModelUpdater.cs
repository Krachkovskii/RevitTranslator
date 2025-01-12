using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Enums;
using RevitTranslator.Extensions;
using RevitTranslator.Models;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.Utils.App;

public class ModelUpdater
{
    private readonly List<string> _nonUpdatableElements = [];
    private TranslationEntity _currentEntity = null!;

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
            foreach (var entity in group.TranslationEntities)
            {
                _currentEntity = entity;
                ProcessEntity();
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

    private void ProcessEntity()
    {
        if (!_currentEntity.IsTranslated()) return;
        if (_currentEntity.NameHasIllegalCharacters())
        {
            _nonUpdatableElements.Add($"{_currentEntity.TranslatedText} " +
                                      $"(Symbol: \"{_currentEntity.TranslatedText
                                          .FirstOrDefault(c => ValidationUtils.ForbiddenParameterSymbols.Contains(c))}\", " +
                                      $"ElementId: {_currentEntity.ElementId})");
            return;
        }

        switch (_currentEntity.Element)
        {
            case Parameter param:
                param.Set(_currentEntity.TranslatedText);
                break;

            case ScheduleField field:
                field.ColumnHeading = _currentEntity.TranslatedText;
                break;

            case TableSectionData tsd:
                tsd.SetCellText(_currentEntity.ScheduleCellCoordinates!.Row, _currentEntity.ScheduleCellCoordinates.Column, _currentEntity.TranslatedText);
                break;

            case TextElement textElement:
                textElement.Text = _currentEntity.TranslatedText;
                break;

            case Dimension dim:
                SetDimensionText(dim);
                break;

            case DimensionSegment dimSegment:
                SetDimensionSegmentText(dimSegment);
                break;

            case Element element:
                if (_currentEntity.TranslationDetails == TranslationDetails.ElementName)
                {
                    element.Name = _currentEntity.TranslatedText;
                }
                break;
        }
    }

    private void SetDimensionSegmentText(DimensionSegment dim)
    {
        switch (_currentEntity.TranslationDetails)
        {
            case TranslationDetails.DimensionAbove:
                dim.Above = _currentEntity.TranslatedText;
                break;
            
            case TranslationDetails.DimensionBelow:
                dim.Below = _currentEntity.TranslatedText;
                break;
            
            case TranslationDetails.DimensionPrefix:
                dim.Prefix = _currentEntity.TranslatedText;
                break;
            
            case TranslationDetails.DimensionSuffix:
                dim.Suffix = _currentEntity.TranslatedText;
                break;
            
            case TranslationDetails.DimensionOverride:
                dim.ValueOverride = _currentEntity.TranslatedText;
                break;
        }
    }

    private void SetDimensionText(Dimension dim)
    {
        switch (_currentEntity.TranslationDetails)
        {
            case TranslationDetails.DimensionAbove:
                dim.Above = _currentEntity.TranslatedText;
                break;
            
            case TranslationDetails.DimensionBelow:
                dim.Below = _currentEntity.TranslatedText;
                break;
            
            case TranslationDetails.DimensionPrefix:
                dim.Prefix = _currentEntity.TranslatedText;
                break;
            
            case TranslationDetails.DimensionSuffix:
                dim.Suffix = _currentEntity.TranslatedText;
                break;
            
            case TranslationDetails.DimensionOverride:
                dim.ValueOverride = _currentEntity.TranslatedText;
                break;
        }
    }

    private void ShowCantTranslateMessage(Document doc)
    {
        MessageBox.Show($"Some translations in document \"{doc.Title}\" weren't updated due to forbidden symbols: \n{string.Join("\n", _nonUpdatableElements)}.",
                                        "Warning",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
    }
}
