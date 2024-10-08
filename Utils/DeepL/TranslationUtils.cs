﻿using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using Newtonsoft.Json;
using RevitTranslatorAddin.Utils.App;

namespace RevitTranslatorAddin.Utils.DeepL;

/// <summary>
/// DeepL-related utils
/// </summary>
public class TranslationUtils
{
    private static int _characterCount = 0;

    private static int _completedTranslationsCount = 0;

    private static int _translationsCount = 0;

    private readonly string _apiTranslateUrl = null;

    private readonly string _apiUsageUrl = null;

    private readonly string _baseApi = "https://api-free.deepl.com/v2/";

    private readonly HttpClient _httpClient = null;

    private readonly Models.DeeplSettings _settings = null;

    public TranslationUtils(Models.DeeplSettings settings, ProgressWindowUtils progressWindowUtils)
    {
        _settings = settings;
        _httpClient = new HttpClient();
        _baseApi = _settings.IsPaidPlan
            ? "https://api.deepl.com/v2/"
            : "https://api-free.deepl.com/v2/";
        _apiTranslateUrl = $"{_baseApi}translate";
        _apiUsageUrl = $"{_baseApi}usage";
        _progressWindowUtils = progressWindowUtils;

        Task.Run(() => GetUsageAsync()).Wait();
    }

    /// <summary>
    /// Counter for translated symbols for this run
    /// </summary>
    internal static int CharacterCount { get; private set; } = 0;

    /// <summary>
    /// Counter for completed translations for this run
    /// </summary>
    internal static int CompletedTranslationsCount { get; private set; } = 0;
    /// <summary>
    /// Translation limits for current DeepL plan
    /// </summary>
    internal static int Limit { get; private set; } = 0;

    /// <summary>
    /// Counter for translated symbols for current billing period
    /// </summary>
    internal static int Usage { get; private set; } = 0;
    private ProgressWindowUtils _progressWindowUtils { get; set; } = null;
    /// <summary>
    /// Checks if translation can be performed based on the provided settings. 
    /// This method attempts to translate a single word.
    /// </summary>
    /// <param name="settings">
    /// The Settings object containing the API key and target language.
    /// </param>
    /// <returns>
    /// True if translation can be performed, false otherwise.
    /// </returns>
    public bool CanTranslate(Models.DeeplSettings settings)
    {
        if (settings.DeeplApiKey == null || settings.TargetLanguage == null)
        {
            ShowCantTranslateMessage();
            return false;
        }

        // Perform test translation to see if translation can be performed.
        // This action is done every time settings are saved or when a command is executed.
        try
        {
            var test = Task.Run(async () =>
            {
                var utils = new TranslationUtils(settings, new ProgressWindowUtils());
                var res = await utils.TranslateBaseAsync("bonjour", new CancellationTokenSource().Token);
                return true;
            }).GetAwaiter().GetResult();

            return test;
        }

        catch (Exception e)
        {
            ShowCantTranslateMessage();
            Debug.WriteLine(e.Message);

            return false;
        }
    }

    //TODO: switch to returning (int, int); do not set properties inside the function. Instead, set properties in the main block
    /// <summary>
    /// Retrieves monthly usage and limits for this API key. Sets corresponding properties.
    /// </summary>
    public async Task GetUsageAsync()
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("DeepL-Auth-Key", _settings.DeeplApiKey);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RevitTranslator");

        var response = await _httpClient.GetAsync(_apiUsageUrl);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var usage = JsonConvert.DeserializeObject<DeepLUsage>(responseBody);

        Usage = usage.CharacterCount;
        Limit = usage.CharacterLimit;
    }

    /// <summary>
    /// Updates counter of finished translations
    /// </summary>
    public void UpdateCompletedCounter()
    {
        var finished = Interlocked.Increment(ref _completedTranslationsCount);
        CompletedTranslationsCount = finished;
        _progressWindowUtils.UpdateCurrent(finished);
    }

    /// <summary>
    /// Clears all properties related to translation count.
    /// </summary>
    internal static void ClearTranslationCount()
    {
        _translationsCount = 0;
        _completedTranslationsCount = 0;
        CompletedTranslationsCount = 0;
        _characterCount = 0;
        CharacterCount = 0;
    }

    /// <summary>
    /// Shows a warning window after translation was cancelled. 
    /// Determines whether the model should be updated.
    /// </summary>
    /// <returns>
    /// Bool value with user's response.
    /// </returns>
    internal static bool ProceedWithUpdate()
    {
        var result = System.Windows.MessageBox.Show($"Translation process was interrupted, or an error was thrown.\n" +
            $"Do you still want to update the model with translated text?",
            "Translation interrupted",
            System.Windows.MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result.Equals(System.Windows.MessageBoxResult.Yes))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Translates a given text using the DeepL translation API.
    /// This method is responsible for updating the translation count, calling the base translation method,
    /// and updating the progress window.
    /// </summary>
    /// <param name="text">
    /// The text to be translated.
    /// </param>
    /// <returns>
    /// Translated text.
    /// </returns>
    internal async Task<string> TranslateTextAsync(string text, CancellationToken token)
    {
        var translatedText = await TranslateBaseAsync(text, token);

        UpdateCharacterCounter(text);
        UpdateCompletedCounter();

        return translatedText;
    }

    /// <summary>
    /// Shows MessageBox with translations that can't be updated due to illegal characters
    /// </summary>
    private void ShowCantTranslateMessage()
    {
        System.Windows.MessageBox.Show("Your settings configuration cannot be used for translation.\n" +
                "Please make sure everything is correct:\n" +
                "• API key\n" +
                "• Target language\n" +
                "• Paid/Free plan\n" +
                "• Translation limits.",
                "Incorrect settings",
                System.Windows.MessageBoxButton.OK,
                MessageBoxImage.Warning);
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
    private async Task<string> TranslateBaseAsync(string text, CancellationToken token)
    {
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("auth_key", _settings.DeeplApiKey),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("context", "(This is a property of an element in a BIM Model)"),
            new KeyValuePair<string, string>("target_lang", _settings.Languages[_settings.TargetLanguage]),
            new KeyValuePair<string, string>("source_lang", _settings.SourceLanguage == null ? null : _settings.Languages[_settings.SourceLanguage])
        ]);

        var response = await _httpClient.PostAsync(_apiTranslateUrl, content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var translationResult = JsonConvert.DeserializeObject<TranslationResult>(responseBody);

        return translationResult?.Translations?[0]?.Text;
    }
    /// <summary>
    /// Updates counter of translated characters for this run
    /// </summary>
    /// <param name="text"></param>
    private void UpdateCharacterCounter(string text)
    {
        Interlocked.Add(ref _characterCount, text.Length);
        CharacterCount = _characterCount;
    }
}