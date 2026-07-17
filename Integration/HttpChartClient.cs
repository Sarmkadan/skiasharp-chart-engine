// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Serializers;

namespace SkiaSharpChartEngine.Integration;

/// <summary>
/// HTTP client for communicating with remote chart services
/// Provides methods for fetching and posting chart data
/// </summary>
public class HttpChartClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpChartClient> _logger;
    private readonly IChartSerializer _serializer;
    private readonly string _baseUrl;

    public HttpChartClient(string baseUrl, ILogger<HttpChartClient> logger, IChartSerializer serializer)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentException("Base URL cannot be empty", nameof(baseUrl));

        _baseUrl = baseUrl.TrimEnd('/');
        BaseUrl = baseUrl.TrimEnd('/');
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Fetches a chart by ID from remote service
    /// </summary>
    public async Task<Chart?> GetChartAsync(string chartId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(chartId))
                throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

            var url = $"{_baseUrl}/api/charts/{chartId}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch chart {ChartId}: {StatusCode}", chartId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var chart = _serializer.Deserialize(content);

            _logger.LogInformation("Chart {ChartId} fetched successfully from {Url}", chartId, url);
            return chart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching chart {ChartId}", chartId);
            throw;
        }
    }

    /// <summary>
    /// Sends a chart to a remote service
    /// </summary>
    public async Task<bool> PostChartAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            var url = $"{_baseUrl}/api/charts";
            var json = _serializer.Serialize(chart);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to post chart {ChartId}: {StatusCode}", chart.Id, response.StatusCode);
                return false;
            }

            _logger.LogInformation("Chart {ChartId} posted successfully to {Url}", chart.Id, url);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting chart {ChartId}", chart.Id);
            throw;
        }
    }

    /// <summary>
    /// Downloads rendered chart data from remote service
    /// </summary>
    public async Task<byte[]?> GetRenderedChartAsync(
        string chartId,
        string format = "png",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(chartId))
                throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

            var url = $"{_baseUrl}/api/export/render?chartId={chartId}&format={format}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download rendered chart {ChartId}: {StatusCode}",
                    chartId, response.StatusCode);
                return null;
            }

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            _logger.LogInformation("Rendered chart {ChartId} downloaded: {Size} bytes", chartId, data.Length);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading rendered chart {ChartId}", chartId);
            throw;
        }
    }

    /// <summary>
    /// Sets HTTP headers for authentication or custom requirements
    /// </summary>
    public void SetDefaultHeaders(string key, string value)
    {
        _httpClient.DefaultRequestHeaders.Add(key, value);
        _logger.LogDebug("Default header set: {Key}", key);
    }

    /// <summary>
    /// Sets authentication token
    /// </summary>
    public void SetAuthenticationToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _logger.LogDebug("Authentication token set");
    }

    /// <summary>
    /// Gets the base URL of the HTTP chart client.
    /// </summary>
    public string BaseUrl { get; }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
