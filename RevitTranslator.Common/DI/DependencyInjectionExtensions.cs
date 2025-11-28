using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.Services;

namespace RevitTranslator.Common.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCommonServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<ScopedWindowService>();
    }
}