using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;

namespace RevitTranslatorAddin;
/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        CreateRibbonPanel();
    }

    private void CreateRibbonPanel()
    {
        RibbonPanel panel = Application.CreateRibbonPanel("Translator");
        string assemblyPath = Assembly.GetExecutingAssembly().Location;

        var settingsButtonData = new PushButtonData("Settings",
            "Translation settings",
            assemblyPath,
            "RevitTranslatorAddin.Commands.SettingsCommand")
        {
            LongDescription = "Set API key, languages to translate to and from, and list of parameters to ignore.",
            Image = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/SettingsIcon32.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/SettingsIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateSelectionButtonData = new PushButtonData("Selection",
            "Translate selection",
            assemblyPath,
            "RevitTranslatorAddin.Commands.TranslateSelectionCommand")
        {
            LongDescription = "Translate all selected elements, including their family type, if applicable.",
            Image = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/SelectionIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/SelectionIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateModelButtonData = new PushButtonData("Model",
            "Translate model",
            assemblyPath,
            "RevitTranslatorAddin.Commands.TranslateModelCommand")
        {
            LongDescription = "Translate all model elements",
            Image = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/AllIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/AllIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateCategoriesButtonData = new PushButtonData("Categories",
            "Translate categories",
            assemblyPath,
            "RevitTranslatorAddin.Commands.TranslateCategoriesCommand")
        {
            LongDescription = "Translate all elements of selected categories",
            Image = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/CategoryIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/CategoryIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var pulldownButtonData = new PulldownButtonData("translator", "Revit Translator")
        {
            LongDescription = "Translate elements across Revit",
            Image = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/Translate32.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslatorAddin;component/Resources/Icons/Translate32.png", UriKind.RelativeOrAbsolute))
        };
        var pulldownButton = (PulldownButton)panel.AddItem(pulldownButtonData);

        pulldownButton.AddPushButton(translateModelButtonData);
        pulldownButton.AddPushButton(translateCategoriesButtonData);
        pulldownButton.AddPushButton(translateSelectionButtonData);
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(settingsButtonData);
    }
}