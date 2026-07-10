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
/// Provides extension methods for AsyncDataLoader to enhance functionality
/// </summary>
public static class AsyncDataLoaderExtensions
{
    /// <summary>
    /// Loads a chart from a file path, automatically determining the file type
    /// and falling back to CSV if JSON fails
    /// </summary>
    /// <param name="loader">The AsyncDataLoader instance</param>
    /// <param name="filePath">Path to the chart file (JSON or CSV)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Loaded chart or null if failed</returns>
    public static async Task<Chart?> LoadChartFromFileWithFallbackAsync(
        this AsyncDataLoader loader,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (loader == null)
            throw new ArgumentNullException(nameof(loader));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        // Try JSON first (most common format)
        var chart = await loader.LoadChartFromFileAsync(filePath, cancellationToken);

        if (chart != null)
            return chart;

        // If JSON failed, try CSV with same name but different extension
        var csvPath = Path.ChangeExtension(filePath, ".csv");
        if (File.Exists(csvPath))
        {
            return await loader.LoadChartFromFileAsync(csvPath, cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Loads all charts from a directory, filtering by file extension
    /// </summary>
    /// <param name="loader">The AsyncDataLoader instance</param>
    /// <param name="directoryPath">Path to the directory containing chart files</param>
    /// <param name="fileExtensions">File extensions to include (e.g., [".json", ".csv"])</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of loaded charts</returns>
    public static async Task<List<Chart>> LoadChartsFromDirectoryAsync(
        this AsyncDataLoader loader,
        string directoryPath,
        string[] fileExtensions,
        CancellationToken cancellationToken = default)
    {
        if (loader == null)
            throw new ArgumentNullException(nameof(loader));

        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));

        if (fileExtensions == null || fileExtensions.Length == 0)
            throw new ArgumentException("At least one file extension must be provided", nameof(fileExtensions));

        var charts = new List<Chart>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                loader.LogDirectoryNotFound(directoryPath);
                return charts;
            }

            // Build search pattern for all specified extensions
            var searchPattern = $"*.{{"{string.Join(",", fileExtensions.Select(e => e.TrimStart('.')))}"}};";

            var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var chart = await loader.LoadChartFromFileAsync(file, cancellationToken);
                if (chart != null)
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
    /// Loads charts from multiple directories, merging results
    /// </summary>
    /// <param name="loader">The AsyncDataLoader instance</param>
    /// <param name="directoryPaths">Array of directory paths to search</param>
    /// <param name="searchPattern">File search pattern (e.g., "*.json")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all loaded charts from all directories</returns>
    public static async Task<List<Chart>> LoadChartsFromDirectoriesAsync(
        this AsyncDataLoader loader,
        string[] directoryPaths,
        string searchPattern = "*.json",
        CancellationToken cancellationToken = default)
    {
        if (loader == null)
            throw new ArgumentNullException(nameof(loader));

        if (directoryPaths == null || directoryPaths.Length == 0)
            throw new ArgumentException("At least one directory path must be provided", nameof(directoryPaths));

        var allCharts = new List<Chart>();

        foreach (var directoryPath in directoryPaths)
        {
            var charts = await loader.LoadChartsFromDirectoryAsync(directoryPath, searchPattern, cancellationToken);
            allCharts.AddRange(charts);
        }

        return allCharts;
    }

    /// <summary>
    /// Validates multiple files at once and returns only the valid ones
    /// </summary>
    /// <param name="loader">The AsyncDataLoader instance</param>
    /// <param name="filePaths">Array of file paths to validate</param>
    /// <returns>List of valid file paths</returns>
    public static List<string> GetValidFiles(
        this AsyncDataLoader loader,
        string[] filePaths)
    {
        if (loader == null)
            throw new ArgumentNullException(nameof(loader));

        if (filePaths == null || filePaths.Length == 0)
            return new List<string>();

        var validFiles = new List<string>();

        foreach (var filePath in filePaths)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && loader.CanLoadFile(filePath))
            {
                validFiles.Add(filePath);
            }
        }

        return validFiles;
    }

    #region Private logging helpers

    private static void LogDirectoryNotFound(this AsyncDataLoader loader, string directoryPath)
    {
        if (loader is IHasLogger hasLogger)
        {
            hasLogger.Logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
        }
    }

    private static void LogDirectoryLoadError(this AsyncDataLoader loader, string directoryPath, Exception ex)
    {
        if (loader is IHasLogger hasLogger)
        {
            hasLogger.Logger.LogError(ex, "Error loading charts from directory {DirectoryPath}", directoryPath);
        }
    }

    // Interface to access logger from extension methods
    private interface IHasLogger
    {
        ILogger<AsyncDataLoader> Logger { get; }
    }

    #endregion
}