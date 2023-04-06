# CompressionHelper

The `CompressionHelper` class provides a set of asynchronous utilities for compressing and decompressing data, as well as helper methods for verifying GZip-encoded content and calculating compression efficiency. It is designed to handle both raw byte arrays and string data, leveraging GZip streams to minimize memory footprint and optimize data transmission.

## API

### Constructors

*   **`CompressionHelper()`**
    Initializes a new instance of the `CompressionHelper` class.

### Methods

*   **`Task<byte[]> CompressAsync(byte[] data)`**
    Compresses the input byte array using the GZip algorithm.
    *   **Parameters:** `data` (The byte array to compress).
    *   **Returns:** A task containing the compressed byte array.
    *   **Throws:** `ArgumentNullException` if `data` is null.

*   **`Task<byte[]> DecompressAsync(byte[] data)`**
    Decompresses the input byte array using the GZip algorithm.
    *   **Parameters:** `data` (The compressed GZip byte array).
    *   **Returns:** A task containing the decompressed byte array.
    *   **Throws:** `ArgumentNullException` if `data` is null; `InvalidDataException` if the input is not a valid GZip stream.

*   **`Task<string> CompressStringAsync(string data)`**
    Compresses a string into a base64-encoded compressed representation.
    *   **Parameters:** `data` (The string content to compress).
    *   **Returns:** A task containing the compressed, encoded string.
    *   **Throws:** `ArgumentNullException` if `data` is null.

*   **`Task<string> DecompressStringAsync(string data)`**
    Decompresses a string from its base64-encoded compressed representation.
    *   **Parameters:** `data` (The compressed string to decompress).
    *   **Returns:** A task containing the original string content.
    *   **Throws:** `ArgumentNullException` if `data` is null.

*   **`double CalculateCompressionRatio(byte[] original, byte[] compressed)`**
    Calculates the compression ratio of the compressed data relative to the original data.
    *   **Parameters:** `original` (The original byte array), `compressed` (The compressed byte array).
    *   **Returns:** The ratio as a double; a value less than 1.0 indicates effective compression.

*   **`bool IsGZipCompressed(byte[] data)`**
    Determines if a byte array starts with the standard GZip magic bytes.
    *   **Parameters:** `data` (The byte array to inspect).
    *   **Returns:** `true` if the array contains a GZip header; otherwise `false`.

## Usage

**Compressing and decompressing byte arrays**
```csharp
var helper = new CompressionHelper();
byte[] originalData = Encoding.UTF8.GetBytes("Large set of data...");

// Compress
byte[] compressedData = await helper.CompressAsync(originalData);

// Verify and Decompress
if (helper.IsGZipCompressed(compressedData))
{
    byte[] decompressedData = await helper.DecompressAsync(compressedData);
    double ratio = helper.CalculateCompressionRatio(originalData, compressedData);
}
```

**Handling string compression**
```csharp
var helper = new CompressionHelper();
string rawString = "{\"key\": \"value\", \"data\": \"...\"}";

// Compress
string compressedString = await helper.CompressStringAsync(rawString);

// Decompress
string originalString = await helper.DecompressStringAsync(compressedString);
```

## Notes

*   **Thread Safety:** The `CompressionHelper` class is stateless and thread-safe, allowing instances to be shared across multiple threads or registered as a singleton in dependency injection containers.
*   **Exception Handling:** Methods that involve decompression may throw `InvalidDataException` if the provided input is malformed or corrupted. Ensure that inputs are validated or wrapped in appropriate try-catch blocks.
*   **Performance:** For extremely large data sets, ensure that the input byte arrays fit within available system memory. Asynchronous methods are used to avoid blocking the calling thread during high-latency I/O or CPU-intensive compression tasks.
