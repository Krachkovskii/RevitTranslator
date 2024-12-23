using System.Windows;
using RevitTranslatorAddin.ViewModels.Contracts;
using Wpf.Ui.Appearance;

namespace RevitTranslator.Views;
/// <summary>
/// Interaction logic for SelectCategoriesWindow.xaml
/// </summary>
public partial class CategoriesWindow : Window
{
    public CategoriesWindow(ICategoriesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ApplicationThemeManager.Apply(this);

        //if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        //if (false)
        //{
        //    //WindowBackdropType = WindowBackdropType.Acrylic;
        //}
        //else
        //{
        //    //ExtendsContentIntoTitleBar = false;
        //    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 245, 245, 245));
        //}
    }
}
