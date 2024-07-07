using System.Windows;
using System.Windows.Controls;

namespace RevitTranslatorAddin.Views;

/// <summary>
/// Class that describes a ProgressWindow.
/// See Utils.ProgressWindowUtils for update methods.
/// </summary>
public class ProgressWindow : Window
{
    internal ProgressBar ProgressBar { get; set; }
    internal TextBlock StatusTextBlock { get; set; }

    public ProgressWindow()
    {
        Width = 400;
        Height = 100;
        WindowStyle = WindowStyle.ToolWindow;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Title = "Translation Progress";

        var grid = new System.Windows.Controls.Grid();
        Content = grid;

        ProgressBar = new ProgressBar { Margin = new Thickness(10) };
        StatusTextBlock = new TextBlock { Margin = new Thickness(10, 0, 10, 10) };

        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        System.Windows.Controls.Grid.SetRow(ProgressBar, 0);
        System.Windows.Controls.Grid.SetRow(StatusTextBlock, 1);

        grid.Children.Add(ProgressBar);
        grid.Children.Add(StatusTextBlock);
    }
}
