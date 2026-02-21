using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Models;
using TranslationService.Utils;

namespace RevitTranslator.Services;

public class ConcurrentTranslationService
{
    private readonly List<Task> _translationTasks = [];
    
    public async Task TranslateEntitiesAsync(TranslationEntity[] entities, CancellationToken token)
    {
        try
        {
            foreach (var entity in entities)
            {
                token.ThrowIfCancellationRequested();
                _translationTasks.Add(Task.Run(() => TranslateEntityAsync(entity, token), token));
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
    
    private static async Task TranslateEntityAsync(TranslationEntity entity, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            
            var translated = await TranslationUtils.TranslateTextAsync(entity.OriginalText, token);
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
        catch (OperationCanceledException exception)
        {
            StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage(exception.Message));
        }
    }
}