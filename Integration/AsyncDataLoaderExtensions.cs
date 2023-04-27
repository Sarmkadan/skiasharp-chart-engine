// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Extension methods for AsyncDataLoader to provide additional functionality
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Integration;

/// <summary>
/// Provides extension methods for <see cref="AsyncDataLoader"/> to enhance functionality
/// with additional chart loading capabilities.
/// </summary>
public static class AsyncDataLoaderExtensions
{
    /// <summary>
    /// Loads a chart from a file path, automatically determining the file type
    /// and falling back to CSV if JSON fails.
    /// </summary>
    /// <param name="loader">The <see cref="AsyncDataLoader"/> instance.</param>
    /// <param name="filePath">Path to the chart file (JSON or CSV).</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A loaded <see cref="Chart"/> instance if successful; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static async Task<Chart?> LoadChartFromFileWithFallbackAsync(
        this AsyncDataLoader loader,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        // Try JSON first (most common format)
        var chart = await loader.LoadChartFromFileAsync(filePath, cancellationToken).ConfigureAwait(false);

        return chart ?? await TryLoadCsvFallbackAsync(loader, filePath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Loads all charts from a directory, filtering by file extension.
    /// </summary>
    /// <param name="loader">The <see cref="AsyncDataLoader"/> instance.</param>
    /// <param name="directoryPath">Path to the directory containing chart files.</param>
    /// <param name="fileExtensions">File extensions to include (e.g., [".json", ".csv"]).</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="List{Chart}"/> containing all successfully loaded charts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="directoryPath"/> is <see langword="null"/>, empty, or consists only of whitespace.
    /// -or-
    /// <paramref name="fileExtensions"/> is <see langword="null"/> or empty.
    /// </exception>
    public static async Task<List<Chart>> LoadChartsFromDirectoryAsync(
        this AsyncDataLoader loader,
        string directoryPath,
        string[] fileExtensions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentException.ThrowIfNullOrEmpty(fileExtensions);

        var charts = new List<Chart>();

        if (!Directory.Exists(directoryPath))
        {
            loader.LogDirectoryNotFound(directoryPath);
            return charts;
        }

        try
        {
            // Get all files with any of the specified extensions
            var files = fileExtensions
                .SelectMany(ext => Directory.GetFiles(directoryPath, $"*{ext}", SearchOption.TopDirectoryOnly))
                .Distinct()
                .ToArray();

            foreach (var file in files)
            {
                var chart = await loader.LoadChartFromFileAsync(file, cancellationToken).ConfigureAwait(false);
                if (chart is not null)
                {
                    charts.Add(chart);
                }
            }
        }
        catch (Exception ex)
        {
            loader.LogDirectoryLoadError(directoryPath, ex);
        }

        return charts;
    }

    /// <summary>
    /// Loads charts from multiple directories, merging results.
    /// </summary>
    /// <param name="loader">The <see cref="AsyncDataLoader"/> instance.</param>
    /// <param name="directoryPaths">Array of directory paths to search.</param>
    /// <param name="searchPattern">File search pattern (e.g., "*.json").</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="List{Chart}"/> containing all loaded charts from all directories.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPaths"/> is <see langword="null"/> or empty.</exception>
    public static async Task<List<Chart>> LoadChartsFromDirectoriesAsync(
        this AsyncDataLoader loader,
        string[] directoryPaths,
        string searchPattern = "*.json",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentException.ThrowIfNullOrEmpty(directoryPaths);

        var allCharts = new List<Chart>();

        foreach (var directoryPath in directoryPaths)
        {
            var charts = await loader.LoadChartsFromDirectoryAsync(directoryPath, [searchPattern], cancellationToken).ConfigureAwait(false);
            allCharts.AddRange(charts);
        }

        return allCharts;
    }

    /// <summary>
    /// Validates multiple files at once and returns only the valid ones.
    /// </summary>
    /// <param name="loader">The <see cref="AsyncDataLoader"/> instance.</param>
    /// <param name="filePaths">Array of file paths to validate.</param>
    /// <returns>A <see cref="List{String}"/> containing only valid file paths.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    public static List<string> GetValidFiles(
        this AsyncDataLoader loader,
        string[] filePaths)
    {
        ArgumentNullException.ThrowIfNull(loader);

        return filePaths
            .Where(filePath => !string.IsNullOrWhiteSpace(filePath) && loader.CanLoadFile(filePath))
            .ToList();
    }

    #region Private logging helpers

    private static void LogDirectoryNotFound(this AsyncDataLoader loader, string directoryPath)
    {
        if (loader is IHasLogger { Logger: var logger })
        {
            logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
        }
    }

    private static void LogDirectoryLoadError(this AsyncDataLoader loader, string directoryPath, Exception ex)
    {
        if (loader is IHasLogger { Logger: var logger })
        {
            logger.LogError(ex, "Error loading charts from directory {DirectoryPath}", directoryPath);
        }
    }

    /// <summary>
    /// Attempts to load a chart from a CSV file with the same base name as the original file.
    /// </summary>
    private static async Task<Chart?> TryLoadCsvFallbackAsync(
        AsyncDataLoader loader,
        string originalFilePath,
        CancellationToken cancellationToken)
    {
        var csvPath = Path.ChangeExtension(originalFilePath, ".csv");
        return File.Exists(csvPath)
            ? await loader.LoadChartFromFileAsync(csvPath, cancellationToken).ConfigureAwait(false)
            : null;
    }

    // Interface to access logger from extension methods
    private interface IHasLogger
    {
        ILogger<AsyncDataLoader> Logger { get; }
    }

    #endregion
}