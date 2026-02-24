using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.Contracts;
using RevitTranslator.UI.Contracts;
using RevitTranslator.UI.Demo.Utils;
using RevitTranslator.UI.Demo.ViewModels;
using RevitTranslator.UI.ViewModels;
using TranslationService.Utils;

namespace RevitTranslator.UI.Demo.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMockUiImpl(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<ICategoriesWindowViewModel, MockCategoriesWindowViewModel>()
            .AddScoped<SettingsViewModel>()
            .AddScoped<IRevitViewProvider, MockRevitViewProvider>()
            .AddScoped<DemoWindow>()
            .AddScoped<DemoViewModel>()
            .AddSingleton(_ => new HttpClient())
            .AddSingleton<DeeplTranslationClient>();

        return serviceCollection;
    }
}