using System.Security.Cryptography;
using System.Text;

namespace TranslationService.Utils;

/// <summary>
/// Provides encryption and decryption for API keys using Windows Data Protection API (DPAPI)
/// </summary>
public static class ApiKeyEncryption
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("RevitTranslator.DeepL.ApiKey.v1");

    /// <summary>
    /// Encrypts an API key using Windows DPAPI
    /// </summary>
    /// <param name="apiKey">The plain text API key to encrypt</param>
    /// <returns>Base64-encoded encrypted API key, or empty string if input is null/empty</returns>
    public static string Encrypt(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return string.Empty;
        }

        try
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(apiKey);
            var encryptedBytes = ProtectedData.Protect(plainTextBytes, Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (CryptographicException)
        {
            // If encryption fails, return empty string
            return string.Empty;
        }
    }

    /// <summary>
    /// Decrypts an API key using Windows DPAPI
    /// </summary>
    /// <param name="encryptedApiKey">The Base64-encoded encrypted API key</param>
    /// <returns>Decrypted plain text API key, or empty string if decryption fails</returns>
    public static string Decrypt(string encryptedApiKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedApiKey))
        {
            return string.Empty;
        }

        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedApiKey);
            var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception)
        {
            // If decryption fails (corrupted data, wrong user, etc.), return empty string
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if a string appears to be encrypted (Base64 format check)
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if the string appears to be Base64-encoded, false otherwise</returns>
    public static bool IsEncrypted(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // DeepL API keys contain colons and hyphens, encrypted values are Base64
        // This is a heuristic check
        if (value.Contains(':') || value.Length < 40)
        {
            return false; // Likely a plain text API key
        }

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
