using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using RevitTranslator.Models;
using RevitTranslator.UI.Views;
using RevitTranslator.Utils.App;
using RevitTranslator.Utils.ElementTextRetrievers;
using RevitTranslator.Utils.Revit;
using RevitTranslator.ViewModels;
using TranslationService.Utils;

namespace RevitTranslator.Services;

public class BaseTranslationService : IRecipient<TokenCancellationRequestedMessage>
{
    private readonly CancellationTokenSource _cts = new();
    private List<DocumentTranslationEntityGroup>? _documentEntities;

    public Element[] SelectedElements { get; set; } = [];
    
    public void Execute()
    {
        var test = TranslationUtils.TryTestTranslate();
        if (!test)
        {
            MessageBox
                .Show("Could not connect to translation service.\nPlease check your credentials and internet connection, and try again.",
                    "Translation Service Error");
            return;
        }
        
        var viewModel = new ProgressWindowViewModel();
        var view = new ProgressWindow(viewModel);
        view.Show();
        
        _documentEntities = GetTextFromElements();

        Task.Run(async () =>
        {
            await Translate();
            UpdateRevitModel();
        });
    }

    private List<DocumentTranslationEntityGroup> GetTextFromElements()
    {
        var entities = new BatchTextRetriever()
            .CreateEntities(SelectedElements, false, out var unitCount);
        StrongReferenceMessenger.Default.Send(new TextRetrievedMessage(unitCount));

        return entities;
    }
    
    private async Task Translate()
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
        Handlers.ActionHandler.Raise(_ => new ModelUpdater().Update(_documentEntities!));
    }

    public void Receive(TokenCancellationRequestedMessage message)
    {
        _cts.Cancel();
    }
}