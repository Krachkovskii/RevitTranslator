namespace RevitTranslatorAddin.Utils.App;
internal class TranslationUnitGroup
{
    internal Document Document { get; private set; } = null;
    internal List<TranslationUnit> TranslationUnits { get; } = [];
    internal TranslationUnitGroup(Document document)
    {
        Document = document;
    }
}
