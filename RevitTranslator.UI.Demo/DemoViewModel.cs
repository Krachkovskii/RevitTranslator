using CommunityToolkit.Mvvm.Input;
using RevitTranslator.Demo.Utils;
using RevitTranslator.Demo.ViewModels;
using RevitTranslator.UI.Views;

namespace RevitTranslator.Demo;

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
        
        view.Show();
    }
    
    [RelayCommand]
    private void ShowCategoriesWindow()
    {
        var vm = new MockCategoriesWindowViewModel();
        var view = new CategoriesWindow(vm);
        
        view.Show();
    }

    [RelayCommand]
    private void TestTranslation()
    {
        new MockTranslationPipeline().Execute();
    }
}