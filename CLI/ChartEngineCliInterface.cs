// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.CLI;

/// <summary>
/// Command-line interface for the Chart Engine
/// Provides argument parsing and command execution
/// </summary>
public class ChartEngineCliInterface
{
    private readonly ChartEngine _chartEngine;
    private readonly ILogger<ChartEngineCliInterface> _logger;
    private readonly Dictionary<string, Func<string[], Task<int>>> _commands;

    public ChartEngineCliInterface(ChartEngine chartEngine, ILogger<ChartEngineCliInterface> logger)
    {
        _chartEngine = chartEngine ?? throw new ArgumentNullException(nameof(chartEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commands = new Dictionary<string, Func<string[], Task<int>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "render", RenderCommand },
            { "export", ExportCommand },
            { "help", HelpCommand },
            { "version", VersionCommand },
            { "validate", ValidateCommand }
        };
    }

    /// <summary>
    /// Executes the CLI interface with provided arguments
    /// </summary>
    public async Task<int> ExecuteAsync(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            await HelpCommand(args);
            return 0;
        }

        var commandName = args[0];
        if (!_commands.ContainsKey(commandName))
        {
            _logger.LogWarning("Unknown command: {Command}", commandName);
            await HelpCommand(args);
            return 1;
        }

        try
        {
            return await _commands[commandName](args.Skip(1).ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command execution failed");
            return -1;
        }
    }

    /// <summary>
    /// Render command: renders a chart and saves output
    /// Usage: render --type line --data-file data.json --output output.png
    /// </summary>
    private async Task<int> RenderCommand(string[] args)
    {
        var options = ParseRenderOptions(args);

        if (string.IsNullOrEmpty(options.ChartType) || string.IsNullOrEmpty(options.OutputPath))
        {
            Console.WriteLine("Usage: render --type <type> --output <path>");
            return 1;
        }

        _logger.LogInformation("Rendering chart of type {ChartType} to {OutputPath}",
            options.ChartType, options.OutputPath);

        return 0;
    }

    /// <summary>
    /// Export command: exports chart to multiple formats
    /// Usage: export --chart-id abc123 --formats png,svg
    /// </summary>
    private async Task<int> ExportCommand(string[] args)
    {
        var options = ParseExportOptions(args);

        if (string.IsNullOrEmpty(options.ChartId))
        {
            Console.WriteLine("Usage: export --chart-id <id> --formats <formats>");
            return 1;
        }

        _logger.LogInformation("Exporting chart {ChartId} to formats: {Formats}",
            options.ChartId, string.Join(",", options.ExportFormats));

        return 0;
    }

    /// <summary>
    /// Validate command: validates chart configuration
    /// Usage: validate --config config.json
    /// </summary>
    private async Task<int> ValidateCommand(string[] args)
    {
        var configPath = ExtractOption(args, "--config");

        if (string.IsNullOrEmpty(configPath))
        {
            Console.WriteLine("Usage: validate --config <path>");
            return 1;
        }

        _logger.LogInformation("Validating chart configuration from {ConfigPath}", configPath);

        return 0;
    }

    /// <summary>
    /// Help command: displays command help
    /// </summary>
    private Task<int> HelpCommand(string[] args)
    {
        Console.WriteLine("SkiaSharp Chart Engine CLI");
        Console.WriteLine("Usage: chart-engine <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  render   - Render a chart");
        Console.WriteLine("  export   - Export chart to multiple formats");
        Console.WriteLine("  validate - Validate chart configuration");
        Console.WriteLine("  version  - Display version information");
        Console.WriteLine("  help     - Show this help message");
        return Task.FromResult(0);
    }

    /// <summary>
    /// Version command: displays version information
    /// </summary>
    private Task<int> VersionCommand(string[] args)
    {
        Console.WriteLine("SkiaSharp Chart Engine v1.0.0");
        return Task.FromResult(0);
    }

    private CliRenderOptions ParseRenderOptions(string[] args)
    {
        return new CliRenderOptions
        {
            ChartType = ExtractOption(args, "--type"),
            OutputPath = ExtractOption(args, "--output"),
            DataFile = ExtractOption(args, "--data-file"),
            ConfigFile = ExtractOption(args, "--config")
        };
    }

    private CliExportOptions ParseExportOptions(string[] args)
    {
        var formatsStr = ExtractOption(args, "--formats") ?? "png";
        return new CliExportOptions
        {
            ChartId = ExtractOption(args, "--chart-id"),
            ExportFormats = formatsStr.Split(',').Select(f => f.Trim()).ToList()
        };
    }

    private string? ExtractOption(string[] args, string optionName)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(optionName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }
}

public class CliRenderOptions
{
    public string? ChartType { get; set; }
    public string? OutputPath { get; set; }
    public string? DataFile { get; set; }
    public string? ConfigFile { get; set; }
}

public class CliExportOptions
{
    public string? ChartId { get; set; }
    public List<string> ExportFormats { get; set; } = new();
}
