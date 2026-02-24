using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RevitTranslator.Ui.Library.Controls;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Ui.Library.Appearance;
using RevitTranslator.Ui.Library.Extensions;

namespace RevitTranslator.UI.Views;

/// <summary>
/// Interaction logic for SelectCategoriesWindow.xaml
/// </summary>
public partial class CategoriesWindow
{
    private readonly ICategoriesWindowViewModel _viewModel;
    
    public CategoriesWindow(ICategoriesWindowViewModel viewModel)
    {
        DataContext = viewModel;
        _viewModel = viewModel;
        
        InitializeComponent();
        
        ApplicationThemeManager.Apply(this);
        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            WindowBackdropType = WindowBackdropType.Acrylic;
        }
    }
    
    private void OnTranslateButtonClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void OnCloseClicked(TitleBar sender, RoutedEventArgs args)
    {
        try
        {
            DialogResult = false;
        }
        catch
        {
            // do nothing
        }
    }

    private void OnItemBorderLeftMouseUp(object sender, MouseButtonEventArgs args)
    {
        var source = args.OriginalSource as DependencyObject;
        if (source.FindParent<CheckBox>() is not null) return;
        
        var border = source.FindParent<Border>("ItemBorder");
        var checkBox = border?.FindVisualChild<CheckBox>();
        if (checkBox is null) return;
        
        checkBox.IsChecked = !checkBox.IsChecked;
    }
}
