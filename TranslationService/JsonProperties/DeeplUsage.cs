namespace TranslationService.JsonProperties;

/// <summary>
/// Handles DeepL API response for usage limits
/// </summary>
public class DeeplUsage
{
    public int CharacterCount { get; set; }
    public int CharacterLimit { get; set; }
}
