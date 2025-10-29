namespace SharedX.Extensions.ListExt;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedX.Extensions.Collections
{
    /// <summary>
    /// Provides extension methods for working with collections efficiently.
    /// </summary>
    public static class CollectionExtensions
    {
        #region List Conversions

        /// <summary>
        /// Converts an IEnumerable to a List, avoiding unnecessary allocations if already a List.
        /// Throws ArgumentNullException if input is null (fail-fast approach).
        /// </summary>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <param name="enumerable">The enumerable to convert.</param>
        /// <returns>A List containing the elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when enumerable is null.</exception>
        /// <remarks>
        /// Performance: If the source is already a List, returns it directly without creating a new list.
        /// </remarks>
        public static List<T> AsList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            return enumerable as List<T> ?? enumerable.ToList();
        }

        #endregion

        #region IsNullOrEmpty

        /// <summary>
        /// Determines whether a collection is null or empty.
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
        /// Determines whether a collection has any elements.
        /// Opposite of IsNullOrEmpty.
        /// </summary>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <param name="collection">The collection to check.</param>
        /// <returns>true if the collection has elements; otherwise, false.</returns>
        public static bool HasItems<T>(this IEnumerable<T>? collection)
        {
            return !collection.IsNullOrEmpty();
        }

        #endregion

        #region ForEach

        /// <summary>
        /// Performs the specified action on each element of the collection.
        /// </summary>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <param name="source">The collection to iterate.</param>
        /// <param name="action">The action to perform on each element.</param>
        /// <exception cref="ArgumentNullException">Thrown when source or action is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the collection with its index.
        /// </summary>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <param name="source">The collection to iterate.</param>
        /// <param name="action">The action to perform on each element (item, index).</param>
        /// <exception cref="ArgumentNullException">Thrown when source or action is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var index = 0;
            foreach (var item in source)
            {
                action(item, index++);
            }
        }

        #endregion

        #region Batch

        /// <summary>
        /// Splits a collection into batches of the specified size.
        /// </summary>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <param name="source">The collection to batch.</param>
        /// <param name="batchSize">The size of each batch.</param>
        /// <returns>An enumerable of batches.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when batchSize is less than 1.</exception>
        /// <remarks>
        /// Performance-optimized using List with pre-allocated capacity.
        /// </remarks>
        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (batchSize < 1)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be at least 1.");

            var batch = new List<T>(batchSize);

            foreach (var item in source)
            {
                batch.Add(item);

                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }

            // Return the last partial batch if it has items
            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        #endregion

        #region DistinctBy (for netstandard2.0 compatibility)

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Returns distinct elements from a sequence according to a specified key selector function.
        /// Backport for .NET Standard 2.0 (native in .NET 6+).
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of key to distinguish elements by.</typeparam>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>An IEnumerable that contains distinct elements from the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source or keySelector is null.</exception>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var seenKeys = new HashSet<TKey>();

            foreach (var element in source)
            {
                var key = keySelector(element);
                if (seenKeys.Add(key))
                {
                    yield return element;
                }
            }
        }
#endif

        #endregion

        #region Shuffle

        /// <summary>
        /// Randomly shuffles the elements of a collection using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <param name="source">The collection to shuffle.</param>
        /// <returns>A new list with elements in random order.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <remarks>
        /// Uses Fisher-Yates shuffle algorithm for uniform random distribution.
        /// Creates a new list; does not modify the original collection.
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
        /// Randomly shuffles the elements of a collection using a specified Random instance.
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

        #region Safe Operations

        /// <summary>
        /// Returns the collection if not null, otherwise returns an empty enumerable.
        /// </summary>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <param name="source">The collection to check.</param>
        /// <returns>The original collection or an empty enumerable.</returns>
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        #endregion

        #region Chunk (for netstandard2.0 compatibility)

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Splits the elements of a sequence into chunks of a specified maximum size.
        /// Backport for .NET Standard 2.0 (native in .NET 6+).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">An IEnumerable to split into chunks.</param>
        /// <param name="size">The maximum size of each chunk.</param>
        /// <returns>An IEnumerable that contains the elements the input sequence split into chunks.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when size is less than 1.</exception>
        /// <remarks>
        /// Similar to Batch but returns arrays instead of lists (matches .NET 6 API).
        /// </remarks>
        public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> source, int size)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size), "Chunk size must be at least 1.");

            var chunk = new List<T>(size);

            foreach (var item in source)
            {
                chunk.Add(item);

                if (chunk.Count == size)
                {
                    yield return chunk.ToArray();
                    chunk.Clear();
                }
            }

            if (chunk.Count > 0)
            {
                yield return chunk.ToArray();
            }
        }
#endif

        #endregion
    }
}
