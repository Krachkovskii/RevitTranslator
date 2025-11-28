using Microsoft.Extensions.DependencyInjection;
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
            .BuildServiceProvider();
    }
    
    public static ServiceProvider ServiceProvider { get; private set; } = null!;
}