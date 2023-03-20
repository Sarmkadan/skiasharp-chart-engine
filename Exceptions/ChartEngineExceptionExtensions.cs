// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SkiaSharpChartEngine.Exceptions;

/// <summary>
/// Extension methods for <see cref="ChartEngineException"/> and its derived types.
/// Provides utility methods for exception handling, analysis, and transformation.
/// </summary>
public static class ChartEngineExceptionExtensions
{
    /// <summary>
    /// Determines if the exception is a chart data validation error.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is an <see cref="InvalidChartDataException"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static bool IsInvalidChartData(this ChartEngineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidChartDataException;
    }

    /// <summary>
    /// Determines if the exception is a rendering error.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is a <see cref="ChartRenderingException"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static bool IsRenderingError(this ChartEngineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is ChartRenderingException;
    }

    /// <summary>
    /// Determines if the exception is an unsupported export format error.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is an <see cref="UnsupportedExportFormatException"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static bool IsUnsupportedExportFormat(this ChartEngineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is UnsupportedExportFormatException;
    }

    /// <summary>
    /// Determines if the exception is a configuration error.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is an <see cref="InvalidConfigurationException"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static bool IsConfigurationError(this ChartEngineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidConfigurationException;
    }

    /// <summary>
    /// Determines if the exception is a resource not found error.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is a <see cref="ResourceNotFoundException"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static bool IsResourceNotFound(this ChartEngineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is ResourceNotFoundException;
    }

    /// <summary>
    /// Gets the error code from the exception. Returns -1 if the exception is null or doesn't have an ErrorCode property.
    /// </summary>
    /// <param name="exception">The exception to get the error code from.</param>
    /// <returns>The error code, or -1 if not available.</returns>
    public static int GetErrorCode(this ChartEngineException exception)
    {
        return exception?.ErrorCode ?? -1;
    }

    /// <summary>
    /// Creates a new exception of the same type with an updated message.
    /// </summary>
    /// <param name="exception">The original exception.</param>
    /// <param name="newMessage">The new message to use.</param>
    /// <returns>A new exception instance with the updated message.</returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static ChartEngineException WithMessage(this ChartEngineException exception, string newMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(newMessage);

        return exception switch
        {
            InvalidChartDataException invalidChartData => new InvalidChartDataException(newMessage, exception),
            ChartRenderingException rendering => new ChartRenderingException(newMessage, exception),
            UnsupportedExportFormatException unsupported => new UnsupportedExportFormatException(unsupported.Format) { ErrorCode = unsupported.ErrorCode },
            InvalidConfigurationException config => new InvalidConfigurationException(newMessage) { ErrorCode = config.ErrorCode },
            ResourceNotFoundException resourceNotFound => new ResourceNotFoundException(resourceNotFound.ResourceName, resourceNotFound.Identifier) { ErrorCode = resourceNotFound.ErrorCode },
            _ => new ChartEngineException(newMessage, exception) { ErrorCode = exception.ErrorCode }
        };
    }

    /// <summary>
    /// Gets all exception messages in the exception hierarchy as a read-only list.
    /// </summary>
    /// <param name="exception">The exception to analyze.</param>
    /// <returns>A read-only list containing all exception messages from the hierarchy.</returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static IReadOnlyList<string> GetAllMessages(this ChartEngineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var messages = new List<string>();
        var current = exception;

        while (current != null)
        {
            messages.Add(current.Message);
            current = current.InnerException as ChartEngineException;
        }

        return messages.AsReadOnly();
    }

    /// <summary>
    /// Gets the root cause exception of the chart engine exception hierarchy.
    /// </summary>
    /// <param name="exception">The exception to analyze.</param>
    /// <returns>The root <see cref="ChartEngineException"/> in the chain, or the original exception if it's not a ChartEngineException.</returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static Exception GetRootCause(this ChartEngineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var current = exception;
        while (current.InnerException is ChartEngineException innerChartException)
        {
            current = innerChartException;
        }

        return current.InnerException ?? current;
    }

    /// <summary>
    /// Determines if this exception or any inner exception has the specified error code.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <param name="errorCode">The error code to search for.</param>
    /// <returns>True if any exception in the hierarchy has the specified error code.</returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static bool HasErrorCode(this ChartEngineException exception, int errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var current = exception;
        while (current != null)
        {
            if (current.ErrorCode == errorCode)
            {
                return true;
            }

            current = current.InnerException as ChartEngineException;
        }

        return false;
    }

    /// <summary>
    /// Gets the formatted error details including error code, type, and message.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>A formatted string with error details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null.</exception>
    public static string FormatErrorDetails(this ChartEngineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var typeName = exception.GetType().Name;
        var errorCode = exception.GetErrorCode();
        var messages = exception.GetAllMessages();

        var message = string.Join(" | ", messages);

        return string.Format(
            CultureInfo.InvariantCulture,
            "[{0}:{1}] {2}",
            typeName,
            errorCode.ToString(CultureInfo.InvariantCulture),
            message);
    }
}