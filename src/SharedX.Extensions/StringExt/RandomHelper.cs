namespace SharedX.Extensions.StringExt;

/// <summary>
///     Thread-safe random number generation using .NET 8 Random.Shared.
/// </summary>
public static class RandomHelper
{
    /// <summary>
    ///     Generates a random integer between min (inclusive) and max (exclusive).
    ///     Thread-safe.
    /// </summary>
    public static int GetRandomNumber(int min, int max)
    {
        return Random.Shared.Next(min, max);
    }

    /// <summary>
    ///     Generates a random double between 0.0 and 1.0.
    ///     Thread-safe.
    /// </summary>
    public static double GetRandomDouble()
    {
        return Random.Shared.NextDouble();
    }

    /// <summary>
    ///     Generates a random string of specified length.
    ///     Thread-safe.
    /// </summary>
    public static string GetRandomString(int length,
        string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
    {
        return string.Create(length, chars, static (span, allowedChars) =>
        {
            for (var i = 0; i < span.Length; i++) span[i] = allowedChars[Random.Shared.Next(allowedChars.Length)];
        });
    }

    /// <summary>
    ///     Shuffles an array in place using Fisher-Yates algorithm.
    ///     Thread-safe.
    /// </summary>
    public static void Shuffle<T>(T[] array)
    {
        for (var i = array.Length - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}