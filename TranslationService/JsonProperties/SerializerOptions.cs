using System.Text.Json;

namespace TranslationService.JsonProperties;

public static class SerializerOptions
{
    public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}