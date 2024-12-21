using System.Windows;

namespace RevitTranslator.Demo;

public partial class App
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        var vm = new DemoViewModel();
        var view = new DemoWindow(vm);
        
        view.Show();
    }
}