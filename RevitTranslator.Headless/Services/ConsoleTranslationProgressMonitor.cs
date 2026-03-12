using RevitTranslator.Revit.Core.Contracts;

namespace RevitTranslator.Headless.Services;

public class ConsoleTranslationProgressMonitor : ITranslationProgressMonitor
{
    public void Initialize()
    {
        Console.WriteLine("[Translation] Starting translation...");
    }

    public void OnTextRetrieved(int unitCount)
    {
        Console.WriteLine($"[Translation] Text retrieved: {unitCount} units to translate.");
    }

    public void OnEntitiesTranslated(int entityCount, int charCount)
    {
        Console.WriteLine($"[Translation] {entityCount} entities translated: {charCount} characters.");
    }

    public void OnTranslationFinished(bool wasCancelled)
    {
        Console.WriteLine(wasCancelled
            ? "[Translation] Translation was cancelled."
            : "[Translation] Translation finished.");
    }

    public void OnModelUpdated(int nonUpdatedEntitiesCount, int updatedInModelCount, int updatedFamiliesCount)
    {
        Console.WriteLine("[Translation] Model updated successfully.");
        if (nonUpdatedEntitiesCount > 0)
            Console.WriteLine($"[Translation] {nonUpdatedEntitiesCount} element name(s) were not updated due to forbidden characters.");
    }

    public Task<bool> ShouldUpdateAfterCancellationAsync()
    {
        return Task.FromResult(false);
    }
}
