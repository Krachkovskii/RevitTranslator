namespace TranslationService.Models;

public class DeeplSettingsDescriptor
{
    public bool IsPaidPlan { get; set; }
    public string DeeplApiKey { get; set; } = string.Empty;
    public LanguageDescriptor? SourceLanguage { get; set; } = DeeplLanguageCodes.LanguageCodes[0];
    public LanguageDescriptor TargetLanguage { get; set; } = DeeplLanguageCodes.LanguageCodes[1];
}