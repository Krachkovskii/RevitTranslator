using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using RevitTranslator.Common.Contracts;
using RevitTranslator.Extensions;
using RevitTranslator.Models;
using TranslationService.Utils;

namespace RevitTranslator.Services;

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

        var sb = new StringBuilder();

        AppendSessionInfo(sb, settings, allEntities);
        sb.AppendLine();
        AppendTranslationTable(sb, allEntities);

        var path = SaveReport(sb.ToString());
        _lastReportPath = path;
    }

    private static void AppendSessionInfo(
        StringBuilder sb,
        TranslationService.Models.DeeplSettingsDescriptor? settings,
        List<TranslationEntity> entities)
    {
        var isFreePlan = settings?.IsPaidPlan != true;
        var sourceLanguage = settings?.SourceLanguage?.VisibleName ?? "Auto";
        var targetLanguage = settings?.TargetLanguage?.VisibleName ?? "Unknown";
        var characterCount = entities.Sum(entity => entity.TranslatedText.Length);

        sb.AppendLine($"Free plan: {isFreePlan}");
        sb.AppendLine($"Source language: {sourceLanguage}");
        sb.AppendLine($"Target language: {targetLanguage}");
        sb.AppendLine($"Elements translated: {entities.Count}");
        sb.AppendLine($"Characters translated: {characterCount}");
    }

    private static void AppendTranslationTable(StringBuilder sb, List<TranslationEntity> entities)
    {
        sb.AppendLine("Document name,ElementId,Source text,Translated text");
        sb.AppendLine();

        foreach (var entity in entities)
        {
            var elementId = entity.ParentElement is not null
                ? entity.ParentElement.Id.ToLong().ToString()
                : entity.ElementId.ToLong().ToString();

            sb.AppendLine(string.Join(",",
                EscapeCsvValue(entity.Document.Title),
                elementId,
                EscapeCsvValue(entity.OriginalText),
                EscapeCsvValue(entity.TranslatedText)));
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
