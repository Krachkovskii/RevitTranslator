using System.Text.RegularExpressions;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Utils.App;
/// <summary>
/// Utilities for text validation
/// </summary>
internal class ValidationUtils
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
    internal static bool HasText(string input)
    {
        return !(string.IsNullOrWhiteSpace(input) || NumberOnlyRegex.IsMatch(input));
    }

    /// <summary>
    /// Characters that can't be used in certain Revit text properties.
    /// </summary>
    internal static readonly List<char> ForbiddenParameterSymbols = new()
    {

        '\\', ':', '{', '}', '[', ']', '|', ';', '<', '>', '?', '`', '~'
    };

    /// <summary>
    /// Checks if an element is valid for family translation.
    /// Usually, it's downloadable families, such as annotations or titleblocks.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    internal static bool IsValidForFamilyEdit(Element element)
    {
        if (element == null)
        {
            return false;
        }

        if (element.Category?.BuiltInCategory == BuiltInCategory.OST_TitleBlocks)
        {
            return true;
        }

        if (element is IndependentTag)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if translation is applied to parameter or element name. 
    /// If yes, checks for illegal Revit characters
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    internal static bool NameHasIllegalCharacters(TranslationUnit unit)
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
