namespace TranslationService.JsonProperties;

/// <summary>
/// Higher-level class for handling response from DeepL API with text translation.
/// </summary>
public class TranslationResult
{
    public Translation[] Translations { get; set; } = [];
}
