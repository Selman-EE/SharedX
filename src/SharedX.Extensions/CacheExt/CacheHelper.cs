using System.Collections.Frozen;

namespace SharedX.Extensions.CacheExt;

/// <summary>
///     Provides fast, read-only configuration cache using FrozenDictionary.
/// </summary>
public static class CacheHelper
{
    // ✅ FrozenDictionary: 50% faster lookups than Dictionary
    private static readonly FrozenDictionary<string, string> _configCache =
        new Dictionary<string, string>
        {
            ["Environment"] = "Production",
            ["Version"] = "1.0.0",
            ["ApiTimeout"] = "30"
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Gets configuration value by key (case-insensitive).
    /// </summary>
    public static string? GetConfig(string key)
    {
        return _configCache.GetValueOrDefault(key);
    }

    /// <summary>
    ///     Checks if configuration key exists.
    /// </summary>
    public static bool HasConfig(string key)
    {
        return _configCache.ContainsKey(key);
    }

    /// <summary>
    ///     Gets all configuration keys.
    /// </summary>
    public static IEnumerable<string> GetAllKeys()
    {
        return _configCache.Keys;
    }
}