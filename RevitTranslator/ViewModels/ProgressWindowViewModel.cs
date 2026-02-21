using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.UI.Contracts;
using TranslationService.Utils;
using Application = System.Windows.Application;

namespace RevitTranslator.ViewModels;

public partial class ProgressWindowViewModel : ObservableObject,
    IProgressWindowViewModel,
    IRecipient<TextRetrievedMessage>,
    IRecipient<EntityTranslatedMessage>,
    IRecipient<TranslationFinishedMessage>,
    IRecipient<ModelUpdatedMessage>,
    IRecipient<TokenCancellationRequestedMessage>,
    IDisposable
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
    private readonly DispatcherTimer _uiUpdateTimer;

    public ProgressWindowViewModel()
    {
        StrongReferenceMessenger.Default.Register<TextRetrievedMessage>(this);
        StrongReferenceMessenger.Default.Register<EntityTranslatedMessage>(this);
        StrongReferenceMessenger.Default.Register<TranslationFinishedMessage>(this);
        StrongReferenceMessenger.Default.Register<ModelUpdatedMessage>(this);

        IsProgressBarIntermediate = true;
        ButtonText = "Retrieving text from elements...";

        // Timer runs on UI thread and updates UI properties every 100ms
        _uiUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _uiUpdateTimer.Tick += OnUiUpdateTimerTick;
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
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
    }

    [RelayCommand]
    private void CancelTranslation()
    {
        StrongReferenceMessenger.Default.Send(new TokenCancellationRequestedMessage("Translation was cancelled by user"));
        ButtonText = "Translation cancelled";
    }

    public void CloseRequested() => CancelTranslation();

    public void UpdateProgress(int translationLength)
    {
        // Only update thread-safe counters, UI updates happen in timer tick
        Interlocked.Add(ref _threadSafeTranslationCount, 1);
        Interlocked.Add(ref _threadSafeSessionCharacterCount, translationLength);
        Interlocked.Add(ref _threadSafeMonthlyCharacterCount, translationLength);
    }

    private void OnUiUpdateTimerTick(object? sender, EventArgs args)
    {
        // Read thread-safe counters and update UI properties on UI thread
        var translationCount = Interlocked.CompareExchange(ref _threadSafeTranslationCount, 0, 0);
        var sessionCharacterCount = Interlocked.CompareExchange(ref _threadSafeSessionCharacterCount, 0, 0);
        var monthlyCharacterCount = Interlocked.CompareExchange(ref _threadSafeMonthlyCharacterCount, 0, 0);

        SessionCharacterCount = sessionCharacterCount;
        MonthlyCharacterCount = monthlyCharacterCount;
        FinishedTranslationCount = translationCount;

        // Check character limit and cancel if exceeded
        if (MonthlyCharacterCount >= MonthlyCharacterLimit && MonthlyCharacterLimit > 0)
        {
            CancelTranslation();
        }
    }

    public void Receive(TextRetrievedMessage message)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            IsProgressBarIntermediate = false;
            TotalTranslationCount = message.EntityCount;
            ButtonText = "Cancel translation";

            // Start UI update timer when translation begins
            _uiUpdateTimer.Start();
        });
    }

    public void Receive(EntityTranslatedMessage message) => UpdateProgress(message.CharacterCount);

    public void Receive(TranslationFinishedMessage message)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            // Stop timer and perform final UI update
            _uiUpdateTimer.Stop();
            OnUiUpdateTimerTick(null, EventArgs.Empty);

            IsProgressBarIntermediate = true;
            ButtonText = "Updating model...";
        });
    }

    public void Receive(ModelUpdatedMessage message)
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            IsProgressBarIntermediate = false;
            ButtonText = _wasTranslationCanceled
                ? "Model successfully updated. Translation was canceled"
                : "Model successfully updated";
        });
    }

    public void Receive(TokenCancellationRequestedMessage message) => _wasTranslationCanceled = true;

    public void Dispose()
    {
        _uiUpdateTimer.Stop();
        _uiUpdateTimer.Tick -= OnUiUpdateTimerTick;
        StrongReferenceMessenger.Default.UnregisterAll(this);
    }
}