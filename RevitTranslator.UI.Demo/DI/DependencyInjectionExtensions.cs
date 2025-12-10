using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Demo.ViewModels;
using RevitTranslator.UI.Contracts;
using RevitTranslator.UI.Views;

namespace RevitTranslator.Demo.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMockUiImpl(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<ICategoriesWindowViewModel, MockCategoriesWindowViewModel>()
            .AddScoped<IProgressWindowViewModel, MockProgressWindowViewModel>()
            .AddScoped<ISettingsViewModel, MockSettingsViewModel>()
            .AddScoped<DemoWindow>()
            .AddScoped<DemoViewModel>();
        
        return serviceCollection;
    }
}