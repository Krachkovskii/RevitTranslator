using System.Net.Http.Headers;
using System.Text.Json;
using TranslationService.JsonProperties;
// ReSharper disable once RedundantUsingDirective
using System.Net.Http;

namespace TranslationService.Utils;

/// <summary>
/// DeepL-related utils
/// </summary>
public static class TranslationUtils
{
    private static readonly HttpClient HttpClient = new();

    // TODO: Move these properties to a Settings Descriptor
    /// <summary>
    /// Translation limits for current DeepL plan
    /// </summary>
    public static int Limit { get; private set; } = -1;

    /// <summary>
    /// Counter for translated symbols for current billing period
    /// </summary>
    public static int Usage { get; private set; } = -1;

    /// <summary>
    /// Checks if translation can be performed based on the provided settingsUtils. 
    /// This method attempts to translate a single word.
    /// </summary>
    /// <param name="settingsUtils">
    /// The Settings object containing the API key and target language.
    /// </param>
    /// <returns>
    /// True if translation can be performed, false otherwise.
    /// </returns>
    public static bool TryTestTranslate()
    {
        if (string.IsNullOrWhiteSpace(DeeplSettingsUtils.CurrentSettings.DeeplApiKey)) return false;

        var test = Task.Run(async () =>
        {
            var res = await TranslateTextAsync("bonjour", new CancellationTokenSource().Token);
            return res is not null;
        }).Result;
        return test;
    }

    public static async Task CheckUsageAsync()
    {
        HttpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("DeepL-Auth-Key", DeeplSettingsUtils.CurrentSettings.DeeplApiKey);
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RevitTranslator");

        var response = await HttpClient.GetAsync(DeeplSettingsUtils.UsageUrl);
        if (!response.IsSuccessStatusCode) return;

        var responseBody = await response.Content.ReadAsStringAsync();
        var usage = JsonSerializer.Deserialize<DeeplUsage>(responseBody, JsonSettings.Options);
        if (usage is null) return;

        Usage = usage.CharacterCount;
        Limit = usage.CharacterLimit;
    }
    
    /// <summary>
    /// Translates a given text using the DeepL translation API.
    /// This is a base method that simply returns translated text.
    /// </summary>
    /// <param name="text">
    /// Text to be translated.
    /// </param>
    /// <returns>
    /// Translated text.
    /// </returns>
    public static async Task<string?> TranslateTextAsync(string text, CancellationToken token)
    {
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("auth_key", DeeplSettingsUtils.CurrentSettings.DeeplApiKey),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("context", "(This is a property of an element in a BIM Model)"),
            new KeyValuePair<string, string>("target_lang", DeeplSettingsUtils.CurrentSettings.TargetLanguage.TargetLanguageCode),
            new KeyValuePair<string, string?>("source_lang", DeeplSettingsUtils.CurrentSettings.SourceLanguage?.SourceLanguageCode)
        ]);

        //TODO: implement response code processing to handle server-side errors
        var response = await HttpClient.PostAsync(DeeplSettingsUtils.TranslationUrl, content, token);
        if (!response.IsSuccessStatusCode) return null;
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var translationResult = JsonSerializer.Deserialize<TranslationResult>(responseBody, JsonSettings.Options);
        return translationResult?.Translations[0].Text;
    }
}