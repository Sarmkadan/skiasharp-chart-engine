# CollectionExtensions

Provides a set of static extension methods for common collection operations, including batching, shuffling, deduplication, and aggregation. These methods are designed to work with `IEnumerable<T>` and related collection types, offering concise alternatives to manual loops and LINQ combinations.

## API

### `Batch<T>`
Splits the source sequence into consecutive batches.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to batch.  
  `size` (`int`) – The maximum number of elements per batch.

- **Returns**  
  `IEnumerable<IEnumerable<T>>` – A sequence of batches, each a sequence of up to `size` elements.

- **Throws**  
  `ArgumentNullException` if `source` is `null`.  
  `ArgumentOutOfRangeException` if `size` is less than 1.

### `IsNullOrEmpty<T>`
Indicates whether the source collection is `null` or contains no elements.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The collection to test.

- **Returns**  
  `bool` – `true` if `source` is `null` or empty; otherwise `false`.

- **Throws**  
  None.

### `GetOrDefault<T>`
Returns the element at the specified index, or the default value of `T` if the index is out of range.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to index.  
  `index` (`int`) – The zero-based index of the element to retrieve.

- **Returns**  
  `T?` – The element at the given index, or `default(T)` if the index is invalid.

- **Throws**  
  `ArgumentNullException` if `source` is `null`.

### `Shuffle<T>`
Returns a new sequence with the elements of the source in random order.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to shuffle.

- **Returns**  
  `IEnumerable<T>` – A shuffled copy of the source sequence.

- **Throws**  
  `ArgumentNullException` if `source` is `null`.

### `GetRandom<T>`
Returns a randomly selected element from the source sequence.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to pick from.

- **Returns**  
  `T` – A random element from the source.

- **Throws**  
  `ArgumentNullException` if `source` is `null`.  
  `InvalidOperationException` if the source sequence is empty.

### `DistinctBy<T, TKey>`
Returns distinct elements from the source sequence according to a specified key selector.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to deduplicate.  
  `keySelector` (`Func<T, TKey>`) – A function to extract the comparison key.

- **Returns**  
  `IEnumerable<T>` – A sequence containing only the first occurrence of each key.

- **Throws**  
  `ArgumentNullException` if `source` or `keySelector` is `null`.

### `ContainsAny<T>`
Determines whether the source sequence contains any of the specified items.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to search.  
  `items` (`IEnumerable<T>`) – The items to look for.

- **Returns**  
  `bool` – `true` if at least one item from `items` is present in `source`; otherwise `false`.

- **Throws**  
  `ArgumentNullException` if `source` or `items` is `null`.

### `FindDuplicates<T>`
Returns all elements that appear more than once in the source sequence.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to examine.

- **Returns**  
  `IEnumerable<T>` – A sequence of duplicate elements (each duplicate appears once in the result).

- **Throws**  
  `ArgumentNullException` if `source` is `null`.

### `ToDictionaryWithDuplicates<TKey, TValue>`
Creates a dictionary from a sequence, grouping duplicate keys into a list of values.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The source sequence.  
  `keySelector` (`Func<T, TKey>`) – A function to extract the key.  
  `valueSelector` (`Func<T, TValue>`) – A function to extract the value.

- **Returns**  
  `Dictionary<TKey, TValue>` – A dictionary where each key maps to a single value. If duplicate keys occur, the last value for that key is retained (or the method may throw – see Notes).

- **Throws**  
  `ArgumentNullException` if `source`, `keySelector`, or `valueSelector` is `null`.  
  `ArgumentException` if duplicate keys are encountered and the implementation does not allow them (check documentation for the specific overload).

### `SumBy<T>`
Computes the sum of a numeric projection of the source sequence.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to aggregate.  
  `selector` (`Func<T, decimal>`) – A function to extract the decimal value.

- **Returns**  
  `decimal` – The sum of the projected values.

- **Throws**  
  `ArgumentNullException` if `source` or `selector` is `null`.  
  `OverflowException` if the sum exceeds the range of `decimal`.

### `AverageOrDefault<T>`
Computes the average of a numeric projection, returning `0` if the sequence is empty.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to aggregate.  
  `selector` (`Func<T, double>`) – A function to extract the double value.

- **Returns**  
  `double` – The average of the projected values, or `0` if the sequence is empty.

- **Throws**  
  `ArgumentNullException` if `source` or `selector` is `null`.

### `ChunkBy<T>`
Splits the source sequence into chunks of a specified size.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The sequence to chunk.  
  `size` (`int`) – The maximum number of elements per chunk.

- **Returns**  
  `IEnumerable<IEnumerable<T>>` – A sequence of chunks.

- **Throws**  
  `ArgumentNullException` if `source` is `null`.  
  `ArgumentOutOfRangeException` if `size` is less than 1.

### `Interleave<T>`
Merges two sequences by alternating elements.

- **Parameters**  
  `source` (`IEnumerable<T>`) – The first sequence.  
  `other` (`IEnumerable<T>`) – The second sequence.

- **Returns**  
  `IEnumerable<T>` – A sequence where elements from `source` and `other` are interleaved. If one sequence is longer, its remaining elements appear at the end.

- **Throws**  
  `ArgumentNullException` if `source` or `other` is `null`.

## Usage

### Example 1: Batching and Shuffling

```csharp
using SkiaSharpChartEngine.Extensions;

var numbers = Enumerable.Range(1, 20);

// Batch into groups of 5
var batches = numbers.Batch(5);
foreach (var batch in batches)
{
    Console.WriteLine(string.Join(", ", batch));
}

// Shuffle the original sequence
var shuffled = numbers.Shuffle();
Console.WriteLine("Shuffled: " + string.Join(", ", shuffled));
```

### Example 2: Deduplication and Random Selection

```csharp
using SkiaSharpChartEngine.Extensions;

var items = new[] { "apple", "banana", "apple", "cherry", "banana" };

// Get distinct items by string length
var distinctByLength = items.DistinctBy(x => x.Length);
Console.WriteLine("Distinct by length: " + string.Join(", ", distinctByLength));

// Pick a random item
var randomItem = items.GetRandom();
Console.WriteLine("Random item: " + randomItem);

// Find duplicates
var duplicates = items.FindDuplicates();
Console.WriteLine("Duplicates: " + string.Join(", ", duplicates));
```

## Notes

- **Thread safety**: None of these methods are thread-safe. If the source collection is modified during enumeration (e.g., while iterating the returned `IEnumerable<T>`), the behavior is undefined and may throw `InvalidOperationException`. Callers must synchronize access or use immutable snapshots.
- **Deferred execution**: Methods that return `IEnumerable<T>` (e.g., `Batch`, `Shuffle`, `DistinctBy`, `ChunkBy`, `Interleave`) use deferred execution. The source sequence is enumerated only when the returned sequence is iterated. Materializing methods (e.g., `GetRandom`, `FindDuplicates`, `ToDictionaryWithDuplicates`) enumerate the source immediately.
- **Null handling**: All methods throw `ArgumentNullException` if a required source or selector parameter is `null`. `IsNullOrEmpty` is the only method that safely accepts `null` (returning `true`).
- **Empty sequences**: `GetRandom` throws `InvalidOperationException` on an empty sequence. `AverageOrDefault` returns `0` instead of throwing. `FindDuplicates` returns an empty sequence. `SumBy` returns `0` for an empty sequence.
- **Duplicate keys in `ToDictionaryWithDuplicates`**: The method name suggests handling duplicates, but the signature returns `Dictionary<TKey, TValue>` (single value per key). Depending on the implementation, duplicate keys may cause an `ArgumentException` or the last value may be kept. Check the specific overload documentation for the exact behavior.
- **Performance**: `Shuffle` and `GetRandom` internally materialize the source into a list to perform random access. For large sequences, consider the memory overhead.
