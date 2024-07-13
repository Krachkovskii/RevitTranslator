using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using RevitTranslatorAddin.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RevitTranslatorAddin.Views;
/// <summary>
/// Interaction logic for NewProgressWindow.xaml
/// </summary>
public partial class NewProgressWindow : FluentWindow
{
    public NewProgressWindow(ProgressWindowViewModel viewModel)
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
}
