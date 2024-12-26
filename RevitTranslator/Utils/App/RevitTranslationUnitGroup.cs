using RevitTranslator.Models;

namespace RevitTranslator.Utils.App;

public class RevitTranslationUnitGroup
{
    public Document Document { get; private set; } = null;
    public List<RevitTranslationUnit> TranslationUnits { get; } = [];
    public RevitTranslationUnitGroup(Document document)
    {
        Document = document;
    }
    public RevitTranslationUnitGroup()
    {
    }
}
