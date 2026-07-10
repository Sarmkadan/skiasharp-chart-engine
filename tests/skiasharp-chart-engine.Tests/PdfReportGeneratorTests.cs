// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Reports;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Reports;

/// <summary>
/// Unit tests for the <see cref="PdfReportGenerator"/> class that verify PDF report generation functionality.
/// </summary>
public class PdfReportGeneratorTests
{
	private readonly Mock<IChartRenderingService> _renderMock;
	private readonly Mock<ILogger<PdfReportGenerator>> _loggerMock;
	private readonly PdfReportGenerator _generator;

	/// <summary>
	/// Initializes a new instance of the <see cref="PdfReportGeneratorTests"/> class.
	/// Sets up mocks for chart rendering service and logger for testing PDF report generation.
	/// </summary>
	public PdfReportGeneratorTests()
	{
		_renderMock = new Mock<IChartRenderingService>();
		_loggerMock = new Mock<ILogger<PdfReportGenerator>>();
		_generator = new PdfReportGenerator(_renderMock.Object, _loggerMock.Object);
	}

	/// <summary>
	/// Creates a test chart with default configuration for use in test scenarios.
	/// </summary>
	/// <param name="id">The identifier for the chart. Defaults to "pdf-chart".</param>
	/// <returns>A configured <see cref="Chart"/> instance with sample data.</returns>
	private static Chart CreateChart(string id = "pdf-chart")
	{
		var chart = new Chart(id);
		var series = new ChartSeries("Data");
		series.AddDataPoint(1.0, 10.0);
		series.AddDataPoint(2.0, 20.0);
		chart.AddSeries(series);
		return chart;
	}

	/// <summary>
	/// Configures the mock chart rendering service to return a successful render result.
	/// </summary>
	private void SetupRenderSuccess()
	{
		_renderMock
			.Setup(r => r.RenderWithExportAsync(
				It.IsAny<Chart>(),
				It.IsAny<ExportOptions>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((Chart c, ExportOptions _, CancellationToken _) =>
				RenderResult.CreateSuccess(c.Id, new byte[] { 0x89, 0x50, 0x4E, 0x47 }, 10, ExportFormat.PNG));
	}

	/// <summary>
	/// Tests that <see cref="PdfReportGenerator.GenerateAsync"/> throws an <see cref="ArgumentNullException"/> when null sections are provided.
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithNullSections_ThrowsArgumentNullException()
	{
		// Act
		Func<Task> act = async () => await _generator.GenerateAsync(null!);

		// Assert
		await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("sections");
	}

	/// <summary>
	/// Tests that <see cref="PdfReportGenerator.GenerateAsync"/> returns valid PDF bytes when empty sections list is provided.
	/// </summary>
	[Fact]
	public async Task GenerateAsync_EmptySections_ReturnsPdfBytes()
	{
		// Act
		var bytes = await _generator.GenerateAsync(new List<ReportSection>());

		// Assert
		bytes.Should().NotBeNull();
		bytes.Length.Should().BeGreaterThan(0);
	}

	/// <summary>
	/// Tests that <see cref="PdfReportGenerator.GenerateAsync"/> returns valid PDF bytes when only text sections are provided.
	/// </summary>
	[Fact]
	public async Task GenerateAsync_TextOnlySection_ReturnsPdfBytes()
	{
		// Arrange
		var sections = new List<ReportSection>
		{
			new ReportSection
			{
				Heading = "Summary",
				BodyText = "This report contains chart data analysis."
			}
		};

		// Act
		var bytes = await _generator.GenerateAsync(sections);

		// Assert
		bytes.Should().NotBeNull();
		bytes.Length.Should().BeGreaterThan(0);
	}

	/// <summary>
	/// Tests that <see cref="PdfReportGenerator.GenerateAsync"/> calls the rendering service when a chart section is provided.
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithChart_CallsRenderingService()
	{
		// Arrange
		SetupRenderSuccess();
		var sections = new List<ReportSection>
		{
			new ReportSection
			{
				Heading = "Chart Section",
				Chart = CreateChart()
			}
		};

		// Act
		var bytes = await _generator.GenerateAsync(sections);

		// Assert
		bytes.Should().NotBeNull();
		bytes.Length.Should().BeGreaterThan(0);
		_renderMock.Verify(r => r.RenderWithExportAsync(
			It.IsAny<Chart>(),
			It.IsAny<ExportOptions>(),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	/// <summary>
	/// Tests that <see cref="PdfReportGenerator.GenerateAsync"/> produces a single PDF when multiple sections are provided.
	/// </summary>
	[Fact]
	public async Task GenerateAsync_MultipleSections_ProducesSinglePdf()
	{
		// Arrange
		SetupRenderSuccess();
		var sections = new List<ReportSection>
		{
			new ReportSection { Heading = "Introduction", BodyText = "Report intro text." },
			new ReportSection { Heading = "Chart 1", Chart = CreateChart("c1") },
			new ReportSection { Heading = "Chart 2", Chart = CreateChart("c2"), PageBreakBefore = true }
		};

		// Act
		var bytes = await _generator.GenerateAsync(sections);

		// Assert
		bytes.Should().NotBeNull();
		bytes.Length.Should().BeGreaterThan(0);
		_renderMock.Verify(r => r.RenderWithExportAsync(
			It.IsAny<Chart>(),
			It.IsAny<ExportOptions>(),
			It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	/// <summary>
	/// Tests that <see cref="PdfReportGenerator.GenerateAsync"/> still produces a PDF even when chart rendering fails.
	/// </summary>
	[Fact]
	public async Task GenerateAsync_RenderFailure_StillProducesPdf()
	{
		// Arrange – render returns failure
		_renderMock
			.Setup(r => r.RenderWithExportAsync(
				It.IsAny<Chart>(),
				It.IsAny<ExportOptions>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((Chart c, ExportOptions _, CancellationToken _) =>
				RenderResult.CreateFailure(c.Id, "render error"));

		var sections = new List<ReportSection>
		{
			new ReportSection { Heading = "Bad Chart", Chart = CreateChart() }
		};

		// Act
		var bytes = await _generator.GenerateAsync(sections);

		// Assert – PDF is still generated even when chart render fails
		bytes.Should().NotBeNull();
		bytes.Length.Should().BeGreaterThan(0);
	}

	/// <summary>
	/// Tests that <see cref="PdfReportGenerator.GenerateToFileAsync"/> throws an <see cref="ArgumentNullException"/> when empty file path is provided.
	/// </summary>
	[Fact]
	public async Task GenerateToFileAsync_WithEmptyPath_ThrowsArgumentNullException()
	{
		// Act
		Func<Task> act = async () =>
			await _generator.GenerateToFileAsync("", new List<ReportSection>());

		// Assert
		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	/// <summary>
	/// Tests that <see cref="PdfReportGenerator.GenerateToFileAsync"/> successfully writes a PDF file to the specified path.
	/// </summary>
	[Fact]
	public async Task GenerateToFileAsync_ValidPath_WritesFile()
	{
		// Arrange
		var outputPath = Path.Combine(Path.GetTempPath(), $"test-report-{Guid.NewGuid():N}.pdf");
		try
		{
			// Act
			await _generator.GenerateToFileAsync(outputPath, new List<ReportSection>
			{
				new ReportSection { Heading = "Section", BodyText = "Body." }
			});

			// Assert
			File.Exists(outputPath).Should().BeTrue();
			new FileInfo(outputPath).Length.Should().BeGreaterThan(0);
		}
		finally
		{
			if (File.Exists(outputPath))
				File.Delete(outputPath);
		}
	}
}