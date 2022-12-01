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
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Repository;

/// <summary>
/// In-memory chart repository implementation
/// </summary>
public class ChartRepository : IChartRepository
{
    private readonly Dictionary<string, Chart> _charts = new();
    private readonly ILogger<ChartRepository> _logger;
    private readonly object _lock = new();

    public ChartRepository(ILogger<ChartRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Chart?> GetByIdAsync(string chartId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => GetById(chartId), cancellationToken);
    }

    public Chart? GetById(string chartId)
    {
        if (string.IsNullOrWhiteSpace(chartId))
            return null;

        lock (_lock)
        {
            if (_charts.TryGetValue(chartId, out var chart))
            {
                _logger.LogDebug($"Retrieved chart: {chartId}");
                return chart.Clone();
            }

            _logger.LogWarning($"Chart not found: {chartId}");
            return null;
        }
    }

    public async Task<List<Chart>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => GetAll(), cancellationToken);
    }

    public List<Chart> GetAll()
    {
        lock (_lock)
        {
            var charts = _charts.Values.Select(c => c.Clone()).ToList();
            _logger.LogInformation($"Retrieved {charts.Count} charts");
            return charts;
        }
    }

    public async Task<List<Chart>> GetByTypeAsync(ChartType chartType, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => GetByType(chartType), cancellationToken);
    }

    public List<Chart> GetByType(ChartType chartType)
    {
        lock (_lock)
        {
            var charts = _charts.Values
                .Where(c => c.Type == chartType)
                .Select(c => c.Clone())
                .ToList();

            _logger.LogInformation($"Retrieved {charts.Count} charts of type {chartType}");
            return charts;
        }
    }

    public async Task<string> SaveAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Save(chart), cancellationToken);
    }

    public string Save(Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        lock (_lock)
        {
            chart.CreatedAt = DateTime.UtcNow;
            chart.ModifiedAt = DateTime.UtcNow;

            _charts[chart.Id] = chart.Clone();

            _logger.LogInformation($"Saved chart: {chart.Id}");
            return chart.Id;
        }
    }

    public async Task<bool> UpdateAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Update(chart), cancellationToken);
    }

    public bool Update(Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        lock (_lock)
        {
            if (!_charts.ContainsKey(chart.Id))
            {
                _logger.LogWarning($"Chart not found for update: {chart.Id}");
                return false;
            }

            chart.ModifiedAt = DateTime.UtcNow;
            _charts[chart.Id] = chart.Clone();

            _logger.LogInformation($"Updated chart: {chart.Id}");
            return true;
        }
    }

    public async Task<bool> DeleteAsync(string chartId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Delete(chartId), cancellationToken);
    }

    public bool Delete(string chartId)
    {
        if (string.IsNullOrWhiteSpace(chartId))
            return false;

        lock (_lock)
        {
            if (_charts.Remove(chartId))
            {
                _logger.LogInformation($"Deleted chart: {chartId}");
                return true;
            }

            _logger.LogWarning($"Chart not found for deletion: {chartId}");
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string chartId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Exists(chartId), cancellationToken);
    }

    public bool Exists(string chartId)
    {
        if (string.IsNullOrWhiteSpace(chartId))
            return false;

        lock (_lock)
        {
            return _charts.ContainsKey(chartId);
        }
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => GetCount(), cancellationToken);
    }

    public int GetCount()
    {
        lock (_lock)
        {
            var count = _charts.Count;
            _logger.LogDebug($"Total charts in repository: {count}");
            return count;
        }
    }

    public async Task<List<Chart>> SearchAsync(Func<Chart, bool> predicate, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Search(predicate), cancellationToken);
    }

    public List<Chart> Search(Func<Chart, bool> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        lock (_lock)
        {
            var results = _charts.Values
                .Where(predicate)
                .Select(c => c.Clone())
                .ToList();

            _logger.LogInformation($"Search found {results.Count} charts");
            return results;
        }
    }
}
