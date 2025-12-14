using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Messages;
using RevitTranslator.Common.Services;
using RevitTranslator.ElementTextRetrievers;
using RevitTranslator.Models;
using RevitTranslator.UI.Views;
using RevitTranslator.Utils;
using RevitTranslator.ViewModels;
using TranslationService.Utils;

namespace RevitTranslator.Services;

public class TranslationManager(
    ProgressWindow progressWindow,
    ConcurrentTranslationService service,
    ModelUpdaterService modelUpdaterService,
    EventHandlers handlers,
    Func<ScopedWindowService> scopedServiceFactory) : IRecipient<TokenCancellationRequestedMessage>
{
    private readonly CancellationTokenSource _cts = new();
    private List<DocumentTranslationEntityGroup>? _documentEntities;
    private Element[] _targetElements = [];

    public async Task ExecuteAsync(Element[] elements)
    {
        _targetElements = elements;

        if (DeeplSettingsUtils.CurrentSettings is null)
        {
            DeeplSettingsUtils.Load();
        }

        var canTranslate = await TranslationUtils.TryTestTranslateAsync();
        if (!canTranslate)
        {
            var parentWindow = Context.UiApplication.MainWindowHandle.ToWindow();
            scopedServiceFactory().ShowDialog<SettingsWindow>(parentWindow);
            
            if (!await TranslationUtils.TryTestTranslateAsync())
            {
                MessageBox
                    .Show("Settings are not valid. Elements will not be translated.",
                        "Translation Service Error");
                return;
            }
        }

        StrongReferenceMessenger.Default.Register(this);

        progressWindow.Show();

        _documentEntities = GetTextFromElements();
        await TranslateDocumentsAsync(_documentEntities);
        await UpdateRevitModelAsync();

        StrongReferenceMessenger.Default.UnregisterAll(this);
    }

    private List<DocumentTranslationEntityGroup> GetTextFromElements()
    {
        var entities = new MultiElementTextRetriever()
            .CreateEntities(_targetElements, false, out var unitCount);
        StrongReferenceMessenger.Default.Send(new TextRetrievedMessage(unitCount));

        return entities;
    }

    private async Task TranslateDocumentsAsync(List<DocumentTranslationEntityGroup> documents)
    {
        try
        {
            _cts.Token.ThrowIfCancellationRequested();
            await service.TranslateEntitiesAsync(documents.SelectMany(entity => entity.TranslationEntities).ToArray(),
                _cts.Token);

            StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(false));
        }
        catch (OperationCanceledException)
        {
            StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(true));
        }
    }

    private Task UpdateRevitModelAsync() =>
        handlers.AsyncHandler.RaiseAsync(_ => modelUpdaterService.Update(_documentEntities!));

    public void Receive(TokenCancellationRequestedMessage message) => _cts.Cancel();
}