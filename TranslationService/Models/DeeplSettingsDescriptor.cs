namespace TranslationService.Models;

public class DeeplSettingsDescriptor
{
    public bool IsPaidPlan { get; set; }

    /// <summary>
    /// The DeepL API key. This value is stored encrypted in the JSON file.
    /// </summary>
    public string DeeplApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the API key in the JSON file is encrypted using DPAPI
    /// </summary>
    public bool IsApiKeyEncrypted { get; set; } = true;

    public LanguageDescriptor? SourceLanguage { get; set; } = DeeplLanguageCodes.TargetLanguages[0];
    public LanguageDescriptor TargetLanguage { get; set; } = DeeplLanguageCodes.TargetLanguages[1];
}