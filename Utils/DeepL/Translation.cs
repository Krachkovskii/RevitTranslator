using Newtonsoft.Json;

namespace RevitTranslatorAddin.Utils.DeepL;

/// <summary>
/// Handles response from DeepL API with text translation.
/// </summary>
public class Translation
{
    [JsonProperty("detected_source_language")]
    public string DetectedSourceLanguage
    {
        get; set;
    }

    [JsonProperty("text")]
    public string Text
    {
        get; set;
    }
}
