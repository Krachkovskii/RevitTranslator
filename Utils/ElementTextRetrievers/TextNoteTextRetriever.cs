using RevitTranslatorAddin.Utils.App;

namespace RevitTranslatorAddin.Utils.ElementTextRetrievers;
internal class TextElementTextRetriever : BaseElementTextRetriever
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
        var unit = new TranslationUnit(textElement, text);

        AddUnitToList(unit);
    }
}
