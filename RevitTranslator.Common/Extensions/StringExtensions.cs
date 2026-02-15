using System.Diagnostics.Contracts;

namespace RevitTranslator.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.
    /// </summary>
    /// <param name="source">Source string.</param>
    /// <param name="value">The string to seek.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>true if the value parameter occurs within this string, or if value is the empty string (""); otherwise, false.</returns>
    // [Pure]
    // public static bool Contains(this string source, string value,
    //     StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    // {
    //     return source.IndexOf(value, comparison) >= 0;
    // }
}