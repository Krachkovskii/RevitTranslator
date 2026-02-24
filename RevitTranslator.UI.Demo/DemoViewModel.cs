using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.UI.Contracts;
using RevitTranslator.UI.Demo.Utils;
using RevitTranslator.UI.Demo.ViewModels;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.UI.Views;
using TranslationService.Utils;

namespace RevitTranslator.UI.Demo;

public partial class DemoViewModel(
    DeeplTranslationClient client)
{
    [RelayCommand]
    private async Task ShowProgressWindowAsync()
    {
        using var scope = Host.ServiceProvider.CreateScope();
        var progressWindow = scope.ServiceProvider.GetRequiredService<ProgressWindow>();
        await new MockTranslationPipeline(progressWindow, client).ExecuteAsync(useMockTranslations: true);
    }

    [RelayCommand]
    private void ShowSettingsWindow()
    {
        using var scope = Host.ServiceProvider.CreateScope();
        var settingsWindow = scope.ServiceProvider.GetRequiredService<SettingsWindow>();
       settingsWindow.ShowDialog();
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
    private async Task TestTranslationAsync()
    {
        using var scope = Host.ServiceProvider.CreateScope();
        var progressWindow = scope.ServiceProvider.GetRequiredService<ProgressWindow>();
        await new MockTranslationPipeline(progressWindow, client).ExecuteAsync();
    }
}
