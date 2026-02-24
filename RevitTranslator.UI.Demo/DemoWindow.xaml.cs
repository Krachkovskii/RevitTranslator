namespace RevitTranslator.UI.Demo;

public partial class DemoWindow
{
    public DemoWindow(DemoViewModel viewModel)
    {
        DataContext = viewModel;
        // ApplicationThemeManager.Apply(this);
        
        InitializeComponent();
    }
}