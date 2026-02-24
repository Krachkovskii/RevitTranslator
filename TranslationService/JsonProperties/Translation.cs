using JetBrains.Annotations;

namespace TranslationService.JsonProperties;

/// <summary>
/// Handles response from DeepL API with text translation.
/// </summary>
[UsedImplicitly]
public sealed record Translation(string? DetectedSourceLanguage, string? Text);
