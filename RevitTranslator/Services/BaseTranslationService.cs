using CommunityToolkit.Mvvm.Messaging;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitTranslator.Common.App.Messages;
using RevitTranslator.Contracts;
using RevitTranslator.Models;
using RevitTranslator.UI.Views;
using RevitTranslator.Utils.App;
using RevitTranslator.Utils.ElementTextRetrievers;
using RevitTranslator.Utils.Revit;
using RevitTranslator.ViewModels;

namespace RevitTranslator.Services;

public class BaseTranslationService : IService, IRecipient<TokenCancellationRequestedMessage>
{
    private readonly CancellationTokenSource _cts = new();
    private List<DocumentTranslationEntityGroup> _documentEntities;

    public Element[] SelectedElements { get; set; } = [];
    
    public void Execute()
    {
        _documentEntities = RetrieveText();

        var viewModel = new ProgressWindowViewModel();
        var view = new ProgressWindow(viewModel);
        view.Show();
        
        Translate();
        UpdateRevitModel();
    }

    private List<DocumentTranslationEntityGroup> RetrieveText()
    {
        
        var retriever = new BatchTextRetriever();
        var units = retriever.CreateUnits(SelectedElements, false, out var unitCount);
        
        StrongReferenceMessenger.Default.Send(new TextRetrievedMessage(unitCount));

        return units;
    }
    
    private void Translate()
    {
        var handler = new ConcurrentTranslationHandler();

        try
        {
            Task.Run(async () =>
            {
                _cts.Token.ThrowIfCancellationRequested();
                await handler.Translate(_documentEntities.SelectMany(entity => entity.TranslationEntities).ToArray(),
                    _cts.Token);

                StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(false));
            });
        }
        catch (OperationCanceledException e)
        {
            StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(true));
        }
    }

    private void UpdateRevitModel()
    {
        new ActionEventHandler().Raise(_ => new ModelUpdater().Update(_documentEntities));
    }

    public void Receive(TokenCancellationRequestedMessage message)
    {
        _cts.Cancel();
    }
}