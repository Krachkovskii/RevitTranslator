namespace TranslationService.JsonProperties;

/// <summary>
/// Higher-level class for handling response from DeepL API with text translation.
/// </summary>
public sealed record TranslationResult(Translation[] Translations);
