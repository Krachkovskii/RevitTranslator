using RevitTranslatorAddin.Utils.App;

namespace RevitTranslatorAddin.Utils.ElementTextRetrievers;

/// <summary>
/// Base class for retrieving text properties from elements
/// </summary>
internal class BaseElementTextRetriever : IDisposable
{
    public List<TranslationUnit> TranslationUnits { get; private set; } = [];

    protected virtual string GetText(object Object) => throw new NotImplementedException();

    /// <summary>
    /// Adds valid translation unit to the list.
    /// </summary>
    /// <param name="unit"></param>
    protected virtual void AddUnitToList(TranslationUnit unit)
    {
        if (unit.OriginalText == string.Empty)
        {
            return;
        }

        TranslationUnits.Add(unit);
    }

    protected virtual void Process(object Object) => throw new NotImplementedException();

    public void Dispose()
    {
        TranslationUnits.Clear();
        TranslationUnits = null;
    }
}
