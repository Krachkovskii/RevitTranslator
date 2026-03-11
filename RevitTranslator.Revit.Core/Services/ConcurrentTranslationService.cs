using System.Text;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Revit.Core.Models;
using TranslationService.Utils;

namespace RevitTranslator.Revit.Core.Services;

/// <summary>
/// This class batches text for translation requests and sends them concurrently.
/// </summary>
public class ConcurrentTranslationService(
    DeeplTranslationClient translationClient,
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
            var batches = CreateBatches(entities);
            foreach (var batch in batches)
            {
                token.ThrowIfCancellationRequested();
                await ProcessBatchAsync(batch, token);
            }
        }
        catch (OperationCanceledException exception)
        {
            Console.WriteLine(exception);
        }
    }

    private static List<List<TranslationEntity>> CreateBatches(TranslationEntity[] entities)
    {
        var batches = new List<List<TranslationEntity>>();
        var currentBatch = new List<TranslationEntity>();
        var currentSizeBytes = 0;

        foreach (var entity in entities)
        {
            var textSizeBytes = Encoding.UTF8.GetByteCount(entity.OriginalText);
            if (currentSizeBytes + textSizeBytes > MaxBatchSizeBytes && currentBatch.Count > 0)
            {
                batches.Add(currentBatch);
                currentBatch = new List<TranslationEntity>();
                currentSizeBytes = 0;
            }

            currentBatch.Add(entity);
            currentSizeBytes += textSizeBytes;
        }

        if (currentBatch.Count > 0) batches.Add(currentBatch);
        return batches;
    }

    private async Task ProcessBatchAsync(List<TranslationEntity> batch, CancellationToken token)
    {
        var texts = batch.Select(entity => entity.OriginalText).ToArray();
        var translations = await translationClient.TranslateTextsAsync(texts, token);
        if (translations is null) return;

        for (var i = 0; i < batch.Count; i++)
        {
            var entity = batch[i];
            var translated = i < translations.Length ? translations[i] : null;

            if (translated is null) continue;

            if (translated == string.Empty)
            {
                progressMonitor.OnEntityTranslated(entity.OriginalText.Length);
                continue;
            }

            entity.TranslatedText = translated;
            progressMonitor.OnEntityTranslated(translated.Length);
        }
    }
}
