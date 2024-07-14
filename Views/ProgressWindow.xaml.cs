using System.Windows;
using System.Windows.Input;
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

    private void Titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
