using System.Windows;
using System.Windows.Media;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.Ui.Library.Appearance;
using RevitTranslator.Ui.Library.Controls;

namespace RevitTranslator.UI.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        InitializeComponent();

        Loaded += OnLoaded;

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

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.LoadedCommand.ExecuteAsync(null);
        }
        catch
        {
            // do nothing
        }
        finally
        {
            Loaded -= OnLoaded;
        }
    }
}