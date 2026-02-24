namespace RevitTranslator.Models;

public sealed class DocumentTranslationEntityGroup(Document document)
{
    public Document Document { get; } = document;
    public List<TranslationEntity> TranslationEntities { get; } = [];
}
