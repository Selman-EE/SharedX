using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedX.Extensions.CollectionExt;

/// <summary>
///     Helper for creating instances and JSON operations (AOT-compatible).
/// </summary>
public static class MessageHelper
{
    /// <summary>
    ///     Creates instance of type T (AOT-compatible, no reflection).
    /// </summary>
    public static T CreateInstance<T>() where T : new()
    {
        return new T();
    }

    /// <summary>
    ///     Deserializes JSON to string (AOT-compatible).
    /// </summary>
    public static string? DeserializeString(string json)
    {
        return JsonSerializer.Deserialize(json, SharedXJsonContext.Default.String);
    }

    /// <summary>
    ///     Deserializes JSON to Dictionary (AOT-compatible).
    /// </summary>
    public static Dictionary<string, object>? DeserializeDictionary(string json)
    {
        return JsonSerializer.Deserialize(json, SharedXJsonContext.Default.DictionaryStringObject);
    }

    /// <summary>
    ///     Serializes string to JSON (AOT-compatible).
    /// </summary>
    public static string SerializeString(string value)
    {
        return JsonSerializer.Serialize(value, SharedXJsonContext.Default.String);
    }

    /// <summary>
    ///     Serializes Dictionary to JSON (AOT-compatible).
    /// </summary>
    public static string SerializeDictionary(Dictionary<string, object> value)
    {
        return JsonSerializer.Serialize(value, SharedXJsonContext.Default.DictionaryStringObject);
    }

    /// <summary>
    ///     Serializes Dictionary to JSON (AOT-compatible).
    /// </summary>
    public static string SerializeStringDictionary(Dictionary<string, string> value)
    {
        return JsonSerializer.Serialize(value, SharedXJsonContext.Default.DictionaryStringString);
    }
}

// ✅ JSON Source Generator for AOT compatibility
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(List<string>))]
internal partial class SharedXJsonContext : JsonSerializerContext
{
}