using System.Windows;
using RevitTranslator.Ui.Library.Controls;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.Ui.Library.Appearance;

namespace RevitTranslator.UI.Views;

/// <summary>
/// Interaction logic for ProgressWindow.xaml
/// </summary>
public partial class ProgressWindow
{
    private readonly ProgressWindowViewModel _viewModel;

    public ProgressWindow(ProgressWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        InitializeComponent();

        Loaded += OnLoaded;
        ContentRendered += OnContentRendered;

        ApplicationThemeManager.Apply(this);
        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            WindowBackdropType = WindowBackdropType.Acrylic;
        }
    }

    private void OnContentRendered(object sender, EventArgs e)
    {
        Top -= 200;
        ContentRendered -= OnContentRendered;
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

    private void OnCloseClicked(TitleBar sender, RoutedEventArgs args)
    {
        var shouldClose = _viewModel.CloseRequested();
        if (shouldClose) Close();
    }
}