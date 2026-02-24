using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.Contracts;
using RevitTranslator.Services;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.Utils;
using System.Net.Http;
using RevitTranslator.UI.Contracts;
using RevitTranslator.ViewModels;
using TranslationService.Utils;

namespace RevitTranslator.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRevitUiImpl(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<ICategoriesWindowViewModel, CategoriesWindowViewModel>()
            .AddScoped<SettingsViewModel>();
    }

    public static IServiceCollection AddRevitServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<TranslationManager>()
            .AddScoped<ConcurrentTranslationService>()
            .AddScoped<CategorySelectionService>()
            .AddScoped<SheetSelectionService>()
            .AddSingleton<IRevitViewProvider, ViewProvider>()
            .AddSingleton<EventHandlers>()
            .AddScoped<ModelUpdaterService>()
            .AddSingleton<UpdaterService>()
            .AddSingleton(_ => new HttpClient())
            .AddSingleton<DeeplTranslationClient>();
    }
}