using TranslationService.Validation;
using Xunit;
using Assert = Xunit.Assert;

namespace RevitTranslator.Tests;

public class ApiKeyValidatorTests
{
    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789abc:fx", true)] // Free plan
    [InlineData("12345678-1234-1234-1234-123456789abc", true)] // Pro plan
    [InlineData("ABCDEF12-ABCD-ABCD-ABCD-ABCDEF123456:fx", true)] // Uppercase
    [InlineData("abcdef12-abcd-abcd-abcd-abcdef123456", true)] // Lowercase
    [InlineData("AbCdEf12-aBcD-AbCd-aBcD-AbCdEf123456:FX", true)] // Mixed case
    public void TryValidate_ValidApiKeys_ReturnsTrue(string apiKey, bool expectedValid)
    {
        // Act
        var result = ApiKeyValidator.TryValidate(apiKey, out var sanitized, out var error);

        // Assert
        Assert.Equal(expectedValid, result);
        Assert.Equal(apiKey, sanitized);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("  12345678-1234-1234-1234-123456789abc:fx  ")] // Leading/trailing spaces
    [InlineData("\t12345678-1234-1234-1234-123456789abc\t")] // Tabs
    [InlineData("  12345678-1234-1234-1234-123456789abc  ")] // Spaces without :fx
    public void TryValidate_ApiKeyWithWhitespace_TrimsAndValidates(string apiKey)
    {
        // Act
        var result = ApiKeyValidator.TryValidate(apiKey, out var sanitized, out var error);

        // Assert
        Assert.True(result);
        Assert.Equal(apiKey.Trim(), sanitized);
        Assert.DoesNotContain(" ", sanitized);
        Assert.DoesNotContain("\t", sanitized);
        Assert.Null(error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\t")]
    public void TryValidate_NullOrEmptyApiKey_ReturnsFalse(string? apiKey)
    {
        // Act
        var result = ApiKeyValidator.TryValidate(apiKey, out var sanitized, out var error);

        // Assert
        Assert.False(result);
        Assert.Equal(string.Empty, sanitized);
        Assert.Equal("API key cannot be empty.", error);
    }

    [Theory]
    [InlineData("too-short")] // Too short
    [InlineData("12345678-1234-1234-1234")] // Incomplete UUID
    [InlineData("short:fx")] // Short with suffix
    public void TryValidate_TooShortApiKey_ReturnsFalse(string apiKey)
    {
        // Act
        var result = ApiKeyValidator.TryValidate(apiKey, out var sanitized, out var error);

        // Assert
        Assert.False(result);
        Assert.Contains("too short", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryValidate_TooLongApiKey_ReturnsFalse()
    {
        // Arrange
        var apiKey = new string('x', 51); // 51 characters

        // Act
        var result = ApiKeyValidator.TryValidate(apiKey, out var sanitized, out var error);

        // Assert
        Assert.False(result);
        Assert.Contains("too long", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789abg:fx")] // Invalid char 'g' at end
    [InlineData("1234567-1234-1234-1234-123456789abc:fx")] // Wrong segment length (7 instead of 8)
    [InlineData("12345678-12345-1234-1234-123456789abc")] // Wrong segment length (5 instead of 4)
    [InlineData("12345678_1234_1234_1234_123456789abc")] // Underscores instead of hyphens
    [InlineData("12345678-1234-1234-1234-123456789abc:free")] // Wrong suffix
    [InlineData("12345678-1234-1234-1234-12345678ZZZZ:fx")] // Invalid hex chars
    public void TryValidate_InvalidFormat_ReturnsFalse(string apiKey)
    {
        // Act
        var result = ApiKeyValidator.TryValidate(apiKey, out var sanitized, out var error);

        // Assert
        Assert.False(result);
        Assert.NotNull(error);
        // Error could be either "too short" or "format is invalid"
        var isValidError = error.Contains("too short", StringComparison.OrdinalIgnoreCase) ||
                           error.Contains("format is invalid", StringComparison.OrdinalIgnoreCase);
        Assert.True(isValidError, $"Expected validation error, got: {error}");
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789abc:fx", true)]
    [InlineData("12345678-1234-1234-1234-123456789abc:FX", true)] // Case insensitive
    [InlineData("12345678-1234-1234-1234-123456789abc", false)] // Pro plan
    [InlineData("  12345678-1234-1234-1234-123456789abc:fx  ", true)] // With whitespace
    public void IsFreePlan_VariousKeys_ReturnsCorrectResult(string apiKey, bool expectedFreePlan)
    {
        // Act
        var result = ApiKeyValidator.IsFreePlan(apiKey);

        // Assert
        Assert.Equal(expectedFreePlan, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsFreePlan_NullOrEmptyKey_ReturnsFalse(string? apiKey, bool expected)
    {
        // Act
        var result = ApiKeyValidator.IsFreePlan(apiKey);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("  test-api-key  ", "test-api-key")]
    [InlineData("\ttest-api-key\t", "test-api-key")]
    [InlineData("test-api-key", "test-api-key")]
    [InlineData("  ", "")]
    [InlineData("", "")]
    public void Sanitize_VariousInputs_TrimsWhitespace(string? input, string expected)
    {
        // Act
        var result = ApiKeyValidator.Sanitize(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Sanitize_NullInput_ReturnsEmpty()
    {
        // Act
        var result = ApiKeyValidator.Sanitize(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TryValidate_RealWorldExample_ValidatesCorrectly()
    {
        // Arrange - Realistic DeepL free API key format
        var freeKey = "a1b2c3d4-e5f6-7890-abcd-ef1234567890:fx";
        var proKey = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

        // Act
        var freeResult = ApiKeyValidator.TryValidate(freeKey, out var freeSanitized, out var freeError);
        var proResult = ApiKeyValidator.TryValidate(proKey, out var proSanitized, out var proError);

        // Assert
        Assert.True(freeResult);
        Assert.Null(freeError);
        Assert.Equal(freeKey, freeSanitized);
        Assert.True(ApiKeyValidator.IsFreePlan(freeKey));

        Assert.True(proResult);
        Assert.Null(proError);
        Assert.Equal(proKey, proSanitized);
        Assert.False(ApiKeyValidator.IsFreePlan(proKey));
    }

    [Fact]
    public void TryValidate_WithLeadingTrailingWhitespace_SanitizesAndValidates()
    {
        // Arrange
        var dirtyKey = "   a1b2c3d4-e5f6-7890-abcd-ef1234567890:fx   ";
        var expectedClean = "a1b2c3d4-e5f6-7890-abcd-ef1234567890:fx";

        // Act
        var result = ApiKeyValidator.TryValidate(dirtyKey, out var sanitized, out var error);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedClean, sanitized);
        Assert.Null(error);
        Assert.False(sanitized.StartsWith(" "));
        Assert.False(sanitized.EndsWith(" "));
    }
}
