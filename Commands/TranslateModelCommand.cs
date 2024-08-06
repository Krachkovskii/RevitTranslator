using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Models;
using System.Windows;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;
using Autodesk.Revit.DB;

namespace RevitTranslatorAddin.Commands;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateModelCommand : ExternalCommand
{
    private TranslationUtils _utils = null;
    private Models.Settings _settings = null;
    public override void Execute()
    {
        if (RevitUtils.Doc != Document)
        {
            RevitUtils.SetUtils(UiApplication);
        }

#if NET8_0
        var res = RevitUtils.ShowNet8Warning();
        var action = RevitUtils.Net8WarningAction(res);

        if (!action)
        {
            return;
        }
#endif

        _settings = Models.Settings.LoadFromJson();

        if (!TranslationUtils.CanTranslate(_settings))
        {
            return;
        }

        // only allow elements of user-visible categories via ElementMulticategoryFilter
        var filter = CreateCategoryFilter();
        var instances = GetModelInstances(filter);
        int types = GetModelTypes(filter);

        var elementCount = CountTotalElements(instances.Count, types);
        var warningResult = ShowElementCountWarning(elementCount);

        if (warningResult.Equals(MessageBoxResult.No))
        {
            return;
        }

        _utils = new TranslationUtils(_settings);
        ProgressWindowUtils.Start();

        RevitUtils.ExEventHandler = new ElementUpdateHandler();
        RevitUtils.ExEvent = ExternalEvent.Create(RevitUtils.ExEventHandler);

        //var finished = _utils.StartTranslation(instances);
        var finishedTask = Task.Run(() => _utils.StartTranslationAsync(instances));
        var finished = finishedTask.GetAwaiter().GetResult();

        if (TranslationUtils.Translations.Count > 0)
        {
            if (!finished)
            {
                var proceed = TranslationUtils.ProceedWithUpdate();
                if (!proceed)
                {
                    return;
                }
            }
            RevitUtils.ExEvent.Raise();
            RevitUtils.SetTemporaryFocus();
        }
        ProgressWindowUtils.End();
    }

    private ElementMulticategoryFilter CreateCategoryFilter()
    {
        // only allow elements of user-visible categories via ElementMulticategoryFilter
        List<BuiltInCategory> categoryList = [];
        var categories = CategoriesModel.GetCategories();
        foreach (Category category in categories)
        {
            categoryList.Add(category.BuiltInCategory);
        }
        var filter = new ElementMulticategoryFilter(categoryList);

        return filter;
    }

    private List<ElementId> GetModelInstances(ElementMulticategoryFilter filter)
    {
        List<ElementId> instances = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .ToElementIds()
            .ToList();

        return instances;
    }

    private int GetModelTypes(ElementMulticategoryFilter filter)
    {
        int types = new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(filter)
            .WhereElementIsElementType()
            .GetElementCount();

        return types;
    }

    private int CountTotalElements(int instCount, int typesCount)
    {
        var rounded = (int)Math.Round((instCount + typesCount) / 100d) * 100;
        
        return rounded;
    }

    private bool ShowElementCountWarning(int count)
    {
        var result = MessageBox.Show($"You're about to translate all text properties of {count}+ elements. " +
                $"It can be time-consuming and you might hit translation limits.\n\n" +
                $"It will also translate elements such as view names or any annotations.\n" +
                $"Consider selecting only necessary categories.\n\n" +
                $"Are you sure you want to translate the whole model?",
                "Large number of translations!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) { return true; }
        else { return false; }
    }
}
