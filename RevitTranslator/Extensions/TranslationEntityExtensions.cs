using RevitTranslator.Enums;
using RevitTranslator.Models;
using RevitTranslator.Utils.App;

namespace RevitTranslator.Extensions;

public static class TranslationEntityExtensions
{
    public static bool HasText(this TranslationEntity entity)
    {
        return !string.IsNullOrWhiteSpace(entity.OriginalText);
    }

    public static bool IsTranslated(this TranslationEntity entity)
    {
        return string.IsNullOrWhiteSpace(entity.TranslatedText) && entity.TranslatedText != entity.OriginalText;
    }
    
    public static bool NameHasIllegalCharacters(this TranslationEntity entity)
    {
        if (entity.Element is not Parameter && entity.TranslationDetails != TranslationDetails.ElementName)
            return false;

        return entity.TranslatedText.Any(c => ValidationUtils.ForbiddenParameterSymbols.Contains(c));
    }
}