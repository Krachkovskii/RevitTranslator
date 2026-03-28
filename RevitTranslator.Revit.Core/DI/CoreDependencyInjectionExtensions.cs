using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Abstractions;
using RevitTranslator.Abstractions.Contracts;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Revit.Core.ElementTextRetrievers;
using RevitTranslator.Revit.Core.Services;

namespace RevitTranslator.Revit.Core.DI;

public static class CoreDependencyInjectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IRevitViewProvider, ViewProvider>()
            .AddSingleton<TranslationReportService>()
            .AddSingleton<ITranslationReportService>(sp => sp.GetRequiredService<TranslationReportService>())
            .AddScoped<ConcurrentTranslationService>()
            .AddScoped<ModelUpdaterService>()
            .AddScoped<MultiElementTextRetriever>();
    }
}
