using System.Windows;
using System.Windows.Media;
using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.ViewModels;

namespace RevitTranslatorAddin.Views;
/// <summary>
/// Interaction logic for ProgressWindow.xaml
/// </summary>
public partial class ProgressWindow : Window
{
    private readonly ProgressWindowUtils _progressWindowUtils = null;
    public ProgressWindow(ProgressWindowViewModel viewModel, ProgressWindowUtils progressWindowUtils)
    {
        InitializeComponent();
        DataContext = viewModel;
        Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 250));
        _progressWindowUtils = progressWindowUtils;
    }

    private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        TranslationUtils.ClearTranslationCount();
        _progressWindowUtils.End();
    }
}
