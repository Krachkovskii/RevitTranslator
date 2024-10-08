﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RevitTranslatorAddin.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RevitTranslatorAddin.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : FluentWindow
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ApplicationThemeManager.Apply(this);

        // Can't set Acrylic (or any sort of backdrop, in that case) in Windows 10.
        // Seems like the best way to check for the system version is via build number
        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        //if (false)
        {
            WindowBackdropType = WindowBackdropType.Acrylic;
        }
        else
        {
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 245, 245, 245));
        }
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
}
