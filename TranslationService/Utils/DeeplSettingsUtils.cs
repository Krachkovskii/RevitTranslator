using System.Text.Json;
using TranslationService.Models;
using TranslationService.Validation;

namespace TranslationService.Utils;

/// <summary>
/// DeepL translation settings
/// </summary>
public static class DeeplSettingsUtils
{
    private const string AddinName = "RevitTranslator";
    private static readonly object LockObject = new();
    private static DeeplSettingsDescriptor? _currentSettings;

    private static string JsonDirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Autodesk",
        "ApplicationPlugins",
        AddinName);
    private static string JsonFilePath => Path.Combine(
        JsonDirectoryPath,
        "settings.json");

    public static DeeplSettingsDescriptor? CurrentSettings
    {
        get
        {
            lock (LockObject)
            {
                return _currentSettings;
            }
        }
        private set
        {
            lock (LockObject)
            {
                _currentSettings = value;
                value?.SetDeeplUrls();
            }
        }
    }

    public static string TranslationUrl { get; private set; } = string.Empty;
    public static string UsageUrl { get; private set; } = string.Empty;
    public static string LanguagesBaseUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Loads the settings from a JSON file.
    /// If file is not created yet, it is created with default settings.
    /// Automatically decrypts the API key if it's encrypted.
    /// </summary>
    /// <returns>True if settings were loaded successfully, false otherwise</returns>
    public static bool Load()
    {
        lock (LockObject)
        {
            try
            {
                if (!File.Exists(JsonFilePath))
                {
                    var newDescriptor = new DeeplSettingsDescriptor();
                    newDescriptor.SaveToJson();
                    return true;
                }

                var json = File.ReadAllText(JsonFilePath);
                var descriptor = JsonSerializer.Deserialize<DeeplSettingsDescriptor>(json);

                if (descriptor is null)
                {
                    return false;
                }

                if (descriptor.IsApiKeyEncrypted)
                {
                    descriptor.DeeplApiKey = ApiKeyEncryption.Decrypt(descriptor.DeeplApiKey);
                }
                else if (!string.IsNullOrWhiteSpace(descriptor.DeeplApiKey))
                {
                    var plainTextKey = descriptor.DeeplApiKey;
                    descriptor.DeeplApiKey = plainTextKey;
                    descriptor.IsApiKeyEncrypted = true;
                    descriptor.SaveToJson();
                }

                CurrentSettings = descriptor;
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                CurrentSettings = null;
                return false;
            }
        }
    }

    public static void Save(this DeeplSettingsDescriptor descriptor) => descriptor.SaveToJson();

    private static void SaveToJson(this DeeplSettingsDescriptor settingsDescriptor)
    {
        lock (LockObject)
        {
            try
            {
                if (!Directory.Exists(JsonDirectoryPath))
                {
                    Directory.CreateDirectory(JsonDirectoryPath);
                }

                var sanitizedApiKey = ApiKeyValidator.Sanitize(settingsDescriptor.DeeplApiKey);

                var descriptorToSave = new DeeplSettingsDescriptor
                {
                    IsPaidPlan = settingsDescriptor.IsPaidPlan,
                    DeeplApiKey = ApiKeyEncryption.Encrypt(sanitizedApiKey),
                    IsApiKeyEncrypted = true,
                    SourceLanguage = settingsDescriptor.SourceLanguage,
                    TargetLanguage = settingsDescriptor.TargetLanguage
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(descriptorToSave, options);
                File.WriteAllText(JsonFilePath, json);

                settingsDescriptor.DeeplApiKey = sanitizedApiKey;
                CurrentSettings = settingsDescriptor;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Failed to save settings. Please check file permissions.", ex);
            }
        }
    }

    private static void SetDeeplUrls(this DeeplSettingsDescriptor? descriptor)
    {
        if (descriptor is null) return;

        var baseUrl = descriptor.IsPaidPlan
            ? "https://api.deepl.com/v2"
            : "https://api-free.deepl.com/v2";

        TranslationUrl = $"{baseUrl}/translate";
        UsageUrl = $"{baseUrl}/usage";
        LanguagesBaseUrl = $"{baseUrl}/languages?type=";
    }
}
