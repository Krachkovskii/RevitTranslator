using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using RevitTranslator.Abstractions;
using RevitTranslator.Abstractions.Contracts;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Revit.Core.Extensions;
using RevitTranslator.Revit.Core.Models;
using TranslationService.Utils;

namespace RevitTranslator.Revit.Core.Services;

public class TranslationReportService : ITranslationReportService
{
    private const string AddinName = "RevitTranslator";

    private string _lastReportPath = string.Empty;

    private static string ReportsDirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Autodesk",
        "ApplicationPlugins",
        AddinName,
        "Reports");

    public void OpenLastReportDirectory()
    {
        if (!File.Exists(_lastReportPath)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{_lastReportPath}\"",
            UseShellExecute = true
        });
    }

    public void OpenReportDirectory()
    {
        if (!Directory.Exists(ReportsDirectoryPath)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = ReportsDirectoryPath,
            UseShellExecute = true
        });
    }

    public void CreateReport(List<DocumentTranslationEntityGroup> documentEntities)
    {
        var settings = DeeplSettingsUtils.CurrentSettings;
        var updatedEntities = documentEntities
            .SelectMany(group => group.TranslationEntities)
            .Where(entity => entity.UpdatedInModel)
            .ToList();

        var nonUpdatedEntities = documentEntities
            .SelectMany(group => group.TranslationEntities)
            .Where(entity => entity.IllegalCharacter.HasValue)
            .ToList();

        var documents = documentEntities.Select(group => group.Document).Distinct().ToList();
        var documentCodeMap = documents.Count > 1
            ? documents
                .Select((doc, index) => (doc, code: GetDocumentCode(index)))
                .ToDictionary(pair => pair.doc, pair => pair.code)
            : null;

        var sb = new StringBuilder();

        AppendSessionInfo(sb, settings, updatedEntities, nonUpdatedEntities, documents, documentCodeMap);
        sb.AppendLine();

        if (nonUpdatedEntities.Count > 0)
        {
            AppendTranslationTable(sb, nonUpdatedEntities, documentCodeMap,
                "✗ The following elements were translated but not updated in the model due to characters that cannot be used in user-set values:",
                includeForbiddenCharacter: true);
            sb.AppendLine();
        }

        AppendTranslationTable(sb,
            updatedEntities,
            documentCodeMap,
            "✓ The following elements have been successfully translated and updated in model:");

        var path = SaveReport(sb.ToString());
        _lastReportPath = path;
    }

    private static string GetDocumentCode(int index)
    {
        return index < 26
            ? ((char)('A' + index)).ToString()
            : GetDocumentCode(index / 26 - 1) + GetDocumentCode(index % 26);
    }

    private static void AppendSessionInfo(
        StringBuilder sb,
        TranslationService.Models.DeeplSettingsDescriptor? settings,
        List<TranslationEntity> updatedEntities,
        List<TranslationEntity> nonUpdatedEntities,
        List<Document> documents,
        Dictionary<Document, string>? documentCodeMap)
    {
        var translationTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
        var deeplPlan = settings?.IsPaidPlan != true ? "Free" : "Paid";
        var sourceLanguage = settings?.SourceLanguage?.VisibleName ?? "Auto-detected";
        var targetLanguage = settings?.TargetLanguage?.VisibleName ?? "Unknown";
        var characterCount = updatedEntities
            .Concat(nonUpdatedEntities)
            .GroupBy(entity => entity.OriginalText)
            .Sum(group => group.First().TranslatedText.Length);

        sb.AppendLine($"Translation session report created at {translationTime}");
        sb.AppendLine($"DeepL API plan: {deeplPlan}");
        sb.AppendLine($"Source language: {sourceLanguage}");
        sb.AppendLine($"Target language: {targetLanguage}");
        sb.AppendLine($"Elements translated: {updatedEntities.Count}");
        sb.AppendLine($"Characters translated: {characterCount}");
        if (nonUpdatedEntities.Count > 0)
        {
            sb.AppendLine($"Elements not updated in the model due to forbidden characters: {nonUpdatedEntities.Count}");
        }

        if (documentCodeMap is null)
        {
            sb.AppendLine($"Document title: {GetDocumentDisplayName(documents[0])}");
        }
        else
        {
            sb.AppendLine("Documents:");
            foreach (var pair in documentCodeMap)
            {
                sb.AppendLine($"  {pair.Value} - {GetDocumentDisplayName(pair.Key)}");
            }
        }
    }

    private static string GetDocumentDisplayName(Document document)
    {
        return document.IsFamilyDocument ? document.Title : document.Title + ".rvt";
    }

    private static void AppendTranslationTable(
        StringBuilder sb,
        List<TranslationEntity> entities,
        Dictionary<Document, string>? documentCodeMap,
        string sectionHeader,
        bool includeForbiddenCharacter = false)
    {
        sb.AppendLine(sectionHeader);

        var header = documentCodeMap is null
            ? "ElementId,Source text,Translated text"
            : "ElementId,Source text,Translated text,Document";

        sb.AppendLine(includeForbiddenCharacter ? header + ",Forbidden character" : header);
        sb.AppendLine();

        foreach (var entity in entities)
        {
            var elementId = entity.ParentElementId is not null
                ? entity.ParentElementId.ToLong().ToString()
                : entity.ElementId.ToLong().ToString();

            var columns = new List<string>
            {
                elementId,
                EscapeCsvValue(entity.OriginalText),
                EscapeCsvValue(entity.TranslatedText)
            };

            if (documentCodeMap is not null)
            {
                columns.Add(documentCodeMap[entity.Document]);
            }

            if (includeForbiddenCharacter)
            {
                columns.Add(EscapeCsvValue(entity.IllegalCharacter?.ToString() ?? string.Empty));
            }

            sb.AppendLine(string.Join(",", columns));
        }
    }

    private static string SaveReport(string content)
    {
        try
        {
            if (!Directory.Exists(ReportsDirectoryPath))
            {
                Directory.CreateDirectory(ReportsDirectoryPath);
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm", CultureInfo.InvariantCulture);
            var filePath = Path.Combine(ReportsDirectoryPath, $"Translation report {timestamp}.csv");

            File.WriteAllText(filePath, content, Encoding.UTF8);
            return filePath;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Report generation should not interrupt the translation workflow
            return string.Empty;
        }
    }

    private static string EscapeCsvValue(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}