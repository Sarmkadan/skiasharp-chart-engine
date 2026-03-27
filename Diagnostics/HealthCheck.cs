// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Diagnostics;

/// <summary>
/// Health check service for monitoring chart engine status
/// Tracks system resources and operational metrics
/// </summary>
public class HealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly List<IHealthCheck> _checks;
    private HealthStatus _lastStatus = HealthStatus.Healthy;

    public HealthCheckService(ILogger<HealthCheckService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _checks = new List<IHealthCheck>();
    }

    /// <summary>
    /// Registers a health check
    /// </summary>
    public void RegisterCheck(IHealthCheck check)
    {
        if (check == null)
            throw new ArgumentNullException(nameof(check));

        _checks.Add(check);
        _logger.LogDebug("Health check registered: {CheckName}", check.Name);
    }

    /// <summary>
    /// Performs all registered health checks
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var result = new HealthCheckResult
        {
            Timestamp = DateTime.UtcNow,
            CheckedAt = DateTime.UtcNow
        };

        var checkResults = new List<HealthCheckEntry>();

        foreach (var check in _checks)
        {
            try
            {
                var checkResult = await check.CheckAsync(cancellationToken);
                checkResults.Add(checkResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing health check {CheckName}", check.Name);
                checkResults.Add(new HealthCheckEntry
                {
                    Name = check.Name,
                    Status = HealthStatus.Unhealthy,
                    Description = ex.Message,
                    Duration = TimeSpan.Zero
                });
            }
        }

        result.Entries = checkResults;

        // Overall status is Unhealthy if any check is unhealthy
        result.Status = checkResults.All(c => c.Status == HealthStatus.Healthy)
            ? HealthStatus.Healthy
            : HealthStatus.Unhealthy;

        _lastStatus = result.Status;

        _logger.LogInformation("Health check completed: {Status}, {HealthyCount}/{TotalCount} checks passed",
            result.Status, checkResults.Count(c => c.Status == HealthStatus.Healthy), checkResults.Count);

        return result;
    }

    /// <summary>
    /// Gets the last known health status
    /// </summary>
    public HealthStatus GetLastStatus() => _lastStatus;

    /// <summary>
    /// Gets the number of registered checks
    /// </summary>
    public int GetCheckCount() => _checks.Count;
}

/// <summary>
/// Interface for custom health checks
/// </summary>
public interface IHealthCheck
{
    string Name { get; }
    Task<HealthCheckEntry> CheckAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    public HealthStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CheckedAt { get; set; }
    public List<HealthCheckEntry> Entries { get; set; } = new();

    public override string ToString()
    {
        var healthyCount = Entries.Count(e => e.Status == HealthStatus.Healthy);
        return $"Status: {Status}, Healthy: {healthyCount}/{Entries.Count}";
    }
}

/// <summary>
/// Individual health check entry
/// </summary>
public class HealthCheckEntry
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Health status
/// </summary>
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>
/// Simple health check implementation for basic connectivity
/// </summary>
public class BasicHealthCheck : IHealthCheck
{
    private readonly Func<Task<bool>>? _checkDelegate;

    public string Name { get; }

    public BasicHealthCheck(string name, Func<Task<bool>>? checkDelegate = null)
    {
        Name = name;
        _checkDelegate = checkDelegate;
    }

    public async Task<HealthCheckEntry> CheckAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            bool isHealthy = true;
            if (_checkDelegate != null)
            {
                isHealthy = await _checkDelegate();
            }

            return new HealthCheckEntry
            {
                Name = Name,
                Status = isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                Description = isHealthy ? "Check passed" : "Check failed",
                Duration = DateTime.UtcNow - startTime
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckEntry
            {
                Name = Name,
                Status = HealthStatus.Unhealthy,
                Description = ex.Message,
                Duration = DateTime.UtcNow - startTime
            };
        }
    }
}
