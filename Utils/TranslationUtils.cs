using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RevitTranslatorAddin.Commands;
using RevitTranslatorAddin.Models;

namespace RevitTranslatorAddin.Utils;

public class TranslationUtils
{
    //private readonly Translator _translator = null;
    private readonly Models.Settings _settings = null;
    private readonly string _apiUrl = "https://api-free.deepl.com/v2/translate";
    private readonly HttpClient _httpClient = null;
    private static int _translationsCount = 0;
    private static int _completedTranslationsCount = 0;
    internal static int CompletedTranslationsCount { get; private set; } = 0;
    internal static int TranslationsCount { get; private set; } = 0;

    // obect, string, string - element to translate (parameter, header, text block etc.),
    // translated value, optional type-specific comment (e.g. "above" or "header")
    internal static ConcurrentBag<(object, string, string)> Translations { get; set; } = [];

    internal TranslationUtils(Models.Settings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient();
    }

    public static bool CanTranslate(Models.Settings settings)
    {
        return settings.DeeplApiKey != null && settings.TargetLanguage != null;
    }

    private async Task<string> TranslateTextAsync(string text)
    {
        Interlocked.Increment(ref _translationsCount);
        TranslationsCount = _translationsCount;

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("auth_key", _settings.DeeplApiKey),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("target_lang", _settings.Languages[_settings.TargetLanguage]),
            new KeyValuePair<string, string>("source_lang", _settings.SourceLanguage == null ? null : _settings.Languages[_settings.SourceLanguage])
        });

        var response = await _httpClient.PostAsync(_apiUrl, content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var translationResult = JsonConvert.DeserializeObject<TranslationResult>(responseBody);

        int finished = Interlocked.Increment(ref _completedTranslationsCount);
        CompletedTranslationsCount = finished;
        ProgressWindowUtils.Update(finished, _settings.TargetLanguage);

        return translationResult?.Translations?[0]?.Text;
    }

    internal static void ClearTranslationCount()
    {
        _translationsCount = 0;
        _completedTranslationsCount = 0;
        CompletedTranslationsCount = 0;
        TranslationsCount = 0;
        Translations = new ConcurrentBag<(object, string, string)>();
    }

    private bool IsNumberOnly(string input)
    {
        return string.IsNullOrEmpty(input) || Regex.IsMatch(input, @"^\d+$");
    }

    internal async Task TranslateTextNoteAsync(TextNote textNote)
    {
        var text = textNote.Text;
        if (!string.IsNullOrEmpty(text) && !IsNumberOnly(text))
        {
            var translated = await TranslateTextAsync(text);
            Translations.Add((textNote, translated, string.Empty));
        }
    }

    internal async Task TranslateDimensionAsync(Dimension dim)
    {
        string above = dim.Above;
        if (!string.IsNullOrEmpty(above) && !IsNumberOnly(above))
        {
            var translated = await TranslateTextAsync(above);
            Translations.Add((dim, translated, "above"));
        }

        string below = dim.Below;
        if (!string.IsNullOrEmpty(below) && !IsNumberOnly(below))
        {
            var translated = await TranslateTextAsync(below);
            Translations.Add((dim, translated, "below"));
        }

        string prefix = dim.Prefix;
        if (!string.IsNullOrEmpty(prefix) && !IsNumberOnly(prefix))
        {
            var translated = await TranslateTextAsync(prefix);
            Translations.Add((dim, translated, "prefix"));
        }

        string suffix = dim.Suffix;
        if (!string.IsNullOrEmpty(suffix) && !IsNumberOnly(suffix))
        {
            var translated = await TranslateTextAsync(suffix);
            Translations.Add((dim, translated, "suffix"));
        }

        string valueOverride = dim.ValueOverride;
        if (!string.IsNullOrEmpty(valueOverride) && !IsNumberOnly(valueOverride))
        {
            var translated = await TranslateTextAsync(valueOverride);
            Translations.Add((dim, translated, "value"));
        }
    }

    internal async Task TranslateElementParametersAsync(Element element)
    {
        List<Parameter> parameters = element.Parameters
            .Cast<Parameter>()
            .Where(p => p.StorageType == StorageType.String && !p.IsReadOnly)
            .ToList();

        foreach (Parameter parameter in parameters)
        {
            var value = parameter.AsString();
            if (string.IsNullOrWhiteSpace(value) || IsNumberOnly(value)) { continue; }

            var translated = await TranslateTextAsync(value);
            if (value == translated) { continue; }

            Translations.Add((parameter, translated, string.Empty));
        }
    }

    internal async Task TranslateScheduleAsync(Document doc, ElementId scheduleId)
    {
        // Translating names
        var s = doc.GetElement(scheduleId) as ViewSchedule;
        var name = s.Name;
        var translated = await TranslateTextAsync(name);
        Translations.Add((s, translated, "name"));

        // Translating header text (NOT field headers)
        TableSectionData tsd = s.GetTableData().GetSectionData(SectionType.Header);
        var r = tsd.NumberOfRows;
        var c = tsd.NumberOfColumns;

        for (var i = 0;  i < r; i++)
        {
            for (var j = 0; j < c; j++)
            {
                var text = tsd.GetCellText(i, j);
                if (string.IsNullOrWhiteSpace(text) || IsNumberOnly(text))
                {
                    continue;
                }
                var h_translated = await TranslateTextAsync(text);
                Translations.Add((tsd, h_translated, $"{i},{j}"));
            }
        }

        // Translating field headers
        ScheduleDefinition sd = s.Definition;
        int fieldCount = sd.GetFieldCount();
        for (int i = 0; i < fieldCount; i++)
        {
            ScheduleField field = sd.GetField(i);
            string header = field.ColumnHeading;
            if (string.IsNullOrWhiteSpace(header) || IsNumberOnly(header))
            {
                continue;
            }
            string f_translated = await TranslateTextAsync(header);
            Translations.Add((field, f_translated, string.Empty));
        }
    }

    internal void StartTranslation(List<ElementId> elements)
    {
        var translationTasks = new List<Task>();
        var doc = RevitUtils.Doc;

        HashSet<ElementId> typeIds = [];

        foreach (ElementId id in elements)
        {
            var el = doc.GetElement(id);

            switch (el)
            {
                case TextNote textNote:
                    translationTasks.Add(Task.Run(() => TranslateTextNoteAsync(textNote)));
                    break;

                case ElementType elementType:
                    typeIds.Add(elementType.Id);
                    break;

                case ScheduleSheetInstance scheduleInstance:
                    translationTasks.Add(Task.Run(() => TranslateScheduleAsync(doc, scheduleInstance.ScheduleId)));
                    break;

                case ViewSchedule viewSchedule:
                    translationTasks.Add(Task.Run(() => TranslateScheduleAsync(doc, viewSchedule.Id)));
                    break;

                case Dimension dim:
                    translationTasks.Add(Task.Run(() => TranslateDimensionAsync(dim)));
                    break;

                case Element element:
                    translationTasks.Add(Task.Run(() => TranslateElementParametersAsync(element)));
                    typeIds.Add(element.GetTypeId());
                    break;
            }
        }

        foreach (ElementId typeId in typeIds)
        {
            if (doc.GetElement(typeId) is ElementType type)
            {
                translationTasks.Add(Task.Run(() => TranslateElementParametersAsync(type)));
            }
        }
        Task.WhenAll(translationTasks).GetAwaiter().GetResult();
    }
}

internal class TranslationResult
{
    internal Translation[] Translations { get; set; }
}

internal class Translation
{
    internal string Text { get; set; }
}
