using Bogus;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Demo.ViewModels;
using RevitTranslator.UI.Views;
using TranslationService.Utils;

namespace RevitTranslator.Demo.Utils;

public class MockTranslationPipeline
{
    public void Execute()
    {
        var faker = new Faker();
        var wordCount = faker.Random.Int(1, 50);
        var words = faker.Lorem.Words(wordCount);
        Console.WriteLine($"Generated {wordCount} words");
        
        DeeplSettingsUtils.Load();

        var viewModel = new MockProgressWindowViewModel(true);
        var view = new ProgressWindow(viewModel);
        view.Show();

        Task.Run(async () =>
        {
            await Task.Delay(2000);
            StrongReferenceMessenger.Default.Send(new TextRetrievedMessage(wordCount));

            await new MockConcurrentTranslationHandler().Translate(words, false);
            
            await Task.Delay(2000);
            StrongReferenceMessenger.Default.Send(new ModelUpdatedMessage());
        });
    }
}