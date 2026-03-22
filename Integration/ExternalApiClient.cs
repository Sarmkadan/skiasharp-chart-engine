// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Integration;

/// <summary>
/// HTTP client for integrating with external APIs.
/// Handles request/response serialization, retries, and error handling.
/// </summary>
public class ExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiClient> _logger;
    private readonly int _maxRetries;
    private readonly int _retryDelayMs;

    public ExternalApiClient(HttpClient httpClient, ILogger<ExternalApiClient> logger, int maxRetries = 3)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxRetries = maxRetries;
        _retryDelayMs = 1000;
    }

    // Send GET request with retry logic
    public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null, empty, or whitespace", nameof(url));

            if (url.Length > 2048)
                throw new ArgumentException("URL exceeds maximum length of 2048 characters", nameof(url));

            return await _executeWithRetryAsync(async () =>
            {
                _logger.LogDebug("GET request to {Url}", url);
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(content);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Url}", url);
            throw;
        }
    }

    // Send POST request with retry logic
    public async Task<T> PostAsync<T>(string url, object data, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null, empty, or whitespace", nameof(url));

            if (url.Length > 2048)
                throw new ArgumentException("URL exceeds maximum length of 2048 characters", nameof(url));

            return await _executeWithRetryAsync(async () =>
            {
                _logger.LogDebug("POST request to {Url}", url);
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(responseContent);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Url}", url);
            throw;
        }
    }

    // Send PUT request
    public async Task<T> PutAsync<T>(string url, object data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("PUT request to {Url}", url);
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT request to {Url}", url);
            throw;
        }
    }

    // Send DELETE request
    public async Task<bool> DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("DELETE request to {Url}", url);
            var response = await _httpClient.DeleteAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE request to {Url}", url);
            throw;
        }
    }

    // Set authorization header
    public void SetAuthorizationHeader(string scheme, string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
        _logger.LogDebug("Authorization header set with scheme {Scheme}", scheme);
    }

    // Add custom header
    public void AddHeader(string name, string value)
    {
        _httpClient.DefaultRequestHeaders.Add(name, value);
        _logger.LogDebug("Header added: {Name}", name);
    }

    // Execute operation with exponential backoff retry
    private async Task<T> _executeWithRetryAsync<T>(Func<Task<T>> operation)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < _maxRetries)
            {
                var delayMs = _retryDelayMs * (int)Math.Pow(2, attempt - 1);
                _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying in {DelayMs}ms", attempt, delayMs);
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException($"Operation failed after {_maxRetries} attempts");
    }
}
