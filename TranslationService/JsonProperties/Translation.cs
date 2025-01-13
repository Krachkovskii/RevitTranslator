namespace TranslationService.JsonProperties;

/// <summary>
/// Handles response from DeepL API with text translation.
/// </summary>
public class Translation
{
    public string? DetectedSourceLanguage { get; set; }
    public string? Text { get; set; }
}
