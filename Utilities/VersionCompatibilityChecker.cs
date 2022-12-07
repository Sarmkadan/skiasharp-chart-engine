// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Checks version compatibility for serialized charts and configurations.
/// Ensures backward compatibility and handles migrations.
/// </summary>
public class VersionCompatibilityChecker
{
    private readonly ILogger<VersionCompatibilityChecker> _logger;
    private const string CurrentVersion = "1.0.0";

    // Supported version range
    private readonly Version _minSupportedVersion = new Version(1, 0, 0);
    private readonly Version _maxSupportedVersion = new Version(1, 9, 9);

    public VersionCompatibilityChecker(ILogger<VersionCompatibilityChecker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Check if version is compatible
    public bool IsCompatible(string version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                _logger.LogWarning("Version string is empty");
                return false;
            }

            if (!Version.TryParse(version, out var parsedVersion))
            {
                _logger.LogWarning("Invalid version format: {Version}", version);
                return false;
            }

            var isCompatible = parsedVersion >= _minSupportedVersion && parsedVersion <= _maxSupportedVersion;

            _logger.LogInformation("Version compatibility check: {Version} - {Result}",
                version, isCompatible ? "Compatible" : "Incompatible");

            return isCompatible;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking version compatibility");
            return false;
        }
    }

    // Get current version
    public string GetCurrentVersion() => CurrentVersion;

    // Check if migration is needed
    public bool MigrationNeeded(string fromVersion)
    {
        try
        {
            if (!Version.TryParse(fromVersion, out var from) || !Version.TryParse(CurrentVersion, out var to))
            {
                return false;
            }

            var needsMigration = from < to;
            if (needsMigration)
            {
                _logger.LogInformation("Migration needed from {FromVersion} to {ToVersion}", from, to);
            }

            return needsMigration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking migration need");
            return false;
        }
    }

    // Get version information
    public VersionInfo GetVersionInfo()
    {
        return new VersionInfo
        {
            CurrentVersion = CurrentVersion,
            MinSupportedVersion = _minSupportedVersion.ToString(),
            MaxSupportedVersion = _maxSupportedVersion.ToString(),
            CheckedAt = DateTime.UtcNow
        };
    }

    // Validate version sequence
    public bool ValidateVersionSequence(params string[] versions)
    {
        try
        {
            if (versions == null || versions.Length == 0)
                return true;

            Version previousVersion = null;

            foreach (var version in versions)
            {
                if (!Version.TryParse(version, out var parsedVersion))
                {
                    _logger.LogWarning("Invalid version in sequence: {Version}", version);
                    return false;
                }

                if (previousVersion != null && parsedVersion < previousVersion)
                {
                    _logger.LogWarning("Version sequence is not ascending");
                    return false;
                }

                previousVersion = parsedVersion;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating version sequence");
            return false;
        }
    }
}

/// <summary>
/// Version information container.
/// </summary>
public class VersionInfo
{
    public string CurrentVersion { get; set; }
    public string MinSupportedVersion { get; set; }
    public string MaxSupportedVersion { get; set; }
    public DateTime CheckedAt { get; set; }

    public override string ToString()
    {
        return $"Version: {CurrentVersion} (Supported: {MinSupportedVersion} - {MaxSupportedVersion})";
    }
}
