using System.Text.RegularExpressions;

namespace TranslationService.Validation;

/// <summary>
/// Validates DeepL API keys for correct format
/// </summary>
public static partial class ApiKeyValidator
{
    // DeepL API keys: 8-4-4-4-12 hex chars (UUID format), optional :fx suffix for free plans
    private const string ApiKeyPattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}(:fx|:FX)?$";

#if NET8_0_OR_GREATER
    [GeneratedRegex(ApiKeyPattern, RegexOptions.Compiled)]
    private static partial Regex ApiKeyRegex();
#else
    private static readonly Regex _apiKeyRegex = new(ApiKeyPattern, RegexOptions.Compiled);
    private static Regex ApiKeyRegex() => _apiKeyRegex;
#endif

    /// <summary>
    /// Validates and sanitizes an API key
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <param name="sanitizedKey">The sanitized (trimmed) API key</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool TryValidate(string? apiKey, out string sanitizedKey, out string? errorMessage)
    {
        sanitizedKey = string.Empty;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            errorMessage = "API key cannot be empty.";
            return false;
        }

        sanitizedKey = apiKey.Trim();

        if (sanitizedKey.Length < 36)
        {
            errorMessage = "API key is too short. DeepL API keys are 36-39 characters long.";
            return false;
        }

        if (sanitizedKey.Length > 50)
        {
            errorMessage = "API key is too long. Please verify your key.";
            return false;
        }

        if (!ApiKeyRegex().IsMatch(sanitizedKey))
        {
            errorMessage = "API key format is invalid. DeepL API keys should follow the format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx (with optional :fx suffix for free plans).";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines if an API key is for a free plan (ends with :fx)
    /// </summary>
    /// <param name="apiKey">The API key to check</param>
    /// <returns>True if free plan, false if pro plan or invalid</returns>
    public static bool IsFreePlan(string? apiKey)
    {
        return !string.IsNullOrWhiteSpace(apiKey) 
               && apiKey!.Trim().EndsWith(":fx", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Sanitizes an API key by trimming whitespace
    /// </summary>
    /// <param name="apiKey">The API key to sanitize</param>
    /// <returns>Sanitized API key, or empty string if input is null/whitespace</returns>
    public static string Sanitize(string? apiKey)
    {
        return string.IsNullOrWhiteSpace(apiKey) ? string.Empty : apiKey.Trim();
    }
}
