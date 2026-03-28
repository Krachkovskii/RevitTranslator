using System.Text;
using RevitTranslator.Abstractions.Contracts;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Revit.Core.Models;
using TranslationService.Utils;

namespace RevitTranslator.Revit.Core.Services;

/// <summary>
/// This class batches text for translation requests and sends them concurrently.
/// Duplicate texts are grouped so each unique text is translated only once.
/// </summary>
public class ConcurrentTranslationService(
    ITranslationClient translationClient,
    ITranslationProgressMonitor progressMonitor)
{
    /// <summary>
    /// Maximum UTF-8 byte size of all texts in a single batch — 50% of the 128 KiB DeepL limit.
    /// This leaves room for URL-encoding overhead and translated text being longer than the source.
    /// </summary>
    private const int MaxBatchSizeBytes = 65_536;

    public async Task TranslateEntitiesAsync(TranslationEntity[] entities, CancellationToken token)
    {
        try
        {
            var groups = entities
                .GroupBy(entity => entity.OriginalText)
                .ToDictionary(group => group.Key, group => group.ToArray());

            var batches = CreateBatches([.. groups.Keys]);
            foreach (var batch in batches)
            {
                token.ThrowIfCancellationRequested();
                await ProcessBatchAsync(batch, groups, token);
            }
        }
        catch (OperationCanceledException exception)
        {
            Console.WriteLine(exception);
        }
    }

    private static List<List<string>> CreateBatches(string[] texts)
    {
        var batches = new List<List<string>>();
        var currentBatch = new List<string>();
        var currentSizeBytes = 0;

        foreach (var text in texts)
        {
            var textSizeBytes = Encoding.UTF8.GetByteCount(text);
            if (currentSizeBytes + textSizeBytes > MaxBatchSizeBytes && currentBatch.Count > 0)
            {
                batches.Add(currentBatch);
                currentBatch = new List<string>();
                currentSizeBytes = 0;
            }

            currentBatch.Add(text);
            currentSizeBytes += textSizeBytes;
        }

        if (currentBatch.Count > 0) batches.Add(currentBatch);
        return batches;
    }

    private async Task ProcessBatchAsync(
        List<string> batch,
        Dictionary<string, TranslationEntity[]> groups,
        CancellationToken token)
    {
        var translations = await translationClient.TranslateTextsAsync(batch.ToArray(), token);
        if (translations is null) return;

        for (var i = 0; i < batch.Count; i++)
        {
            var originalText = batch[i];
            var translated = i < translations.Length ? translations[i] : null;
            if (translated is null) continue;

            var entityGroup = groups[originalText];
            if (translated == string.Empty)
            {
                progressMonitor.OnEntitiesTranslated(entityGroup.Length, originalText.Length);
                continue;
            }

            foreach (var entity in entityGroup)
            {
                entity.TranslatedText = translated;
            }

            progressMonitor.OnEntitiesTranslated(entityGroup.Length, translated.Length);
        }
    }
}
