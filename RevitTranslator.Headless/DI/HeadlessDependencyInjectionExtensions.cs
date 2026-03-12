using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Headless.Services;
using RevitTranslator.Revit.Core.Contracts;

namespace RevitTranslator.Headless.DI;

public static class HeadlessDependencyInjectionExtensions
{
    public static IServiceCollection AddHeadlessServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<ITranslationProgressMonitor, ConsoleTranslationProgressMonitor>()
            .AddScoped<ISettingsValidator, HeadlessSettingsValidator>();
    }
}
