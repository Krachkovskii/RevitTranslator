using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.App.Messages;
using RevitTranslator.Models;
using TranslationService.Utils;

namespace RevitTranslator.Utils.App;

public class ConcurrentTranslationHandler
{
    private readonly List<Task> _translationTasks = [];
    
    public async Task Translate(TranslationEntity[] entities, CancellationToken token)
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
        }
    }
    
    private async Task TranslateEntityAsync(TranslationEntity entity, CancellationToken token)
    {
        var translated = await TranslationUtils.TranslateTextAsync(entity.OriginalText, token);
        if (translated is null)
        {
            StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage());
            return;
        }
        entity.TranslatedText = translated;
        StrongReferenceMessenger.Default.Send(new EntityTranslatedMessage(translated.Length));
    }
}