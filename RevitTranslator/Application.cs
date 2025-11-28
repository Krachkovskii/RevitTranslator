using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitTranslator.Handlers;

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
        EventHandlers.ActionHandler = new ActionEventHandler();
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

        var pulldownButtonData = new PulldownButtonData("translator", "Revit Translator")
        {
            LongDescription = "Translate elements across Revit",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/Translate32.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/Translate32.png", UriKind.RelativeOrAbsolute))
        };
        var pulldownButton = (PulldownButton)panel.AddItem(pulldownButtonData);

        pulldownButton.AddPushButton(translateModelButtonData);
        pulldownButton.AddPushButton(translateCategoriesButtonData);
        pulldownButton.AddPushButton(translateSelectionButtonData);
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(settingsButtonData);
    }
}