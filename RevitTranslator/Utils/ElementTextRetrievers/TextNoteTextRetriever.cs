using RevitTranslator.Models;

namespace RevitTranslator.Utils.ElementTextRetrievers;
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
        var unit = new TranslationEntity(textElement, text);

        AddUnitToList(unit);
    }

    protected override string GetText(object Object)
    {
        return Object is not TextElement textElement ? string.Empty : textElement.Text;
    }
}
