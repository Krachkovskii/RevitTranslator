using System.Windows;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Services;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.UI.Views;
using TranslationService.Utils;

namespace RevitTranslator.Services;

public class UiSettingsValidator(
    DeeplTranslationClient translationClient,
    Func<ScopedWindowService> scopedServiceFactory) : ISettingsValidator
{
    public async Task<bool> TryEnforceValidSettingsAsync()
    {
        if (DeeplSettingsUtils.CurrentSettings is null)
        {
            if (!DeeplSettingsUtils.Load())
            {
                MessageBox.Show("Failed to load settings. Please check file permissions and try again.",
                    "Settings Error");
                return false;
            }
        }

        if (await translationClient.CanTranslateAsync())
            return true;

        var parentWindow = Context.UiApplication.MainWindowHandle.ToWindow();
        scopedServiceFactory().ShowDialog<SettingsWindow>(parentWindow);

        if (await translationClient.CanTranslateAsync())
            return true;

        MessageBox.Show("Settings are not valid. Elements will not be translated.",
            "Translation Service Error");
        return false;
    }
}
