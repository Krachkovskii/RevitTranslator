using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Messages;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.UI.Views;

namespace RevitTranslator.Services;

public class UiTranslationProgressMonitor(ProgressWindow progressWindow) : ITranslationProgressMonitor
{
    private TaskCompletionSource<bool>? _updateDecisionTcs;

    public void Initialize()
    {
        progressWindow.Owner = Context.UiApplication.MainWindowHandle.ToWindow();
        progressWindow.Show();
    }

    public void OnTextRetrieved(int unitCount)
    {
        StrongReferenceMessenger.Default.Send(new TextRetrievedMessage(unitCount));
    }

    public void OnEntitiesTranslated(int entityCount, int charCount)
    {
        StrongReferenceMessenger.Default.Send(new EntitiesTranslatedMessage(entityCount, charCount));
    }

    public void OnTranslationFinished(bool wasCancelled)
    {
        StrongReferenceMessenger.Default.Send(new TranslationFinishedMessage(wasCancelled));
    }

    public void OnModelUpdated(int nonUpdatedEntitiesCount, int updatedInModelCount, int updatedFamiliesCount)
    {
        StrongReferenceMessenger.Default.Send(new ModelUpdatedMessage(nonUpdatedEntitiesCount, updatedInModelCount, updatedFamiliesCount));
    }

    public Task<bool> ShouldUpdateAfterCancellationAsync()
    {
        _updateDecisionTcs = new TaskCompletionSource<bool>();
        StrongReferenceMessenger.Default.Register<UiTranslationProgressMonitor, ModelUpdateDecisionMessage>(
            this, static (monitor, msg) =>
            {
                StrongReferenceMessenger.Default.Unregister<ModelUpdateDecisionMessage>(monitor);
                monitor._updateDecisionTcs?.TrySetResult(msg.ShouldUpdate);
            });
        return _updateDecisionTcs.Task;
    }
}
