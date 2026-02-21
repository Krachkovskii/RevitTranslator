using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.Contracts;
using RevitTranslator.UI.Contracts;
using RevitTranslator.UI.Demo.Utils;
using RevitTranslator.UI.Demo.ViewModels;

namespace RevitTranslator.UI.Demo.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMockUiImpl(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<ICategoriesWindowViewModel, MockCategoriesWindowViewModel>()
            .AddScoped<IProgressWindowViewModel, MockProgressWindowViewModel>()
            .AddScoped<ISettingsViewModel, MockSettingsViewModel>()
            .AddScoped<IRevitViewProvider, MockRevitViewProvider>()
            .AddScoped<DemoWindow>()
            .AddScoped<DemoViewModel>();

        return serviceCollection;
    }
}