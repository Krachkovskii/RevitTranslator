namespace RevitTranslator.Models;

public class DocumentTranslationEntityGroup(Document document)
{
    public Document Document { get; init; } = document;
    public List<TranslationEntity> TranslationEntities { get; } = [];
}
