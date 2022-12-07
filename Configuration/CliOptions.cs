// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SkiaSharpChartEngine.Configuration;

/// <summary>
/// Command-line interface options
/// Defines CLI arguments and flags for the chart engine
/// </summary>
public class CliOptions
{
    /// <summary>
    /// Input file path (JSON or CSV)
    /// </summary>
    public string? InputFile { get; set; }

    /// <summary>
    /// Output directory path
    /// </summary>
    public string? OutputDirectory { get; set; } = "./output";

    /// <summary>
    /// Chart configuration file
    /// </summary>
    public string? ConfigFile { get; set; }

    /// <summary>
    /// Export formats (comma-separated)
    /// </summary>
    public string? ExportFormats { get; set; } = "png";

    /// <summary>
    /// Output width in pixels
    /// </summary>
    public int Width { get; set; } = 800;

    /// <summary>
    /// Output height in pixels
    /// </summary>
    public int Height { get; set; } = 600;

    /// <summary>
    /// Verbosity level (quiet, normal, verbose, debug)
    /// </summary>
    public string? Verbosity { get; set; } = "normal";

    /// <summary>
    /// Enable verbose logging
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    /// Show help message
    /// </summary>
    public bool ShowHelp { get; set; }

    /// <summary>
    /// Show version information
    /// </summary>
    public bool ShowVersion { get; set; }

    /// <summary>
    /// Number of parallel rendering threads
    /// </summary>
    public int ConcurrencyLevel { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Enable rendering cache
    /// </summary>
    public bool EnableCache { get; set; } = true;

    /// <summary>
    /// Cache maximum size in MB
    /// </summary>
    public int CacheMaxSizeMb { get; set; } = 100;

    /// <summary>
    /// Enable performance monitoring
    /// </summary>
    public bool MonitorPerformance { get; set; }

    /// <summary>
    /// Validates the CLI options
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrEmpty(InputFile) && !ShowHelp && !ShowVersion)
        {
            errors.Add("Input file is required");
        }

        if (Width < 100 || Width > 4000)
        {
            errors.Add("Width must be between 100 and 4000 pixels");
        }

        if (Height < 100 || Height > 4000)
        {
            errors.Add("Height must be between 100 and 4000 pixels");
        }

        if (ConcurrencyLevel < 1 || ConcurrencyLevel > 64)
        {
            errors.Add("Concurrency level must be between 1 and 64");
        }

        if (CacheMaxSizeMb < 1 || CacheMaxSizeMb > 1000)
        {
            errors.Add("Cache size must be between 1 and 1000 MB");
        }

        var validVerbosity = new[] { "quiet", "normal", "verbose", "debug" };
        if (!validVerbosity.Contains(Verbosity?.ToLowerInvariant() ?? "normal"))
        {
            errors.Add("Invalid verbosity level. Must be one of: quiet, normal, verbose, debug");
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Gets the export formats as a list
    /// </summary>
    public List<string> GetExportFormats()
    {
        if (string.IsNullOrEmpty(ExportFormats))
            return new List<string> { "png" };

        return ExportFormats.Split(',')
            .Select(f => f.Trim().ToLowerInvariant())
            .Where(f => !string.IsNullOrEmpty(f))
            .ToList();
    }

    /// <summary>
    /// Gets the cache size in bytes
    /// </summary>
    public long GetCacheSizeBytes() => (long)CacheMaxSizeMb * 1024 * 1024;
}

/// <summary>
/// Parser for command-line arguments
/// </summary>
public class CliOptionParser
{
    /// <summary>
    /// Parses command-line arguments into CliOptions
    /// </summary>
    public static CliOptions Parse(string[] args)
    {
        var options = new CliOptions();

        if (args == null || args.Length == 0)
            return options;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "--input" or "-i":
                    if (i + 1 < args.Length)
                        options.InputFile = args[++i];
                    break;

                case "--output" or "-o":
                    if (i + 1 < args.Length)
                        options.OutputDirectory = args[++i];
                    break;

                case "--config" or "-c":
                    if (i + 1 < args.Length)
                        options.ConfigFile = args[++i];
                    break;

                case "--format" or "-f":
                    if (i + 1 < args.Length)
                        options.ExportFormats = args[++i];
                    break;

                case "--width" or "-w":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var width))
                        options.Width = width;
                    break;

                case "--height" or "-h":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var height))
                        options.Height = height;
                    break;

                case "--verbose" or "-v":
                    options.Verbose = true;
                    options.Verbosity = "verbose";
                    break;

                case "--debug":
                    options.Verbosity = "debug";
                    break;

                case "--help":
                    options.ShowHelp = true;
                    break;

                case "--version":
                    options.ShowVersion = true;
                    break;

                case "--cache":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var cacheSize))
                        options.CacheMaxSizeMb = cacheSize;
                    break;

                case "--concurrency" or "-j":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var concurrency))
                        options.ConcurrencyLevel = concurrency;
                    break;

                case "--monitor-performance":
                    options.MonitorPerformance = true;
                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// Displays help message
    /// </summary>
    public static void DisplayHelp()
    {
        Console.WriteLine("SkiaSharp Chart Engine CLI");
        Console.WriteLine("Usage: chart-engine [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --input, -i <file>           Input file (JSON or CSV)");
        Console.WriteLine("  --output, -o <dir>          Output directory (default: ./output)");
        Console.WriteLine("  --config, -c <file>         Chart configuration file");
        Console.WriteLine("  --format, -f <formats>      Export formats (default: png)");
        Console.WriteLine("  --width, -w <pixels>        Output width (default: 800)");
        Console.WriteLine("  --height, -h <pixels>       Output height (default: 600)");
        Console.WriteLine("  --concurrency, -j <count>   Parallel threads (default: CPU count)");
        Console.WriteLine("  --cache <mb>                Cache size in MB (default: 100)");
        Console.WriteLine("  --verbose, -v               Enable verbose logging");
        Console.WriteLine("  --debug                     Enable debug logging");
        Console.WriteLine("  --monitor-performance       Monitor performance metrics");
        Console.WriteLine("  --help                      Display this help message");
        Console.WriteLine("  --version                   Display version information");
    }
}
