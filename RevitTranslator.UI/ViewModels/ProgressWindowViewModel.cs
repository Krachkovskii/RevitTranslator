using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Abstractions;
using RevitTranslator.Abstractions.Contracts;
using RevitTranslator.Common.Messages;
using TranslationService.Utils;

namespace RevitTranslator.UI.ViewModels;

public partial class ProgressWindowViewModel : ObservableObject,
    IRecipient<TextRetrievedMessage>,
    IRecipient<EntitiesTranslatedMessage>,
    IRecipient<TranslationFinishedMessage>,
    IRecipient<ModelUpdatedMessage>,
    IDisposable
{
    private readonly DeeplTranslationClient _deeplClient;
    private readonly ITranslationReportService _reportService;
    
    [ObservableProperty] private int _totalTranslationCount;
    [ObservableProperty] private int _finishedTranslationCount;
    [ObservableProperty] private int _monthlyCharacterLimit;
    [ObservableProperty] private int _monthlyCharacterCount;
    [ObservableProperty] private int _sessionCharacterCount;
    [ObservableProperty] private bool _isProgressBarIntermediate;
    [ObservableProperty] private bool _isMainButtonEnabled;
    [ObservableProperty] private string _buttonText = "";
    [ObservableProperty] private string _buttonSubtext = "";
    [ObservableProperty] private string _illegalCharacterWarning = string.Empty;
    [ObservableProperty] private string _updatedElementsText = string.Empty;

    private bool _wasTranslationCanceled;
    private bool _isTranslationActive;
    private bool _isAwaitingConfirmation;
    private bool _isModelUpdateActive;
    
    public ProgressWindowViewModel(DeeplTranslationClient deeplClient, ITranslationReportService reportService)
    {
        _deeplClient = deeplClient;
        _reportService = reportService;

        StrongReferenceMessenger.Default.Register<TextRetrievedMessage>(this);
        StrongReferenceMessenger.Default.Register<EntitiesTranslatedMessage>(this);
        StrongReferenceMessenger.Default.Register<TranslationFinishedMessage>(this);
        StrongReferenceMessenger.Default.Register<ModelUpdatedMessage>(this);

        IsProgressBarIntermediate = true;
        ButtonText = "Retrieving text from elements...";
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        await _deeplClient.CheckUsageAsync();
        MonthlyCharacterCount = _deeplClient.Usage;
        MonthlyCharacterLimit = _deeplClient.Limit;
    }

    [RelayCommand]
    private void HandleButtonClick()
    {
        if (_isAwaitingConfirmation)
            ConfirmModelUpdate();
        else
            CancelTranslation();
    }

    [RelayCommand]
    private void OpenReportCommand() => _reportService.OpenLastReportDirectory();

    private void CancelTranslation()
    {
        StrongReferenceMessenger.Default.Send(
            new TokenCancellationRequestedMessage("Translation was cancelled by user"));
        _wasTranslationCanceled = true;
        _isTranslationActive = false;
        IsMainButtonEnabled = false;
        ButtonText = "Cancelling...";
    }

    private void ConfirmModelUpdate()
    {
        _isAwaitingConfirmation = false;
        _isModelUpdateActive = true;
        IsMainButtonEnabled = false;
        IsProgressBarIntermediate = true;
        ButtonText = "Updating model...";
        ButtonSubtext = "";
        StrongReferenceMessenger.Default.Send(new ModelUpdateDecisionMessage(true));
    }

    public bool CloseRequested()
    {
        if (_isTranslationActive)
        {
            CancelTranslation();
            return false;
        }
        if (_isModelUpdateActive) return false;
        if (!_isAwaitingConfirmation) return true;
        
        _isAwaitingConfirmation = false;
        StrongReferenceMessenger.Default.Send(new ModelUpdateDecisionMessage(false));
        return true;
    }

    private void UpdateProgress(int entityCount, int charCount)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            FinishedTranslationCount += entityCount;
            SessionCharacterCount += charCount;
            MonthlyCharacterCount += charCount;

            if (MonthlyCharacterLimit > 0 && MonthlyCharacterCount >= MonthlyCharacterLimit)
                CancelTranslation();
        });
    }

    public void Receive(TextRetrievedMessage message)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            _isTranslationActive = true;
            IsMainButtonEnabled = true;

            IsProgressBarIntermediate = false;
            TotalTranslationCount = message.EntityCount;
            ButtonText = "Cancel translation";
        });
    }

    public void Receive(EntitiesTranslatedMessage message) => UpdateProgress(message.EntityCount, message.CharacterCount);

    public void Receive(TranslationFinishedMessage message)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            _isTranslationActive = false;

            if (message.CancelRequested)
            {
                _isAwaitingConfirmation = true;
                IsMainButtonEnabled = true;
                ButtonText = "Proceed with model update?";
                ButtonSubtext = "Close this window to leave the model unchanged";
            }
            else
            {
                _isModelUpdateActive = true;
                IsMainButtonEnabled = false;
                IsProgressBarIntermediate = true;
                ButtonText = "Updating model...";
            }
        });
    }

    public void Receive(ModelUpdatedMessage message)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            _isModelUpdateActive = false;
            IsProgressBarIntermediate = false;

            ButtonText = "Model successfully updated";
            ButtonSubtext = _wasTranslationCanceled
                ? "Translation was canceled. Window can be closed now."
                : "Window can be closed now.";

            var elementSuffix = message.UpdatedInModelCount == 1 ? "element" : "elements";
            UpdatedElementsText = message.UpdatedFamiliesCount > 0
                ? $"Updated {message.UpdatedInModelCount} {elementSuffix} in model and {message.UpdatedFamiliesCount} {(message.UpdatedFamiliesCount == 1 ? "family" : "families")}"
                : $"Updated {message.UpdatedInModelCount} {elementSuffix} in model";

            if (message.NonUpdatedEntitiesCount == 0) return;

            var nameSuffix = message.NonUpdatedEntitiesCount == 1 ? "name was" : "names were";
            IllegalCharacterWarning = $"{message.NonUpdatedEntitiesCount} element {nameSuffix} translated, but not updated due to forbidden characters.\nSee report for details.";
        });
    }

    public void Dispose()
    {
        StrongReferenceMessenger.Default.UnregisterAll(this);
    }
}
