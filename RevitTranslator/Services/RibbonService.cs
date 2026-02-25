using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Commands;
using RevitTranslator.Common.Messages;

namespace RevitTranslator.Services;

public class RibbonService : IRecipient<SettingsValidityChangedMessage>, IDisposable
{
    private PushButton _selectionButton = null!;
    private PushButton _sheetsButton = null!;
    private PushButton _categoriesButton = null!;
    private PushButton _modelButton = null!;

    private string _selectionTooltip = string.Empty;
    private string _sheetsTooltip = string.Empty;
    private string _categoriesTooltip = string.Empty;
    private string _modelTooltip = string.Empty;

    private const string DisabledTooltip = "Ensure your settings are valid to translate the model";

    public RibbonService() => StrongReferenceMessenger.Default.Register(this);

    // Order: selection -> sheets -> categories -> model -> [separator] -> settings
    public void CreateRibbonPanel(UIControlledApplication app)
    {
        var panel = app.CreateRibbonPanel("Translator");
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        var translateSelectionButtonData = new PushButtonData("Selection",
            "Translate selection",
            assemblyPath,
            typeof(TranslateSelectionCommand).FullName)
        {
            LongDescription = "Translate all selected elements, including their family type, if applicable.",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/SelectionIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/SelectionIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateSheetsButtonData = new PushButtonData("Sheets",
            "Translate sheets",
            assemblyPath,
            typeof(TranslateSheetsCommand).FullName)
        {
            LongDescription = "Translate all elements of selected sheets",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/CategoryIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/CategoryIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateCategoriesButtonData = new PushButtonData("Categories",
            "Translate categories",
            assemblyPath,
            typeof(TranslateCategoriesCommand).FullName)
        {
            LongDescription = "Translate all elements of selected categories",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/CategoryIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/CategoryIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var translateModelButtonData = new PushButtonData("Model",
            "Translate model",
            assemblyPath,
            typeof(TranslateModelCommand).FullName)
        {
            LongDescription = "Translate all model elements",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/AllIcon16.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/AllIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var settingsButtonData = new PushButtonData("Settings",
            "Translation settings",
            assemblyPath,
            typeof(SettingsCommand).FullName)
        {
            LongDescription = "Set API key, languages to translate to and from, and list of parameters to ignore.",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/SettingsIcon32.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/SettingsIcon32.png", UriKind.RelativeOrAbsolute))
        };

        var pulldownButtonData = new PulldownButtonData("translator", "Revit Translator")
        {
            LongDescription = "Translate elements across Revit",
            Image = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/Translate32.png", UriKind.RelativeOrAbsolute)),
            LargeImage = new BitmapImage(new Uri("/RevitTranslator;component/Resources/Icons/Translate32.png", UriKind.RelativeOrAbsolute))
        };
        var pulldownButton = (PulldownButton)panel.AddItem(pulldownButtonData);

        _selectionButton = pulldownButton.AddPushButton(translateSelectionButtonData);
        _sheetsButton = pulldownButton.AddPushButton(translateSheetsButtonData);
        _categoriesButton = pulldownButton.AddPushButton(translateCategoriesButtonData);
        _modelButton = pulldownButton.AddPushButton(translateModelButtonData);
        
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(settingsButtonData);

        _selectionTooltip = _selectionButton.ToolTip;
        _sheetsTooltip = _sheetsButton.ToolTip;
        _categoriesTooltip = _categoriesButton.ToolTip;
        _modelTooltip = _modelButton.ToolTip;

        ChangeRevitButtonsState(false);
    }

    public void ChangeRevitButtonsState(bool shouldBeEnabled)
    {
        _selectionButton.Enabled = shouldBeEnabled;
        _sheetsButton.Enabled = shouldBeEnabled;
        _categoriesButton.Enabled = shouldBeEnabled;
        _modelButton.Enabled = shouldBeEnabled;

        if (shouldBeEnabled)
        {
            _selectionButton.ToolTip = _selectionTooltip;
            _sheetsButton.ToolTip = _sheetsTooltip;
            _categoriesButton.ToolTip = _categoriesTooltip;
            _modelButton.ToolTip = _modelTooltip;
            return;
        }

        _selectionButton.ToolTip = DisabledTooltip;
        _sheetsButton.ToolTip = DisabledTooltip;
        _categoriesButton.ToolTip = DisabledTooltip;
        _modelButton.ToolTip = DisabledTooltip;
    }

    public void Receive(SettingsValidityChangedMessage message) => ChangeRevitButtonsState(message.IsValid);
    
    public void Dispose() => StrongReferenceMessenger.Default.UnregisterAll(this);
}
