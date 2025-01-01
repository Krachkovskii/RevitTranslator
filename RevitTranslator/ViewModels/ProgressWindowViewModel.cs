using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.App.Messages;
using RevitTranslator.UI.Contracts;
using TranslationService.Utils;

namespace RevitTranslator.ViewModels;

public partial class ProgressWindowViewModel : ObservableObject,
    IProgressWindowViewModel,
    IRecipient<TextRetrievedMessage>,
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
    
    public ProgressWindowViewModel()
    {
        StrongReferenceMessenger.Default.Register<TextRetrievedMessage>(this);
        StrongReferenceMessenger.Default.Register<EntityTranslatedMessage>(this);
        StrongReferenceMessenger.Default.Register<TranslationFinishedMessage>(this);
        
        ButtonText = "Retrieving text from elements...";

        Task.Run(async () =>
        {
            await TranslationUtils.CheckUsageAsync();

            if (TranslationUtils.Usage == -1)
            {
                CancelTranslation();
                return;
            }
            
            MonthlyCharacterCount = TranslationUtils.Usage;
            MonthlyCharacterLimit = TranslationUtils.Limit;
        });
    }

    [RelayCommand]
    private void CancelTranslation()
    {
        StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage());
        ButtonText = "Translation cancelled";
    }

    public void CloseRequested()
    {
        StrongReferenceMessenger.Default.UnregisterAll(this);
        CancelTranslation();
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
    }

    public void Receive(TextRetrievedMessage message)
    {
        IsProgressBarIntermediate = false;
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
}