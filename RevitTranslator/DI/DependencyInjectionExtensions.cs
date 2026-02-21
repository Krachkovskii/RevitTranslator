using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.Contracts;
using RevitTranslator.Services;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Utils;
using RevitTranslator.ViewModels;

namespace RevitTranslator.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRevitUiImpl(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<ICategoriesWindowViewModel, CategoriesWindowViewModel>()
            .AddScoped<IProgressWindowViewModel, ProgressWindowViewModel>()
            .AddScoped<ISettingsViewModel, SettingsViewModel>();
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
            .AddSingleton<UpdaterService>();
    }
}