using CommunityToolkit.Mvvm.Input;
using RevitTranslator.UI.Demo.Utils;
using RevitTranslator.UI.Demo.ViewModels;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.UI.Views;

namespace RevitTranslator.UI.Demo;

public partial class DemoViewModel
{
    [RelayCommand]
    private void ShowProgressWindow()
    {
        var vm = new MockProgressWindowViewModel();
        var view = new ProgressWindow(vm);

        view.Show();
    }

    [RelayCommand]
    private void ShowSettingsWindow()
    {
        var vm = new MockSettingsViewModel();
        var view = new SettingsWindow(vm);

        view.ShowDialog();
    }

    [RelayCommand]
    private void ShowCategoriesWindow()
    {
        var vm = new MockCategoriesWindowViewModel();
        var view = new CategoriesWindow(vm);

        view.Show();
    }

    [RelayCommand]
    private void ShowViewsWindow()
    {
        var vm = new SheetsViewModel(new MockRevitViewProvider());
        var view = new SheetsWindow(vm);

        view.ShowDialog();
    }

    [RelayCommand]
    private void TestTranslation()
    {
        new MockTranslationPipeline().Execute();
    }
}