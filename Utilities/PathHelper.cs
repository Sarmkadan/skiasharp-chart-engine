// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Path and file system utilities for chart export and storage
/// Handles path validation, file operations, and directory management
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Validates a file path is safe and doesn't escape intended directory
    /// Prevents directory traversal attacks
    /// </summary>
    public static bool IsValidPath(string path, string? basePath = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(path);

            if (!string.IsNullOrWhiteSpace(basePath))
            {
                var fullBasePath = Path.GetFullPath(basePath);
                if (!fullPath.StartsWith(fullBasePath))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets safe filename by removing invalid characters
    /// </summary>
    public static string GetSafeFilename(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be empty", nameof(filename));

        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = filename;

        foreach (var c in invalidChars)
        {
            safeName = safeName.Replace(c, '_');
        }

        return safeName.Length > 255 ? safeName[..255] : safeName;
    }

    /// <summary>
    /// Generates a unique filename by appending a number if file exists
    /// </summary>
    public static string GetUniqueFilename(string directory, string filename)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Directory {directory} not found");

        var fullPath = Path.Combine(directory, filename);
        if (!File.Exists(fullPath))
            return filename;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
        var extension = Path.GetExtension(filename);

        int counter = 1;
        while (File.Exists(Path.Combine(directory, $"{nameWithoutExt}_{counter}{extension}")))
        {
            counter++;
        }

        return $"{nameWithoutExt}_{counter}{extension}";
    }

    /// <summary>
    /// Ensures directory exists, creating it if necessary
    /// </summary>
    public static bool EnsureDirectoryExists(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets file extension for given export format
    /// </summary>
    public static string GetFileExtension(string format)
    {
        return format?.ToLowerInvariant() switch
        {
            "png" => ".png",
            "svg" => ".svg",
            "pdf" => ".pdf",
            "jpeg" or "jpg" => ".jpg",
            "bmp" => ".bmp",
            "webp" => ".webp",
            _ => ".bin"
        };
    }

    /// <summary>
    /// Gets MIME type for given file format
    /// </summary>
    public static string GetMimeType(string format)
    {
        return format?.ToLowerInvariant() switch
        {
            "png" => "image/png",
            "svg" => "image/svg+xml",
            "pdf" => "application/pdf",
            "jpeg" or "jpg" => "image/jpeg",
            "bmp" => "image/bmp",
            "webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Combines path segments safely
    /// </summary>
    public static string CombinePath(params string[] segments)
    {
        if (segments == null || segments.Length == 0)
            throw new ArgumentException("At least one path segment is required", nameof(segments));

        var validSegments = segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (validSegments.Length == 0)
            throw new ArgumentException("All path segments were empty", nameof(segments));

        return Path.Combine(validSegments);
    }

    /// <summary>
    /// Gets relative path from base directory
    /// </summary>
    public static string GetRelativePath(string baseDirectory, string fullPath)
    {
        try
        {
            var baseDirPath = Path.GetFullPath(baseDirectory);
            var fullFilePath = Path.GetFullPath(fullPath);

            var baseUri = new Uri(baseDirPath + Path.DirectorySeparatorChar);
            var fileUri = new Uri(fullFilePath);

            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString()
                .Replace('/', Path.DirectorySeparatorChar));
        }
        catch
        {
            return fullPath;
        }
    }

    /// <summary>
    /// Normalizes path separators for current platform
    /// </summary>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.Replace('/', Path.DirectorySeparatorChar)
                  .Replace('\\', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Cleans up a directory by removing old files
    /// </summary>
    public static int CleanupOldFiles(string directory, TimeSpan olderThan)
    {
        if (!Directory.Exists(directory))
            return 0;

        int deletedCount = 0;
        var cutoffTime = DateTime.Now - olderThan;

        foreach (var file in Directory.GetFiles(directory))
        {
            try
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTime < cutoffTime)
                {
                    File.Delete(file);
                    deletedCount++;
                }
            }
            catch
            {
                // Continue on error
            }
        }

        return deletedCount;
    }
}
