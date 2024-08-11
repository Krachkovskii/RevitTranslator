using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.App;
internal class ValidationUtils
{
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
}
