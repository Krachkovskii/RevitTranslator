using System.Windows;
using RevitTranslatorAddin.ViewModels;

namespace RevitTranslatorAddin.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
/// 
public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
