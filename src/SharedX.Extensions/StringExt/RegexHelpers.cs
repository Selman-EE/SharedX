using System.Text.RegularExpressions;

namespace SharedX.Extensions.StringExt;

public static partial class RegexHelpers
{
    /// <summary>
    ///     Email validation regex (AOT-compiled at build time).
    /// </summary>
    [GeneratedRegex(@"^[\w\.-]+@[\w\.-]+\.\w+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    /// <summary>
    ///     Phone number validation (Polish format).
    /// </summary>
    [GeneratedRegex(@"^(\+48)?[\s-]?\d{3}[\s-]?\d{3}[\s-]?\d{3}$")]
    private static partial Regex PhoneRegex();

    /// <summary>
    ///     URL validation regex.
    /// </summary>
    [GeneratedRegex(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b")]
    private static partial Regex UrlRegex();

    public static bool IsValidEmail(this string? input)
    {
        return !string.IsNullOrWhiteSpace(input) && EmailRegex().IsMatch(input);
    }

    public static bool IsValidPhone(this string? input)
    {
        return !string.IsNullOrWhiteSpace(input) && PhoneRegex().IsMatch(input);
    }

    public static bool IsValidUrl(this string? input)
    {
        return !string.IsNullOrWhiteSpace(input) && UrlRegex().IsMatch(input);
    }
}