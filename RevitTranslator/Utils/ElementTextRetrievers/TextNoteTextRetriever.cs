using RevitTranslator.Utils.App;

namespace RevitTranslator.Utils.ElementTextRetrievers;
public class TextElementTextRetriever : BaseElementTextRetriever
{
    public TextElementTextRetriever(TextElement textElement)
    {
        Process(textElement);
    }

    protected override string GetText(object Object) 
    {
        if (Object is not TextElement textElement)
        {
            return string.Empty;
        }

        return textElement.Text;
    }

    protected override void Process(object Object) 
    {
        if (Object is not TextElement textElement)
        {
            return;
        }

        var text = GetText(textElement);
        var unit = new RevitTranslationUnit(textElement, text);

        AddUnitToList(unit);
    }
}
