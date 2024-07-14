using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Models;
using System.Windows;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateModelCommand : ExternalCommand
{
    private TranslationUtils _utils = null;
    private Models.Settings _settings = null;
    private static ExternalEvent _exEvent = null;
    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetUtils(UiApplication);
        }

        _settings = Models.Settings.LoadFromJson();

        if (!TranslationUtils.CanTranslate(_settings))
        {
            return;
        }

        // only allow elements of user-visible categories
        List<BuiltInCategory> categoryList = [];
        var categories = CategoriesModel.GetCategories();
        foreach (Category category in categories)
        {
            categoryList.Add(category.BuiltInCategory);
        }
        var filter = new ElementMulticategoryFilter(categoryList);

        List<ElementId> instances = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .ToElementIds()
            .ToList();
        int types = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsElementType()
            .GetElementCount();

        var rounded = (int)Math.Round((instances.Count + types) / 100d) * 100;

        var result = MessageBox.Show($"You're about to translate all text properties of {rounded}+ elements. " +
            $"It can be time-consuming and you might hit translation limits.\n\n" +
            $"It will also translate elements such as view names or any annotations.\n" +
            $"Consider selecting only necessary categories.\n\n" +
            $"Are you sure you want to translate the whole model?", 
            "Large number of translations!", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No)
        {
            return;
        }

        _utils = new TranslationUtils(_settings);
        ProgressWindowUtils.Start();

        IExternalEventHandler handler = new ElementUpdateHandler();
        _exEvent = ExternalEvent.Create(handler);
        var finished = _utils.StartTranslation(instances);

        if (TranslationUtils.Translations.Count > 0)
        {
            _exEvent.Raise();
            RevitUtils.SetTemporaryFocus();
        }
        else
        {
            // shutting down the window ONLY in case if there are no translations, i.e. event is not triggered
            // otherwise, it's called from external event above
            ProgressWindowUtils.End();
        }
    }
}
