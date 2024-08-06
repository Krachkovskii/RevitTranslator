using System.Diagnostics;
using System.Windows;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.ViewModels;

namespace RevitTranslatorAddin.Utils.Revit;

public class ElementUpdateHandler : IExternalEventHandler
{
    private List<string> _cantUpdate = [];
    /// <summary>
    /// This handler updates all elements in the active document after all translations have been completed.
    /// After all element have been updated, it calls ```TranslationUtils.ClearTranslationCount()``` method.
    /// </summary>
    public void Execute(UIApplication app)
    {
        ProgressWindowUtils.PW.Dispatcher.Invoke(() => ProgressWindowUtils.VM.UpdateStarted());

        using (var t = new Transaction(app.ActiveUIDocument.Document, "Update Element Translations"))
        {
            t.Start();
            try
            {
                foreach (var unit in TranslationUtils.Translations)
                {
                    try
                    {
                        if (unit.Element == null)
                        {
                            continue;
                        }

                        if (unit.TranslatedText.Any(c => RevitUtils.ForbiddenSymbols.Contains(c)))
                        {
                            AddUntranslatableSymbol(unit);
                            continue;
                        }

                        UpdateElementTranslation(unit);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error while updating the value: {ex.Message}");
                    }
                }

                if (_cantUpdate.Count > 0)
                {
                    MessageBox.Show($"The following translations couldn't be applied due to forbidden symbols: \n{string.Join("\n", _cantUpdate)}.",
                                        "Warning",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                }

                t.Commit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                    "Error updating elements",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                t.RollBack();
            }
        };

        ProgressWindowUtils.PW.Dispatcher.Invoke(() => ProgressWindowUtils.VM.UpdateFinished());
        ClearEvents();
    }

    public string GetName()
    {
        return "Element Updater";
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

    private void SetElementName(TranslationUnit unit, Element element)
    {
        element.Name = unit.TranslatedText;
    }

    private void AddUntranslatableSymbol(TranslationUnit unit)
    {
        _cantUpdate.Add($"{unit.TranslatedText} (Symbol: \"{unit.TranslatedText.FirstOrDefault(c => RevitUtils.ForbiddenSymbols.Contains(c))}\", ElementId: {unit.ElementId})");
    }

    private void ClearEvents()
    {
        RevitUtils.ExEvent = null;
        RevitUtils.ExEventHandler = null;
    }
}
