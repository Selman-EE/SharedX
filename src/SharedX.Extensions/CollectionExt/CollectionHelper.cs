namespace SharedX.Extensions.CollectionExt;

/// <summary>
///     Provides extension methods for working with collections efficiently.
/// </summary>
public static class CollectionHelper
{
    #region List Conversions

    /// <summary>
    ///     Converts an IEnumerable to a List, avoiding unnecessary allocations if already a List.
    ///     Throws ArgumentNullException if input is null (fail-fast approach).
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="enumerable">The enumerable to convert.</param>
    /// <returns>A List containing the elements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when enumerable is null.</exception>
    /// <remarks>
    ///     Performance: If the source is already a List, returns it directly without creating a new list.
    /// </remarks>
    public static List<T> AsList<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
            throw new ArgumentNullException(nameof(enumerable));

        return enumerable as List<T> ?? enumerable.ToList();
    }

    #endregion


    /// <summary>
    ///     Returns the collection if not null, otherwise returns an empty enumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="source">The collection to check.</param>
    /// <returns>The original collection or an empty enumerable.</returns>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    public static IEnumerable<T> DistinctBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector)
    {
        return source.DistinctBy(keySelector);
    }

    #region IsNullOrEmpty

    /// <summary>
    ///     Determines whether a collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>true if the collection is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        if (collection == null)
            return true;

        // Optimize for ICollection
        if (collection is ICollection<T> genericCollection)
            return genericCollection.Count == 0;

        // Fallback to enumeration
        return !collection.Any();
    }

    /// <summary>
    ///     Determines whether a collection has any elements.
    ///     Opposite of IsNullOrEmpty.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>true if the collection has elements; otherwise, false.</returns>
    public static bool HasItems<T>(this IEnumerable<T>? collection)
    {
        return !collection.IsNullOrEmpty();
    }

    #endregion


    #region Shuffle

    /// <summary>
    ///     Randomly shuffles the elements of a collection using the Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="source">The collection to shuffle.</param>
    /// <returns>A new list with elements in random order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <remarks>
    ///     Uses Fisher-Yates shuffle algorithm for uniform random distribution.
    ///     Creates a new list; does not modify the original collection.
    /// </remarks>
    public static List<T> Shuffle<T>(this IEnumerable<T> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var list = source.AsList();
        var random = new Random();
        var n = list.Count;

        // Fisher-Yates shuffle
        for (var i = n - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            // Swap
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    /// <summary>
    ///     Randomly shuffles the elements of a collection using a specified Random instance.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="source">The collection to shuffle.</param>
    /// <param name="random">The Random instance to use for shuffling.</param>
    /// <returns>A new list with elements in random order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source or random is null.</exception>
    public static List<T> Shuffle<T>(this IEnumerable<T> source, Random random)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (random == null)
            throw new ArgumentNullException(nameof(random));

        var list = source.AsList();
        var n = list.Count;

        for (var i = n - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    #endregion
}