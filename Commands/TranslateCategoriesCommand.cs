using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Models;
using RevitTranslatorAddin.Utils;
using RevitTranslatorAddin.ViewModels;
using RevitTranslatorAddin.Views;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
internal class TranslateCategoriesCommand : ExternalCommand
{
    private TranslationUtils _utils = null;
    private Models.Settings _settings = null;
    internal static ExternalEvent TranslateCategoriesExtrnalEvent = null;
    private List<ElementId> _elements = [];
    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetUtils(UiApplication);
        }

        _settings = Models.Settings.LoadFromJson();

        if (!TranslationUtils.CanTranslate(_settings))
        {
            TaskDialog.Show("Error", "Set up API key and target language.");
            return;
        }

        var utils = new TranslationUtils(_settings);
        _utils = utils;

        // initialising a class to fill static property that contains all categories
        new CategoriesModel();
        var viewModel = new TranslateCategoriesViewModel(utils);
        var settingsWindow = new TranslateCategoriesWindow(viewModel);
        settingsWindow.Show();

        IExternalEventHandler handler = new RevitElementUpdateHandler();
        TranslateCategoriesExtrnalEvent = ExternalEvent.Create(handler);

        _utils.StartTranslation(_elements);   
    }

    internal static List<ElementId> GetElementsFromCategories(List<BuiltInCategory> categories)
    {
        var filter = new ElementMulticategoryFilter(categories);
        var elements = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .ToElementIds()
            .ToList();

        return elements;
    }
}