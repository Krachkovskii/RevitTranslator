using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Models;
using TranslationService.Utils;

namespace RevitTranslator.Handlers;

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
        try
        {
            token.ThrowIfCancellationRequested();
            
            var translated = await TranslationUtils.Translate(entity.OriginalText, token);
            switch (translated)
            {
                case null:
                    return;
                case "":
                    translated = entity.OriginalText;
                    break;
                default:
                    entity.TranslatedText = translated;
                    break;
            }

            StrongReferenceMessenger.Default.Send(new EntityTranslatedMessage(translated.Length));
        }
        catch (OperationCanceledException)
        {
            StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage());
        }
    }
}