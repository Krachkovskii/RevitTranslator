using Autodesk.Revit.UI;
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
    private ExternalEvent _externalEvent;
    
    public Element[] SelectedElements { get; set; } = [];
    
    public void Execute()
    {
        _externalEvent = ExternalEvent.Create(new ModelUpdateHandler());
        var documentEntities = RetrieveText();

        var viewModel = new ProgressWindowViewModel();
        var view = new ProgressWindow(viewModel);
        view.Show();
        
        Translate(documentEntities);
        UpdateRevitModel();
    }

    private List<DocumentTranslationEntityGroup> RetrieveText()
    {
        var retriever = new BatchTextRetriever();
        return retriever.Translate(SelectedElements, false);
    }
    
    private void Translate(List<DocumentTranslationEntityGroup> documentEntities)
    {
        var handler = new NewConcurrentTranslationHandler();
        var cts = new CancellationTokenSource();

        try
        {
            foreach (var documentEntity in documentEntities)
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
        _externalEvent.Raise();
    }
}