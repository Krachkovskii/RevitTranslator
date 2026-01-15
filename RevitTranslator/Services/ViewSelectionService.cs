using RevitTranslator.Common.Models.Views;
using RevitTranslator.Extensions;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.UI.Views;

namespace RevitTranslator.Services;

public class ViewSelectionService(ViewsWindow window, ViewsViewModel viewModel)
{
    public Element[] SelectedElements { get; private set; } = [];

    public bool Initialize()
    {
        var result = window.ShowDialog();
        if (result is false) return false;

        IEnumerable<Element> elements = new List<Element>();
        var document = Context.ActiveDocument;
        if (document is null) return false;

        var sheets = viewModel.ViewTypes
            .SelectMany(viewType => viewType.Views)
            .Where(view => view.IsChecked)
            .Select(view => view.Model
                .Id
                .ToElementId()
                .ToElement<View>(document))
            .OfType<ViewSheet>()
            .ToArray();

        if (sheets.Length == 0) return false;

        var viewTypes = new List<Type>
        {
            typeof(ViewPlan),
            typeof(View3D),
            typeof(ViewSheet),
            typeof(ViewDrafting),
            typeof(ViewSection)
        };

        foreach (var sheet in sheets)
        {
            var sheetElements = new FilteredElementCollector(document, sheet.Id)
                .WherePasses(new ElementMulticlassFilter(viewTypes, true))
                .ToElements();

            var sheetViewsElements = sheet
                .GetAllPlacedViews()
                .ToElements<View>(document)
                .Select(v => document.EnumerateInstances(v.Id))
                .SelectMany(instance => instance);

            // TODO: Ignore some categories, e.g. detail line. Might be user-adjustable from settings
            elements = elements
                .Concat([sheet])
                .Concat(sheetElements)
                .Concat(sheetViewsElements);
        }

        SelectedElements = elements.Distinct().ToArray();
        return true;
    }
}