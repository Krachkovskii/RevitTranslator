using RevitTranslatorAddin.Models;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.ViewModels;
using RevitTranslatorAddin.Views;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
internal class TranslateCategoriesCommand : BaseCommand
{
    /// <summary>
    /// Window that displays all available categories and allows user to select all necessary categories
    /// </summary>
    internal static TranslateCategoriesWindow Window { get; set; } = null;

    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetRevitUtils(UiApplication);
        }

        CreateAndSetUtils();

        if (!_translationUtils.CanTranslate(_settings))
        {
            return;
        }

        // initializing a class to fill static property that contains all categories
        new CategoriesModel();

        var viewModel = new TranslateCategoriesViewModel(_translationUtils, _progressWindowUtils);
        Window = new TranslateCategoriesWindow(viewModel);
        Window.Show();

        RevitUtils.CreateAndAssignEvents();
    }

    /// <summary>
    /// Gets all elements from selected categories
    /// </summary>
    /// <param name="categories">
    /// Categories to select elements
    /// </param>
    /// <returns>
    /// List of ElementIds
    /// </returns>
    internal static List<Element> GetElementsFromCategories(List<BuiltInCategory> categories)
    {
        var filter = new ElementMulticategoryFilter(categories);
        var elements = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .ToList();

        return elements;
    }
}