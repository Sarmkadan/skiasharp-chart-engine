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
    // Batch collection into chunks
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive", nameof(batchSize));

        var list = source.ToList();
        for (int i = 0; i < list.Count; i += batchSize)
        {
            yield return list.Skip(i).Take(batchSize);
        }
    }

    // Check if collection is null or empty
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
    {
        return source == null || !source.Any();
    }

    // Get or default value
    public static T? GetOrDefault<T>(this IEnumerable<T> source, T? defaultValue = default)
    {
        return source?.FirstOrDefault() ?? defaultValue;
    }

    // Shuffle collection
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var random = new Random();
        var list = source.ToList();
        return list.OrderBy(_ => random.Next());
    }

    // Get random element
    public static T GetRandom<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        if (list.Count == 0)
            throw new InvalidOperationException("Cannot get random from empty collection");

        var random = new Random();
        return list[random.Next(list.Count)];
    }

    // Distinct by selector
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
    {
        var seen = new HashSet<TKey>();
        return source.Where(item => seen.Add(selector(item)));
    }

    // Check if collection contains any element matching predicate
    public static bool ContainsAny<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return source != null && source.Any(predicate);
    }

    // Find duplicates
    public static IEnumerable<T> FindDuplicates<T>(this IEnumerable<T> source)
    {
        return source
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
    }

    // Convert to dictionary with duplicate handling
    public static Dictionary<TKey, TValue> ToDictionaryWithDuplicates<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        var dict = new Dictionary<TKey, TValue>();

        foreach (var kvp in source)
        {
            dict[kvp.Key] = kvp.Value; // Last value wins
        }

        return dict;
    }

    // Sum by selector
    public static decimal SumBy<T>(this IEnumerable<T> source, Func<T, decimal> selector)
    {
        return source?.Sum(selector) ?? 0;
    }

    // Average by selector with null handling
    public static double AverageOrDefault<T>(this IEnumerable<T> source, Func<T, double> selector, double defaultValue = 0)
    {
        var items = source?.ToList();
        return items?.Any() == true ? items.Average(selector) : defaultValue;
    }

    // Chunk by predicate
    public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
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

    // Interleave multiple collections
    public static IEnumerable<T> Interleave<T>(params IEnumerable<T>[] collections)
    {
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
