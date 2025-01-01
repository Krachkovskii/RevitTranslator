using RevitTranslator.Models;
using TranslationService.Utils;

namespace RevitTranslator.Utils.App;

public class NewConcurrentTranslationHandler
{
    private readonly List<Task> _translationTasks = [];
    
    public async Task Execute(List<TranslationEntity> entities, CancellationToken token)
    {
        try
        {
            foreach (var entity in entities)
            {
                token.ThrowIfCancellationRequested();
                _translationTasks.Add(Task.Run(async () => await TranslateEntityAsync(entity, token), token));
            }
        }
        catch (OperationCanceledException exception)
        {
            Console.WriteLine(exception);
        }

        finally
        {
            await Task.WhenAll(_translationTasks);
            _translationTasks.Clear();
        }
    }
    
    private async Task TranslateEntityAsync(TranslationEntity entity, CancellationToken token)
    {
        var translated = await TranslationUtils.TranslateTextAsync(entity.OriginalText, token);
        entity.TranslatedText = translated;
    }
}