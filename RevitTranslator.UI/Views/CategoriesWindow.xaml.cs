using System.Windows;
using RevitTranslator.UI.Contracts;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RevitTranslator.UI.Views;
/// <summary>
/// Interaction logic for SelectCategoriesWindow.xaml
/// </summary>
public partial class CategoriesWindow
{
    public CategoriesWindow(ICategoriesWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        ApplicationThemeManager.Apply(this);

        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            WindowBackdropType = WindowBackdropType.Acrylic;
        }
    }
    
    private void OnTranslateButtonClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
