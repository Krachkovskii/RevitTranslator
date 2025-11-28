using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Demo.Utils;
using RevitTranslator.UI.Contracts;
using TranslationService.Utils;

namespace RevitTranslator.Demo.ViewModels;

public partial class MockProgressWindowViewModel : ObservableObject, 
        IProgressWindowViewModel, 
        IRecipient<EntityTranslatedMessage>,
        IRecipient<TranslationFinishedMessage>,
        IRecipient<ModelUpdatedMessage>,
        IRecipient<TextRetrievedMessage>
{
    [ObservableProperty] private int _totalTranslationCount;
    [ObservableProperty] private int _finishedTranslationCount;
    [ObservableProperty] private int _monthlyCharacterLimit;
    [ObservableProperty] private int _monthlyCharacterCount;
    [ObservableProperty] private int _sessionCharacterCount;
    [ObservableProperty] private string _buttonText;
    [ObservableProperty] private bool _isProgressBarIntermediate;
    [ObservableProperty] private bool _modelUpdateFinished;

    private int _threadSafeTranslationCount;
    private int _threadSafeSessionCharacterCount;
    private int _threadSafeMonthlyCharacterCount;

    public MockProgressWindowViewModel()
    {
        StrongReferenceMessenger.Default.RegisterAll(this);
        
        Task.Run(async () =>
        {
            var faker = new Faker();
            IsProgressBarIntermediate = true;
            ButtonText = "Collecting text from elements";

            await Task.Delay(2000);
            var translations = faker.Lorem.Words(faker.Random.Int(10, 500));
            TotalTranslationCount = translations.Length;
            
            IsProgressBarIntermediate = false;
            ButtonText = "Cancel translation";
            MonthlyCharacterLimit = faker.Random.Number(1, 100) * 1000;
            MonthlyCharacterCount = (int)(MonthlyCharacterLimit * faker.Random.Double(0, 0.2));

            await new MockConcurrentTranslationHandler()
                .TranslateAsync(translations.ToArray(), true);
        });
    }

    public MockProgressWindowViewModel(bool useTranslationService)
    {
        StrongReferenceMessenger.Default.RegisterAll(this);
        
        IsProgressBarIntermediate = true;
        ButtonText = "Collecting text from elements";
        
        Task.Run(async () =>
        {
            await TranslationUtils.CheckUsageAsync();

            if (TranslationUtils.Usage == -1)
            {
                CancelTranslation();
                return;
            }
            
            _threadSafeMonthlyCharacterCount = TranslationUtils.Usage;
            MonthlyCharacterCount = _threadSafeMonthlyCharacterCount;
            MonthlyCharacterLimit = TranslationUtils.Limit;
        }).GetAwaiter().GetResult();
    }

    [RelayCommand]
    private void CancelTranslation()
    {
        StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage(""));
        ButtonText = "Translation cancelled";
    }

    public void CloseRequested()
    {
        StrongReferenceMessenger.Default.UnregisterAll(this);
        CancelTranslation();
    }

    public Task FinalizeTranslationAsync()
    {
        IsProgressBarIntermediate = true;
        return UpdateModelAsync();
    }

    private async Task UpdateModelAsync()
    {
        ButtonText = "Updating model";
        await Task.Delay(2000);
        
        ButtonText = "Model updated";
        IsProgressBarIntermediate = false;
    }

    public void UpdateProgress(int translationLength)
    {
        var translationCount = Interlocked.Add(ref _threadSafeTranslationCount, 1);
        var sessionCharacterCount = Interlocked.Add(ref _threadSafeSessionCharacterCount, translationLength);
        var monthlyCharacterCount = Interlocked.Add(ref _threadSafeMonthlyCharacterCount, translationLength);
        
        SessionCharacterCount = sessionCharacterCount;
        MonthlyCharacterCount = monthlyCharacterCount;
        FinishedTranslationCount = translationCount;

        SessionCharacterCount += translationLength;
        MonthlyCharacterCount += translationLength;
        FinishedTranslationCount++;

        if (MonthlyCharacterCount >= MonthlyCharacterLimit)
        {
            CancelTranslation();
        }
    }

    public void Receive(TextRetrievedMessage message)
    {
        IsProgressBarIntermediate = false;
        TotalTranslationCount = message.EntityCount;
        ButtonText = "Cancel translation";
    }

    public void Receive(EntityTranslatedMessage message)
    {
        UpdateProgress(message.CharacterCount);
    }

    public void Receive(TranslationFinishedMessage message)
    {
        IsProgressBarIntermediate = true;
        ButtonText = "Updating model...";
    }

    public void Receive(ModelUpdatedMessage message)
    {
        IsProgressBarIntermediate = false;
        ButtonText = "Model successfully updated";
    }
}