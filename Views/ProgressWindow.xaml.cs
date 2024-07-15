using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.ViewModels;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;

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
        //Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 250));
    }

    private void Titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        TranslationUtils.ClearTranslationCount();
        ProgressWindowUtils.End();
    }
}
