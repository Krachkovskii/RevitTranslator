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
    private static DeeplSettingsDto? _currentSettings;

    private static string JsonDirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Autodesk",
        "ApplicationPlugins",
        AddinName);

    private static string JsonFilePath => Path.Combine(JsonDirectoryPath, "settings.json");

    public static string TranslationUrl { get; private set; } = string.Empty;
    public static string UsageUrl { get; private set; } = string.Empty;
    public static string LanguagesBaseUrl { get; private set; } = string.Empty;

    public static DeeplSettingsDto? CurrentSettings
    {
        get
        {
            lock (LockObject)
            {
                return _currentSettings;
            }
        }
        // ReSharper disable once MemberCanBePrivate.Global
        set
        {
            lock (LockObject)
            {
                _currentSettings = value;
                value?.SetDeeplUrls();
            }
        }
    }

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
                    var newDescriptor = new DeeplSettingsDto();
                    newDescriptor.SaveToJson();
                    return true;
                }

                var json = File.ReadAllText(JsonFilePath);
                var descriptor = JsonSerializer.Deserialize<DeeplSettingsDto>(json);

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

    /// <summary>
    /// Validates that settings are usable for translation:
    /// a non-null target language and a working API key verified via a real DeepL request.
    /// Loads settings from disk if not yet loaded.
    /// </summary>
    public static async Task<bool> ValidateAsync(DeeplTranslationClient client)
    {
        if (CurrentSettings is null) Load();
        if (CurrentSettings?.TargetLanguage is null) return false;

        return await client.CanTranslateAsync();
    }

    public static void UpdateInMemory(this DeeplSettingsDto descriptor)
    {
        lock (LockObject)
        {
            _currentSettings = descriptor;
            descriptor.SetDeeplUrls();
        }
    }

    public static void Save(this DeeplSettingsDto descriptor) => descriptor.SaveToJson();

    private static void SaveToJson(this DeeplSettingsDto settingsDescriptor)
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

                var descriptorToSave = new DeeplSettingsDto
                {
                    IsPaidPlan = settingsDescriptor.IsPaidPlan,
                    DeeplApiKey = ApiKeyEncryption.Encrypt(sanitizedApiKey),
                    IsApiKeyEncrypted = true,
                    SourceLanguage = settingsDescriptor.SourceLanguage,
                    TargetLanguage = settingsDescriptor.TargetLanguage
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
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

    private static void SetDeeplUrls(this DeeplSettingsDto? descriptor)
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
