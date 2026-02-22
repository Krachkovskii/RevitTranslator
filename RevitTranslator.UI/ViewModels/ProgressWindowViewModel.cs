using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;

namespace RevitTranslator.UI.ViewModels;

public partial class ProgressWindowViewModel : ObservableObject,
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
    [ObservableProperty] private bool _isProgressBarIntermediate;
    [ObservableProperty] private bool _modelUpdateFinished;
    [ObservableProperty] private string _buttonText = "";

    private int _threadSafeTranslationCount;
    private int _threadSafeSessionCharacterCount;
    private int _threadSafeMonthlyCharacterCount;
    private bool _wasTranslationCanceled;
    private bool _canCancelTranslation;
    private readonly DispatcherTimer _uiUpdateTimer;

    public ProgressWindowViewModel()
    {
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

    [RelayCommand(CanExecute = nameof(CanCancelTranslation))]
    private void CancelTranslation()
    {
        StrongReferenceMessenger.Default.Send(
            new TokenCancellationRequestedMessage("Translation was cancelled by user"));
        ButtonText = "Translation cancelled";
    }

    private bool CanCancelTranslation() => _canCancelTranslation;

    public void CloseRequested() => CancelTranslation();

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
            _canCancelTranslation = true;
            CancelTranslationCommand.NotifyCanExecuteChanged();

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
            _canCancelTranslation = false;
            CancelTranslationCommand.NotifyCanExecuteChanged();

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