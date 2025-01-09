using System.Text.Json;
using TranslationService.Models;

namespace TranslationService.Utils;

/// <summary>
/// DeepL translation settings
/// </summary>
public static class DeeplSettingsUtils
{
    private const string AddinName = "RevitTranslator";
    
    private static string JsonDirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Autodesk",
        "ApplicationPlugins",
        AddinName);
    private static string JsonFilePath => Path.Combine(
        JsonDirectoryPath,
        "settings.json");

    public static DeeplSettingsDescriptor CurrentSettings { get; set; } = new();
    public static string TranslationUrl { get; private set; } = "https://api-free.deepl.com/v2/translate";
    public static string UsageUrl { get; private set; } = "https://api-free.deepl.com/v2/usage";

    /// <summary>
    /// Loads the settings from a JSON file.
    /// </summary>
    /// <returns>An instance of the Settings class with the loaded settings.</returns>
    public static void Load()
    {
        if (!File.Exists(JsonFilePath)) return;
        
        var json = File.ReadAllText(JsonFilePath);
        CurrentSettings = JsonSerializer.Deserialize<DeeplSettingsDescriptor>(json)!;
    }

    public static void Save(this DeeplSettingsDescriptor descriptor)
    {
        if (descriptor.IsPaidPlan != CurrentSettings.IsPaidPlan)
        {
            var translationUrl = CurrentSettings.IsPaidPlan
                ? "https://api.deepl.com/v2/"
                : "https://api-free.deepl.com/v2";
            var usageUrl = CurrentSettings.IsPaidPlan
                ? "https://api.deepl.com/v2/"
                : "https://api-free.deepl.com/v2";
            TranslationUrl = $"{translationUrl}/translate";
            UsageUrl = $"{usageUrl}/usage";
        }
        
        descriptor.SaveToJson();
        CurrentSettings = descriptor;
    }

    private static void SaveToJson(this DeeplSettingsDescriptor descriptor)
    {
        var json = JsonSerializer.Serialize(descriptor);
        if (!Directory.Exists(JsonDirectoryPath))
        {
            Directory.CreateDirectory(JsonDirectoryPath);
        }
        File.WriteAllText(JsonFilePath, json);
        
        CurrentSettings = descriptor;
    }
}
