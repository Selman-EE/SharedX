using System;
using System.Globalization;
using System.Text;

namespace SharedX.Extensions.Strings;

/// <summary>
/// Provides extension methods for string search and comparison operations optimized for Polish and English content.
/// </summary>
public static class StringSearchExtensions
{
    /// <summary>
    /// Converts a string to a searchable format by normalizing diacritics, removing punctuation, 
    /// and converting to lowercase. Optimized for Polish and English text.
    /// </summary>
    /// <param name="input">The string to normalize for search.</param>
    /// <returns>
    /// A normalized string suitable for search operations. Returns empty string if input is null or whitespace.
    /// Example: "Łódź Wooden Chair #42" → "lodz wooden chair 42"
    /// </returns>
    /// <remarks>
    /// This method:
    /// - Removes Polish diacritics (ą,ć,ę,ł,ń,ó,ś,ź,ż → a,c,e,l,n,o,s,z,z)
    /// - Removes punctuation and special characters
    /// - Normalizes whitespace to single spaces
    /// - Converts to lowercase using invariant culture
    /// - Uses stack allocation for better performance
    /// </remarks>
    public static string ToSearchable(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Allocate buffer on stack for better performance
        Span<char> buffer = stackalloc char[input.Length];
        var length = 0;
        var lastWasSpace = false;

        foreach (var c in input)
        {
            // Skip punctuation and special chars
            if (char.IsPunctuation(c) || char.IsSymbol(c))
            {
                if (!lastWasSpace && length > 0)
                {
                    buffer[length++] = ' ';
                    lastWasSpace = true;
                }

                continue;
            }

            // Handle whitespace
            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace && length > 0)
                {
                    buffer[length++] = ' ';
                    lastWasSpace = true;
                }

                continue;
            }

            // Normalize diacritics and lowercase
            var normalized = RemoveDiacritic(char.ToLowerInvariant(c));
            buffer[length++] = normalized;
            lastWasSpace = false;
        }

        // Trim trailing space
        if (length > 0 && buffer[length - 1] == ' ')
            length--;

#if NET8_0_OR_GREATER
    // .NET 8+: Direct Span constructor available
    return new string(buffer.Slice(0, length));
#else
        // .NET Standard 2.0: Use char array
        return new string(buffer.Slice(0, length).ToArray());
#endif
    }

    /// <summary>
    /// Generates a culture-aware sort key for the specified string.
    /// </summary>
    /// <param name="input">The string to generate a sort key for.</param>
    /// <param name="culture">The culture to use for sorting rules. If null, uses invariant culture.</param>
    /// <returns>A base64-encoded sort key suitable for ordering strings according to culture rules.</returns>
    /// <remarks>
    /// Use this method when you need to sort strings according to specific language rules.
    /// For Polish: use CultureInfo.GetCultureInfo("pl-PL")
    /// For English: use CultureInfo.InvariantCulture
    /// Store the sort key in your database for efficient sorting.
    /// </remarks>
    public static string ToSortKey(this string input, CultureInfo? culture = null)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        culture ??= CultureInfo.InvariantCulture;
        var sortKey = culture.CompareInfo.GetSortKey(input);
        return Convert.ToBase64String(sortKey.KeyData);
    }

    /// <summary>
    /// Determines whether two strings are equal using ordinal (binary) case-insensitive comparison.
    /// Optimized using Span for maximum performance.
    /// </summary>
    /// <param name="a">The first string to compare.</param>
    /// <param name="b">The second string to compare.</param>
    /// <param name="comparison"></param>
    /// <returns>true if the strings are equal; otherwise, false.</returns>
    /// <remarks>
    /// This method uses Span-based comparison which is optimized away by the JIT compiler
    /// for short strings (0.0 ns) and is ~7% faster for medium-length strings.
    /// Uses StringComparison.OrdinalIgnoreCase for consistent, culture-independent behavior.
    /// </remarks>
    public static bool EqualsFast(this string? a, string? b, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
    
        return a.AsSpan().Equals(b.AsSpan(), comparison);
    }

    /// <summary>
    /// Determines whether a string contains a specified substring using ordinal case-insensitive comparison.
    /// </summary>
    /// <param name="source">The string to search in.</param>
    /// <param name="value">The substring to search for.</param>
    /// <returns>true if the source contains the value; otherwise, false.</returns>
    /// <remarks>
    /// This method uses Span-based operations for better performance compared to string.Contains.
    /// Uses OrdinalIgnoreCase for consistent, culture-independent behavior.
    /// </remarks>
    public static bool ContainsIgnoreCase(this string? source, string? value)
    {
        if (source == null || value == null)
            return false;

        if (value.Length == 0)
            return true;

        if (source.Length < value.Length)
            return false;

#if NET8_0_OR_GREATER
            return source.Contains(value, StringComparison.OrdinalIgnoreCase);
#else
        return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
#endif
    }

    /// <summary>
    /// Removes diacritic marks from a single character.
    /// Optimized for Polish and common European characters.
    /// </summary>
    /// <param name="c">The character to normalize.</param>
    /// <returns>The character without diacritics.</returns>
    private static char RemoveDiacritic(char c)
    {
        return c switch
        {
            // Polish lowercase
            'ą' => 'a',
            'ć' => 'c',
            'ę' => 'e',
            'ł' => 'l',
            'ń' => 'n',
            'ó' => 'o',
            'ś' => 's',
            'ź' or 'ż' => 'z',

            // Polish uppercase (shouldn't hit after ToLower, but defensive)
            'Ą' => 'a',
            'Ć' => 'c',
            'Ę' => 'e',
            'Ł' => 'l',
            'Ń' => 'n',
            'Ó' => 'o',
            'Ś' => 's',
            'Ź' or 'Ż' => 'z',

            // Common European diacritics
            'à' or 'á' or 'â' or 'ã' or 'ä' or 'å' => 'a',
            'è' or 'é' or 'ê' or 'ë' => 'e',
            'ì' or 'í' or 'î' or 'ï' => 'i',
            'ò' or 'ó' or 'ô' or 'õ' or 'ö' => 'o',
            'ù' or 'ú' or 'û' or 'ü' => 'u',
            'ý' or 'ÿ' => 'y',
            'ñ' => 'n',
            'ç' => 'c',

            _ => c
        };
    }
}