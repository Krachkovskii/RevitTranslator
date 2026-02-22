using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using TranslationService.Utils;

namespace RevitTranslator.UI.ViewModels;

public partial class ProgressWindowViewModel : ObservableObject,
    IRecipient<TextRetrievedMessage>,
    IRecipient<EntityTranslatedMessage>,
    IRecipient<TranslationFinishedMessage>,
    IRecipient<ModelUpdatedMessage>,
    IDisposable
{
    private readonly DeeplTranslationClient _deeplClient;
    [ObservableProperty] private int _totalTranslationCount;
    [ObservableProperty] private int _finishedTranslationCount;
    [ObservableProperty] private int _monthlyCharacterLimit;
    [ObservableProperty] private int _monthlyCharacterCount;
    [ObservableProperty] private int _sessionCharacterCount;
    [ObservableProperty] private bool _isProgressBarIntermediate;
    [ObservableProperty] private bool _isMainButtonEnabled;
    [ObservableProperty] private string _buttonText = "";
    [ObservableProperty] private string _buttonSubtext = "";

    private int _threadSafeTranslationCount;
    private int _threadSafeSessionCharacterCount;
    private int _threadSafeMonthlyCharacterCount;
    private bool _wasTranslationCanceled;
    private bool _isTranslationActive;
    private bool _isAwaitingConfirmation;
    private bool _isModelUpdateActive;
    private readonly DispatcherTimer _uiUpdateTimer;

    public bool IsAwaitingConfirmation => _isAwaitingConfirmation;

    public ProgressWindowViewModel(DeeplTranslationClient deeplClient)
    {
        _deeplClient = deeplClient;
        
        StrongReferenceMessenger.Default.Register<TextRetrievedMessage>(this);
        StrongReferenceMessenger.Default.Register<EntityTranslatedMessage>(this);
        StrongReferenceMessenger.Default.Register<TranslationFinishedMessage>(this);
        StrongReferenceMessenger.Default.Register<ModelUpdatedMessage>(this);

        IsProgressBarIntermediate = true;
        ButtonText = "Retrieving text from elements...";

        _uiUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _uiUpdateTimer.Tick += OnUiUpdateTimerTick;

        // TODO: Check monthly limit and usage
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        await _deeplClient.CheckUsageAsync();
        _threadSafeMonthlyCharacterCount = MonthlyCharacterCount = _deeplClient.Limit;
        MonthlyCharacterCount = _deeplClient.Limit;
    }

    [RelayCommand]
    private void HandleButtonClick()
    {
        if (_isAwaitingConfirmation)
            ConfirmModelUpdate();
        else
            CancelTranslation();
    }

    private void CancelTranslation()
    {
        StrongReferenceMessenger.Default.Send(
            new TokenCancellationRequestedMessage("Translation was cancelled by user"));
        _uiUpdateTimer.Stop();
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

    private void UpdateProgress(int translationLength)
    {
        Interlocked.Add(ref _threadSafeTranslationCount, 1);
        Interlocked.Add(ref _threadSafeSessionCharacterCount, translationLength);
        Interlocked.Add(ref _threadSafeMonthlyCharacterCount, translationLength);
    }

    private void OnUiUpdateTimerTick(object? sender, EventArgs args)
    {
        var translationCount = Interlocked.CompareExchange(ref _threadSafeTranslationCount, 0, 0);
        var sessionCharacterCount = Interlocked.CompareExchange(ref _threadSafeSessionCharacterCount, 0, 0);
        var monthlyCharacterCount = Interlocked.CompareExchange(ref _threadSafeMonthlyCharacterCount, 0, 0);

        SessionCharacterCount = sessionCharacterCount;
        MonthlyCharacterCount = monthlyCharacterCount;
        FinishedTranslationCount = translationCount;

        if (MonthlyCharacterCount >= MonthlyCharacterLimit && MonthlyCharacterLimit > 0)
        {
            CancelTranslation();
        }
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

            _uiUpdateTimer.Start();
        });
    }

    public void Receive(EntityTranslatedMessage message) => UpdateProgress(message.CharacterCount);

    public void Receive(TranslationFinishedMessage message)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            _isTranslationActive = false;
            _uiUpdateTimer.Stop();
            OnUiUpdateTimerTick(null, EventArgs.Empty);

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
        });
    }

    public void Dispose()
    {
        _uiUpdateTimer.Stop();
        _uiUpdateTimer.Tick -= OnUiUpdateTimerTick;
        StrongReferenceMessenger.Default.UnregisterAll(this);
    }
}
