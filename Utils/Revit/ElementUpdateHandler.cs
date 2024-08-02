using System.Diagnostics;
using System.Windows;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.ViewModels;

namespace RevitTranslatorAddin.Utils.Revit;

public class ElementUpdateHandler : IExternalEventHandler
{
    /// <summary>
    /// This handler updates all elements in the active document after all translations have been completed.
    /// After all element have been updated, it calls ```TranslationUtils.ClearTranslationCount()``` method.
    /// </summary>
    public void Execute(UIApplication app)
    {
        List<string> _cantTranslate = [];

        ProgressWindowUtils.PW.Dispatcher.Invoke(() => ProgressWindowUtils.VM.UpdateStarted());

        using (var t = new Transaction(app.ActiveUIDocument.Document, "Update Element Translations"))
        {
            t.Start();
            // Catching any transaction-related errors.
            try
            {
                foreach (var unit in TranslationUtils.Translations)
                {
                    // catching any errors related to element updates. If raised, simply go to the next element.
                    try
                    {
                        // triple.Item1 contains an element (parameter, property, or element itself) that will be updated
                        if (unit.Element == null)
                        {
                            continue;
                        }

                        // unit.TranslatedText contains translation of text to be updated
                        if (unit.TranslatedText.Any(c => RevitUtils.ForbiddenSymbols.Contains(c)))
                        {
                            // triple.Item4 contains ElementId of an element
                            _cantTranslate.Add($"{unit.TranslatedText} (Symbol: \"{unit.TranslatedText.FirstOrDefault(c => RevitUtils.ForbiddenSymbols.Contains(c))}\", ElementId: {unit.ElementId})");
                            continue;
                        }

                        switch (unit.Element)
                        {
                            case Parameter param:
                                param.Set(unit.TranslatedText);
                                break;

                            case ScheduleField field:
                                field.ColumnHeading = unit.TranslatedText;
                                break;

                            case TableSectionData tsd:
                                tsd.SetCellText(unit.TableSectionCoordinates.Row, unit.TableSectionCoordinates.Column, unit.TranslatedText);
                                break;

                            case TextNote textNote:
                                textNote.Text = unit.TranslatedText;
                                break;

                            case Dimension dim:
                                // triple.Item3 contains additional comments on what part of the element will be updated
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
                                break;

                            case Element element:
                                if (unit.TranslationDetails == TranslationDetails.ElementName)
                                {
                                    element.Name = unit.TranslatedText;
                                }
                                break;

                            case object _:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error while updating the value: {ex.Message}");
                    }
                }

                if (_cantTranslate.Count > 0)
                {
                    MessageBox.Show($"The following translations couldn't be applied due to forbidden symbols: \n{string.Join("\n", _cantTranslate)}.",
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

        RevitUtils.ExEvent = null;
        RevitUtils.ExEventHandler = null;
    }

    public string GetName()
    {
        return "Element Updater";
    }
}
