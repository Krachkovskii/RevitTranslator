using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Ui.Library.Controls;
using RevitTranslator.UI.ViewModels;
using RevitTranslator.Ui.Library.Appearance;

namespace RevitTranslator.UI.Views;

/// <summary>
/// Interaction logic for ProgressWindow.xaml
/// </summary>
public partial class ProgressWindow : IRecipient<ModelUpdatedMessage>
{
    private readonly ProgressWindowViewModel _viewModel;

    public ProgressWindow(ProgressWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        StrongReferenceMessenger.Default.Register(this);
        InitializeComponent();

        Loaded += OnLoaded;

        ApplicationThemeManager.Apply(this);
        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            WindowBackdropType = WindowBackdropType.Acrylic;
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

    private void OnCloseClicked(TitleBar sender, RoutedEventArgs args)
    {
        var shouldClose = _viewModel.CloseRequested();
        StrongReferenceMessenger.Default.UnregisterAll(this);
        if (!shouldClose) return;
        
        Close();
    }

    public void Receive(ModelUpdatedMessage message) => LastReportButton.Visibility = Visibility.Visible;
}