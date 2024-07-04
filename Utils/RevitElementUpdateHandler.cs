using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTranslatorAddin.Utils;

public class RevitElementUpdateHandler : IExternalEventHandler
{
    public void Execute(UIApplication app)
    {
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
                    }
                }

                TranslationUtils.ClearTranslationCount();
                t.Commit();
            }
            catch { t.RollBack(); }
        };
    }

    public string GetName()
    {
        return "Element Updater";
    }
}
