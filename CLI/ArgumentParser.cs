// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.CLI;

/// <summary>
/// Parses CLI arguments into key-value pairs supporting multiple formats:
/// --key=value, --key value, -k value
/// </summary>
public class ArgumentParser
{
    private readonly ILogger<ArgumentParser> _logger;

    /// <summary>
    /// Initializes a new instance of the ArgumentParser class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger instance used for debug and warning messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public ArgumentParser(ILogger<ArgumentParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Parses CLI arguments into key-value pairs supporting multiple formats:
    /// --key=value, --key value, -k value
    /// </summary>
    /// <param name="args">The command line arguments to parse.</param>
    /// <returns>A dictionary containing parsed argument keys and values.</returns>
    public Dictionary<string, string> Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (args.Length == 0)
            return result;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            // Skip non-option arguments
            if (!arg.StartsWith("-"))
                continue;

            // Handle --key=value format
            if (arg.Contains("="))
            {
                var parts = arg.TrimStart('-').Split('=', 2);
                if (parts.Length == 2)
                {
                    result[parts[0]] = parts[1];
                    _logger.LogDebug("Parsed argument: {Key}={Value}", parts[0], parts[1]);
                }
                continue;
            }

            // Handle --key or -k followed by value
            var key = arg.TrimStart('-');
            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
            {
                var value = args[i + 1];
                result[key] = value;
                i++; // Skip next argument as it was consumed as value
                _logger.LogDebug("Parsed argument: {Key}={Value}", key, value);
            }
            else
            {
                // Flag without value
                result[key] = "true";
                _logger.LogDebug("Parsed flag: {Key}", key);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates that all specified required argument keys are present in the parsed arguments.
    /// </summary>
    /// <param name="args">The dictionary of parsed arguments to check.</param>
    /// <param name="requiredKeys">The keys that must be present.</param>
    /// <returns>True if all required keys are present; otherwise, false.</returns>
    public bool ValidateRequired(Dictionary<string, string> args, params string[] requiredKeys)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(requiredKeys);

        var missing = requiredKeys.Where(k => !args.ContainsKey(k)).ToList();

        if (missing.Any())
        {
            _logger.LogWarning("Missing required arguments: {MissingArgs}", string.Join(", ", missing));
            return false;
        }

        return true;
    }

    /// <summary>
    /// Retrieves the value for a specified argument key, returning a default value if the key is not found.
    /// </summary>
    /// <param name="args">The dictionary of parsed arguments.</param>
    /// <param name="key">The argument key to look up.</param>
    /// <param name="defaultValue">The default value to return if the key is not found.</param>
    /// <returns>The argument value if found, otherwise the default value.</returns>
    public string GetValue(Dictionary<string, string> args, string key, string defaultValue = "")
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(defaultValue);

        return args.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Parses a comma-separated list of values from a specified argument key.
    /// </summary>
    /// <param name="args">The dictionary of parsed arguments.</param>
    /// <param name="key">The argument key containing the comma-separated values.</param>
    /// <returns>A list of trimmed, non-empty strings, or an empty list if the key is not found.</returns>
    public List<string> ParseList(Dictionary<string, string> args, string key)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(key);

        if (!args.TryGetValue(key, out var value))
            return new List<string>();

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }
}
