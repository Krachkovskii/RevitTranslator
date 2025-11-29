using RevitTranslator.Enums;
using RevitTranslator.Models;
using RevitTranslator.Utils;

namespace RevitTranslator.Extensions;

public static class TranslationEntityExtensions
{
    extension(TranslationEntity entity)
    {
        public bool HasText()
        {
            return !string.IsNullOrWhiteSpace(entity.OriginalText);
        }

        public bool IsTranslated()
        {
            return !string.IsNullOrWhiteSpace(entity.TranslatedText) && entity.TranslatedText != entity.OriginalText;
        }

        public bool NameHasIllegalCharacters()
        {
            if (entity.Element is not Parameter && entity.TranslationDetails != TranslationDetails.ElementName)
                return false;

            return entity.TranslatedText.Any(c => ValidationUtils.ForbiddenParameterSymbols.Contains(c));
        }
    }
}