using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Microsoft.Xaml.Behaviors;
using Nice3point.Revit.Toolkit.External;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitTranslator.Utils;
using TriggerBase = Microsoft.Xaml.Behaviors.TriggerBase;

namespace RevitTranslator;
/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        _ = new Host();
        CreateRibbonPanel();
        FixBehaviors();
    }

    private void CreateRibbonPanel()
    {
        var panel = Application.CreateRibbonPanel("Translator");
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        var settingsButtonData = new PushButtonData("Settings",
            "Translation settings",
            assemblyPath,
            "RevitTranslator.Commands.SettingsCommand")
        {
            LongDescription = "Set API key, languages to translate to and from, and list of parameters to ignore.",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/SettingsIcon32.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/SettingsIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateSelectionButtonData = new PushButtonData("Selection",
            "Translate selection",
            assemblyPath,
            "RevitTranslator.Commands.TranslateSelectionCommand")
        {
            LongDescription = "Translate all selected elements, including their family type, if applicable.",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/SelectionIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/SelectionIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateModelButtonData = new PushButtonData("Model",
            "Translate model",
            assemblyPath,
            "RevitTranslator.Commands.TranslateModelCommand")
        {
            LongDescription = "Translate all model elements",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/AllIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/AllIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateCategoriesButtonData = new PushButtonData("Categories",
            "Translate categories",
            assemblyPath,
            "RevitTranslator.Commands.TranslateCategoriesCommand")
        {
            LongDescription = "Translate all elements of selected categories",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/CategoryIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/CategoryIcon32.png", UriKind.RelativeOrAbsolute))
        };
        
        var translateViewsButtonData = new PushButtonData("Views",
            "Translate views",
            assemblyPath,
            "RevitTranslator.Commands.TranslateViewsCommand")
        {
            LongDescription = "Translate all elements of selected views",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/CategoryIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/CategoryIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var pulldownButtonData = new PulldownButtonData("translator", "Revit Translator")
        {
            LongDescription = "Translate elements across Revit",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/Translate32.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/Translate32.png", UriKind.RelativeOrAbsolute))
        };
        var pulldownButton = (PulldownButton)panel.AddItem(pulldownButtonData);

        pulldownButton.AddPushButton(translateModelButtonData);
        pulldownButton.AddPushButton(translateCategoriesButtonData);
        pulldownButton.AddPushButton(translateViewsButtonData);
        pulldownButton.AddPushButton(translateSelectionButtonData);
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(settingsButtonData);
    }
    
    private static void FixBehaviors()
    {
        //https://github.com/microsoft/XamlBehaviorsWpf/issues/86
        _ = new DefaultTriggerAttribute(typeof(Trigger), typeof(TriggerBase), null);
    }
}