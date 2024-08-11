using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Models;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.ViewModels;
using RevitTranslatorAddin.Views;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
internal class TranslateCategoriesCommand : ExternalCommand
{
    private TranslationUtils _translationUtils = null;
    private ProgressWindowUtils _progressWindowUtils = null;
    private Models.Settings _settings = null;
    internal static TranslateCategoriesWindow Window { get; set; } = null;

    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetUtils(UiApplication);
        }

        CreateAndSetUtils();

        if (!_translationUtils.CanTranslate(_settings))
        {
            return;
        }

        // initialising a class to fill static property that contains all categories
        new CategoriesModel();

        var viewModel = new TranslateCategoriesViewModel(_translationUtils, _progressWindowUtils);
        Window = new TranslateCategoriesWindow(viewModel);
        Window.Show();

        RevitUtils.ExEventHandler = new ElementUpdateHandler();
        RevitUtils.ExEvent = ExternalEvent.Create(RevitUtils.ExEventHandler);
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
    private void CreateAndSetUtils()
    {
        _settings = Models.Settings.LoadFromJson();
        _progressWindowUtils = new ProgressWindowUtils();
        ElementUpdateHandler.ProgressWindowUtils = _progressWindowUtils;
        _translationUtils = new TranslationUtils(_settings, _progressWindowUtils);
        _progressWindowUtils.TranslationUtils = _translationUtils;
    }
}