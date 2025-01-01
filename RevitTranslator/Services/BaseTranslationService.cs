using Nice3point.Revit.Toolkit.External.Handlers;
using RevitTranslator.Contracts;
using RevitTranslator.Models;
using RevitTranslator.UI.Views;
using RevitTranslator.Utils.App;
using RevitTranslator.Utils.ElementTextRetrievers;
using RevitTranslator.Utils.Revit;
using RevitTranslator.ViewModels;

namespace RevitTranslator.Services;

public class BaseTranslationService : IService
{
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
        return retriever.Translate(SelectedElements, false);
    }
    
    private void Translate()
    {
        var handler = new NewConcurrentTranslationHandler();
        var cts = new CancellationTokenSource();

        try
        {
            foreach (var documentEntity in _documentEntities)
            {
                cts.Token.ThrowIfCancellationRequested();
                Task.Run(async () => await handler.Execute(documentEntity.TranslationEntities, cts.Token), cts.Token);
            }
        }
        catch (OperationCanceledException e)
        {
            // do nothing
        }
    }

    private void UpdateRevitModel()
    {
        new ActionEventHandler().Raise(_ => new ModelUpdater().Update(_documentEntities));
    }
}