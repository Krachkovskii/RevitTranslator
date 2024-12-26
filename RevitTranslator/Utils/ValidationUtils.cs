using System.Text.RegularExpressions;
using RevitTranslator.Models;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.Utils;
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
    public static bool IsValidForFamilyEdit(Element? element)
    {
        //TODO: Update category validation
        if (element is null) return false;
        // if (element.Category?.BuiltInCategory == BuiltInCategory.OST_TitleBlocks) return true;

        return element is IndependentTag;
    }

    /// <summary>
    /// Checks if translation is applied to parameter or element name. 
    /// If yes, checks for illegal Revit characters
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
     public static bool NameHasIllegalCharacters(RevitTranslationUnit unit)
     {
         if (unit.Element is Parameter p || unit.TranslationDetails == TranslationDetails.ElementName)
         {
             if (unit.TranslatedText.Any(c => ValidationUtils.ForbiddenParameterSymbols.Contains(c)))
             {
                 return true;
             }
         }
         return false;
     }
}
