using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.ViewModels;

namespace RevitTranslatorAddin.Utils.DeepL;

public class TranslationUtils
{
    private readonly Models.Settings _settings = null;
    private readonly string _apiUrl = "https://api-free.deepl.com/v2/translate";
    private readonly HttpClient _httpClient = null;
    private static int _translationsCount = 0;
    private static int _completedTranslationsCount = 0;
    private static int _characterCount = 0;
    internal static int CompletedTranslationsCount { get; private set; } = 0;
    internal static int TranslationsCount { get; private set; } = 0;
    internal static int CharacterCount { get; private set; } = 0;

    /// <summary>
    /// List of elements to translate, intended to use with asynchronous translation methods.
    /// Each tuple stores three values:
    /// - element to be translated, e.g. a parameter or a textbox; (object)
    /// - translated text; (string)
    /// - element-specific context for Revit elements that have multiple text fields, e.g. "above" or "header". (string)
    /// </summary>
    internal static ConcurrentBag<(object, string, string, ElementId)> Translations { get; set; } = [];

    public TranslationUtils(Models.Settings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient();
        _apiUrl = _settings.IsPaidPlan
            ? "https://api.deepl.com/v2/translate"
            : "https://api-free.deepl.com/v2/translate";
    }

    /// <summary>
    /// Checks if translation can be performed based on the provided settings. 
    /// This method tries to translate a single word.
    /// </summary>
    /// <param name="settings">The settings object containing the API key and target language.</param>
    /// <returns>True if translation can be performed, false otherwise.</returns>
    public static bool CanTranslate(Models.Settings settings)
    {
        if (settings.DeeplApiKey == null || settings.TargetLanguage == null)
        {
            MessageBox.Show("Your settings configuration cannot be used for translation.\n" +
                "Please make sure everything is correct:\n" +
                "• API key\n" +
                "• Target language\n" +
                "• Paid/Free plan\n" +
                "• Translation limits.",
                "Incorrect settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }

        // Perform test translation to see if translation can be performed.
        // This action is done every time settings are saved or when a command is executed.
        try
        {
            var test = Task.Run(async () =>
            {
                var utils = new TranslationUtils(settings);
                var res = await utils.TranslateBaseAsync("bonjour", new CancellationTokenSource().Token);
                return true;
            }).GetAwaiter().GetResult();

            return test;
        }

        catch (Exception e)
        {
            MessageBox.Show("Your settings configuration cannot be used for translation.\n" +
                "Please make sure everything is correct:\n" +
                "• API key\n" +
                "• Target language\n" +
                "• Paid/Free plan\n" +
                "• Translation limits.",
                "Incorrect settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            
            Debug.WriteLine(e.Message);

            return false;
        }
    }

    /// <summary>
    /// Translates a given text using the DeepL translation API.
    /// This is a base method that simply returns translated text.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <returns>The translated text.</returns>
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

        var response = await _httpClient.PostAsync(_apiUrl, content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var translationResult = JsonConvert.DeserializeObject<TranslationResult>(responseBody);

        //await Task.Delay(500);

        return translationResult?.Translations?[0]?.Text;
    }

    /// <summary>
    /// Translates a given text using the DeepL translation API.
    /// This method is responsible for updating the translation count, calling the base translation method,
    /// and updating the progress window.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <returns>The translated text.</returns>
    private async Task<string> TranslateTextAsync(CancellationToken token, string text)
    {
        Interlocked.Increment(ref _translationsCount);
        TranslationsCount = _translationsCount;

        var translatedText = await TranslateBaseAsync(text, token);
        
        Interlocked.Add(ref _characterCount, text.Length);
        CharacterCount = _characterCount;

        var finished = Interlocked.Increment(ref _completedTranslationsCount);
        CompletedTranslationsCount = finished;
        ProgressWindowUtils.Update(finished, _settings.TargetLanguage);

        //await Task.Delay(100);

        return translatedText;
    }

    /// <summary>
    /// Translate context of a TextNote.
    /// </summary>
    /// <param name="textNote"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal async Task TranslateTextNoteAsync(TextNote textNote, CancellationToken token)
    {
        var text = textNote.Text;
        if (!IsTextOnly(text))
        {
            var translated = await TranslateTextAsync(token, text);
            if (translated != null)
            {
                Translations.Add((textNote, translated, string.Empty, textNote.Id));
            }
        }  
    }

    /// <summary>
    /// Translate dimension overrides.
    /// </summary>
    /// <param name="dim"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal async Task TranslateDimensionAsync(Dimension dim, CancellationToken token)
    {
        var above = dim.Above;
        if (IsTextOnly(above))
        {
            var translated = await TranslateTextAsync(token, above);
            if (translated != null)
            {
                Translations.Add((dim, translated, "above", dim.Id));
            }
        }

        var below = dim.Below;
        if (IsTextOnly(below))
        {
            var translated = await TranslateTextAsync(token, below);
            if (translated != null)
            {
                Translations.Add((dim, translated, "below", dim.Id));
            }
        }

        var prefix = dim.Prefix;
        if (IsTextOnly(prefix))
        {
            var translated = await TranslateTextAsync(token, prefix);
            if (translated != null)
            {
                Translations.Add((dim, translated, "prefix", dim.Id));
            }
        }

        var suffix = dim.Suffix;
        if (IsTextOnly(suffix))
        {
            var translated = await TranslateTextAsync(token, suffix);
            if (translated != null)
            {
                Translations.Add((dim, translated, "suffix", dim.Id));
            }
        }

        var valueOverride = dim.ValueOverride;
        if (IsTextOnly(valueOverride))
        {
            var translated = await TranslateTextAsync(token, valueOverride);
            if (translated != null)
            {
                Translations.Add((dim, translated, "value", dim.Id));

            }
        }
    }

    /// <summary>
    /// Extract and translate name of the element and all values of its text parameters.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal async Task TranslateElementParametersAsync(Element element, CancellationToken token)
    {
        var parameters = element.Parameters
            .Cast<Parameter>()
            .Where(p => p.StorageType == StorageType.String && !p.IsReadOnly)
            .ToList();

        foreach (var parameter in parameters)
        {
            var value = parameter.AsString();
            if (!IsTextOnly(value)) 
            { 
                continue; 
            }

            var translated = await TranslateTextAsync(token, value);
            if (translated == null || value == translated) { continue; }
            Translations.Add((parameter, translated, string.Empty, element.Id));
        }

        var name = element.Name;
        var translated_name = await TranslateTextAsync(token, name);
        if (translated_name != null)
        {
            Translations.Add((element, translated_name, "name", element.Id));
        }
    }

    /// <summary>
    /// Translate schedule name and headers.
    /// </summary>
    /// <param name="scheduleId">ElementId of a schedule.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal async Task TranslateScheduleAsync(ElementId scheduleId, CancellationToken token)
    {
        // Translating names
        var s = RevitUtils.Doc.GetElement(scheduleId) as ViewSchedule;
        var name = s.Name;
        var translated = await TranslateTextAsync(token, name);
        if (translated != null)
        {
            Translations.Add((s, translated, "name", scheduleId));
        }

        // Translating header text (NOT field headers)

        //TableSectionData tsd = s.GetTableData().GetSectionData(SectionType.Header);
        //var r = tsd.NumberOfRows;
        //var c = tsd.NumberOfColumns;

        //for (var i = 0;  i < r; i++)
        //{
        //    for (var j = 0; j < c; j++)
        //    {
        //        var text = tsd.GetCellText(i, j);
        //        if (string.IsNullOrWhiteSpace(text) || IsNumberOnly(text))
        //        {
        //            continue;
        //        }
        //        var h_translated = await TranslateTextAsync(token, text);
        //        if (h_translated != null)
        //        {
        //            Translations.Add((tsd, h_translated, $"{i},{j}"));
        //        }
        //    }
        //}

        // Translating field headers
        var sd = s.Definition;
        var fieldCount = sd.GetFieldCount();
        for (var i = 0; i < fieldCount; i++)
        {
            var field = sd.GetField(i);
            var header = field.ColumnHeading;
            if (!IsTextOnly(header))
            {
                continue;
            }
            var f_translated = await TranslateTextAsync(token, header);
            Translations.Add((field, f_translated, string.Empty, scheduleId));
        }
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
        TranslationsCount = 0;
        Translations = new ConcurrentBag<(object, string, string, ElementId)>();
    }

    private static readonly Regex NumberOnlyRegex = new(@"^\d+$");
    /// <summary>
    /// Checks if the input contains only text and is not a null, whitespace, numeric or alphanumeric sequence.
    /// </summary>
    /// <param name="input">
    /// String to check.
    /// </param>
    /// <returns>
    /// Bool that indicates whether the input contains only numbers
    /// </returns>
    private bool IsTextOnly(string input)
    {
        return !(string.IsNullOrWhiteSpace(input) || NumberOnlyRegex.IsMatch(input));
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
        var result = MessageBox.Show($"You've interrupted translation process.\n" +
            $"Do you still want to update the model?",
            "Translation interrupted",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result.Equals(MessageBoxResult.Yes))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Generic method for translating a set of elements. Calls appropriate translation methods for each type of
    /// element. This is a synchronous method that freezes the main thread.
    /// </summary>
    /// <param name="elements">
    /// A set of ElementIds to translate.
    /// </param>
    /// <returns>
    /// A boolean value indicating the state of finished operation:
    ///     true - all translations finished successfully
    ///     false - error occurred or cancellation was requested
    /// </returns>
    internal bool StartTranslation(List<ElementId> elements)
    {
        ProgressWindowViewModel.Cts = new CancellationTokenSource();

        var translationTasks = new List<Task>();
        var doc = RevitUtils.Doc;

        HashSet<ElementId> typeIds = [];

        var token = ProgressWindowViewModel.Cts.Token;

        try
        {
            foreach (var id in elements)
            {
                if(token.IsCancellationRequested)
                {
                    break;
                }

                var el = doc.GetElement(id);

                switch (el)
                {
                    case TextNote textNote:
                        translationTasks.Add(Task.Run(async () => { 
                            token.ThrowIfCancellationRequested();
                            await TranslateTextNoteAsync(textNote, token); 
                        }, token));
                        break;

                    case ElementType elementType:
                        typeIds.Add(elementType.Id);
                        break;

                    case ScheduleSheetInstance scheduleInstance:
                        translationTasks.Add(Task.Run( async () => { 
                            token.ThrowIfCancellationRequested();
                            await TranslateScheduleAsync(scheduleInstance.ScheduleId, token); 
                        }));
                        break;

                    case ViewSchedule viewSchedule:
                        translationTasks.Add(Task.Run(async () => { 
                            token.ThrowIfCancellationRequested();
                            await TranslateScheduleAsync(viewSchedule.Id, token); 
                        }));
                        break;

                    case Dimension dim:
                        translationTasks.Add(Task.Run( async () => { 
                            token.ThrowIfCancellationRequested();
                            await TranslateDimensionAsync(dim, token); 
                        }));
                        break;

                    case Element element:
                        translationTasks.Add(Task.Run(async () => { 
                            token.ThrowIfCancellationRequested();
                            await TranslateElementParametersAsync(element, token); 
                        }));
                        typeIds.Add(element.GetTypeId());
                        break;
                }
            }

            foreach (var typeId in typeIds)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                if (doc.GetElement(typeId) is ElementType type)
                {
                    translationTasks.Add(Task.Run(async () => { 
                            token.ThrowIfCancellationRequested();
                            await TranslateElementParametersAsync(type, token); 
                    }));
                }
            }

            //Task.WhenAll(translationTasks);
            //Task.WhenAll(translationTasks).GetAwaiter().GetResult();
            Task.WaitAll(translationTasks.ToArray(), token);
            return true;
        }

        catch (OperationCanceledException)
        {
            // Handle cancellation
            Debug.WriteLine("Translation operation was cancelled.");
            return false;
        }

        catch (AggregateException ae)
        {
            if (ae.InnerExceptions.Any(e => e is OperationCanceledException))
            {
                Debug.WriteLine("Translation operation was cancelled.");
                return false;
            }
            Debug.WriteLine($"An error occurred during translation: {ae.Message}");
            return false;
        }

        catch (Exception ex)
        {
            // Handle other exceptions
            Debug.WriteLine($"An error occurred during translation: {ex.Message}");
            return false;
        }

        finally
        {
            ProgressWindowViewModel.Cts.Dispose();
        }
    }
}


/// <summary>
/// Classes for handling responses from DeepL API.
/// </summary>
public class TranslationResult
{
    [JsonProperty("translations")]
    public Translation[] Translations
    {
        get; set;
    }
}

public class Translation
{
    [JsonProperty("detected_source_language")]
    public string DetectedSourceLanguage
    {
        get; set;
    }

    [JsonProperty("text")]
    public string Text
    {
        get; set;
    }
}