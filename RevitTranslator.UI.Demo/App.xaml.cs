using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RevitTranslator.UI.Demo;

public partial class App
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        _ = new Host();

        Host.ServiceProvider.GetRequiredService<DemoWindow>().Show();
    }
}