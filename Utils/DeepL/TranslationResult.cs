using Newtonsoft.Json;

namespace RevitTranslatorAddin.Utils.DeepL;
public class TranslationResult
{
    [JsonProperty("translations")]
    public Translation[] Translations
    {
        get; set;
    }
}
