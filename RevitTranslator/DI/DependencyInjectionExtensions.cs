using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Services;
using RevitTranslator.UI.Contracts;
using RevitTranslator.ViewModels;

namespace RevitTranslator.DI;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddRevitUiImpl()
        {
            return serviceCollection
                .AddScoped<ICategoriesWindowViewModel, CategoriesWindowViewModel>()
                .AddScoped<IProgressWindowViewModel, ProgressWindowViewModel>()
                .AddScoped<ISettingsViewModel, SettingsViewModel>();
        }

        public IServiceCollection AddRevitServices()
        {
            return serviceCollection
                .AddScoped<BaseTranslationService>()
                .AddScoped<ConcurrentTranslationService>()
                .AddScoped<ModelUpdaterService>();
        }
    }
}