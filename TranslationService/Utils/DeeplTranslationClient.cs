using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using TranslationService.JsonProperties;
using TranslationService.Models;
// ReSharper disable once RedundantUsingDirective
using System.Net.Http;

namespace TranslationService.Utils;

public sealed class DeeplTranslationClient
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _semaphore = new(1, 5);
    private readonly TimeSpan _baseRetryDelay;
    private readonly DeeplSettingsDescriptor? _settingsOverride;
    private readonly string? _translationUrlOverride;
    private readonly string? _usageUrlOverride;

    private DeeplSettingsDescriptor? Settings => _settingsOverride ?? DeeplSettingsUtils.CurrentSettings;
    private string TranslationUrl => _translationUrlOverride ?? DeeplSettingsUtils.TranslationUrl;
    private string UsageUrl => _usageUrlOverride ?? DeeplSettingsUtils.UsageUrl;

    public int Limit { get; private set; } = -1;
    public int Usage { get; private set; } = -1;

    public DeeplTranslationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseRetryDelay = TimeSpan.FromMilliseconds(200);
    }

    internal DeeplTranslationClient(
        HttpClient httpClient,
        DeeplSettingsDescriptor settings,
        string translationUrl,
        string usageUrl,
        TimeSpan baseRetryDelay)
    {
        _httpClient = httpClient;
        _settingsOverride = settings;
        _translationUrlOverride = translationUrl;
        _usageUrlOverride = usageUrl;
        _baseRetryDelay = baseRetryDelay;
    }

    /// <summary>
    /// Checks if translation can be performed based on the provided settings.
    /// This method attempts to translate a single word.
    /// </summary>
    /// <returns>
    /// True if translation can be performed, false otherwise.
    /// </returns>
    public async Task<bool> CanTranslateAsync()
    {
        return await CheckUsageAsync() && Settings is not null;
    }
    
    public Task<IReadOnlyCollection<LanguageDescriptor>> GetSourceLanguagesAsync(string apiKey = "") => GetLanguagesAsync(false, apiKey);
    public Task<IReadOnlyCollection<LanguageDescriptor>> GetTargetLanguagesAsync(string apiKey = "") => GetLanguagesAsync(true, apiKey);

    private async Task<IReadOnlyCollection<LanguageDescriptor>> GetLanguagesAsync(bool getTargetLanguages, string apiKey)
    {
        try
        {
            if (Settings is null) return [];

            var parameter = getTargetLanguages ? "target" : "source";
            var languagesUrl = $"{DeeplSettingsUtils.LanguagesBaseUrl}{parameter}";
            using var request = new HttpRequestMessage(HttpMethod.Get, languagesUrl);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("DeepL-Auth-Key", 
                    string.IsNullOrEmpty(Settings?.DeeplApiKey) ? apiKey : Settings!.DeeplApiKey);
            request.Headers.UserAgent.ParseAdd("RevitTranslator");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return [];

            var responseBody = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<ApiLanguageItem[]>(responseBody, JsonSettings.Options);
            if (entries is null) return [];

            return entries
                .Select(entry => new LanguageDescriptor(entry.Name, string.Empty, entry.Language))
                .ToArray();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or TimeoutException)
        {
            return [];
        }
    }
    
    public async Task<bool> CheckUsageAsync()
    {
        try
        {
            if (Settings is null) return false;

            using var request = new HttpRequestMessage(HttpMethod.Get, UsageUrl);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("DeepL-Auth-Key", Settings.DeeplApiKey);
            request.Headers.UserAgent.ParseAdd("RevitTranslator");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var responseBody = await response.Content.ReadAsStringAsync();
            var usage = JsonSerializer.Deserialize<DeeplUsage>(responseBody, JsonSettings.Options);
            if (usage is null) return false;

            Usage = usage.CharacterCount;
            Limit = usage.CharacterLimit;

            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or TimeoutException)
        {
            return false;
        }
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
    public async Task<string?> TranslateTextAsync(string text, CancellationToken token)
    {
        var acquired = false;
        try
        {
            await _semaphore.WaitAsync(token);
            acquired = true;
            token.ThrowIfCancellationRequested();
            return await ProcessTranslationRequestAsync(text, token);
        }
        catch (Exception ex) when (ex is OperationCanceledException or HttpRequestException or JsonException or TaskCanceledException)
        {
            return null;
        }
        finally
        {
            if (acquired) _semaphore.Release();
        }
    }

    private async Task<string?> ProcessTranslationRequestAsync(string text, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            var response = await SendTranslationRequestWithRateLimitAsync(text, token);
            if (response is null) return null;

            var responseBody = await response.Content.ReadAsStringAsync();
            var translationResult = JsonSerializer.Deserialize<TranslationResult>(responseBody, JsonSettings.Options);

            return translationResult?.Translations[0].Text;
        }
        catch (Exception ex) when (ex is OperationCanceledException or JsonException or InvalidOperationException)
        {
            return null;
        }
    }

    private async Task<HttpResponseMessage?> SendTranslationRequestWithRateLimitAsync(string text, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            var retryCount = 0;
            var retryLimit = 20;
            while (true)
            {
                if (retryCount > retryLimit) return null;

                using var content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("text", text),
                    new KeyValuePair<string, string>("context", "(This is a property of an element in a BIM Model)"),
                    new KeyValuePair<string, string>("target_lang", Settings!.TargetLanguage.TargetLanguageCode),
                    new KeyValuePair<string, string?>("source_lang", Settings.SourceLanguage?.SourceLanguageCode)
                ]);
                using var request = new HttpRequestMessage(HttpMethod.Post, TranslationUrl)
                {
                    Content = content
                };
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("DeepL-Auth-Key", Settings!.DeeplApiKey);
                request.Headers.UserAgent.ParseAdd("RevitTranslator");

                var response = await _httpClient.SendAsync(request, token);
                switch (response.StatusCode)
                {
                    case (HttpStatusCode) 429:
                        // too many requests; try again after a delay
                        retryCount++;
                        await Task.Delay(TimeSpan.FromMilliseconds(retryCount * _baseRetryDelay.TotalMilliseconds), token);
                        continue;

                    case (HttpStatusCode) 400 // bad request (wrong parameters)
                        or (HttpStatusCode) 404: // URL is wrong
                        // cannot proceed after these codes
                        throw new OperationCanceledException(
                            "Unknown internal exception. Please contact developer via e-mail or LinkedIn.");

                    case (HttpStatusCode) 403: // authorization error
                        throw new OperationCanceledException(
                            "Authorisation failed. Please check your API key and \"Pro\" plan checkbox.");

                    case (HttpStatusCode) 456: // quota exceeded
                        throw new OperationCanceledException(
                            "Quota exceeded. Please try again later.");

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
        catch (OperationCanceledException exception)
        {
            StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage(exception.Message));
            return null;
        }
        catch (HttpRequestException ex) when (IsNetworkUnavailable(ex))
        {
            StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage("Network error occurred. Please check your connection."));
            return null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    private static bool IsNetworkUnavailable(HttpRequestException ex)
    {
#if NET8_0_OR_GREATER
        return ex.HttpRequestError is HttpRequestError.NameResolutionError
            or HttpRequestError.ConnectionError;
#else
        return ex.InnerException is SocketException socketEx
               && socketEx.SocketErrorCode is (SocketError.NetworkUnreachable
                   or SocketError.NetworkDown
                   or SocketError.HostUnreachable
                   or SocketError.HostNotFound);
#endif
    }
}
