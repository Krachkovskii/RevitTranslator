namespace TranslationService.Models;

public static class DeeplLanguageCodes
{
    // Source: https://developers.deepl.com/docs/v/en/resources/supported-languages
    /// <summary>
    /// Language codes for DeepL. First item is readable language name, second item is language code.
    /// </summary>
    public static LanguageDescriptor[] LanguageCodes { get; } =
    [
        new("Bulgarian", "bg", "bg"),
        new("Chinese (simplified)", "zh", "zh-hans"),
        new("Chinese (traditional)", "zh", "zh-hant"),
        new("Czech", "cs", "cs"),
        new("Danish", "da", "da"),
        new("Dutch", "nl", "nl"),
        new("English (American)", "en", "en-us"),
        new("English (British)", "en", "en-gb"),
        new("Estonian", "et", "et"),
        new("Finnish", "fi", "fi"),
        new("French", "fr", "fr"),
        new("German", "de", "de"),
        new("Greek", "el", "el"),
        new("Hungarian", "hu", "hu"),
        new("Indonesian", "id", "id"),
        new("Italian", "it", "it"),
        new("Japanese", "ja", "ja"),
        new("Korean", "ko", "ko"),
        new("Latvian", "lv", "lv"),
        new("Lithuanian", "lt", "lt"),
        new("Norwegian (Bokmål)", "nb", "nb"),
        new("Polish", "pl", "pl"),
        new("Portuguese (Brazilian)", "pt", "pt-br"),
        new("Portuguese (European)", "pt", "pt-pt"),
        new("Romanian", "ro", "ro"),
        new("Russian", "ru", "ru"),
        new("Slovak", "sk", "sk"),
        new("Slovenian", "sl", "sl"),
        new("Spanish", "es", "es"),
        new("Swedish", "sv", "sv"),
        new("Turkish", "tr", "tr"),
        new("Ukrainian", "uk", "uk")
    ];
}
