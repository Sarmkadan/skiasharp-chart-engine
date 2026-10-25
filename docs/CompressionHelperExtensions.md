# CompressionHelperExtensions

The `CompressionHelperExtensions` class provides extension methods on top of `CompressionHelper` for compressing and decompressing collections of byte arrays or strings, and for compressing or decompressing streams. It lives in the `SkiaSharpChartEngine.Utilities` namespace and does not change `CompressionHelper` itself.

All operations use GZip through the underlying `CompressionHelper.CompressAsync` and `CompressionHelper.DecompressAsync` calls. Each collection method compresses every item separately, concatenates the results, and then compresses the concatenation once more.

## API
* `public static Task<byte[]> CompressManyAsync(this CompressionHelper helper, IEnumerable<byte[]> dataChunks)`: Compresses each byte array separately, concatenates the results, and compresses the concatenation again. Returns an empty array when `dataChunks` is empty. Throws `ArgumentNullException` if either argument is null.
* `public static Task<IReadOnlyList<byte[]>> DecompressManyAsync(this CompressionHelper helper, byte[] compressedData)`: Intended to reverse `CompressManyAsync`. Throws `ArgumentNullException` if either argument is null. See the Notes section: in the current implementation it does not return the original list of chunks.
* `public static Task<string> CompressManyStringsAsync(this CompressionHelper helper, IEnumerable<string> texts)`: Encodes each string as UTF-8, compresses each one separately, concatenates the results, compresses the concatenation again, and returns the final bytes as a base64 string. Returns `string.Empty` when `texts` is empty. Throws `ArgumentNullException` if either argument is null.
* `public static Task<IReadOnlyList<string>> DecompressManyStringsAsync(this CompressionHelper helper, string compressedBase64)`: Decodes the base64 input and decompresses it. Throws `ArgumentNullException` if `helper` or `compressedBase64` is null, and `ArgumentException` if `compressedBase64` is empty. Throws `FormatException` if the input is not valid base64. See the Notes section for the chunk-boundary limitation.
* `public static Task<byte[]> CompressStreamAsync(this CompressionHelper helper, Stream inputStream)`: Copies the whole stream into memory and returns its GZip-compressed bytes. Throws `ArgumentNullException` if either argument is null.
* `public static Task<byte[]> DecompressStreamAsync(this CompressionHelper helper, Stream inputStream)`: Copies the whole stream into memory and returns its decompressed bytes. Throws `ArgumentNullException` if either argument is null.

## Usage
The following examples demonstrate how to use the `CompressionHelperExtensions` class:
```csharp
var helper = new CompressionHelper(logger);

// Example 1: Compress a stream and decompress it again
using var input = File.OpenRead("chart-data.json");
byte[] compressed = await helper.CompressStreamAsync(input);
byte[] original = await helper.DecompressStreamAsync(new MemoryStream(compressed));

// Example 2: Compress a single base64 payload from a list of strings
string packed = await helper.CompressManyStringsAsync(new[] { "series-a", "series-b" });

// Example 3: Check the result of a string round trip
IReadOnlyList<string> texts = await helper.DecompressManyStringsAsync(packed);
// Currently returns one concatenated entry: "series-aseries-b" (see Notes)
```

## Notes
* **Round-trip limitation.** `DecompressManyAsync` and `DecompressManyStringsAsync` do not restore the original collection. The GZip stream is decompressed as a whole, so the chunk boundaries are lost. Calling them on the output of `CompressManyAsync` or `CompressManyStringsAsync` returns a single item that contains all of the original data joined together. For example, `["alpha", "beta", "gamma"]` comes back as `["alphabetagamma"]`. Use `CompressStreamAsync` or `CompressAsync` directly if you need a faithful round trip of one payload. Fixing the chunk framing would change the format of existing compressed data, so it is not changed here.
* **Empty items are dropped.** `CompressionHelper.CompressAsync` returns an empty input unchanged, so an empty string or byte array in a collection adds no bytes. After a round trip it disappears from the result. `["a", "", "b"]` comes back as `["ab"]`.
* **Errors from the helper are swallowed.** `CompressionHelper.CompressAsync` and `DecompressAsync` catch their own exceptions and log them. On failure they return the input unchanged. As a result, a `DecompressManyAsync` call on data that is not GZip returns that data as a single item. It does not throw an `InvalidDataException`, and the `catch` blocks in the extension methods are effectively never reached for that case.
* **Exceptions that do propagate.** `FormatException` from invalid base64 in `DecompressManyStringsAsync` and `ArgumentNullException` / `ArgumentException` from argument checks are thrown directly.
* **Memory use.** Every method buffers its whole input in memory (`MemoryStream`, `ToList()`). Use the stream methods only when the payload comfortably fits in memory.
* **Thread safety.** The extension methods keep no state of their own. Their thread safety depends on the `CompressionHelper` instance, which holds only a logger reference.
