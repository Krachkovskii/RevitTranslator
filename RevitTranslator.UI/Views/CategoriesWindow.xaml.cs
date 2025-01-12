using System.Windows;
using System.Windows.Controls;
using RevitTranslator.Common.Models;
using RevitTranslator.UI.Contracts;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using ListView = Wpf.Ui.Controls.ListView;

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

        Closed += OnWindowClosing;
        
        ApplicationThemeManager.Apply(this);

        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            WindowBackdropType = WindowBackdropType.Acrylic;
        }
    }
    
    private void OnTranslateButtonClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnWindowClosing(object? sender, EventArgs e)
    {
        _viewModel.OnCloseRequested();
    }

    private void UpdateCheckbox(object sender, SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count == 0) return;
        
        var category = (ObservableCategoryDescriptor)args.AddedItems[0]!;
        category.IsChecked = !category.IsChecked;

        // deselecting the element in the UI. It's done here to avoid ListViewItem style override
        var listview = (ListView)sender;
        listview.SelectedIndex = -1;
    }
}
