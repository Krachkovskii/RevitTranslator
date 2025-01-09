using System.Text.RegularExpressions;
using RevitTranslator.Enums;
using RevitTranslator.Models;

namespace RevitTranslator.Utils.App;
/// <summary>
/// Utilities for text validation
/// </summary>
public static class ValidationUtils
{
    /// <summary>
    /// Preset regex to avoid initialization on every call
    /// </summary>
    private static readonly Regex NumberOnlyRegex = new(@"^\d+$");

    /// <summary>
    /// Checks if the input contains only text and is not a null, whitespace, numeric or alphanumeric sequence.
    /// </summary>
    /// <param name="input">
    /// String to check.
    /// </param>
    /// <returns>
    /// Bool that indicates whether the input contains only numbers
    /// </returns>
    public static bool HasText(string input)
    {
        return !(string.IsNullOrWhiteSpace(input) || NumberOnlyRegex.IsMatch(input));
    }

    /// <summary>
    /// Characters that can't be used in certain Revit text properties.
    /// </summary>
    public static readonly List<char> ForbiddenParameterSymbols = new()
    {

        '\\', ':', '{', '}', '[', ']', '|', ';', '<', '>', '?', '`', '~'
    };

    /// <summary>
    /// Checks if an element is valid for family translation.
    /// Usually, it's downloadable families, such as annotations or titleblocks.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static bool IsValidForFamilyEdit(this Element element)
    {
        return element.Category?.BuiltInCategory == BuiltInCategory.OST_TitleBlocks;
    }

    /// <summary>
    /// Checks if translation is applied to parameter or element name. 
    /// If yes, checks for illegal Revit characters
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
     public static bool NameHasIllegalCharacters(this TranslationEntity entity)
     {
         if (entity.Element is not Parameter && entity.TranslationDetails != TranslationDetails.ElementName)
             return false;

         return entity.TranslatedText.Any(c => ForbiddenParameterSymbols.Contains(c));
     }
}
