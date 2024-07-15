using System.Windows;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Utils.DeepL;
using static Autodesk.Revit.DB.SpecTypeId;

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

        ProgressWindowUtils.RevitUpdate();

        using (var t = new Transaction(app.ActiveUIDocument.Document, "Update Element Translations"))
        {
            t.Start();
            try
            {
                foreach (var triple in TranslationUtils.Translations)
                {
                    try
                    {
                        if (triple.Item1 == null)
                        {
                            continue;
                        }

                        if (triple.Item2.Any(c => RevitUtils.ForbiddenSymbols.Contains(c)))
                        {
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
                                continue;
                        }
                    }
                    catch { }
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

        TranslationUtils.ClearTranslationCount();
        ProgressWindowUtils.End();
    }

    public string GetName()
    {
        return "Element Updater";
    }
}
