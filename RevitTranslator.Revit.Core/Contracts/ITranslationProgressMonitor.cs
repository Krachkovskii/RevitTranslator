namespace RevitTranslator.Revit.Core.Contracts;

public interface ITranslationProgressMonitor
{
    void Initialize();
    void OnTextRetrieved(int unitCount);
    void OnEntitiesTranslated(int entityCount, int charCount);
    void OnTranslationFinished(bool wasCancelled);
    void OnModelUpdated();
    Task<bool> ShouldUpdateAfterCancellationAsync();
}
