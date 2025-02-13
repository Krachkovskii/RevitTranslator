using System.Text.RegularExpressions;

namespace RevitTranslator.Utils.App;

/// <summary>
/// Utilities for text validation
/// </summary>
public static class ValidationUtils
{
    private static readonly Regex NumberOnlyRegex = new(@"^\d+$");

    /// <summary>
    /// Checks if the input contains letters and is not a null, whitespace or numeric sequence.
    /// </summary>
    /// <param name="input">
    /// String to check.
    /// </param>
    /// <returns>
    /// Bool that indicates whether the input contains only numbers
    /// </returns>
    public static bool HasText(this string input)
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
}
