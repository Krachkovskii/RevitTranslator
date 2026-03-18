using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Services;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.UI.Contracts;
using RevitTranslator.ViewModels;
using TranslationService.Utils;
using System.Net.Http;

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
            .AddSingleton<RibbonService>()
            .AddScoped<CategorySelectionService>()
            .AddScoped<SheetSelectionService>()
            .AddSingleton<UpdaterService>()
            .AddSingleton(_ => new HttpClient())
            .AddSingleton<DeeplTranslationClient>()
            .AddScoped<ITranslationProgressMonitor, UiTranslationProgressMonitor>()
            .AddScoped<TranslationManager>()
            .AddScoped<ISettingsValidator, UiSettingsValidator>();
    }
}
