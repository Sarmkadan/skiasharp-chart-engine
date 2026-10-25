# CacheKeyBuilderValidation

The `CacheKeyBuilderValidation` is a static helper class that validates cache keys produced by `CacheKeyBuilder` within the `skiasharp-chart-engine`. It offers three entry points with different reporting styles: a problem list for diagnostics (`Validate`), a boolean check for filtering (`IsValid`), and a guard that throws for fail-fast argument checking (`EnsureValid`). All three delegate the format check to `CacheKeyBuilder.IsValidCacheKey`, so the rules live in one place.

## API

*   **`Validate(string? key)`**
    Validates a cache key and returns every problem found.
    *   **Parameters:** `key` (string?) - The cache key to validate.
    *   **Returns:** An `IReadOnlyList<string>` of validation messages. The list is empty when the key is valid.
    *   **Exceptions:** `ArgumentNullException` when `key` is `null`.
    *   **Behavior:** A null, empty, or whitespace-only key yields a single message ("Cache key cannot be null, empty, or whitespace.") and returns immediately. Otherwise, a key rejected by `CacheKeyBuilder.IsValidCacheKey` yields a single message ("Cache key does not follow the valid format.").

*   **`IsValid(string? key)`**
    Checks whether a cache key is valid.
    *   **Parameters:** `key` (string?) - The cache key to validate.
    *   **Returns:** `true` if the key is non-blank and passes the format check; otherwise, `false`.
    *   **Exceptions:** `ArgumentNullException` when `key` is `null`.

*   **`EnsureValid(string? key)`**
    Throws if a cache key is invalid. Intended for guarding public entry points.
    *   **Parameters:** `key` (string?) - The cache key to validate.
    *   **Exceptions:** `ArgumentNullException` when `key` is `null`. `ArgumentException` when the key is invalid; the message is `Invalid cache key: ` followed by the joined problem messages, and the parameter name is `key`.

## Validation Rules

A key is valid when all of the following hold:

1.  It is not `null`, empty, or whitespace-only.
2.  Its first character is a letter or a digit (`char.IsLetterOrDigit`).

The format check does not inspect any other characters. Keys produced by `CacheKeyBuilder` (such as `chart_<id>` or `render_<hash>`) begin with a letter and therefore pass.

## Usage

```csharp
// Example 1: Collecting every problem with a key for logging
IReadOnlyList<string> problems = CacheKeyBuilderValidation.Validate(candidateKey);
if (problems.Count > 0)
{
    _logger.LogWarning("Rejected cache key: {Problems}", string.Join("; ", problems));
}

// Example 2: Filtering keys during cache cleanup
var validKeys = allKeys.Where(CacheKeyBuilderValidation.IsValid).ToList();

// Example 3: Guarding a public method argument
public void Store(string key, object value)
{
    CacheKeyBuilderValidation.EnsureValid(key);
    _cache.Set(key, value);
}
```

## Notes

*   **Thread-Safety:** All methods are stateless and only read their input, so they are safe for concurrent use.
*   **Null Handling:** `Validate`, `IsValid`, and `EnsureValid` throw `ArgumentNullException` for `null` input rather than reporting it as a problem. Only blank strings are reported through the returned list or `false`.
*   **Error Reporting:** `Validate` can return at most one message per call, so the list is useful for uniform diagnostics rather than enumerating several distinct defects.
*   **Scope of Checks:** The validator is intentionally lenient. It does not enforce a specific prefix, length limit, or allowed character set. A key that passes validation is not guaranteed to match a key produced by `CacheKeyBuilder`.
*   **Related:** See [CacheKeyBuilder](CacheKeyBuilder.md) for key generation.
