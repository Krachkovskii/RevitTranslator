using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.UI.Views;

namespace RevitTranslator.UI.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddUi(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<SettingsWindow>()
            .AddScoped<ProgressWindow>()
            .AddScoped<CategoriesWindow>();
        
        return serviceCollection;
    }
}