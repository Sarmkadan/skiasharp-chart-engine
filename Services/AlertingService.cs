// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Alert management service for chart-related events and thresholds.
/// Tracks, filters, and notifies on chart rendering issues and anomalies.
/// </summary>
public class AlertingService
{
    private readonly List<Alert> _alerts;
    private readonly Dictionary<string, AlertRule> _rules;
    private readonly ILogger<AlertingService> _logger;
    private readonly object _lockObject = new object();

    public event EventHandler<AlertEventArgs> AlertTriggered;

    public AlertingService(ILogger<AlertingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _alerts = new List<Alert>();
        _rules = new Dictionary<string, AlertRule>();
    }

    // Register alert rule
    public void RegisterRule(string ruleName, AlertRule rule)
    {
        if (string.IsNullOrWhiteSpace(ruleName) || rule == null)
            return;

        lock (_lockObject)
        {
            _rules[ruleName] = rule;
            _logger.LogInformation("Alert rule registered: {RuleName}", ruleName);
        }
    }

    // Check condition and raise alert if needed
    public async Task CheckAsync(string ruleName, bool condition, string message, AlertSeverity severity = AlertSeverity.Info)
    {
        try
        {
            if (condition)
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    RuleName = ruleName,
                    Message = message,
                    Severity = severity,
                    Timestamp = DateTime.UtcNow,
                    Acknowledged = false
                };

                lock (_lockObject)
                {
                    _alerts.Add(alert);
                }

                _logger.LogWarning("Alert triggered: {RuleName} - {Message}", ruleName, message);

                // Raise event
                AlertTriggered?.Invoke(this, new AlertEventArgs { Alert = alert });

                await Task.Delay(10); // Simulate async operation
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking alert condition");
        }
    }

    // Acknowledge alert
    public bool AcknowledgeAlert(string alertId)
    {
        lock (_lockObject)
        {
            var alert = _alerts.FirstOrDefault(a => a.Id == alertId);
            if (alert != null)
            {
                alert.Acknowledged = true;
                alert.AcknowledgedAt = DateTime.UtcNow;
                _logger.LogInformation("Alert acknowledged: {AlertId}", alertId);
                return true;
            }

            return false;
        }
    }

    // Get active alerts
    public List<Alert> GetActiveAlerts()
    {
        lock (_lockObject)
        {
            return _alerts.Where(a => !a.Acknowledged).ToList();
        }
    }

    // Get alerts by severity
    public List<Alert> GetAlertsBySeverity(AlertSeverity severity)
    {
        lock (_lockObject)
        {
            return _alerts.Where(a => a.Severity == severity).ToList();
        }
    }

    // Clear acknowledged alerts
    public int ClearAcknowledgedAlerts()
    {
        lock (_lockObject)
        {
            var count = _alerts.RemoveAll(a => a.Acknowledged);
            _logger.LogInformation("Cleared {Count} acknowledged alerts", count);
            return count;
        }
    }

    // Get alert history
    public List<Alert> GetAlertHistory(TimeSpan? timeWindow = null)
    {
        lock (_lockObject)
        {
            if (timeWindow == null)
                return new List<Alert>(_alerts);

            var cutoff = DateTime.UtcNow - timeWindow.Value;
            return _alerts.Where(a => a.Timestamp >= cutoff).ToList();
        }
    }

    // Get alert statistics
    public AlertStatistics GetStatistics()
    {
        lock (_lockObject)
        {
            return new AlertStatistics
            {
                TotalAlerts = _alerts.Count,
                ActiveAlerts = _alerts.Count(a => !a.Acknowledged),
                CriticalAlerts = _alerts.Count(a => a.Severity == AlertSeverity.Critical),
                WarningAlerts = _alerts.Count(a => a.Severity == AlertSeverity.Warning),
                InfoAlerts = _alerts.Count(a => a.Severity == AlertSeverity.Info),
                LastAlertTime = _alerts.LastOrDefault()?.Timestamp ?? DateTime.UtcNow
            };
        }
    }
}

public class Alert
{
    public string Id { get; set; }
    public string RuleName { get; set; }
    public string Message { get; set; }
    public AlertSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Acknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}

public class AlertRule
{
    public string Name { get; set; }
    public Func<bool> Condition { get; set; }
    public string Message { get; set; }
    public AlertSeverity Severity { get; set; }
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public class AlertEventArgs : EventArgs
{
    public Alert Alert { get; set; }
}

public class AlertStatistics
{
    public int TotalAlerts { get; set; }
    public int ActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int WarningAlerts { get; set; }
    public int InfoAlerts { get; set; }
    public DateTime LastAlertTime { get; set; }
}

/// <summary>
/// Extension methods for AlertingService providing additional alert management functionality.
/// </summary>
public static class AlertingServiceExtensions
{
    /// <summary>
    /// Checks if there are any active alerts matching the specified severity level.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <param name="severity">The severity level to check for</param>
    /// <returns>True if there are active alerts with the specified severity; otherwise false</returns>
    public static bool HasActiveAlerts(this AlertingService service, AlertSeverity severity)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        var activeAlerts = service.GetActiveAlerts();
        return activeAlerts.Any(a => a.Severity == severity);
    }

    /// <summary>
    /// Gets the most recent alert by timestamp.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <returns>The most recent alert, or null if no alerts exist</returns>
    public static Alert GetMostRecentAlert(this AlertingService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        return service.GetAlertHistory()
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets all alerts that match the specified condition predicate.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <param name="predicate">The condition to filter alerts</param>
    /// <returns>List of alerts matching the predicate</returns>
    public static List<Alert> GetAlertsWhere(this AlertingService service, Func<Alert, bool> predicate)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return service.GetAlertHistory()
            .Where(predicate)
            .ToList();
    }

    /// <summary>
    /// Gets the count of active alerts grouped by severity.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <returns>Dictionary with severity as key and count as value</returns>
    public static Dictionary<AlertSeverity, int> GetActiveAlertsCountBySeverity(this AlertingService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        var activeAlerts = service.GetActiveAlerts();
        return Enum.GetValues(typeof(AlertSeverity))
            .Cast<AlertSeverity>()
            .ToDictionary(
                severity => severity,
                severity => activeAlerts.Count(a => a.Severity == severity)
            );
    }

    /// <summary>
    /// Checks if a specific alert rule is currently active (has triggered alerts that haven't been acknowledged).
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <param name="ruleName">The name of the rule to check</param>
    /// <returns>True if the rule has active alerts; otherwise false</returns>
    public static bool IsRuleActive(this AlertingService service, string ruleName)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name cannot be null or empty", nameof(ruleName));

        var activeAlerts = service.GetActiveAlerts();
        return activeAlerts.Any(a => string.Equals(a.RuleName, ruleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the first unacknowledged alert with the specified severity level.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <param name="severity">The severity level to find</param>
    /// <returns>The first unacknowledged alert with the specified severity, or null if none exists</returns>
    public static Alert GetFirstUnacknowledgedAlert(this AlertingService service, AlertSeverity severity)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        return service.GetActiveAlerts()
            .FirstOrDefault(a => a.Severity == severity);
    }

    /// <summary>
    /// Gets the total number of alerts that have been triggered.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <returns>The total count of all alerts</returns>
    public static int GetTotalAlertsCount(this AlertingService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        return service.GetAlertHistory()
            .Count;
    }

    /// <summary>
    /// Checks if there are any alerts (active or acknowledged) within the specified time window.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <param name="timeWindow">The time window to check within</param>
    /// <returns>True if there are alerts within the time window; otherwise false</returns>
    public static bool HasAlertsInTimeWindow(this AlertingService service, TimeSpan timeWindow)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        var recentAlerts = service.GetAlertHistory(timeWindow);
        return recentAlerts.Count > 0;
    }

    /// <summary>
    /// Gets all alerts that have been acknowledged.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <returns>List of acknowledged alerts</returns>
    public static List<Alert> GetAcknowledgedAlerts(this AlertingService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        return service.GetAlertHistory()
            .Where(a => a.Acknowledged)
            .ToList();
    }

    /// <summary>
    /// Gets the percentage of alerts that are still active (not acknowledged).
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <returns>Percentage of active alerts (0-100)</returns>
    public static double GetActiveAlertsPercentage(this AlertingService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        var stats = service.GetStatistics();
        if (stats.TotalAlerts == 0)
            return 0;

        return Math.Round((double)stats.ActiveAlerts / stats.TotalAlerts * 100, 2);
    }

    /// <summary>
    /// Gets the most severe active alert.
    /// </summary>
    /// <param name="service">The alerting service instance</param>
    /// <returns>The most severe active alert, or null if no active alerts exist</returns>
    public static Alert GetMostSevereActiveAlert(this AlertingService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        return service.GetActiveAlerts()
            .OrderByDescending(a => a.Severity)
            .FirstOrDefault();
    }
}
