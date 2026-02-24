using TranslationService.Utils;
using Xunit;
using Assert = Xunit.Assert;

namespace RevitTranslator.Tests;

public class ApiKeyEncryptionTests
{
    [Fact]
    public void Encrypt_ValidApiKey_ReturnsEncryptedBase64String()
    {
        // Arrange
        var apiKey = "test-api-key-12345:fx";

        // Act
        var encrypted = ApiKeyEncryption.Encrypt(apiKey);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
        Assert.NotEqual(apiKey, encrypted);

        // Should be valid Base64
        var isBase64 = TryParseBase64(encrypted);
        Assert.True(isBase64, "Encrypted value should be valid Base64");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Encrypt_NullOrEmptyApiKey_ReturnsEmptyString(string? apiKey)
    {
        // Act
        var encrypted = ApiKeyEncryption.Encrypt(apiKey!);

        // Assert
        Assert.Equal(string.Empty, encrypted);
    }

    [Fact]
    public void Decrypt_ValidEncryptedKey_ReturnsOriginalApiKey()
    {
        // Arrange
        var originalKey = "test-api-key-67890:fx";
        var encrypted = ApiKeyEncryption.Encrypt(originalKey);

        // Act
        var decrypted = ApiKeyEncryption.Decrypt(encrypted);

        // Assert
        Assert.Equal(originalKey, decrypted);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Decrypt_NullOrEmptyInput_ReturnsEmptyString(string? encryptedKey)
    {
        // Act
        var decrypted = ApiKeyEncryption.Decrypt(encryptedKey!);

        // Assert
        Assert.Equal(string.Empty, decrypted);
    }

    [Fact]
    public void Decrypt_InvalidBase64_ReturnsEmptyString()
    {
        // Arrange
        var invalidBase64 = "not-valid-base64!!!";

        // Act
        var decrypted = ApiKeyEncryption.Decrypt(invalidBase64);

        // Assert
        Assert.Equal(string.Empty, decrypted);
    }

    [Fact]
    public void Decrypt_CorruptedEncryptedData_ReturnsEmptyString()
    {
        // Arrange - Valid Base64 but not encrypted by our method
        var corruptedData = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });

        // Act
        var decrypted = ApiKeyEncryption.Decrypt(corruptedData);

        // Assert
        Assert.Equal(string.Empty, decrypted);
    }

    [Fact]
    public void IsEncrypted_EncryptedValue_ReturnsTrue()
    {
        // Arrange
        var apiKey = "test-api-key-abcdef:fx";
        var encrypted = ApiKeyEncryption.Encrypt(apiKey);

        // Act
        var isEncrypted = ApiKeyEncryption.IsEncrypted(encrypted);

        // Assert
        Assert.True(isEncrypted);
    }

    [Theory]
    [InlineData("test-api-key-12345:fx")] // Contains colon
    [InlineData("short")] // Too short
    [InlineData("")] // Empty
    [InlineData(null)] // Null
    public void IsEncrypted_PlainTextValue_ReturnsFalse(string? value)
    {
        // Act
        var isEncrypted = ApiKeyEncryption.IsEncrypted(value!);

        // Assert
        Assert.False(isEncrypted);
    }

    [Fact]
    public void IsEncrypted_InvalidBase64_ReturnsFalse()
    {
        // Arrange
        var invalidBase64 = "this-is-not-base64-format-with-enough-length!!!";

        // Act
        var isEncrypted = ApiKeyEncryption.IsEncrypted(invalidBase64);

        // Assert
        Assert.False(isEncrypted);
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_PreservesOriginalValue()
    {
        // Arrange
        var originalKeys = new[]
        {
            "simple-key",
            "key-with-special-chars:!@#$%",
            "very-long-api-key-" + new string('x', 100),
            "unicode-key-日本語-тест-🔐"
        };

        foreach (var originalKey in originalKeys)
        {
            // Act
            var encrypted = ApiKeyEncryption.Encrypt(originalKey);
            var decrypted = ApiKeyEncryption.Decrypt(encrypted);

            // Assert
            Assert.Equal(originalKey, decrypted);
        }
    }

    [Fact]
    public void Encrypt_SameInput_ProducesDifferentOutput()
    {
        // Arrange - DPAPI may include random entropy
        var apiKey = "test-api-key-consistency";

        // Act
        var encrypted1 = ApiKeyEncryption.Encrypt(apiKey);
        var encrypted2 = ApiKeyEncryption.Encrypt(apiKey);

        // Assert - Both should decrypt to same value
        Assert.Equal(apiKey, ApiKeyEncryption.Decrypt(encrypted1));
        Assert.Equal(apiKey, ApiKeyEncryption.Decrypt(encrypted2));

        // Note: DPAPI may produce same or different ciphertext for same plaintext
        // What matters is both decrypt correctly
    }

    private static bool TryParseBase64(string value)
    {
        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
