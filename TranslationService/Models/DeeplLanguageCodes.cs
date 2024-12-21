namespace TranslationService.Models;

public static class DeeplLanguageCodes
{
    // Source: https://developers.deepl.com/docs/v/en/resources/supported-languages
    /// <summary>
    /// Language codes for DeepL. First item is readable language name, second item is language code.
    /// </summary>
    public static LanguageDescriptor[] LanguageCodes { get; } = new LanguageDescriptor[]
    {
        new("Bulgarian", "bg"),
        new("Chinese (simplified)", "zh"),
        new("Czech", "cs"),
        new("Danish", "da"),
        new("Dutch", "nl"),
        new("English (American)", "en-us"),
        new("English (British)", "en-gb"),
        new("Estonian", "et"),
        new("Finnish", "fi"),
        new("French", "fr"),
        new("German", "de"),
        new("Greek", "el"),
        new("Hungarian", "hu"),
        new("Indonesian", "id"),
        new("Italian", "it"),
        new("Japanese", "ja"),
        new("Korean", "ko"),
        new("Latvian", "lv"),
        new("Lithuanian", "lt"),
        new("Norwegian (Bokmål)", "nb"),
        new("Polish", "pl"),
        new("Portuguese (Brazilian)", "pt-br"),
        new("Portuguese (European)", "pt-pt"),
        new("Romanian", "ro"),
        new("Russian", "ru"),
        new("Slovak", "sk"),
        new("Slovenian", "sl"),
        new("Spanish", "es"),
        new("Swedish", "sv"),
        new("Turkish", "tr"),
        new("Ukrainian", "uk")
    };
}
