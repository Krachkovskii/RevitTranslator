using Bogus;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.UI.Views;
using TranslationService.Utils;

namespace RevitTranslator.UI.Demo.Utils;

public class MockTranslationPipeline(ProgressWindow progressWindow, DeeplTranslationClient client)
{
    public async Task ExecuteAsync(bool useMockTranslations = false)
    {
        var faker = new Faker();
        var wordCount = faker.Random.Int(1, 50);
        var words = faker.Lorem.Words(wordCount);

        if (!useMockTranslations && !DeeplSettingsUtils.Load())
        {
            Console.WriteLine("Failed to load settings");
            return;
        }

        progressWindow.Show();

        await Task.Delay(2000);
        StrongReferenceMessenger.Default.Send(new TextRetrievedMessage(wordCount));

        await new MockConcurrentTranslationHandler(client).TranslateAsync(words, useMockTranslations);

        await Task.Delay(2000);
        StrongReferenceMessenger.Default.Send(new ModelUpdatedMessage());
    }
}