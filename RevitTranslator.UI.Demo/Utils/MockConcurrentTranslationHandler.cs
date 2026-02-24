using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using TranslationService.Utils;

namespace RevitTranslator.UI.Demo.Utils;

public class MockConcurrentTranslationHandler(DeeplTranslationClient client) : IRecipient<TokenCancellationRequestedMessage>
{
    private readonly List<Task> _tasks = [];
    private readonly CancellationTokenSource _cts = new();
    private CancellationToken _cancellationToken;
    private bool _useMockTranslations;
    private TaskCompletionSource<bool>? _decisionTcs;
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

            await Task.WhenAll(_tasks);

            if (_cts.IsCancellationRequested)
                throw new OperationCanceledException(_cancellationToken);

            StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(false));
            await SimulateModelUpdateAsync();
        }
        catch (OperationCanceledException)
        {
            StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(true));
            if (await WaitForUserDecisionAsync())
                await SimulateModelUpdateAsync();
        }
        finally
        {
            StrongReferenceMessenger.Default.UnregisterAll(this);
        }
    }

    private async Task<bool> WaitForUserDecisionAsync()
    {
        _decisionTcs = new TaskCompletionSource<bool>();
        StrongReferenceMessenger.Default.Register<MockConcurrentTranslationHandler, ModelUpdateDecisionMessage>(
            this, static (r, msg) => r._decisionTcs?.TrySetResult(msg.ShouldUpdate));
        return await _decisionTcs.Task;
    }

    private async Task SimulateModelUpdateAsync()
    {
        await Task.Delay(1500);
        StrongReferenceMessenger.Default.Send(new ModelUpdatedMessage());
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
                await client.TranslateTextAsync(text, _cancellationToken);
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
