namespace TranslationService.Models;

public class DeeplSettingsDescriptor
{
    public bool IsPaidPlan { get; set; }
    public string DeeplApiKey { get; set; } = string.Empty;
    public LanguageDescriptor? SourceLanguage { get; set; } = DeeplLanguageCodes.TargetLanguages[0];
    public LanguageDescriptor TargetLanguage { get; set; } = DeeplLanguageCodes.TargetLanguages[1];
}