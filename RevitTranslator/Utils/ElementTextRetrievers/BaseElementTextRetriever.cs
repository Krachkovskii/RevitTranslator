using RevitTranslator.Models;

namespace RevitTranslator.Utils.ElementTextRetrievers;

/// <summary>
/// Base class for retrieving text properties from elements
/// </summary>
public class BaseElementTextRetriever : IDisposable
{
    public List<TranslationEntity> ElementTranslationUnits { get; private set; } = [];

    protected virtual void AddUnitToList(TranslationEntity entity)
    {
        if (entity.OriginalText == string.Empty) return;

        ElementTranslationUnits.Add(entity);
    }

    public void Dispose()
    {
        ElementTranslationUnits.Clear();
        ElementTranslationUnits = null;
    }
    
    protected virtual void Process(object Object) => throw new NotImplementedException();
    protected virtual string GetText(object Object) => throw new NotImplementedException();
}
