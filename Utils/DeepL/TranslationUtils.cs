using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.Revit;
using RevitTranslatorAddin.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace RevitTranslatorAddin.Utils.DeepL;

public class TranslationUtils
{
    private readonly Models.Settings _settings = null;
    private readonly string _baseApi = "https://api-free.deepl.com/v2/";
    private readonly string _apiTranslateUrl = null;
    private readonly string _apiUsageUrl = null;
    private readonly HttpClient _httpClient = null;
    private static int _translationsCount = 0;
    private static int _completedTranslationsCount = 0;
    private static int _characterCount = 0;
    internal static int CompletedTranslationsCount { get; private set; } = 0;
    internal static int TranslationsCount { get; private set; } = 0;
    internal static int CharacterCount { get; private set; } = 0;
    internal static int Usage { get; private set; } = 0;
    internal static int Limit { get; private set; } = 0;

    internal CancellationToken token
    {
        get; set;
    }

    //TODO: switch from tuple to an object with corresponding properties for more clarity
    /// <summary>
    /// List of elements to translate, intended to use with asynchronous translation methods.
    /// Each tuple stores three values:
    /// - element to be translated, e.g. a parameter or a textbox; (object)
    /// - translated text; (string)
    /// - element-specific context for Revit elements that have multiple text fields, e.g. "above" or "header". (string)
    /// </summary>

    //internal static ConcurrentBag<(object, string, string, ElementId)> Translations { get; set; } = [];

    internal static ConcurrentBag<TranslationUnit> Translations { get; set; } = [];
    private ProgressWindowUtils _progressWindowUtils { get; set; } = null;

    public TranslationUtils(Models.Settings settings, ProgressWindowUtils progressWindowUtils)
    {
        _settings = settings;
        _httpClient = new HttpClient();
        _baseApi = _settings.IsPaidPlan
            ? "https://api.deepl.com/v2/"
            : "https://api-free.deepl.com/v2/";
        _apiTranslateUrl = $"{_baseApi}translate";
        _apiUsageUrl = $"{_baseApi}usage";
        _progressWindowUtils = progressWindowUtils;

        Task.Run( () => GetUsageAsync() ).Wait();
    }

    /// <summary>
    /// Checks if translation can be performed based on the provided settings. 
    /// This method attempts to translate a single word.
    /// </summary>
    /// <param name="settings">
    /// The Settings object containing the API key and target language.
    /// </param>
    /// <returns>
    /// Bool
    ///     True if translation can be performed, false otherwise.
    /// </returns>
    public static bool CanTranslate(Models.Settings settings)
    {
        if (settings.DeeplApiKey == null || settings.TargetLanguage == null)
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
            System.Windows.MessageBox.Show("Your settings configuration cannot be used for translation.\n" +
                "Please make sure everything is correct:\n" +
                "• API key\n" +
                "• Target language\n" +
                "• Paid/Free plan\n" +
                "• Translation limits.",
                "Incorrect settings",
                System.Windows.MessageBoxButton.OK,
                MessageBoxImage.Warning);
            
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
    /// Translates a given text using the DeepL translation API.
    /// This is a base method that simply returns translated text.
    /// </summary>
    /// <param name="text">
    /// String
    ///     Text to be translated.
    /// </param>
    /// <returns>
    /// String
    ///     Translated text.
    ///     </returns>
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
        UpdateTranslationCounter();

        var translatedText = await TranslateBaseAsync(text, token);

        UpdateCharacterCounter(text);
        UpdateCompletedCounter();

        return translatedText;
    }

    private void UpdateTranslationCounter() 
    {
        Interlocked.Increment(ref _translationsCount);
        TranslationsCount = _translationsCount;
    }

    private void UpdateCharacterCounter(string text)
    {
        Interlocked.Add(ref _characterCount, text.Length);
        CharacterCount = _characterCount;
    }

    public void UpdateCompletedCounter()
    {
        var finished = Interlocked.Increment(ref _completedTranslationsCount);
        CompletedTranslationsCount = finished;
        _progressWindowUtils.UpdateCurrent(finished);
    }

    //internal async Task TranslateMaterialAsync(Material element, CancellationToken token)
    //{
        //TODO: implement material-specific properties
    //}

        /// <summary>
        /// Extract and translate name of the element and all values of its text parameters.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="token"></param>
        /// <returns></returns>
    internal async Task TranslateElementParametersAsync(Element element)
    {
        var parameters = GetElementParameters(element);

        foreach (var parameter in parameters)
        {
            await TranslateParameter(parameter);
        }

        await TranslateElementName(element);
    }

    /// <summary>
    /// Get list of modifiable parameters for a given Element.
    /// </summary>
    /// <param name="element">
    /// Element to get parameters from.
    /// </param>
    /// <returns>
    /// List of Parameters.
    /// </returns>
    private List<Parameter> GetElementParameters(Element element)
    {
        var parameters = element.Parameters
            .Cast<Parameter>()
            .Where(p => p.StorageType == StorageType.String 
                        && !p.IsReadOnly 
                        && p.UserModifiable 
                        && p.HasValue)
            .ToList();

        return parameters;
    }

    /// <summary>
    /// Translates contents of a single text parameter.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    private async Task TranslateParameter(Parameter param)
    {
        if ( !(param.StorageType == StorageType.String) )
        {
            return;
        }

        var value = param.AsString();

        if (!IsTextOnly(value))
        {
            return;
        }

        var translated = await TranslateTextAsync(value, token);

        if (translated == null || value == translated) 
        { 
            return; 
        }

        var unit = new TranslationUnit();
        unit.OriginalText = value;
        unit.TranslatedText = translated;
        unit.Element = param;
        unit.ParentElement = param.Element;

        Translations.Add(unit);
    }

    private async Task TranslateElementName(Element element)
    {
        var name = element.Name;
        var translated = await TranslateTextAsync(name, token);
        if (translated != null)
        {
            var unit = new TranslationUnit();
            unit.OriginalText = name;
            unit.TranslatedText = translated;
            unit.Element = element;
            unit.TranslationDetails = TranslationDetails.ElementName;

            Translations.Add(unit);
        }
    }

    /// <summary>
    /// Translate schedule name and headers.
    /// </summary>
    /// <param name="scheduleId">ElementId of a schedule.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal async Task TranslateScheduleAsync(ElementId scheduleId)
    {
        // Translating names
        var s = RevitUtils.Doc.GetElement(scheduleId) as ViewSchedule;

        await TranslateElementName(s);

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
            TranslateScheduleHeader(sd, s, i);
        }
    }

    private async void TranslateScheduleHeader(ScheduleDefinition sd, ViewSchedule parent, int fieldIndex)
    {
        var field = sd.GetField(fieldIndex);
        var header = field.ColumnHeading;
        if (!IsTextOnly(header))
        {
            return;
        }
        var translated = await TranslateTextAsync(header, token);

        var unit = new TranslationUnit();
        unit.OriginalText = header;
        unit.TranslatedText = translated;
        unit.Element = field;
        unit.ParentElement = parent;
        
        Translations.Add(unit);
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

        Translations = new ConcurrentBag<TranslationUnit>();
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
}