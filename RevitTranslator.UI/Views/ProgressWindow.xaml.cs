using System.Windows;
using RevitTranslator.Ui.Library.Controls;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Ui.Library.Appearance;

namespace RevitTranslator.UI.Views;

/// <summary>
/// Interaction logic for ProgressWindow.xaml
/// </summary>
public partial class ProgressWindow
{
    private readonly IProgressWindowViewModel _viewModel;
    
    public ProgressWindow(IProgressWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        InitializeComponent();
        
        ApplicationThemeManager.Apply(this);
        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            WindowBackdropType = WindowBackdropType.Acrylic;
        }
    }

    private void OnCloseClicked(TitleBar sender, RoutedEventArgs args)
    {
        _viewModel.CloseRequested();
    }
}
