// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for CompressionHelper that provide additional convenience
// operations while maintaining compatibility with the existing API.
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Extension methods for <see cref="CompressionHelper"/> that provide additional
/// compression/decompression operations for collections and streams.
/// </summary>
public static class CompressionHelperExtensions
{
    /// <summary>
    /// Compresses a collection of byte arrays into a single compressed byte array.
    /// Each inner array is compressed separately, then all compressed results are
    /// concatenated and compressed again for optimal space efficiency.
    /// </summary>
    /// <param name="helper">The compression helper instance.</param>
    /// <param name="dataChunks">Collection of byte arrays to compress.</param>
    /// <returns>A single compressed byte array containing all compressed chunks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> or <paramref name="dataChunks"/> is null.</exception>
    public static async Task<byte[]> CompressManyAsync(this CompressionHelper helper, IEnumerable<byte[]> dataChunks)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentNullException.ThrowIfNull(dataChunks);

        var chunks = dataChunks.ToList();
        if (chunks.Count == 0)
        {
            return Array.Empty<byte>();
        }

        // Compress each chunk individually
        var compressedChunks = new List<byte[]>();
        foreach (var chunk in chunks)
        {
            compressedChunks.Add(await helper.CompressAsync(chunk));
        }

        // Concatenate all compressed chunks
        using var concatenatedStream = new MemoryStream();
        foreach (var compressedChunk in compressedChunks)
        {
            await concatenatedStream.WriteAsync(compressedChunk);
        }

        var concatenatedData = concatenatedStream.ToArray();
        return await helper.CompressAsync(concatenatedData);
    }

    /// <summary>
    /// Decompresses a collection of byte arrays that were previously compressed
    /// using <see cref="CompressManyAsync"/>.
    /// </summary>
    /// <param name="helper">The compression helper instance.</param>
    /// <param name="compressedData">The compressed byte array containing multiple chunks.</param>
    /// <returns>Collection of decompressed byte arrays in original order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> or <paramref name="compressedData"/> is null.</exception>
    public static async Task<IReadOnlyList<byte[]>> DecompressManyAsync(this CompressionHelper helper, byte[] compressedData)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentNullException.ThrowIfNull(compressedData);

        var decompressed = await helper.DecompressAsync(compressedData);

        // Decompress the concatenated data
        using var inputStream = new MemoryStream(decompressed);
        using var outputListStream = new MemoryStream();
        await inputStream.CopyToAsync(outputListStream);
        var allChunks = outputListStream.ToArray();

        // Find individual chunk boundaries by checking for GZip magic bytes
        var result = new List<byte[]>();
        using var chunksStream = new MemoryStream(allChunks);

        while (chunksStream.Position < chunksStream.Length)
        {
            // Read the data at current position
            var buffer = new byte[1024];
            var startPosition = chunksStream.Position;
            int bytesRead = await chunksStream.ReadAsync(buffer);

            // Check if current position starts a new GZip chunk
            if (helper.IsGZipCompressed(buffer))
            {
                // Found next chunk, rewind and read it
                chunksStream.Position = startPosition;
                using var chunkStream = new MemoryStream();
                await chunksStream.CopyToAsync(chunkStream);
                var chunkData = chunkStream.ToArray();
                result.Add(await helper.DecompressAsync(chunkData));
            }
            else
            {
                // If we reached the end without finding another chunk, this is the last one
                if (chunksStream.Position >= chunksStream.Length)
                {
                    chunksStream.Position = startPosition;
                    using var lastChunkStream = new MemoryStream();
                    await chunksStream.CopyToAsync(lastChunkStream);
                    var lastChunkData = lastChunkStream.ToArray();
                    result.Add(await helper.DecompressAsync(lastChunkData));
                }
            }
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Compresses a collection of strings into a single compressed base64 string.
    /// Each string is compressed and base64 encoded separately, then all results
    /// are concatenated and compressed again for optimal storage.
    /// </summary>
    /// <param name="helper">The compression helper instance.</param>
    /// <param name="texts">Collection of strings to compress.</param>
    /// <returns>A single compressed base64 string containing all compressed texts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> or <paramref name="texts"/> is null.</exception>
    public static async Task<string> CompressManyStringsAsync(this CompressionHelper helper, IEnumerable<string> texts)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentNullException.ThrowIfNull(texts);

        var stringList = texts.ToList();
        if (stringList.Count == 0)
        {
            return string.Empty;
        }

        // Compress each string individually
        var compressedStrings = new List<byte[]>();
        foreach (var text in stringList)
        {
            compressedStrings.Add(await helper.CompressAsync(System.Text.Encoding.UTF8.GetBytes(text)));
        }

        // Concatenate all compressed byte arrays
        using var concatenatedStream = new MemoryStream();
        foreach (var compressedBytes in compressedStrings)
        {
            await concatenatedStream.WriteAsync(compressedBytes);
        }

        var concatenatedData = concatenatedStream.ToArray();
        var finalCompressed = await helper.CompressAsync(concatenatedData);
        return Convert.ToBase64String(finalCompressed);
    }

    /// <summary>
    /// Decompresses a collection of strings that were previously compressed
    /// using <see cref="CompressManyStringsAsync"/>.
    /// </summary>
    /// <param name="helper">The compression helper instance.</param>
    /// <param name="compressedBase64">The compressed base64 string containing multiple texts.</param>
    /// <returns>Collection of decompressed strings in original order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> or <paramref name="compressedBase64"/> is null.</exception>
    /// <exception cref="FormatException"><paramref name="compressedBase64"/> is not a valid base64 string.</exception>
    public static async Task<IReadOnlyList<string>> DecompressManyStringsAsync(this CompressionHelper helper, string compressedBase64)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentException.ThrowIfNullOrEmpty(compressedBase64);

        var compressedData = Convert.FromBase64String(compressedBase64);
        var decompressedBytes = await helper.DecompressAsync(compressedData);

        // Decompress the concatenated data
        using var inputStream = new MemoryStream(decompressedBytes);
        using var outputListStream = new MemoryStream();
        await inputStream.CopyToAsync(outputListStream);
        var allChunks = outputListStream.ToArray();

        // Find individual chunk boundaries
        var result = new List<string>();
        using var chunksStream = new MemoryStream(allChunks);

        while (chunksStream.Position < chunksStream.Length)
        {
            // Read the data at current position
            var buffer = new byte[1024];
            var startPosition = chunksStream.Position;
            int bytesRead = await chunksStream.ReadAsync(buffer);

            // Check if current position starts a new GZip chunk
            if (helper.IsGZipCompressed(buffer))
            {
                // Found next chunk, rewind and read it
                chunksStream.Position = startPosition;
                using var chunkStream = new MemoryStream();
                await chunksStream.CopyToAsync(chunkStream);
                var chunkData = chunkStream.ToArray();
                result.Add(System.Text.Encoding.UTF8.GetString(await helper.DecompressAsync(chunkData)));
            }
            else
            {
                // If we reached the end without finding another chunk, this is the last one
                if (chunksStream.Position >= chunksStream.Length)
                {
                    chunksStream.Position = startPosition;
                    using var lastChunkStream = new MemoryStream();
                    await chunksStream.CopyToAsync(lastChunkStream);
                    var lastChunkData = lastChunkStream.ToArray();
                    result.Add(System.Text.Encoding.UTF8.GetString(await helper.DecompressAsync(lastChunkData)));
                }
            }
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Compresses a stream asynchronously and returns the compressed data.
    /// </summary>
    /// <param name="helper">The compression helper instance.</param>
    /// <param name="inputStream">The input stream to compress.</param>
    /// <returns>Compressed byte array.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> or <paramref name="inputStream"/> is null.</exception>
    public static async Task<byte[]> CompressStreamAsync(this CompressionHelper helper, Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentNullException.ThrowIfNull(inputStream);

        using var outputStream = new MemoryStream();
        await inputStream.CopyToAsync(outputStream);
        return await helper.CompressAsync(outputStream.ToArray());
    }

    /// <summary>
    /// Decompresses a stream asynchronously and returns the decompressed data.
    /// </summary>
    /// <param name="helper">The compression helper instance.</param>
    /// <param name="inputStream">The compressed input stream.</param>
    /// <returns>Decompressed byte array.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> or <paramref name="inputStream"/> is null.</exception>
    public static async Task<byte[]> DecompressStreamAsync(this CompressionHelper helper, Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentNullException.ThrowIfNull(inputStream);

        using var outputStream = new MemoryStream();
        await inputStream.CopyToAsync(outputStream);
        return await helper.DecompressAsync(outputStream.ToArray());
    }
}
