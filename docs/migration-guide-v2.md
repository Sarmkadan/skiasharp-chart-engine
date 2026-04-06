## Migration Guide from v1.x to v2.0

### Breaking Changes

* Removed deprecated API endpoints
* Changed default cache duration from 30 minutes to 1 hour

### New Features

* Added support for animated chart transitions
* Improved performance with concurrent rendering
* Enhanced security with input validation and sanitization

### Step-by-Step Migration

1. Update NuGet package to v2.0
2. Review and update API endpoint usage
3. Configure new features and settings as needed

### Code Examples

* Old API endpoint: `ChartEngine.RenderChart(chart)`
* New API endpoint: `ChartEngine.RenderChartAsync(chart)`

* Old cache configuration: `CacheDurationMinutes = 30`
* New cache configuration: `CacheDurationSeconds = 3600`

### Troubleshooting

* Review error messages and logs for issues
* Check API endpoint and cache configuration updates