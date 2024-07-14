using System.Windows;
using RevitTranslatorAddin.ViewModels;
using Wpf.Ui.Appearance;

namespace RevitTranslatorAddin.Views;
/// <summary>
/// Interaction logic for ProgressWindow.xaml
/// </summary>
public partial class ProgressWindow : Window
{
    public ProgressWindow(ProgressWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        //ApplicationThemeManager.Apply(this);
    }
}
