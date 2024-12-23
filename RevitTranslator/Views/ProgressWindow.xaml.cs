using ProgressWindowViewModel = RevitTranslator.ViewModels.ProgressWindowViewModel;

namespace RevitTranslator.Views;
/// <summary>
/// Interaction logic for ProgressWindow.xaml
/// </summary>
public partial class ProgressWindow : Window
{
    public ProgressWindow(ProgressWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 250));
    }
}
