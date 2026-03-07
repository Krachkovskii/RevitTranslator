using RevitTranslator.Revit.Core.Extensions;
using RevitTranslator.Revit.Core.Models;

namespace RevitTranslator.Revit.Core.ElementTextRetrievers;

/// <summary>
/// Base class for retrieving text properties from elements
/// </summary>
public class BaseElementTextRetriever : IDisposable
{
    public List<TranslationEntity> ElementTranslationUnits { get; } = [];

    protected virtual void AddUnitToList(TranslationEntity entity)
    {
        if (!entity.HasText()) return;

        ElementTranslationUnits.Add(entity);
    }

    public void Dispose()
    {
        ElementTranslationUnits.Clear();
    }

    protected virtual void Process(object Object) => throw new NotImplementedException();
    protected virtual string GetText(object Object) => throw new NotImplementedException();
}
