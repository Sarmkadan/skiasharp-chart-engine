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
/// Routes CLI commands to appropriate handlers based on command name and arguments.
/// Implements command pattern with pluggable command executors.
/// </summary>
public class CommandRouter
{
    private readonly Dictionary<string, ICommandExecutor> _commandHandlers;
    private readonly ILogger<CommandRouter> _logger;
    private readonly ArgumentParser _argumentParser;

    public CommandRouter(ILogger<CommandRouter> logger, ArgumentParser argumentParser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _argumentParser = argumentParser ?? throw new ArgumentNullException(nameof(argumentParser));
        _commandHandlers = new Dictionary<string, ICommandExecutor>(StringComparer.OrdinalIgnoreCase);
    }

    // Register a command handler for a specific command name
    public void RegisterCommand(string commandName, ICommandExecutor executor)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("Command name cannot be empty", nameof(commandName));

        _commandHandlers[commandName] = executor ?? throw new ArgumentNullException(nameof(executor));
        _logger.LogInformation("Registered command: {CommandName}", commandName);
    }

    // Route command execution to appropriate handler
    public async Task<int> RouteAsync(string[] args)
    {
        try
        {
            if (args == null || args.Length == 0)
            {
                DisplayHelp();
                return 0;
            }

            var commandName = args[0];

            // Handle help command
            if (commandName == "--help" || commandName == "-h" || commandName == "help")
            {
                DisplayHelp();
                return 0;
            }

            // Check if command is registered
            if (!_commandHandlers.TryGetValue(commandName, out var executor))
            {
                _logger.LogWarning("Unknown command: {CommandName}", commandName);
                Console.WriteLine($"Error: Unknown command '{commandName}'");
                DisplayHelp();
                return 1;
            }

            // Parse remaining arguments
            var parsedArgs = _argumentParser.Parse(args.Skip(1).ToArray());

            // Execute command
            var result = await executor.ExecuteAsync(parsedArgs);
            return result ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command routing error");
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    // Display available commands
    public void DisplayHelp()
    {
        Console.WriteLine("SkiaSharp Chart Engine CLI");
        Console.WriteLine("Usage: skiasharp-chart <command> [options]");
        Console.WriteLine("\nAvailable commands:");

        foreach (var cmd in _commandHandlers.Keys.OrderBy(k => k))
        {
            Console.WriteLine($"  {cmd}");
        }

        Console.WriteLine("\nOptions:");
        Console.WriteLine("  --help, -h       Show this help message");
        Console.WriteLine("  --version        Show version information");
    }

    // Get registered commands
    public IEnumerable<string> GetRegisteredCommands() => _commandHandlers.Keys;
}

/// <summary>
/// Interface for command executors implementing the command pattern.
/// </summary>
public interface ICommandExecutor
{
    Task<bool> ExecuteAsync(Dictionary<string, string> arguments);
}
