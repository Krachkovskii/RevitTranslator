using System.Text.Json;

namespace TranslationService.JsonProperties;

public static class JsonSettings
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
    };
}