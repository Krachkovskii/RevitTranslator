using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Services;
using RevitTranslator.UI.Views;
using CategoriesViewModel = RevitTranslator.ViewModels.CategoriesViewModel;

namespace RevitTranslator.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class TranslateCategoriesCommand : ExternalCommand
{
    public override void Execute()
    {
        var viewModel = new CategoriesViewModel();
        var view = new CategoriesWindow(viewModel);
        view.ShowDialog();

        if (viewModel.SelectedCategories.Count == 0) return;

        var service = new CategoryTranslationService();
        service.SelectedCategories = viewModel.SelectedCategories.ToList();
        service.Execute();
    }
}