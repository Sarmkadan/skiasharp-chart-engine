// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharpChartEngine.Extensions;

/// <summary>
/// Extension methods for IEnumerable and collections.
/// Provides utilities for grouping, filtering, and transforming data.
/// </summary>
public static class CollectionExtensions
{
    private static readonly Random _sharedRandom = new();
    // Batch collection into chunks
    /// <summary>
    /// Partitions the source sequence into chunks of the specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence to batch.</param>
    /// <param name="batchSize">The maximum size of each batch. Must be positive.</param>
    /// <returns>An enumerable of batches, each containing up to <paramref name="batchSize"/> elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="batchSize"/> is not positive.</exception>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);

        var list = source.ToList();
        for (int i = 0; i < list.Count; i += batchSize)
        {
            yield return list.Skip(i).Take(batchSize);
        }
    }

    /// <summary>
    /// Determines whether the specified sequence is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to check.</param>
    /// <returns><see langword="true"/> if the sequence is null or empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Returns the first element of the sequence, or a specified default value if the sequence is empty or null.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to get the first element from.</param>
    /// <param name="defaultValue">The default value to return if the sequence is empty or null.</param>
    /// <returns>The first element of the sequence, or <paramref name="defaultValue"/> if the sequence is empty or null.</returns>
    public static T? GetOrDefault<T>(this IEnumerable<T> source, T? defaultValue = default)
    {
        return source?.FirstOrDefault() ?? defaultValue;
    }

    /// <summary>
    /// Returns a new sequence containing the elements of the original sequence in random order.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to shuffle.</param>
    /// <returns>A new sequence with elements in random order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = source.ToList();
        return list.OrderBy(_ => _sharedRandom.Next());
    }

    /// <summary>
    /// Returns a random element from the sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to get a random element from.</param>
    /// <returns>A random element from the sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
    public static T GetRandom<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = source.ToList();
        if (list.Count == 0)
            throw new InvalidOperationException("Cannot get random from empty collection");

        return list[_sharedRandom.Next(list.Count)];
    }

    /// <summary>
    /// Returns distinct elements from the sequence by using a specified key selector function to compare values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used to determine uniqueness.</typeparam>
    /// <param name="source">The sequence to remove duplicates from.</param>
    /// <param name="selector">A function to extract the key for each element.</param>
    /// <returns>An enumerable that contains only distinct elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var seen = new HashSet<TKey>();
        return source.Where(item => seen.Add(selector(item)));
    }

    /// <summary>
    /// Determines whether any element of the sequence satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to check.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns><see langword="true"/> if any elements in the sequence pass the test in the specified predicate; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static bool ContainsAny<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return source.Any(predicate);
    }

    /// <summary>
    /// Returns all elements that appear more than once in the sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to search for duplicates.</param>
    /// <returns>An enumerable containing all duplicate elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> FindDuplicates<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
    }

    /// <summary>
    /// Converts a sequence of key-value pairs to a dictionary, with later values overwriting earlier ones for duplicate keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="source">The sequence of key-value pairs to convert.</param>
    /// <returns>A dictionary containing the key-value pairs from the source sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static Dictionary<TKey, TValue> ToDictionaryWithDuplicates<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var dict = new Dictionary<TKey, TValue>();

        foreach (var kvp in source)
        {
            dict[kvp.Key] = kvp.Value; // Last value wins
        }

        return dict;
    }

    /// <summary>
    /// Computes the sum of the sequence of values obtained by invoking the selector function on each element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to sum.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>The sum of the projected values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
    public static decimal SumBy<T>(this IEnumerable<T> source, Func<T, decimal> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return source.Sum(selector);
    }

    /// <summary>
    /// Computes the average of the sequence of values obtained by invoking the selector function on each element.
    /// Returns a specified default value if the sequence is empty or null.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to average.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <param name="defaultValue">The value to return if the sequence is empty or null.</param>
    /// <returns>The average of the projected values, or <paramref name="defaultValue"/> if the sequence is empty or null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
    public static double AverageOrDefault<T>(this IEnumerable<T> source, Func<T, double> selector, double defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var items = source.ToList();
        return items.Any() ? items.Average(selector) : defaultValue;
    }

    /// <summary>
    /// Partitions the source sequence into chunks separated by elements that satisfy the predicate.
    /// Each chunk ends when an element satisfies the predicate and the chunk is not empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to chunk.</param>
    /// <param name="predicate">A function to test each element for chunk separation.</param>
    /// <returns>An enumerable of chunks, each ending before an element that satisfies the predicate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var chunk = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item) && chunk.Count > 0)
            {
                yield return chunk;
                chunk = new List<T>();
            }

            chunk.Add(item);
        }

        if (chunk.Count > 0)
            yield return chunk;
    }

    /// <summary>
    /// Interleaves multiple sequences by taking elements alternately from each sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequences.</typeparam>
    /// <param name="collections">The sequences to interleave.</param>
    /// <returns>An enumerable that contains elements from each sequence in interleaved order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collections"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> Interleave<T>(params IEnumerable<T>[] collections)
    {
        ArgumentNullException.ThrowIfNull(collections);

        var enumerators = collections.Select(c => c.GetEnumerator()).ToList();

        try
        {
            bool hasMore = true;
            while (hasMore)
            {
                hasMore = false;

                foreach (var enumerator in enumerators)
                {
                    if (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                        hasMore = true;
                    }
                }
            }
        }
        finally
        {
            foreach (var enumerator in enumerators)
                enumerator.Dispose();
        }
    }
}
