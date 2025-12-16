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

        IEnumerable<Element> test = new List<Element>();
        var document = Context.ActiveDocument;
        if (document is null) return false;

        var views = viewModel.ViewTypes
            .SelectMany(viewType => viewType.Views)
            .Where(view => view.IsChecked)
            .Select(view => view.Model
                .Id
                .ToElementId()
                .ToElement<View>(document))
            .Cast<View>()
            .ToArray();
        
        if (views.Length == 0) return false;
        
        var viewTypes = new List<Type>
        {
            typeof(ViewPlan),
            typeof(View3D),
            typeof(ViewSheet),
            typeof(ViewDrafting),
            typeof(ViewSection)
        };

        foreach (var view in views)
        {
            if (view is ViewSheet sheet)
            {
                var sheetElements = new FilteredElementCollector(document, sheet.Id)
                    .WherePasses(new ElementMulticlassFilter(viewTypes, true))
                    .ToElements();
                
                var elements = sheet
                    .GetAllPlacedViews()
                    .ToElements<View>(document)
                    .Select(v => document.EnumerateInstances(v.Id))
                    .SelectMany(instance => instance)
                    .Concat(sheetElements)
                    .Concat([sheet]);

                test = test.Concat(elements);
            }
            else
            {
                var elements = document
                    .EnumerateInstances(view.Id)
                    .Concat([view]);
                
                test = test.Concat(elements);
            }
        }

        SelectedElements = test.Distinct().ToArray();
        return true;
    }
}