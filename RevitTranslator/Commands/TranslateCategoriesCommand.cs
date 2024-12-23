﻿using RevitTranslator.Models;
using RevitTranslator.Views;
using RevitTranslatorAddin.Models;
using RevitTranslatorAddin.Utils.Revit;
using CategoriesViewModel = RevitTranslator.ViewModels.CategoriesViewModel;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
public class TranslateCategoriesCommand : BaseTranslationCommand
{
    /// <summary>
    /// Window that displays all available categories and allows user to select all necessary categories
    /// </summary>
    public static CategoriesWindow Window { get; set; } = null;

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
        
        RevitUtils.CreateAndAssignEvents();

        var categoryDescriptors = new CategoriesModel()
            .AllValidCategories
            .Select(category => new CategoryDescriptor
            {
#if REVIT2025_OR_GREATER
                Id = category.Id.Value,
#else
                Id = category.Id.IntegerValue,
#endif
                Name = category.Name,
                Type = category.CategoryType.ToString(),
                IsChecked = false,
                IsBuiltInCategory = category.BuiltInCategory != BuiltInCategory.INVALID
            })
            .ToList();

        var viewModel = new CategoriesViewModel(_translationUtils, _progressWindowUtils, categoryDescriptors);
        Window = new CategoriesWindow(viewModel);
        Window.Show();
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
    public static List<Element> GetElementsFromCategories(List<BuiltInCategory> categories)
    {
        return new FilteredElementCollector(RevitUtils.Doc)
            .WherePasses(new ElementMulticategoryFilter(categories))
            .ToList();
    }
}