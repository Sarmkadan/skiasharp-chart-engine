// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SkiaSharpChartEngine.Exceptions;

/// <summary>
/// Base exception for all chart engine operations
/// </summary>
public class ChartEngineException : Exception
{
    public ChartEngineException() : base() { }

    public ChartEngineException(string message) : base(message) { }

    public ChartEngineException(string message, Exception innerException) : base(message, innerException) { }

    public int ErrorCode { get; set; } = 1000;
}

/// <summary>
/// Thrown when chart data validation fails
/// </summary>
public class InvalidChartDataException : ChartEngineException
{
    public InvalidChartDataException(string message) : base(message)
    {
        ErrorCode = 1001;
    }

    public InvalidChartDataException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = 1001;
    }
}

/// <summary>
/// Thrown when rendering operation fails
/// </summary>
public class ChartRenderingException : ChartEngineException
{
    public ChartRenderingException(string message) : base(message)
    {
        ErrorCode = 1002;
    }

    public ChartRenderingException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = 1002;
    }
}

/// <summary>
/// Thrown when export format is not supported
/// </summary>
public class UnsupportedExportFormatException : ChartEngineException
{
    public UnsupportedExportFormatException(string format) : base($"Export format '{format}' is not supported")
    {
        ErrorCode = 1003;
        Format = format;
    }

    public string Format { get; set; }
}

/// <summary>
/// Thrown when configuration is invalid
/// </summary>
public class InvalidConfigurationException : ChartEngineException
{
    public InvalidConfigurationException(string message) : base(message)
    {
        ErrorCode = 1004;
    }
}

/// <summary>
/// Thrown when a required resource is not found
/// </summary>
public class ResourceNotFoundException : ChartEngineException
{
    public ResourceNotFoundException(string resourceName, string identifier)
        : base($"Resource '{resourceName}' with identifier '{identifier}' was not found")
    {
        ErrorCode = 1005;
        ResourceName = resourceName;
        Identifier = identifier;
    }

    public string ResourceName { get; set; }
    public string Identifier { get; set; }
}
