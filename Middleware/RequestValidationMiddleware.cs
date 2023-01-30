// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Validation;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Middleware for validating incoming requests before processing.
/// Enforces content-type, payload size, and schema validation.
/// </summary>
public class RequestValidationMiddleware
{
    private readonly ILogger<RequestValidationMiddleware> _logger;
    private readonly InputValidator _validator;
    private readonly long _maxPayloadSize;
    private readonly HashSet<string> _allowedContentTypes;

    public RequestValidationMiddleware(ILogger<RequestValidationMiddleware> logger, long maxPayloadSize = 10_485_760)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = new InputValidator(logger);
        _maxPayloadSize = maxPayloadSize; // 10 MB default
        _allowedContentTypes = new HashSet<string>
        {
            "application/json",
            "application/x-www-form-urlencoded",
            "text/csv",
            "application/xml"
        };
    }

    // Validate request headers
    public bool ValidateHeaders(Dictionary<string, string> headers)
    {
        try
        {
            if (headers == null || headers.Count == 0)
            {
                _logger.LogWarning("Request has no headers");
                return false;
            }

            if (!headers.TryGetValue("Content-Type", out var contentType))
            {
                _logger.LogWarning("Missing Content-Type header");
                return false;
            }

            if (!_allowedContentTypes.Any(ct => contentType.Contains(ct)))
            {
                _logger.LogWarning("Unsupported Content-Type: {ContentType}", contentType);
                return false;
            }

            _logger.LogDebug("Headers validation passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating headers");
            return false;
        }
    }

    // Validate request payload size
    public bool ValidatePayloadSize(byte[] payload)
    {
        try
        {
            if (payload == null)
            {
                _logger.LogWarning("Payload is null");
                return false;
            }

            if (payload.Length > _maxPayloadSize)
            {
                _logger.LogWarning("Payload size {Size} exceeds maximum {Max}", payload.Length, _maxPayloadSize);
                return false;
            }

            _logger.LogDebug("Payload size validation passed: {Size} bytes", payload.Length);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payload size");
            return false;
        }
    }

    // Validate JSON payload schema
    public bool ValidateJsonSchema(string jsonPayload, params string[] requiredFields)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonPayload))
            {
                _logger.LogWarning("JSON payload is empty");
                return false;
            }

            // Basic JSON structure validation (would use JSON Schema in production)
            if (!jsonPayload.Trim().StartsWith("{"))
            {
                _logger.LogWarning("Invalid JSON format");
                return false;
            }

            // Validate required fields are present
            foreach (var field in requiredFields)
            {
                if (!jsonPayload.Contains($"\"{field}\""))
                {
                    _logger.LogWarning("Missing required field: {Field}", field);
                    return false;
                }
            }

            _logger.LogDebug("JSON schema validation passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JSON schema");
            return false;
        }
    }

    // Validate query parameters
    public Dictionary<string, string> ValidateQueryParameters(Dictionary<string, string> queryParams)
    {
        try
        {
            if (queryParams == null)
                return new Dictionary<string, string>();

            var validated = new Dictionary<string, string>();

            foreach (var kvp in queryParams)
            {
                // Reject parameters with suspicious characters
                if (_containsMaliciousPatterns(kvp.Value))
                {
                    _logger.LogWarning("Suspicious parameter value rejected: {Key}", kvp.Key);
                    continue;
                }

                validated[kvp.Key] = kvp.Value;
            }

            _logger.LogDebug("Query parameter validation passed. Validated: {Count}/{Total}", validated.Count, queryParams.Count);
            return validated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating query parameters");
            return new Dictionary<string, string>();
        }
    }

    // Add allowed content type
    public void AddAllowedContentType(string contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            _allowedContentTypes.Add(contentType);
            _logger.LogInformation("Added allowed content type: {ContentType}", contentType);
        }
    }

    private bool _containsMaliciousPatterns(string value)
    {
        // Check for common injection patterns
        var suspiciousPatterns = new[] { "<script", "javascript:", "onclick", "onerror", "DROP TABLE", "DELETE FROM" };
        return suspiciousPatterns.Any(pattern => value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
