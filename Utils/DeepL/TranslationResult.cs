using Newtonsoft.Json;

namespace RevitTranslatorAddin.Utils.DeepL;

/// <summary>
/// Higher-level class for handling response from DeepL API with text translation.
/// </summary>
public class TranslationResult
{
    [JsonProperty("translations")]
    public Translation[] Translations
    {
        get; set;
    }
}
