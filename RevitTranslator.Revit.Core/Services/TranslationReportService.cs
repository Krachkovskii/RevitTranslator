using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using RevitTranslator.Common.Contracts;
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
        var allEntities = documentEntities
            .SelectMany(group => group.TranslationEntities)
            .Where(entity => !string.IsNullOrEmpty(entity.TranslatedText))
            .ToList();

        var documents = documentEntities.Select(group => group.Document).Distinct().ToList();
        var documentCodeMap = documents.Count > 1
            ? documents
                .Select((doc, index) => (doc, code: GetDocumentCode(index)))
                .ToDictionary(pair => pair.doc, pair => pair.code)
            : null;

        var sb = new StringBuilder();

        AppendSessionInfo(sb, settings, allEntities, documents, documentCodeMap);
        sb.AppendLine();
        AppendTranslationTable(sb, allEntities, documentCodeMap);

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
        List<TranslationEntity> entities,
        List<Document> documents,
        Dictionary<Document, string>? documentCodeMap)
    {
        var translationTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
        var deeplPlan = settings?.IsPaidPlan != true ? "Free" : "Paid";
        var sourceLanguage = settings?.SourceLanguage?.VisibleName ?? "Auto-detected";
        var targetLanguage = settings?.TargetLanguage?.VisibleName ?? "Unknown";
        var characterCount = entities.Sum(entity => entity.TranslatedText.Length);

        sb.AppendLine($"Report created at {translationTime}");
        sb.AppendLine($"DeepL API plan: {deeplPlan}");
        sb.AppendLine($"Source language: {sourceLanguage}");
        sb.AppendLine($"Target language: {targetLanguage}");
        sb.AppendLine($"Elements translated: {entities.Count}");
        sb.AppendLine($"Characters translated: {characterCount}");

        if (documentCodeMap is null)
        {
            var document = documents[0];
            var documentName = document.IsFamilyDocument ? document.Title : document.Title + ".rvt";
            sb.AppendLine($"Document title: {documentName}");
        }
        else
        {
            sb.AppendLine("Documents:");
            foreach (var (doc, code) in documentCodeMap)
            {
                var documentName = doc.IsFamilyDocument ? doc.Title : doc.Title + ".rvt";
                sb.AppendLine($"  {code} - {documentName}");
            }
        }
    }

    private static void AppendTranslationTable(
        StringBuilder sb,
        List<TranslationEntity> entities,
        Dictionary<Document, string>? documentCodeMap)
    {
        sb.AppendLine(documentCodeMap is null
            ? "ElementId,Source text,Translated text"
            : "ElementId,Source text,Translated text,Document");
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
