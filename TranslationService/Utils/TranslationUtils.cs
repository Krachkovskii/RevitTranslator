using System.Diagnostics;
using System.Net.Http.Headers;

namespace TranslationService.Utils;

/// <summary>
/// DeepL-related utils
/// </summary>
public class TranslationUtils
{
    private static int _characterCount;
    private static int _completedTranslationsCount;
    private static int _translationsCount;
    private readonly string _apiTranslateUrl;
    private readonly string _apiUsageUrl;
    private readonly string _baseApi;
    private readonly HttpClient _httpClient;
    private readonly DeeplSettingsDescriptor _settingsDescriptor;

    public TranslationUtils(DeeplSettingsDescriptor settingsDescriptor)
    {
        _settingsDescriptor = settingsDescriptor;
        _httpClient = new HttpClient();
        _baseApi = _settingsDescriptor.IsPaidPlan
            ? "https://api.deepl.com/v2/"
            : "https://api-free.deepl.com/v2/";
        _apiTranslateUrl = $"{_baseApi}translate";
        _apiUsageUrl = $"{_baseApi}usage";

        if (string.IsNullOrWhiteSpace(settingsDescriptor.DeeplApiKey))
        {
            // without this block, the line below throws an exception, if settingsDescriptor are empty.
            return;
        }

        Task.Run(GetUsageAsync).Wait();
    }

    /// <summary>
    /// Counter for translated symbols for this run
    /// </summary>
    public static int CharacterCount { get; private set; }

    /// <summary>
    /// Counter for completed translations for this run
    /// </summary>
    public static int CompletedTranslationsCount { get; private set; }
    /// <summary>
    /// Translation limits for current DeepL plan
    /// </summary>
    public static int Limit { get; private set; }

    /// <summary>
    /// Counter for translated symbols for current billing period
    /// </summary>
    public static int Usage { get; private set; }

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
    public bool TryTestTranslate(DeeplSettingsDescriptor settingsUtils)
    {
        if (string.IsNullOrWhiteSpace(_settingsDescriptor.DeeplApiKey))
        {
            ShowCantTranslateMessage();
            return false;
        }

        // Perform test translation to see if translation can be performed.
        // This action is done every time settingsUtils are saved or when a command is executed.
        try
        {
            var test = Task.Run(async () =>
            {
                // var utils = new TranslationUtils(settingsUtils, new ProgressWindowUtils());
                // var res = await utils.TranslateBaseAsync("bonjour", new CancellationTokenSource().Token);
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
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("DeepL-Auth-Key", _settingsDescriptor.DeeplApiKey);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RevitTranslator");

        var response = await _httpClient.GetAsync(_apiUsageUrl);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var usage = JsonConvert.DeserializeObject<DeeplUsage>(responseBody);
        if (usage is null) return;

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
        // _progressWindowUtils.UpdateCurrent(finished);
    }

    /// <summary>
    /// Clears all properties related to translation count.
    /// </summary>
    public static void ClearTranslationCount()
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
    public static bool ProceedWithUpdate()
    {
        var result = MessageBox.Show($"Translation process was interrupted, or an error was thrown.\n" +
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
    public async Task<string?> TranslateTextAsync(string text, CancellationToken token)
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
        System.Windows.MessageBox.Show("Your settingsUtils configuration cannot be used for translation.\n" +
                "Please make sure everything is correct:\n" +
                "• API key\n" +
                "• Target language\n" +
                "• Paid/Free plan\n" +
                "• Translation limits.",
                "Incorrect settingsUtils",
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
    private async Task<string?> TranslateBaseAsync(string text, CancellationToken token)
    {
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("auth_key", _settingsDescriptor.DeeplApiKey),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("context", "(This is a property of an element in a BIM Model)"),
            new KeyValuePair<string, string>("target_lang", _settingsDescriptor.TargetLanguage.LanguageCode),
            new KeyValuePair<string, string?>("source_lang", _settingsDescriptor.SourceLanguage?.LanguageCode)
        ]);

        var response = await _httpClient.PostAsync(_apiTranslateUrl, content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var translationResult = JsonConvert.DeserializeObject<TranslationResult>(responseBody);
        if (translationResult is null) return string.Empty;

        return translationResult.Translations[0].Text;
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