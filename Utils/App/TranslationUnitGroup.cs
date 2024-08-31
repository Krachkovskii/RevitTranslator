using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RevitTranslatorAddin.Tests")]
namespace RevitTranslatorAddin.Utils.App;
public class TranslationUnitGroup
{
    public Document Document { get; private set; } = null;
    public List<TranslationUnit> TranslationUnits { get; } = [];
    public TranslationUnitGroup(Document document)
    {
        Document = document;
    }
    public TranslationUnitGroup()
    {
    }
}
