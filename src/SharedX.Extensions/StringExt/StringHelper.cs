using System.Buffers;
using System.Globalization;

namespace SharedX.Extensions.StringExt;

/// <summary>
///     Provides extension methods for string search and comparison operations optimized for Polish and English content.
/// </summary>
public static class StringHelper
{
    private static readonly SearchValues<char> _punctuationAndSymbols =
        SearchValues.Create("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~");

    private static readonly SearchValues<char> _polishDiacritics =
        SearchValues.Create("ąćęłńóśźżĄĆĘŁŃÓŚŹŻ");

    // ✅ SearchValues for fast character lookup
    private static readonly SearchValues<char> _specialChars =
        SearchValues.Create("!@#$%^&*()");

    private static readonly SearchValues<char> _digits =
        SearchValues.Create("0123456789");

    private static readonly SearchValues<char> _vowels =
        SearchValues.Create("aeiouAEIOU");

    /// <summary>
    ///     Converts a string to a searchable format by normalizing diacritics, removing punctuation,
    ///     and converting to lowercase. Optimized for Polish and English text.
    /// </summary>
    /// <param name="input">The string to normalize for search.</param>
    /// <returns>
    ///     A normalized string suitable for search operations. Returns empty string if input is null or whitespace.
    ///     Example: "Łódź Wooden Chair #42" → "lodz wooden chair 42"
    /// </returns>
    public static string ToSearchable(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // ✅ .NET 8: Use stackalloc for better performance (no heap allocation)
        Span<char> buffer = stackalloc char[input.Length];
        var length = 0;
        var lastWasSpace = false;

        foreach (var c in input)
        {
            // ✅ Use SearchValues for faster lookup
            if (char.IsPunctuation(c) || char.IsSymbol(c))
            {
                if (!lastWasSpace && length > 0)
                {
                    buffer[length++] = ' ';
                    lastWasSpace = true;
                }

                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace && length > 0)
                {
                    buffer[length++] = ' ';
                    lastWasSpace = true;
                }

                continue;
            }

            // Normalize and add character
            var normalized = RemoveDiacritic(char.ToLowerInvariant(c));
            buffer[length++] = normalized;
            lastWasSpace = false;
        }

        // Trim trailing space
        if (length > 0 && buffer[length - 1] == ' ')
            length--;

        return new string(buffer[..length]);
    }

    /// <summary>
    ///     Generates a culture-aware sort key for the specified string as HEX string.
    ///     More efficient than Base64 - avoids LOH for large strings.
    /// </summary>
    public static string ToSortKey(this string input, CultureInfo? culture = null)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        culture ??= CultureInfo.InvariantCulture;
        var sortKey = culture.CompareInfo.GetSortKey(input);

        // ✅ Convert to HEX (2x length, but avoids Base64 overhead)
        return Convert.ToHexString(sortKey.KeyData);
    }

    /// <summary>
    ///     Converts sort key HEX string back to byte array (for comparison/storage).
    /// </summary>
    public static byte[] FromSortKeyHex(string hexString)
    {
        return Convert.FromHexString(hexString);
    }

    /// <summary>
    ///     Determines whether two strings are equal using ordinal case-insensitive comparison.
    /// </summary>
    public static bool EqualsFast(this string? a, string? b,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        return a.AsSpan().Equals(b.AsSpan(), comparison);
    }

    /// <summary>
    ///     Replaces character in place (returns new string, but uses Span for speed).
    /// </summary>
    public static string ReplaceFast(this string input, char oldChar, char newChar)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return string.Create(input.Length, (input, oldChar, newChar),
            static (span, state) =>
            {
                state.input.AsSpan().CopyTo(span);
                span.Replace(state.oldChar, state.newChar);
            });
    }

    /// <summary>
    ///     Determines whether a string contains a specified substring using case-insensitive comparison.
    /// </summary>
    public static bool ContainsFast(this string? source, string? value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (source == null || value == null)
            return false;

        if (value.Length == 0)
            return true;

        if (source.Length < value.Length)
            return false;

        return source.Contains(value, comparison);
    }

    /// <summary>
    ///     Checks if string contains special characters.
    /// </summary>
    public static bool HasSpecialCharacters(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return input.AsSpan().IndexOfAny(_specialChars) >= 0;
    }

    /// <summary>
    ///     Checks if string contains only digits.
    /// </summary>
    public static bool IsNumeric(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        foreach (var c in input.AsSpan())
            if (!char.IsDigit(c))
                return false;

        return true;
    }

    /// <summary>
    ///     Checks if string contains vowels.
    /// </summary>
    public static bool HasVowels(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return input.AsSpan().IndexOfAny(_vowels) >= 0;
    }

    /// <summary>
    ///     Truncates string to specified length with optional suffix.
    /// </summary>
    public static string Truncate(this string? input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input ?? string.Empty;

        var truncatedLength = maxLength - suffix.Length;
        if (truncatedLength <= 0)
            return suffix;

        return string.Create(maxLength, (input, truncatedLength, suffix),
            static (span, state) =>
            {
                state.input.AsSpan(0, state.truncatedLength).CopyTo(span);
                state.suffix.AsSpan().CopyTo(span[state.truncatedLength..]);
            });
    }

    /// <summary>
    ///     Checks if string contains Polish diacritics.
    /// </summary>
    public static bool HasPolishDiacritics(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return input.AsSpan().IndexOfAny(_polishDiacritics) >= 0;
    }

    /// <summary>
    ///     Removes diacritic marks from a character (optimized for Polish).
    /// </summary>
    private static char RemoveDiacritic(char c)
    {
        return c switch
        {
            // Polish lowercase
            'ą' => 'a', 'ć' => 'c', 'ę' => 'e', 'ł' => 'l',
            'ń' => 'n', 'ó' => 'o', 'ś' => 's', 'ź' or 'ż' => 'z',

            // Polish uppercase
            'Ą' => 'a', 'Ć' => 'c', 'Ę' => 'e', 'Ł' => 'l',
            'Ń' => 'n', 'Ó' => 'o', 'Ś' => 's', 'Ź' or 'Ż' => 'z',

            // Common European diacritics
            'à' or 'á' or 'â' or 'ã' or 'ä' or 'å' => 'a',
            'è' or 'é' or 'ê' or 'ë' => 'e',
            'ì' or 'í' or 'î' or 'ï' => 'i',
            'ò' or 'ô' or 'õ' or 'ö' => 'o',
            'ù' or 'ú' or 'û' or 'ü' => 'u',
            'ý' or 'ÿ' => 'y',
            'ñ' => 'n', 'ç' => 'c',

            _ => c
        };
    }
}