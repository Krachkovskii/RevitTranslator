using Newtonsoft.Json;

namespace RevitTranslatorAddin.Utils.DeepL;
public class DeepLUsage
{
    [JsonProperty("character_count")]
    public int CharacterCount
    {
        get; set;
    }

    [JsonProperty("character_limit")]
    public int CharacterLimit
    {
        get; set;
    }
}
