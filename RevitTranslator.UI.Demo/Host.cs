using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.DI;
using RevitTranslator.Demo.DI;
using RevitTranslator.UI.DI;

namespace RevitTranslator.Demo;

public class Host
{
    public Host()
    {
        ServiceProvider = new ServiceCollection()
            .AddUi()
            .AddMockUiImpl()
            .AddCommonServices()
            .BuildServiceProvider();
    }
    
    public static ServiceProvider ServiceProvider { get; private set; } = null!;
}