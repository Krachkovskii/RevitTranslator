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
                foreach (var triple in TranslationUtils.Translations)
                {
                    // catching any errors related to element updates. If raised, simply go to the next element.
                    try
                    {
                        // triple.Item1 contains an element (parameter, property, or element itself) that will be updated
                        if (triple.Item1 == null)
                        {
                            continue;
                        }

                        // triple.Item2 contains translation of text to be updated
                        if (triple.Item2.Any(c => RevitUtils.ForbiddenSymbols.Contains(c)))
                        {
                            // triple.Item4 contains ElementId of an element
                            _cantTranslate.Add($"{triple.Item2} (Symbol: \"{triple.Item2.FirstOrDefault(c => RevitUtils.ForbiddenSymbols.Contains(c))}\", ElementId: {triple.Item4})");
                            continue;
                        }

                        switch (triple.Item1)
                        {
                            case Parameter param:
                                param.Set(triple.Item2);
                                break;

                            case ScheduleField field:
                                field.ColumnHeading = triple.Item2;
                                break;

                            case TableSectionData tsd:
                                var values = triple.Item3.Split(',').ToList();
                                var row = int.Parse(values[0]);
                                var column = int.Parse(values[1]);
                                tsd.SetCellText(row, column, triple.Item2);
                                break;

                            case TextNote textNote:
                                textNote.Text = triple.Item2;
                                break;

                            case Dimension dim:
                                // triple.Item3 contains additional comments on what part of the element will be updated
                                switch (triple.Item3)
                                {
                                    case "above":
                                        dim.Above = triple.Item2;
                                        break;
                                    case "below":
                                        dim.Below = triple.Item2;
                                        break;
                                    case "prefix":
                                        dim.Prefix = triple.Item2;
                                        break;
                                    case "suffix":
                                        dim.Suffix = triple.Item2;
                                        break;
                                    case "value":
                                        dim.ValueOverride = triple.Item2;
                                        break;
                                }
                                break;

                            case Element element:
                                if (triple.Item3 == "name")
                                {
                                    element.Name = triple.Item2;
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
        //TranslationUtils.ClearTranslationCount();
        //ProgressWindowUtils.End();
    }

    public string GetName()
    {
        return "Element Updater";
    }
}
