using System.Text.RegularExpressions;

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
    internal static bool IsTextOnly(string input)
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

}
