using System.Diagnostics;
using System.Windows;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.App;

namespace RevitTranslatorAddin.Utils.Revit;

public class ElementUpdateHandler : IExternalEventHandler
{
    internal static List<TranslationUnit> TranslationUnits { get; set; } = [];
    internal static ProgressWindowUtils ProgressWindowUtils { get; set; } = null;
    private readonly List<string> _cantUpdate = [];
    public void Execute(UIApplication app)
    {
        ProgressWindowUtils.PW.Dispatcher.Invoke(() => ProgressWindowUtils.VM.UpdateStarted());

        using (var t = new Transaction(app.ActiveUIDocument.Document, "Update Element Translations"))
        {
            t.Start();
            try
            {
                foreach (var unit in TranslationUnits)
                {
                    UpdateElement(unit);
                }

                if (_cantUpdate.Count > 0)
                {
                    ShowCantTranslateMessage();
                }

                t.Commit();
            }
            catch (Exception ex)
            {
                ShowTransactionErrorMessage(ex);
                t.RollBack();
            }
        };

        FinalizeUpdate();
    }

    public string GetName()
    {
        return "Element Updater";
    }

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

            if (unit.TranslatedText.Any(c => RevitUtils.ForbiddenSymbols.Contains(c)))
            {
                AddUntranslatableSymbol(unit);
                return;
            }

            UpdateElementTranslation(unit);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error while updating the value: {ex.Message}");
        }
    }

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
                tsd.SetCellText(unit.TableSectionCoordinates.Row, unit.TableSectionCoordinates.Column, unit.TranslatedText);
                break;

            case TextNote textNote:
                SetTextNoteText(unit, textNote);
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

    private void SetScheduleField(TranslationUnit unit, ScheduleField field)
    {
        field.ColumnHeading = unit.TranslatedText;
    }

    private void SetTextNoteText(TranslationUnit unit, TextNote textNote)
    {
        textNote.Text = unit.TranslatedText;
    }

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

    private void SetElementName(TranslationUnit unit, Element element)
    {
        element.Name = unit.TranslatedText;
    }

    private void AddUntranslatableSymbol(TranslationUnit unit)
    {
        _cantUpdate.Add($"{unit.TranslatedText} " +
            $"(Symbol: \"{unit.TranslatedText.FirstOrDefault(c => RevitUtils.ForbiddenSymbols.Contains(c))}\", " +
            $"ElementId: {unit.ElementId})");
    }

    private void ShowCantTranslateMessage()
    {
        MessageBox.Show($"These values weren't applied due to forbidden symbols: \n{string.Join("\n", _cantUpdate)}.",
                                        "Warning",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
    }

    private void ShowTransactionErrorMessage(Exception ex)
    {
        MessageBox.Show(ex.Message,
                        "Error updating elements",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
    }

    private void FinalizeUpdate()
    {
        ProgressWindowUtils.PW.Dispatcher.Invoke(() => ProgressWindowUtils.VM.UpdateFinished());

        TranslationUnits.Clear();
        _cantUpdate.Clear();
        ProgressWindowUtils = null;

        RevitUtils.ExEvent = null;
        RevitUtils.ExEventHandler = null;
    }
}
