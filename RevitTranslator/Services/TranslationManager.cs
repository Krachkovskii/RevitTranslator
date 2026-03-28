using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Revit.Abstractions.Contracts;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Revit.Core.ElementTextRetrievers;
using RevitTranslator.Revit.Core.Models;
using RevitTranslator.Revit.Core.Services;
using RevitTranslator.Revit.Core.Utils;
using TranslationService.Exceptions;

namespace RevitTranslator.Services;

public class TranslationManager(
    ITranslationProgressMonitor progressMonitor,
    ISettingsValidator settingsValidator,
    ConcurrentTranslationService service,
    ModelUpdaterService modelUpdaterService,
    IRevitHandler handler,
    TranslationReportService reportService,
    MultiElementTextRetriever multiElementTextRetriever) : IRecipient<TokenCancellationRequestedMessage>
{
    private readonly CancellationTokenSource _cts = new();
    private List<DocumentTranslationEntityGroup>? _documentEntities;
    private Element[] _targetElements = [];

    public async Task ExecuteAsync(Element[] elements)
    {
        _targetElements = elements;

        if (!await settingsValidator.TryEnforceValidSettingsAsync())
            return;

        StrongReferenceMessenger.Default.Register(this);
        progressMonitor.Initialize();

        _documentEntities = GetTextFromElements();
        await TranslateDocumentsAsync(_documentEntities);
        if (await ShouldUpdateModelAsync())
        {
            await UpdateRevitModelAsync();
            reportService.CreateReport(_documentEntities);
        }

        StrongReferenceMessenger.Default.UnregisterAll(this);
    }

    private List<DocumentTranslationEntityGroup> GetTextFromElements()
    {
        var entities = multiElementTextRetriever
            .CreateEntities(_targetElements, false, out var unitCount);
        progressMonitor.OnTextRetrieved(unitCount);

        return entities;
    }

    private async Task TranslateDocumentsAsync(List<DocumentTranslationEntityGroup> documents)
    {
        try
        {
            _cts.Token.ThrowIfCancellationRequested();
            await service.TranslateEntitiesAsync(documents.SelectMany(entity => entity.TranslationEntities).ToArray(),
                _cts.Token);

            progressMonitor.OnTranslationFinished(false);
        }
        catch (FatalTranslationException)
        {
            _cts.Cancel();
            progressMonitor.OnTranslationFinished(true);
        }
        catch (OperationCanceledException)
        {
            progressMonitor.OnTranslationFinished(true);
        }
    }

    private Task<bool> ShouldUpdateModelAsync()
    {
        return _cts.IsCancellationRequested 
            ? progressMonitor.ShouldUpdateAfterCancellationAsync() 
            : Task.FromResult(true);
    }

    private Task UpdateRevitModelAsync() =>
        handler.RaiseAsync(() => modelUpdaterService.Update(_documentEntities!));

    public void Receive(TokenCancellationRequestedMessage message) => _cts.Cancel();
}
