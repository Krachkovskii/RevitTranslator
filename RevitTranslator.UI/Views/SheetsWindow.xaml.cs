using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RevitTranslator.Ui.Library.Controls;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Ui.Library.Appearance;
using RevitTranslator.Ui.Library.Extensions;
using RevitTranslator.UI.ViewModels;

namespace RevitTranslator.UI.Views;

/// <summary>
/// Interaction logic for SelectCategoriesWindow.xaml
/// </summary>
public partial class SheetsWindow
{
    private readonly SheetsViewModel _sheetModel;
    
    public SheetsWindow(SheetsViewModel sheetModel)
    {
        DataContext = sheetModel;
        _sheetModel = sheetModel;
        
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
        DialogResult = false;
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
