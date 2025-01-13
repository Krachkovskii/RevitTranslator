using Bogus;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;

namespace RevitTranslator.Demo.Utils;

public class MockConcurrentTranslationHandler
{
    private readonly List<Task> _tasks = [];
    private readonly Faker _faker = new();
    private CancellationToken _cancellationToken;

    public async Task Translate(string[] texts, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        try
        {
            foreach (var text in texts)
            {
                await Task.Delay(_faker.Random.Int(100, 500), _cancellationToken);
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
            await Task.Delay(_faker.Random.Int(500, 4000), _cancellationToken);
            Console.WriteLine($"Translated: {text}");
            StrongReferenceMessenger.Default.Send(new EntityTranslatedMessage(text.Length));
        }));
    } 
}