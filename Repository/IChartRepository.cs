// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Repository;

/// <summary>
/// Defines the contract for persistence operations on <see cref="Chart"/> objects,
/// supporting both asynchronous and synchronous retrieval, storage, updates, and deletion.
/// </summary>
public interface IChartRepository
{
    Task<Chart?> GetByIdAsync(string chartId, CancellationToken cancellationToken = default);

    Chart? GetById(string chartId);

    Task<List<Chart>> GetAllAsync(CancellationToken cancellationToken = default);

    List<Chart> GetAll();

    Task<List<Chart>> GetByTypeAsync(ChartType chartType, CancellationToken cancellationToken = default);

    List<Chart> GetByType(ChartType chartType);

    Task<string> SaveAsync(Chart chart, CancellationToken cancellationToken = default);

    string Save(Chart chart);

    Task<bool> UpdateAsync(Chart chart, CancellationToken cancellationToken = default);

    bool Update(Chart chart);

    Task<bool> DeleteAsync(string chartId, CancellationToken cancellationToken = default);

    bool Delete(string chartId);

    Task<bool> ExistsAsync(string chartId, CancellationToken cancellationToken = default);

    bool Exists(string chartId);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    int GetCount();

    Task<List<Chart>> SearchAsync(Func<Chart, bool> predicate, CancellationToken cancellationToken = default);

    List<Chart> Search(Func<Chart, bool> predicate);
}
