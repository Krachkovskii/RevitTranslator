using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
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
    }
}