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

    public ArgumentParser(ILogger<ArgumentParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Dictionary<string, string> Parse(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (args == null || args.Length == 0)
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

    // Validate that required arguments are present
    public bool ValidateRequired(Dictionary<string, string> args, params string[] requiredKeys)
    {
        var missing = requiredKeys.Where(k => !args.ContainsKey(k)).ToList();

        if (missing.Any())
        {
            _logger.LogWarning("Missing required arguments: {MissingArgs}", string.Join(", ", missing));
            return false;
        }

        return true;
    }

    // Get argument value with default fallback
    public string GetValue(Dictionary<string, string> args, string key, string defaultValue = "")
    {
        return args.TryGetValue(key, out var value) ? value : defaultValue;
    }

    // Parse multiple values separated by comma
    public List<string> ParseList(Dictionary<string, string> args, string key)
    {
        if (!args.TryGetValue(key, out var value))
            return new List<string>();

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }
}
