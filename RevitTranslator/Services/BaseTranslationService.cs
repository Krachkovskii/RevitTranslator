using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.ElementTextRetrievers;
using RevitTranslator.Handlers;
using RevitTranslator.Models;
using RevitTranslator.UI.Views;
using RevitTranslator.Utils.App;
using RevitTranslator.ViewModels;
using TranslationService.Utils;

namespace RevitTranslator.Services;

public class BaseTranslationService(
    ProgressWindow window,
    ConcurrentTranslationHandler handler,
    ModelUpdater modelUpdater) : IRecipient<TokenCancellationRequestedMessage>
{
    private readonly CancellationTokenSource _cts = new();
    private List<DocumentTranslationEntityGroup>? _documentEntities;

    public Element[] SelectedElements { get; set; } = [];
    
    public void Execute()
    {
        if (DeeplSettingsUtils.CurrentSettings is null)
        {
            DeeplSettingsUtils.Load();
        }

        // weird, but necessary to stay on an STA thread (until I fix the architecture)
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
    
    private async Task TranslateAsync()
    {
        var handler = new ConcurrentTranslationHandler();

        try
        {
            _cts.Token.ThrowIfCancellationRequested();
            await handler.Translate(_documentEntities!.SelectMany(entity => entity.TranslationEntities).ToArray(),
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
        EventHandlers.ActionHandler.Raise(_ => modelUpdater.Update(_documentEntities!));
    }
    
    public void Receive(TokenCancellationRequestedMessage message)
    {
        _cts.Cancel();
    }
}