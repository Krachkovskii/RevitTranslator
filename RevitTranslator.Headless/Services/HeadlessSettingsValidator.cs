using RevitTranslator.Revit.Core.Contracts;
using TranslationService.Utils;

namespace RevitTranslator.Headless.Services;

public class HeadlessSettingsValidator(DeeplTranslationClient translationClient) : ISettingsValidator
{
    public async Task<bool> TryEnforceValidSettingsAsync()
    {
        if (DeeplSettingsUtils.CurrentSettings is null && !DeeplSettingsUtils.Load())
        {
            Console.WriteLine("[Settings] Failed to load settings.");
            return false;
        }

        var canTranslate = await translationClient.CanTranslateAsync();
        if (!canTranslate)
        {
            Console.WriteLine("[Settings] Settings are not valid. Elements will not be translated.");
        }

        return canTranslate;
    }
}
