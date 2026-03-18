namespace TranslationService.Exceptions;

/// <summary>
/// Thrown when a translation error is unrecoverable and the entire operation must be aborted.
/// Examples: invalid API key, quota exceeded, network unavailable.
/// </summary>
public sealed class FatalTranslationException(string message) : Exception(message);
