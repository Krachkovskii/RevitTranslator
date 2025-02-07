using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using TranslationService.Utils;

namespace RevitTranslator.Demo.Utils;

public class MockConcurrentTranslationHandler : IRecipient<TokenCancellationRequestedMessage>
{
    private readonly List<Task> _tasks = [];
    private readonly CancellationTokenSource _cts = new();
    private CancellationToken _cancellationToken;
    private bool _useMockTranslations;

    public async Task Translate(string[] texts, bool useMockTranslations)
    {
        _cancellationToken = _cts.Token;
        _useMockTranslations = useMockTranslations;
        
        StrongReferenceMessenger.Default.Register(this);
        try
        {
            foreach (var text in texts)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                AddTranslationTask(text);
            }

            await Task.WhenAll(_tasks)
                .ContinueWith(_ =>
                        StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(false)),
                    _cancellationToken);
        }
        catch (OperationCanceledException)
        {
            StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(true));
        }
    }

    private void AddTranslationTask(string text)
    {
        _tasks.Add(Task.Run(async () =>
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (_useMockTranslations)
            {
                await Task.Delay(500); // Simulate translation delay
            }
            else
            {
                await TranslationUtils.Translate(text, _cancellationToken);
            }
            
            StrongReferenceMessenger.Default.Send(new EntityTranslatedMessage(text.Length));
        }, _cancellationToken));
    } 
    
    public void Receive(TokenCancellationRequestedMessage message)
    {
        if (_cts.IsCancellationRequested) return;
        
        _cts.Cancel();
    }
}