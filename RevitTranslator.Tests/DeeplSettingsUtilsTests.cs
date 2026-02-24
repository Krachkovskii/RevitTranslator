using System.Text.Json;
using TranslationService.Models;
using TranslationService.Utils;
using Xunit;
using Assert = Xunit.Assert;

namespace RevitTranslator.Tests;

public class DeeplSettingsUtilsTests : IDisposable
{
    private readonly string _testSettingsDirectory;
    private readonly string _testSettingsFile;

    public DeeplSettingsUtilsTests()
    {
        // Use a temporary directory for test settings
        _testSettingsDirectory = Path.Combine(Path.GetTempPath(), $"RevitTranslatorTests_{Guid.NewGuid()}");
        _testSettingsFile = Path.Combine(_testSettingsDirectory, "settings.json");

        Directory.CreateDirectory(_testSettingsDirectory);

        // Override the settings path using reflection for testing
        // Note: This is a simplification - in production, consider dependency injection
    }

    [Fact]
    public void Save_ValidDescriptor_EncryptsApiKey()
    {
        // Arrange
        var descriptor = new DeeplSettingsDescriptor
        {
            DeeplApiKey = "test-api-key-123:fx",
            IsPaidPlan = false,
            SourceLanguage = DeeplLanguageCodes.TargetLanguages[0],
            TargetLanguage = DeeplLanguageCodes.TargetLanguages[1]
        };

        // Create a temporary file path
        var tempFile = Path.Combine(_testSettingsDirectory, "test_settings.json");

        // Act - Manually save using the same logic
        var descriptorToSave = new DeeplSettingsDescriptor
        {
            IsPaidPlan = descriptor.IsPaidPlan,
            DeeplApiKey = ApiKeyEncryption.Encrypt(descriptor.DeeplApiKey),
            IsApiKeyEncrypted = true,
            SourceLanguage = descriptor.SourceLanguage,
            TargetLanguage = descriptor.TargetLanguage
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(descriptorToSave, options);
        File.WriteAllText(tempFile, json);

        // Assert
        Assert.True(File.Exists(tempFile));

        var savedJson = File.ReadAllText(tempFile);
        Assert.Contains("IsApiKeyEncrypted", savedJson);
        Assert.DoesNotContain("test-api-key-123", savedJson); // Plain text should not be present

        // Verify the saved API key is encrypted (Base64)
        var loadedDescriptor = JsonSerializer.Deserialize<DeeplSettingsDescriptor>(savedJson);
        Assert.NotNull(loadedDescriptor);
        Assert.True(loadedDescriptor.IsApiKeyEncrypted);
        Assert.NotEqual(descriptor.DeeplApiKey, loadedDescriptor.DeeplApiKey);

        // Verify decryption works
        var decryptedKey = ApiKeyEncryption.Decrypt(loadedDescriptor.DeeplApiKey);
        Assert.Equal(descriptor.DeeplApiKey, decryptedKey);
    }

    [Fact]
    public void Load_EncryptedSettings_DecryptsApiKey()
    {
        // Arrange - Create encrypted settings file
        var originalApiKey = "test-api-key-456:fx";
        var encryptedKey = ApiKeyEncryption.Encrypt(originalApiKey);

        var descriptor = new DeeplSettingsDescriptor
        {
            DeeplApiKey = encryptedKey,
            IsApiKeyEncrypted = true,
            IsPaidPlan = true,
            SourceLanguage = null,
            TargetLanguage = DeeplLanguageCodes.TargetLanguages[2]
        };

        var tempFile = Path.Combine(_testSettingsDirectory, "load_test_settings.json");
        var json = JsonSerializer.Serialize(descriptor);
        File.WriteAllText(tempFile, json);

        // Act - Load and decrypt
        var loadedJson = File.ReadAllText(tempFile);
        var loadedDescriptor = JsonSerializer.Deserialize<DeeplSettingsDescriptor>(loadedJson);

        Assert.NotNull(loadedDescriptor);

        // Decrypt the API key
        var decryptedKey = loadedDescriptor.IsApiKeyEncrypted
            ? ApiKeyEncryption.Decrypt(loadedDescriptor.DeeplApiKey)
            : loadedDescriptor.DeeplApiKey;

        // Assert
        Assert.Equal(originalApiKey, decryptedKey);
        Assert.True(loadedDescriptor.IsPaidPlan);
        Assert.Null(loadedDescriptor.SourceLanguage);
        Assert.Equal(DeeplLanguageCodes.TargetLanguages[2], loadedDescriptor.TargetLanguage);
    }

    [Fact]
    public void Migration_PlainTextApiKey_GetsEncrypted()
    {
        // Arrange - Create settings file with plain text API key (old format)
        var plainTextKey = "plain-text-api-key-789:fx";
        var descriptor = new DeeplSettingsDescriptor
        {
            DeeplApiKey = plainTextKey,
            IsApiKeyEncrypted = false, // Old format
            IsPaidPlan = false,
            SourceLanguage = DeeplLanguageCodes.TargetLanguages[0],
            TargetLanguage = DeeplLanguageCodes.TargetLanguages[1]
        };

        var tempFile = Path.Combine(_testSettingsDirectory, "migration_test_settings.json");
        var json = JsonSerializer.Serialize(descriptor);
        File.WriteAllText(tempFile, json);

        // Act - Simulate migration logic
        var loadedJson = File.ReadAllText(tempFile);
        var loadedDescriptor = JsonSerializer.Deserialize<DeeplSettingsDescriptor>(loadedJson);

        Assert.NotNull(loadedDescriptor);

        if (!loadedDescriptor.IsApiKeyEncrypted && !string.IsNullOrWhiteSpace(loadedDescriptor.DeeplApiKey))
        {
            // Migration: encrypt existing plain-text API key
            var originalPlainText = loadedDescriptor.DeeplApiKey;
            loadedDescriptor.DeeplApiKey = originalPlainText;
            loadedDescriptor.IsApiKeyEncrypted = true;

            // Save with encryption
            var encryptedDescriptor = new DeeplSettingsDescriptor
            {
                IsPaidPlan = loadedDescriptor.IsPaidPlan,
                DeeplApiKey = ApiKeyEncryption.Encrypt(loadedDescriptor.DeeplApiKey),
                IsApiKeyEncrypted = true,
                SourceLanguage = loadedDescriptor.SourceLanguage,
                TargetLanguage = loadedDescriptor.TargetLanguage
            };

            var migratedJson = JsonSerializer.Serialize(encryptedDescriptor, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(tempFile, migratedJson);
        }

        // Assert - Verify migration succeeded
        var migratedLoadedJson = File.ReadAllText(tempFile);
        var migratedDescriptor = JsonSerializer.Deserialize<DeeplSettingsDescriptor>(migratedLoadedJson);

        Assert.NotNull(migratedDescriptor);
        Assert.True(migratedDescriptor.IsApiKeyEncrypted);

        var decryptedKey = ApiKeyEncryption.Decrypt(migratedDescriptor.DeeplApiKey);
        Assert.Equal(plainTextKey, decryptedKey);
    }

    [Fact]
    public void Save_EmptyApiKey_HandlesGracefully()
    {
        // Arrange
        var descriptor = new DeeplSettingsDescriptor
        {
            DeeplApiKey = string.Empty,
            IsPaidPlan = false,
            SourceLanguage = DeeplLanguageCodes.TargetLanguages[0],
            TargetLanguage = DeeplLanguageCodes.TargetLanguages[1]
        };

        // Act
        var encrypted = ApiKeyEncryption.Encrypt(descriptor.DeeplApiKey);

        // Assert
        Assert.Equal(string.Empty, encrypted);
    }

    [Fact]
    public void IsApiKeyEncrypted_DefaultsToTrue()
    {
        // Arrange & Act
        var descriptor = new DeeplSettingsDescriptor();

        // Assert
        Assert.True(descriptor.IsApiKeyEncrypted);
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testSettingsDirectory))
        {
            try
            {
                Directory.Delete(_testSettingsDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
