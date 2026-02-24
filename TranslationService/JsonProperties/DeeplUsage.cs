namespace TranslationService.JsonProperties;

/// <summary>
/// Handles DeepL API response for usage limits
/// </summary>
public record DeeplUsage(int CharacterCount, int CharacterLimit);
