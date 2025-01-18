using RevitTranslator.Models;

namespace RevitTranslator.ElementTextRetrievers;

public class TextElementTextRetriever : BaseElementTextRetriever
{
    public TextElementTextRetriever(TextElement textElement)
    {
        Process(textElement);
    }

    protected override sealed void Process(object Object) 
    {
        if (Object is not TextElement textElement) return;

        var text = GetText(textElement);
        var unit = new TranslationEntity
        {
            Element = textElement,
            ElementId = textElement.Id,
            Document = textElement.Document,
            OriginalText = text,
        };

        AddUnitToList(unit);
    }

    protected override string GetText(object Object)
    {
        return Object is not TextElement textElement ? string.Empty : textElement.Text;
    }
}
