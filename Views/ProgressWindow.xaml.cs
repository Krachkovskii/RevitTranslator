using System.Windows;
using System.Windows.Media;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.ViewModels;

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
        Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 250));
    }

    private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        TranslationUtils.ClearTranslationCount();
        ProgressWindowUtils.End();
    }
}
