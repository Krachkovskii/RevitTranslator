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

    private static DeeplSettingsDescriptor? _currentSettings;
    public static DeeplSettingsDescriptor? CurrentSettings 
    { 
        get => _currentSettings;
        private set 
        {
            _currentSettings = value;
            SetDeeplUrls(value); 
        }
    }

    public static string TranslationUrl { get; private set; } = string.Empty;
    public static string UsageUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Loads the settings from a JSON file. 
    /// If file is not created yet, it is created with default settings.
    /// </summary>
    /// <returns>An instance of the Settings class with the loaded settings.</returns>
    public static void Load()
    {
        if (!File.Exists(JsonFilePath))
        {
            var descriptor = new DeeplSettingsDescriptor();
            descriptor.SaveToJson();
            return;
        }
        
        var json = File.ReadAllText(JsonFilePath);
        CurrentSettings = JsonSerializer.Deserialize<DeeplSettingsDescriptor>(json)!;
    }

    public static void Save(this DeeplSettingsDescriptor descriptor) => descriptor.SaveToJson();

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

    private static void SetDeeplUrls(DeeplSettingsDescriptor? descriptor)
    {
        if (descriptor is null) return;
        
        var translationUrl = descriptor.IsPaidPlan
            ? "https://api.deepl.com/v2"
            : "https://api-free.deepl.com/v2";
        var usageUrl = descriptor.IsPaidPlan
            ? "https://api.deepl.com/v2"
            : "https://api-free.deepl.com/v2";
        
        TranslationUrl = $"{translationUrl}/translate";
        UsageUrl = $"{usageUrl}/usage";
    }
}
