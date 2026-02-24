using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.DI;
using RevitTranslator.DI;
using RevitTranslator.UI.DI;

namespace RevitTranslator;

public class Host
{
    public Host()
    {
        ServiceProvider = new ServiceCollection()
            .AddUi()
            .AddRevitUiImpl()
            .AddRevitServices()
            .AddCommonServices()
            .BuildServiceProvider();
    }
    
    public static ServiceProvider ServiceProvider { get; private set; } = null!;
}