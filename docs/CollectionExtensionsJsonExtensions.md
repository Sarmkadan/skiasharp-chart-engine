# CollectionExtensionsJsonExtensions

Provides JSON serialization and deserialization helpers for common collection types (`ICollection<T>` and `IDictionary<TKey,TValue>`). The methods wrap `System.Text.Json` to convert collections to and from JSON strings, offering both throwing and non‑throwing variants for safe handling of invalid input.

## API

### `public static string ToJson<T>(this ICollection<T> source)`
Serializes an `ICollection<T>` instance to a JSON string.

- **Parameters**
  - `source`: The collection to serialize. If `null`, an `ArgumentNullException` is thrown.
- **Return value**
  - A JSON string representing the collection. Returns an empty array (`[]`) for an empty collection.
- **Exceptions**
  - `ArgumentNullException` – `source` is `null`.
  - `JsonException` – Serialization fails (e.g., the element type `T` is not JSON‑serializable).

### `public static ICollection<T>? FromJsonToCollection<T>(this string json)`
Deserializes a JSON string into an `ICollection<T>` instance.

- **Parameters**
  - `json`: The JSON string to deserialize. If `null` or whitespace, the method returns `null`.
- **Return value**
  - A new `List<T>` containing the deserialized elements, or `null` when the input is null/empty or does not represent a JSON array.
- **Exceptions**
  - `JsonException` – The JSON is malformed or cannot be converted to `ICollection<T>`.

### `public static bool TryFromJsonToCollection<T>(this string json, [NotNullWhen(true)] out ICollection<T>? result)`
Attempts to deserialize a JSON string into an `ICollection<T>` without throwing exceptions.

- **Parameters**
  - `json`: The JSON string to parse.
  - `result`: When the method returns `true`, contains the deserialized collection; otherwise `null`.
- **Return value**
  - `true` if `json` is a valid JSON array and can be converted to `ICollection<T>`; otherwise `false`.
- **Notes**
  - No exceptions are thrown for invalid input; the method returns `false` and `result` is set to `null`.

### `public static string ToJson<TKey, TValue>(this IDictionary<TKey, TValue> source)`
Serializes an `IDictionary<TKey,TValue>` instance to a JSON object string.

- **Parameters**
  - `source`: The dictionary to serialize. If `null`, an `ArgumentNullException` is thrown.
- **Return value**
  - A JSON object string representing the dictionary. Returns `{}` for an empty dictionary.
- **Exceptions**
  - `ArgumentNullException` – `source` is `null`.
  - `JsonException` – Serialization fails (e.g., key or value types are not JSON‑serializable).

### `public static Dictionary<TKey, TValue>? FromJsonToDictionary<TKey, TValue>(this string json)`
Deserializes a JSON string into a `Dictionary<TKey,TValue>` instance.

- **Parameters**
  - `json`: The JSON string to deserialize. If `null` or whitespace, the method returns `null`.
- **Return value**
  - A new `Dictionary<TKey,TValue>` containing the deserialized key/value pairs, or `null` when the input is null/empty or does not represent a JSON object.
- **Exceptions**
  - `JsonException` – The JSON is malformed or cannot be converted to `Dictionary<TKey,TValue>`.

### `public static bool TryFromJsonToDictionary<TKey, TValue>(this string json, [NotNullWhen(true)] out Dictionary<TKey, TValue>? result)`
Attempts to deserialize a JSON string into a `Dictionary<TKey,TValue>` without throwing exceptions.

- **Parameters**
  - `json`: The JSON string to parse.
  - `result`: When the method returns `true`, contains the deserialized dictionary; otherwise `null`.
- **Return value**
  - `true` if `json` is a valid JSON object and can be converted to `Dictionary<TKey,TValue>`; otherwise `false`.
- **Notes**
  - No exceptions are thrown for invalid input; the method returns `false` and `result` is set to `null`.

## Usage

## Usage

Serializing and deserializing a list of integers:

```csharp
using System.Collections.Generic;
using static SkiSharpChartEngine.Extensions.CollectionExtensionsJsonExtensions;

var numbers = new List<int> { 1, 2, 3 };
string json = numbers.ToJson(); // "[1,2,3]"

ICollection<int>? restored = json.FromJsonToCollection<int>();
// restored contains { 1, 2, 3 }
```

Safely parsing JSON into a dictionary with fallback:

```csharp
using System.Collections.Generic;
using static SkiSharpChartEngine.Extensions.CollectionExtensionsJsonExtensions;

string maybeJson = GetJsonFromSomewhere(); // could be null or invalid
if (TryFromJsonToDictionary<string, double>(maybeJson, out var dict))
{
    // Use dict safely
    double value = dict["key"];
}
else
{
    // Handle invalid or missing JSON
    dict = new Dictionary<string, double>();
}
```

## Notes

- All methods are statically stateless; they are safe to call from multiple threads concurrently as long as the caller does not mutate the supplied collection or dictionary while the operation is in progress.
- Passing `null` for the source collection throws `ArgumentNullException`; the nullable‑return variants (`FromJsonTo*`) return `null` for null or whitespace input rather than throwing.
- The deserialization methods produce concrete implementations (`List<T>` for collections, `Dictionary<TKey,TValue>` for dictionaries). If the caller requires a different collection type, they must convert the result themselves.
- Type `T`, `TKey`, and `TValue` must be JSON‑serializable by `System.Text.Json`; otherwise a `JsonException` is thrown (or the trying variant returns `false`).
- The methods do not preserve object references or handle circular references; such graphs will cause serialization to fail.
