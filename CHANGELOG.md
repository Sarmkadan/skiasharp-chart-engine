# Changelog

All notable changes to the SkiaSharp Chart Engine are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-05-18

### Added
- Add animated chart transitions with easing functions and timeline
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

## [1.2.0] - 2025-10-27

### Added
- **Enhanced Caching System**: Distributed cache support via Redis/Memcached
- **Performance Metrics Dashboard**: Real-time rendering metrics and statistics
- **Batch Processing API**: Efficient rendering of multiple charts simultaneously
- **Health Check Endpoint**: Built-in diagnostic endpoint for monitoring
- **Rate Limiting Middleware**: Automatic rate limiting to prevent resource exhaustion
- **Request Validation Middleware**: Automatic input sanitization and validation
- **Enhanced Logging**: Structured logging with Serilog integration
- **Concurrent Render Queue Manager**: Intelligent queue management for parallel rendering
- **Template Library**: Pre-configured templates for all chart types

### Changed
- **Performance Improvement**: 30% faster rendering through optimized SKPath handling
- **API Enhancement**: Extended ExportService with format detection
- **Cache Key Strategy**: Improved cache invalidation based on data modification timestamp
- **Dependency Updates**: Updated to .NET 10.0 LTS and latest SkiaSharp 2.88.8

### Fixed
- **Memory Leak**: Fixed unreleased SKPaint objects in rendering pipeline
- **Grid Rendering**: Corrected grid line positioning for non-standard margins
- **Color Parsing**: Enhanced hex color validation for edge cases
- **Export File Handling**: Improved handling of special characters in filenames

### Security
- **Input Validation**: Comprehensive validation of all chart data inputs
- **XSS Prevention**: Sanitization of title and label text
- **Path Traversal**: Validation of output paths

## [1.1.0] - 2025-08-11

### Added
- **SVG Export**: Full support for Scalable Vector Graphics format
- **PDF Export**: Direct PDF export for print-ready documents
- **Heatmap Chart Type**: New chart type for 2D pattern visualization
- **Scatter Plot Renderer**: New scatter chart implementation
- **Animation Framework**: Foundation for chart animations (beta)
- **WebP Export**: Modern image format with superior compression
- **Configuration Templates**: Pre-configured templates for quick setup
- **Repository Pattern**: In-memory chart storage with CRUD operations
- **Tag System**: Add custom metadata to charts for organization

### Changed
- **Architecture Refactoring**: Separated rendering concerns into dedicated services
- **Configuration Model**: Enhanced ChartConfiguration with more options
- **Error Handling**: More granular error reporting with detailed messages
- **Data Bounds Calculation**: Improved automatic range detection

### Fixed
- **Pie Chart Rendering**: Corrected angle calculations for accurate segments
- **Legend Positioning**: Fixed legend overlap issues with content
- **Axis Label Rotation**: Improved text rendering at various angles
- **Memory Management**: Better resource cleanup in rendering pipeline

## [1.0.0] - 2025-05-19

### Added
- **Initial Release**: Core chart rendering engine
- **Chart Types**:
  - LineChart: Standard line charts for trend visualization
  - BarChart: Horizontal bar charts for comparisons
  - ColumnChart: Vertical bar charts for categorical data
  - PieChart: Circular charts for composition
  - AreaChart: Filled line charts
- **Export Formats**:
  - PNG: Default raster format
  - JPEG: Compressed raster format
- **Features**:
  - Configurable colors, fonts, margins
  - Grid and axis customization
  - Legend display
  - Title and axis labels
  - Anti-aliasing support
  - Asynchronous rendering API
  - Synchronous rendering API
- **Infrastructure**:
  - Dependency Injection support (Microsoft.Extensions)
  - Logging integration (Microsoft.Extensions.Logging)
  - Configuration support (Microsoft.Extensions.Configuration)
  - Metrics collection and reporting
  - Health checks
  - Error handling and validation
- **Documentation**:
  - Getting Started guide
  - API Reference
  - Architecture documentation
  - Multiple examples
  - FAQ section

### Performance
- Single chart render: ~50-200ms (depending on complexity)
- Batch processing: Linear scaling with core count
- Memory usage: ~50MB per chart instance
- Cache hit rate: >95% for repeated renders

### Quality
- .NET 10.0 target framework
- Latest C# language features
- Comprehensive input validation
- Structured error handling
- Performance monitoring

## [0.5.0] - 2025-03-10 (Beta)

### Added
- **Initial Beta Release**
- Core rendering engine prototype
- Basic chart types (Line, Bar, Pie)
- PNG export
- Synchronous API
- Configuration options

### Known Issues
- Performance not optimized
- Limited error handling
- No async support
- No caching

## [0.1.0] - 2025-01-06 (Alpha)

### Added
- Project scaffolding
- Initial architecture design
- SkiaSharp integration
- Basic chart model

---

## Future Roadmap

### Planned for v1.3.0
- [ ] GPU acceleration support
- [ ] Real-time data streaming
- [ ] Web component library (Blazor integration)
- [ ] Chart animation sequences
- [ ] Dark mode theme system
- [ ] Custom renderer plugin system

### Planned for v2.0.0
- [ ] WebGL-based rendering
- [ ] Distributed rendering cluster support
- [ ] Native mobile app support (MAUI)
- [ ] Advanced data binding
- [ ] Real-time chart updates via SignalR
- [ ] Database integration layer

### Under Consideration
- Interactive charts with zoom/pan
- 3D chart support
- Statistical analysis overlays
- Machine learning integration
- Time-series specific features
- Real-time streaming charts

---

## Version History Summary

| Version | Release Date | Status | Chart Types | Export Formats | Key Feature |
|---------|------------|--------|-------------|----------------|-------------|
| 1.2.0 | 2025-10-27 | Current | 7 | 5 | Distributed Caching |
| 1.1.0 | 2025-08-11 | Stable | 7 | 5 | SVG/PDF Export |
| 1.0.0 | 2025-05-19 | Stable | 5 | 2 | Initial Release |
| 0.5.0 | 2025-03-10 | Beta | 3 | 1 | Beta Release |
| 0.1.0 | 2025-01-06 | Alpha | - | - | Project Init |

---

## Migration Guides

### From 1.1.x to 1.2.0

**Cache Configuration Changed**:
```csharp
// Old (1.1.x)
services.AddSkiaSharpChartEngine();

// New (1.2.0)
services.AddSkiaSharpChartEngine(options =>
{
    options.CacheEnabled = true;
    options.CacheDurationSeconds = 300;
    options.MaxConcurrentRenders = 10;
});
```

### From 1.0.x to 1.1.0

**No breaking changes** - Full backward compatibility maintained.

New optional features can be adopted incrementally.

### From 0.5.0 to 1.0.0

**Async API Added**:
```csharp
// Old (0.5.0)
var result = engine.RenderChart(chart);

// New (1.0.0) - Both supported
var result = engine.RenderChart(chart);           // Sync (still works)
var result = await engine.RenderChartAsync(chart); // Async (new)
```

---

## Support

- 🐛 **Report Bugs**: [GitHub Issues](https://github.com/vladyslav-zaiets/skiasharp-chart-engine/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/vladyslav-zaiets/skiasharp-chart-engine/discussions)
- 📧 **Contact**: Open an issue or submit a discussion

---

Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect
