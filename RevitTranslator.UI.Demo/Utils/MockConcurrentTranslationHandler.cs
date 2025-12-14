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
    private static readonly SemaphoreSlim Semaphore = new(5, 10);

    public async Task TranslateAsync(string[] texts, bool useMockTranslations)
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
                await SimulateTranslationWithSemaphoreAsync();
            }
            else
            {
                await TranslationUtils.TranslateTextAsync(text, _cancellationToken);
            }
            
            StrongReferenceMessenger.Default.Send(new EntityTranslatedMessage(text.Length));
        }, _cancellationToken));
    }

    private async Task SimulateTranslationWithSemaphoreAsync()
    {
        await Semaphore.WaitAsync(_cancellationToken);
        try
        {
            _cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(500);
        }
        catch (OperationCanceledException)
        {
            // do nothing
        }
        finally
        {
            Semaphore.Release();
        }
    }
    
    public void Receive(TokenCancellationRequestedMessage message)
    {
        if (_cts.IsCancellationRequested) return;
        
        _cts.Cancel();
    }
}