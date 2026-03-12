using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Contracts;
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
    [ObservableProperty] private string _elapsedTime = "";
    [ObservableProperty] private string _illegalCharacterWarning = string.Empty;

    private int _threadSafeTranslationCount;
    private int _threadSafeSessionCharacterCount;
    private int _threadSafeMonthlyCharacterCount;
    private bool _wasTranslationCanceled;
    private bool _isTranslationActive;
    private bool _isAwaitingConfirmation;
    private bool _isModelUpdateActive;
    private readonly DispatcherTimer _uiUpdateTimer;
    private DateTime _translationStartTime;
    
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

        _uiUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _uiUpdateTimer.Tick += OnUiUpdateTimerTick;
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        await _deeplClient.CheckUsageAsync();
        _threadSafeMonthlyCharacterCount = MonthlyCharacterCount = _deeplClient.Usage;
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

    private void UpdateProgress(int entityCount, int charCount)
    {
        Interlocked.Add(ref _threadSafeTranslationCount, entityCount);
        Interlocked.Add(ref _threadSafeSessionCharacterCount, charCount);
        Interlocked.Add(ref _threadSafeMonthlyCharacterCount, charCount);
    }

    private void OnUiUpdateTimerTick(object? sender, EventArgs args)
    {
        var translationCount = Interlocked.CompareExchange(ref _threadSafeTranslationCount, 0, 0);
        var sessionCharacterCount = Interlocked.CompareExchange(ref _threadSafeSessionCharacterCount, 0, 0);
        var monthlyCharacterCount = Interlocked.CompareExchange(ref _threadSafeMonthlyCharacterCount, 0, 0);

        SessionCharacterCount = sessionCharacterCount;
        MonthlyCharacterCount = monthlyCharacterCount;
        FinishedTranslationCount = translationCount;

        var elapsed = DateTime.Now - _translationStartTime;
        ElapsedTime = elapsed.TotalMinutes >= 1
            ? $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s elapsed"
            : $"{(int)elapsed.TotalSeconds}s elapsed";

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

            _translationStartTime = DateTime.Now;
            _uiUpdateTimer.Start();
        });
    }

    public void Receive(EntitiesTranslatedMessage message) => UpdateProgress(message.EntityCount, message.CharacterCount);

    public void Receive(TranslationFinishedMessage message)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            _isTranslationActive = false;
            _uiUpdateTimer.Stop();
            OnUiUpdateTimerTick(null, EventArgs.Empty);

            var elapsed = DateTime.Now - _translationStartTime;
            var elapsedText = elapsed.TotalMinutes >= 1
                ? $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s"
                : $"{(int)elapsed.TotalSeconds}s";
            ElapsedTime = $"Translation finished in {elapsedText}";

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

            if (message.NonUpdatedEntitiesCount == 0) return;
            
            var nameSuffix = message.NonUpdatedEntitiesCount == 1 ? "name was" : "names were";
            IllegalCharacterWarning = $"{message.NonUpdatedEntitiesCount} element {nameSuffix} translated, but not updated due to forbidden characters.\nSee report for details.";
        });
    }

    public void Dispose()
    {
        _uiUpdateTimer.Stop();
        _uiUpdateTimer.Tick -= OnUiUpdateTimerTick;
        StrongReferenceMessenger.Default.UnregisterAll(this);
    }
}
