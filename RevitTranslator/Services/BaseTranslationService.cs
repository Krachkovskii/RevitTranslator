using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.ElementTextRetrievers;
using RevitTranslator.Models;
using RevitTranslator.UI.Views;
using RevitTranslator.Utils;
using RevitTranslator.ViewModels;
using TranslationService.Utils;

namespace RevitTranslator.Services;

public class BaseTranslationService(
    ProgressWindow window,
    ConcurrentTranslationService service,
    ModelUpdaterService modelUpdaterService) : IRecipient<TokenCancellationRequestedMessage>
{
    private readonly CancellationTokenSource _cts = new();
    private List<DocumentTranslationEntityGroup>? _documentEntities;

    public Element[] SelectedElements { get; set; } = [];
    
    // TODO: pass selection as a parameter
    public void Execute()
    {
        if (DeeplSettingsUtils.CurrentSettings is null)
        {
            DeeplSettingsUtils.Load();
        }

        // weird, but necessary to stay on an STA thread (until I fix the architecture)
        // TODO: Refactor as async method
        var canTranslate = Task.Run(async () => await TranslationUtils.TryTestTranslateAsync()).Result;
        if (!canTranslate)
        {
            MessageBox
                .Show("Could not connect to translation service.\nPlease check your credentials and internet connection, and try again.",
                    "Translation Service Error");
            return;
        }
        
        StrongReferenceMessenger.Default.Register(this);

        window.Show();
        
        _documentEntities = GetTextFromElements();

        // TODO: Refactor as async invocation, without task
        Task.Run(async () =>
        {
            await TranslateAsync();
            UpdateRevitModel();
            
            StrongReferenceMessenger.Default.UnregisterAll(this);
        });
    }

    private List<DocumentTranslationEntityGroup> GetTextFromElements()
    {
        var entities = new MultiElementTextRetriever()
            .CreateEntities(SelectedElements, false, out var unitCount);
        StrongReferenceMessenger.Default.Send(new TextRetrievedMessage(unitCount));

        return entities;
    }
    
    // todo: rename method
    private async Task TranslateAsync()
    {
        try
        {
            _cts.Token.ThrowIfCancellationRequested();
            await service.TranslateAsync(_documentEntities!.SelectMany(entity => entity.TranslationEntities).ToArray(),
                _cts.Token);

            StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(false));
        }
        catch (OperationCanceledException)
        {
            StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(true));
        }
    }

    private void UpdateRevitModel()
    {
        EventHandlers.ActionHandler.Raise(_ => modelUpdaterService.Update(_documentEntities!));
    }
    
    public void Receive(TokenCancellationRequestedMessage message)
    {
        _cts.Cancel();
    }
}