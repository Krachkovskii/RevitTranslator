using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.Contracts;
using RevitTranslator.Revit.Core.Services;
using RevitTranslator.Revit.Core.Utils;

namespace RevitTranslator.Revit.Core.DI;

public static class CoreDependencyInjectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<EventHandlers>()
            .AddSingleton<IRevitViewProvider, ViewProvider>()
            .AddSingleton<TranslationReportService>()
            .AddSingleton<ITranslationReportService>(sp => sp.GetRequiredService<TranslationReportService>())
            .AddScoped<ConcurrentTranslationService>()
            .AddScoped<ModelUpdaterService>();
    }
}
