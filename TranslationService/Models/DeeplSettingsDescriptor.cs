namespace TranslationService.Models;

public class DeeplSettingsDescriptor
{
    public bool IsPaidPlan = false;
    public string DeeplApiKey { get; set; } = string.Empty;
    public LanguageDescriptor? SourceLanguage { get; set; } = DeeplLanguageCodes.LanguageCodes[0];
    public LanguageDescriptor TargetLanguage { get; set; } = DeeplLanguageCodes.LanguageCodes[1];

    // TODO: implement blacklist for parameters and words
    //public List<string> IgnoreParameters { get; set; } = [];
    //public List<string> IgnoreValues { get; set; } = [];
}