using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
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
    private static readonly SemaphoreSlim Semaphore = new(5, 10);

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
    /// <returns>
    /// True if translation can be performed, false otherwise.
    /// </returns>
    public static bool TryTestTranslate()
    {
        if (string.IsNullOrWhiteSpace(DeeplSettingsUtils.CurrentSettings?.DeeplApiKey)) return false;

        var test = Task.Run(async () =>
        {
            var res = await ProcessTranslationRequest("bonjour", new CancellationTokenSource().Token);
            return res is not null;
        }).Result;
        return test;
    }

    public static async Task CheckUsageAsync()
    {
        HttpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("DeepL-Auth-Key", DeeplSettingsUtils.CurrentSettings!.DeeplApiKey);
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
    /// Translates text asynchronously. Can be used concurrently.
    /// </summary>
    /// <param name="text">Text to be translated.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Translated text; null if there has been a translation error.
    /// If server response does not allow further translation (e.g. quota exceeded,
    /// or request configuration is invalid), <c>OperationCanceledException</c> will be thrown, 
    /// and token cancellation will be requested.</returns>
    public static async Task<string?> Translate(string text, CancellationToken token)
    {
        await Semaphore.WaitAsync(token);
        try
        {
            token.ThrowIfCancellationRequested();
            return await ProcessTranslationRequest(text, token);
        }
        catch
        {
            return null;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static async Task<string?> ProcessTranslationRequest(string text, CancellationToken token)
    {
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("auth_key", DeeplSettingsUtils.CurrentSettings!.DeeplApiKey),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("context", "(This is a property of an element in a BIM Model)"),
            new KeyValuePair<string, string>("target_lang", DeeplSettingsUtils.CurrentSettings.TargetLanguage.TargetLanguageCode),
            new KeyValuePair<string, string?>("source_lang", DeeplSettingsUtils.CurrentSettings.SourceLanguage?.SourceLanguageCode)
        ]);

        try
        {
            token.ThrowIfCancellationRequested();
            
            var response = await SendTranslationRequestWithRateLimit(content, token);
            if (response is null) return null;
        
            // ReSharper disable once MethodSupportsCancellation
            var responseBody = await response.Content.ReadAsStringAsync();
            var translationResult = JsonSerializer.Deserialize<TranslationResult>(responseBody, JsonSettings.Options);
        
            return translationResult?.Translations[0].Text;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static async Task<HttpResponseMessage?> SendTranslationRequestWithRateLimit(FormUrlEncodedContent content, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            var retryCount = 0;
            var retryLimit = 5;
            while (true)
            {
                if (retryCount > retryLimit) return null;
                
                var response = await HttpClient.PostAsync(DeeplSettingsUtils.TranslationUrl, content, token);
                switch (response.StatusCode)
                {
                    case (HttpStatusCode) 429:
                        // too many requests; try again after a delay
                        retryCount++;
                        await Task.Delay(TimeSpan.FromMilliseconds(retryCount * 200), token);
                        continue;
                    
                    case (HttpStatusCode) 456 // quota exceeded
                        or (HttpStatusCode) 400 // bad request (wrong parameters)
                        or (HttpStatusCode) 403 // authorisation failed
                        or (HttpStatusCode) 404: // URL is wrong
                        // cannot proceed after these codes
                        throw new OperationCanceledException();
                    
                    case HttpStatusCode.OK:
                        return response;
                    
                    default:
                        // 413 - request too large
                        // 414 - URI too long
                        // 5** - server errors
                        // request-specific errors; can proceed with other requests
                        return null;
                }
            }
        }
        catch (OperationCanceledException)
        {
            StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage());
            return null;
        }
    }
}