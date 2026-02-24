using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.UI.Views;

namespace RevitTranslator.UI.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddUi(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<SettingsWindow>()
            .AddTransient<Func<SettingsWindow>>(sp => sp.GetRequiredService<SettingsWindow>)
            .AddScoped<ProgressWindowViewModel>()
            .AddScoped<ProgressWindow>()
            .AddTransient<Func<ProgressWindow>>(sp => sp.GetRequiredService<ProgressWindow>)
            .AddScoped<CategoriesWindow>()
            .AddScoped<SheetsViewModel>()
            .AddScoped<SheetsWindow>();

        return serviceCollection;
    }
}