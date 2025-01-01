using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.App.Messages;
using RevitTranslator.Demo.Utils;
using RevitTranslator.UI.Contracts;

namespace RevitTranslator.Demo.ViewModels;

public partial class MockProgressWindowViewModel : ObservableObject, 
        IProgressWindowViewModel, 
        IRecipient<EntityTranslatedMessage>,
        IRecipient<TranslationFinishedMessage>
{
    [ObservableProperty] private int _totalTranslationCount;
    [ObservableProperty] private int _finishedTranslationCount;
    [ObservableProperty] private int _monthlyCharacterLimit;
    [ObservableProperty] private int _monthlyCharacterCount;
    [ObservableProperty] private int _sessionCharacterCount;
    [ObservableProperty] private string _buttonText;
    [ObservableProperty] private bool _isProgressBarIntermediate;
    [ObservableProperty] private bool _modelUpdateFinished;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public MockProgressWindowViewModel()
    {
        StrongReferenceMessenger.Default.RegisterAll(this);
        
        Task.Run(async () =>
        {
            List<string> translations = [];
            var faker = new Faker();
            IsProgressBarIntermediate = true;
            ButtonText = "Collecting text from elements";
            
            for (var i = 0; i < faker.Random.Int(10, 500); i++)
            {
                await Task.Delay(faker.Random.Int(50, 100));
                translations.Add(faker.Lorem.Sentence());
                TotalTranslationCount++;
            }
            
            Console.WriteLine($"Total char count: {translations.Sum(t => t.Length)}");
            
            IsProgressBarIntermediate = false;
            ButtonText = "Cancel translation";
            MonthlyCharacterLimit = faker.Random.Number(1, 100) * 1000;
            MonthlyCharacterCount = (int)(MonthlyCharacterLimit * faker.Random.Double(0, 0.2));

            await new MockConcurrentTranslationHandler()
                .Translate(translations.ToArray(), _cancellationTokenSource.Token);
        });
    }

    [RelayCommand]
    private void CancelTranslation()
    {
        if (IsProgressBarIntermediate) return;
        _cancellationTokenSource.Cancel();
        
        ButtonText = "Canceling translation...";
        
        // await Task.Delay(1000);
        ButtonText = "Translation cancelled";
    }

    public void CloseRequested()
    {
        StrongReferenceMessenger.Default.UnregisterAll(this);
        CancelTranslation();
    }

    public void FinalizeTranslation()
    {
        IsProgressBarIntermediate = true;
        UpdateModel();
    }

    private async void UpdateModel()
    {
        ButtonText = "Updating model";
        await Task.Delay(2000);
        
        ButtonText = "Model updated";
        IsProgressBarIntermediate = false;
    }

    public void UpdateProgress(int translationLength)
    {
        SessionCharacterCount += translationLength;
        MonthlyCharacterCount += translationLength;
        FinishedTranslationCount++;

        if (MonthlyCharacterCount >= MonthlyCharacterLimit)
        {
            CancelTranslation();
        }
        // ReSharper disable once InvertIf
        if (FinishedTranslationCount == TotalTranslationCount)
        {
            FinalizeTranslation();
        }
    }

    public void Receive(EntityTranslatedMessage message)
    {
        UpdateProgress(message.CharacterCount);
    }

    public void Receive(TranslationFinishedMessage message) 
    {
        FinalizeTranslation();
    }
}