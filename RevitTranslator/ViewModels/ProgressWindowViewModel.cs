using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.UI.Contracts;
using TranslationService.Utils;

namespace RevitTranslator.ViewModels;

public partial class ProgressWindowViewModel : ObservableObject,
    IProgressWindowViewModel,
    IRecipient<TextRetrievedMessage>,
    IRecipient<EntityTranslatedMessage>,
    IRecipient<TranslationFinishedMessage>,
    IRecipient<ModelUpdatedMessage>,
    IRecipient<TokenCancellationRequestedMessage>
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
    private bool _wasTranslationCanceled;
    
    public ProgressWindowViewModel()
    {
        StrongReferenceMessenger.Default.Register<TextRetrievedMessage>(this);
        StrongReferenceMessenger.Default.Register<EntityTranslatedMessage>(this);
        StrongReferenceMessenger.Default.Register<TranslationFinishedMessage>(this);
        StrongReferenceMessenger.Default.Register<ModelUpdatedMessage>(this);

        IsProgressBarIntermediate = true;
        ButtonText = "Retrieving text from elements...";

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
        StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage("Translation was cancelled by user"));
        ButtonText = "Translation cancelled";
    }

    public void CloseRequested()
    {
        StrongReferenceMessenger.Default.UnregisterAll(this);
        CancelTranslation();
    }

    public void UpdateProgress(int translationLength)
    {
        var translationCount = Interlocked.Add(ref _threadSafeTranslationCount, 1);
        var sessionCharacterCount = Interlocked.Add(ref _threadSafeSessionCharacterCount, translationLength);
        var monthlyCharacterCount = Interlocked.Add(ref _threadSafeMonthlyCharacterCount, translationLength);
        
        SessionCharacterCount = sessionCharacterCount;
        MonthlyCharacterCount = monthlyCharacterCount;
        FinishedTranslationCount = translationCount;

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
        ButtonText = _wasTranslationCanceled 
            ? "Model successfully updated. Translation was canceled"
            : "Model successfully updated";
    }

    public void Receive(TokenCancellationRequestedMessage message)
    {
        _wasTranslationCanceled = true;
    }
}