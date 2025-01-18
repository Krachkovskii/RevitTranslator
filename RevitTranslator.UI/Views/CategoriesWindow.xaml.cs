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
    private readonly ICategoriesWindowViewModel _viewModel;
    
    public CategoriesWindow(ICategoriesWindowViewModel viewModel)
    {
        DataContext = viewModel;
        _viewModel = viewModel;
        InitializeComponent();

        Closed += OnWindowClosing;
        
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

    private void OnWindowClosing(object? sender, EventArgs e)
    {
        _viewModel.OnCloseRequested();
    }
}
