namespace RevitTranslator.Revit.Core.Contracts;

public interface ITranslationProgressMonitor
{
    void Initialize();
    void OnTextRetrieved(int unitCount);
    void OnEntityTranslated(int charCount);
    void OnTranslationFinished(bool wasCancelled);
    void OnModelUpdated();
    void OnNonUpdatableElements(IReadOnlyList<string> elements, string documentTitle);
    Task<bool> ShouldUpdateAfterCancellationAsync();
}
