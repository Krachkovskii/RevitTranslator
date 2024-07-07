﻿using System.Windows;
using Autodesk.Revit.UI;

namespace RevitTranslatorAddin.Utils;

public class RevitElementUpdateHandler : IExternalEventHandler
{
    /// <summary>
    /// This handler updates all elements in the active document after all translations have been completed.
    /// After all element have been updated, it calls ```TranslationUtils.ClearTranslationCount()``` method.
    /// </summary>
    public void Execute(UIApplication app)
    {
        List<string> _cantTranslate = [];

        ProgressWindowUtils.RevitUpdate();
        
        using (Transaction t = new Transaction(app.ActiveUIDocument.Document, "Update Element Translations"))
        {
            t.Start();
            try
            {
                foreach (var triple in TranslationUtils.Translations)
                {
                    if (triple.Item1 == null)
                    {
                        continue;
                    }

                    if (triple.Item2.Any(c => RevitUtils.ForbiddenSymbols.Contains(c)))
                    {
                        _cantTranslate.Add($"{triple.Item2} (Id: {triple.Item4})");
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

                        case ViewSchedule schedule:
                            schedule.Name = triple.Item2;
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
                        case object _:
                            continue;
                    }
                }

                if (_cantTranslate.Count > 0)
                {
                    MessageBox.Show($"The following translations couldn't be applied due to forbidden symbols: {string.Join(", ", _cantTranslate)}.",
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

            TranslationUtils.ClearTranslationCount();
        };
    }

    public string GetName()
    {
        return "Element Updater";
    }
}
