// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using SkiaSharpChartEngine.Utilities;

namespace SkiaSharpChartEngine.Benchmarks;

/// <summary>
/// Benchmarks for StringFormatHelper hot paths.
/// Covers camelCase/snake_case conversion (string.Create) and CSV generation (pooled StringBuilder).
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class StringFormatBenchmarks
{
    private static readonly object[] CsvValues =
        { "Monthly Revenue Report", 1_234_567.89, "North America", true, null! };

    private const string CamelCaseInput = "salesPerformanceMonthlyDashboard";
    private const string SnakeCaseInput = "sales_performance_monthly_dashboard";

    [Benchmark(Description = "CamelCaseToTitleCase — string.Create (no StringBuilder)")]
    public string CamelCaseToTitleCase()
        => StringFormatHelper.CamelCaseToTitleCase(CamelCaseInput);

    [Benchmark(Description = "SnakeCaseToTitleCase — string.Create (no Split+LINQ)")]
    public string SnakeCaseToTitleCase()
        => StringFormatHelper.SnakeCaseToTitleCase(SnakeCaseInput);

    [Benchmark(Description = "ToCsvLine — pooled StringBuilder (5 values)")]
    public string ToCsvLine()
        => StringFormatHelper.ToCsvLine(CsvValues);

    [Benchmark(Description = "Repeat — pooled StringBuilder (20×)")]
    public string Repeat_Twenty()
        => StringFormatHelper.Repeat("-", 20);

    [Benchmark(Description = "FormatNumberWithUnits — billions")]
    public string FormatNumberWithUnits_Billions()
        => StringFormatHelper.FormatNumberWithUnits(1_234_567_890);

    [Benchmark(Description = "FormatNumberWithUnits — thousands")]
    public string FormatNumberWithUnits_Thousands()
        => StringFormatHelper.FormatNumberWithUnits(4_567.5);

    [Benchmark(Description = "FormatPercentage — 2 decimal places")]
    public string FormatPercentage()
        => StringFormatHelper.FormatPercentage(73.456, 2);
}
