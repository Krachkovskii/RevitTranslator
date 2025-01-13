using System.Windows.Media;
using RevitTranslator.UI.Contracts;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RevitTranslator.UI.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow
{
    public SettingsWindow(ISettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ApplicationThemeManager.Apply(this);

        // Can't set Acrylic (or any sort of backdrop, in that case) in Windows 10.
        // Seems like the best way to check for the system version is via build number
        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        //if (false)
        {
            WindowBackdropType = WindowBackdropType.Acrylic;
        }
        else
        {
            Background = new SolidColorBrush(Color.FromArgb(0, 245, 245, 245));
        }
    }
}
