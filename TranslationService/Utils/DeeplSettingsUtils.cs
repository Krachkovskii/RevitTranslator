using System.Xml;
using TranslationService.Models;

namespace TranslationService.Utils;

/// <summary>
/// DeepL translation settings
/// </summary>
public static class DeeplSettingsUtils
{
    private const string AddinName = "RevitTranslator";
    
    private static string _jsonDirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Autodesk",
        "ApplicationPlugins",
        AddinName);
    private static string _jsonFilePath => Path.Combine(
        _jsonDirectoryPath,
        "settings.json");

    /// <summary>
    /// Loads the settings from a JSON file.
    /// </summary>
    /// <returns>An instance of the Settings class with the loaded settings.</returns>
    public static DeeplSettingsDescriptor? TryLoadFromJson()
    {
        if (!File.Exists(_jsonFilePath)) return null;
        
        var json = File.ReadAllText(_jsonFilePath);
        return JsonConvert.Des<DeeplSettingsDescriptor>(json);
    }

    /// <summary>
    /// Saves the class instance to JSON file
    /// </summary>
    public static void SaveToJson(this DeeplSettingsDescriptor descriptor)
    {
        var json = JsonConvert.SerializeObject(descriptor, Formatting.Indented);
        if (!Directory.Exists(_jsonDirectoryPath))
        {
            Directory.CreateDirectory(_jsonDirectoryPath);
        }
        File.WriteAllText(_jsonFilePath, json);
    }
}
