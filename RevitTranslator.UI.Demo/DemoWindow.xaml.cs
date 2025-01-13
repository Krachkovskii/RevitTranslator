using Wpf.Ui.Appearance;

namespace RevitTranslator.Demo;

public partial class DemoWindow
{
    public DemoWindow(DemoViewModel viewModel)
    {
        DataContext = viewModel;
        ApplicationThemeManager.Apply(this);
        
        InitializeComponent();
    }
}