using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                var res = await utils.TranslateBaseAsync("bonjour");
                return true;
            }).GetAwaiter().GetResult();

            return test;
        }

        catch
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
    }

    /// <summary>
    /// Translates a given text using the DeepL translation API.
    /// This is a base method that simply returns translated text.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <returns>The translated text.</returns>
    private async Task<string> TranslateBaseAsync(string text)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("auth_key", _settings.DeeplApiKey),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("context", "(This is a property of an element in a BIM Model)"),
            new KeyValuePair<string, string>("target_lang", _settings.Languages[_settings.TargetLanguage]),
            new KeyValuePair<string, string>("source_lang", _settings.SourceLanguage == null ? null : _settings.Languages[_settings.SourceLanguage])
        });

        var response = await _httpClient.PostAsync(_apiUrl, content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var translationResult = JsonConvert.DeserializeObject<TranslationResult>(responseBody);

        await Task.Delay(1000);

        return translationResult?.Translations?[0]?.Text;
    }

    /// <summary>
    /// Translates a given text using the DeepL translation API.
    /// This method is responsible for updating the translation count, calling the base translation method,
    /// and updating the progress window.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <returns>The translated text.</returns>
    private async Task<string> TranslateTextAsync(string text)
    {
        var translatedText = await TranslateBaseAsync(text);

        Interlocked.Increment(ref _translationsCount);
        TranslationsCount = _translationsCount;
        Interlocked.Add(ref _characterCount, text.Length);
        CharacterCount = _characterCount;

        var finished = Interlocked.Increment(ref _completedTranslationsCount);
        CompletedTranslationsCount = finished;
        ProgressWindowUtils.Update(finished, _settings.TargetLanguage);

        await Task.Delay(500);

        return translatedText;
    }

    internal async Task TranslateTextNoteAsync(TextNote textNote, CancellationToken token)
    {
        var text = textNote.Text;
        if (!string.IsNullOrEmpty(text) && !IsNumberOnly(text))
        {
            var translated = await TranslateTextAsync(text);
            if (translated != null)
            {
                Translations.Add((textNote, translated, string.Empty, textNote.Id));
            }
        }
    }

    internal async Task TranslateDimensionAsync(Dimension dim, CancellationToken token)
    {
        var above = dim.Above;
        if (!string.IsNullOrEmpty(above) && !IsNumberOnly(above))
        {
            var translated = await TranslateTextAsync(above);
            if (translated != null)
            {
                Translations.Add((dim, translated, "above", dim.Id));
            }
        }

        var below = dim.Below;
        if (!string.IsNullOrEmpty(below) && !IsNumberOnly(below))
        {
            var translated = await TranslateTextAsync(below);
            if (translated != null)
            {
                Translations.Add((dim, translated, "below", dim.Id));
            }
        }

        var prefix = dim.Prefix;
        if (!string.IsNullOrEmpty(prefix) && !IsNumberOnly(prefix))
        {
            var translated = await TranslateTextAsync(prefix);
            if (translated != null)
            {
                Translations.Add((dim, translated, "prefix", dim.Id));
            }
        }

        var suffix = dim.Suffix;
        if (!string.IsNullOrEmpty(suffix) && !IsNumberOnly(suffix))
        {
            var translated = await TranslateTextAsync(suffix);
            if (translated != null)
            {
                Translations.Add((dim, translated, "suffix", dim.Id));
            }
        }

        var valueOverride = dim.ValueOverride;
        if (!string.IsNullOrEmpty(valueOverride) && !IsNumberOnly(valueOverride))
        {
            var translated = await TranslateTextAsync(valueOverride);
            if (translated != null)
            {
                Translations.Add((dim, translated, "value", dim.Id));

            }
        }
    }

    internal async Task TranslateElementParametersAsync(Element element, CancellationToken token)
    {
        var parameters = element.Parameters
            .Cast<Parameter>()
            .Where(p => p.StorageType == StorageType.String && !p.IsReadOnly)
            .ToList();

        foreach (var parameter in parameters)
        {
            var value = parameter.AsString();
            if (string.IsNullOrWhiteSpace(value) || IsNumberOnly(value)) { continue; }

            var translated = await TranslateTextAsync(value);
            if (translated == null || value == translated) { continue; }
            Translations.Add((parameter, translated, string.Empty, element.Id));
        }

        var name = element.Name;
        var translated_name = await TranslateTextAsync(name);
        if (translated_name != null)
        {
            Translations.Add((element, translated_name, "name", element.Id));
        }
    }

    internal async Task TranslateScheduleAsync(Document doc, ElementId scheduleId, CancellationToken token)
    {
        // Translating names
        var s = doc.GetElement(scheduleId) as ViewSchedule;
        var name = s.Name;
        var translated = await TranslateTextAsync(name);
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
        //        var h_translated = await TranslateTextAsync(text);
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
            if (string.IsNullOrWhiteSpace(header) || IsNumberOnly(header))
            {
                continue;
            }
            var f_translated = await TranslateTextAsync(header);
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
        TranslationsCount = 0;
        Translations = new ConcurrentBag<(object, string, string, ElementId)>();
    }

    private bool IsNumberOnly(string input)
    {
        return string.IsNullOrEmpty(input) || Regex.IsMatch(input, @"^\d+$");
    }

    /// <summary>
    /// Generic method for translating a set of elements. Calls appropriate translation methods for each
    /// element type. This is a synchronous method that freezes the main thread.
    /// </summary>
    /// <param name="elements">
    /// A set of ElementIds to translate.
    /// </param>
    /// <returns>
    /// A boolean value indicating whether the translations have finished or not.
    /// </returns>
    internal bool StartTranslation(List<ElementId> elements)
    {
        ProgressWindowViewModel.Cts = new CancellationTokenSource();

        var translationTasks = new List<Task>();
        var doc = RevitUtils.Doc;

        HashSet<ElementId> typeIds = [];

        try {
            var token = ProgressWindowViewModel.Cts.Token;
            //token.ThrowIfCancellationRequested();

            foreach (var id in elements)
            {
                token.ThrowIfCancellationRequested();
                var el = doc.GetElement(id);

                switch (el)
                {
                    case TextNote textNote:
                        translationTasks.Add(Task.Run(() => TranslateTextNoteAsync(textNote, token)));
                        break;

                    case ElementType elementType:
                        typeIds.Add(elementType.Id);
                        break;

                    case ScheduleSheetInstance scheduleInstance:
                        translationTasks.Add(Task.Run(() => TranslateScheduleAsync(doc, scheduleInstance.ScheduleId, token)));
                        break;

                    case ViewSchedule viewSchedule:
                        translationTasks.Add(Task.Run(() => TranslateScheduleAsync(doc, viewSchedule.Id, token)));
                        break;

                    case Dimension dim:
                        translationTasks.Add(Task.Run(() => TranslateDimensionAsync(dim, token)));
                        break;

                    case Element element:
                        translationTasks.Add(Task.Run(() => TranslateElementParametersAsync(element, token)));
                        typeIds.Add(element.GetTypeId());
                        break;
                }
            }

            foreach (var typeId in typeIds)
            {
                token.ThrowIfCancellationRequested();
                if (doc.GetElement(typeId) is ElementType type)
                {
                    translationTasks.Add(Task.Run(() => TranslateElementParametersAsync(type, token)));
                }
            }

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
            ProgressWindowViewModel.Cts?.Dispose();
            ProgressWindowViewModel.Cts = null;
        }
    }
}


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