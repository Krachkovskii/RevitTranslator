using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Revit.Core.Enums;
using RevitTranslator.Revit.Core.Extensions;
using RevitTranslator.Revit.Core.Models;
using RevitTranslator.Revit.Core.Utils;

namespace RevitTranslator.Revit.Core.Services;

public class ModelUpdaterService(ITranslationProgressMonitor progressMonitor)
{
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

        progressMonitor.OnModelUpdated();
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
            _currentEntity.IllegalCharacter = _currentEntity.TranslatedText
                .FirstOrDefault(c => ValidationUtils.ForbiddenParameterSymbols.Contains(c));
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

        _currentEntity.UpdatedInModel = true;
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
}
