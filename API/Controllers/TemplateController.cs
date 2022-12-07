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
using SkiaSharpChartEngine.API.Responses;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine.API.Controllers;

/// <summary>
/// REST API controller for chart template operations.
/// Provides template browsing, creation, and management.
/// </summary>
public class TemplateController
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<TemplateController> _logger;
    private readonly Dictionary<string, ChartTemplate> _templates;

    public TemplateController(IConfigurationService configurationService, ILogger<TemplateController> logger)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _templates = new Dictionary<string, ChartTemplate>();
        _initializeDefaultTemplates();
    }

    // Get all available templates
    public ApiResponse<List<ChartTemplate>> GetAllTemplates()
    {
        try
        {
            _logger.LogInformation("Retrieving all templates. Count: {Count}", _templates.Count);
            return ApiResponse<List<ChartTemplate>>.Success(_templates.Values.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates");
            return ApiResponse<List<ChartTemplate>>.Failure($"Error: {ex.Message}");
        }
    }

    // Get template by ID
    public ApiResponse<ChartTemplate> GetTemplateById(string templateId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateId))
                return ApiResponse<ChartTemplate>.Failure("Template ID cannot be empty");

            if (!_templates.TryGetValue(templateId, out var template))
            {
                _logger.LogWarning("Template not found: {TemplateId}", templateId);
                return ApiResponse<ChartTemplate>.Failure($"Template '{templateId}' not found");
            }

            return ApiResponse<ChartTemplate>.Success(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template: {TemplateId}", templateId);
            return ApiResponse<ChartTemplate>.Failure($"Error: {ex.Message}");
        }
    }

    // Get templates by chart type
    public ApiResponse<List<ChartTemplate>> GetTemplatesByType(ChartType chartType)
    {
        try
        {
            var matching = _templates.Values
                .Where(t => t.ChartConfiguration?.ChartType == chartType)
                .ToList();

            _logger.LogInformation("Found {Count} templates for type {ChartType}", matching.Count, chartType);
            return ApiResponse<List<ChartTemplate>>.Success(matching);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates by type");
            return ApiResponse<List<ChartTemplate>>.Failure($"Error: {ex.Message}");
        }
    }

    // Create custom template from chart configuration
    public async Task<ApiResponse<ChartTemplate>> CreateTemplateAsync(
        string name,
        ChartConfiguration config,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return ApiResponse<ChartTemplate>.Failure("Template name cannot be empty");

            if (config == null)
                return ApiResponse<ChartTemplate>.Failure("Chart configuration cannot be null");

            await Task.Delay(10, cancellationToken);

            var templateId = Guid.NewGuid().ToString();
            var template = new ChartTemplate
            {
                Id = templateId,
                Name = name,
                ChartConfiguration = config,
                CreatedAt = DateTime.UtcNow
            };

            _templates[templateId] = template;
            _logger.LogInformation("Template created: {TemplateId}", templateId);

            return ApiResponse<ChartTemplate>.Success(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return ApiResponse<ChartTemplate>.Failure($"Error: {ex.Message}");
        }
    }

    // Delete template
    public async Task<ApiResponse<bool>> DeleteTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateId))
                return ApiResponse<bool>.Failure("Template ID cannot be empty");

            await Task.Delay(5, cancellationToken);

            if (!_templates.Remove(templateId))
            {
                _logger.LogWarning("Template not found for deletion: {TemplateId}", templateId);
                return ApiResponse<bool>.Failure("Template not found");
            }

            _logger.LogInformation("Template deleted: {TemplateId}", templateId);
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template");
            return ApiResponse<bool>.Failure($"Error: {ex.Message}");
        }
    }

    private void _initializeDefaultTemplates()
    {
        // Initialize with basic templates
        var lineConfig = _configurationService.CreateConfigurationFromTemplate(ChartType.Line);
        _templates["default_line"] = new ChartTemplate
        {
            Id = "default_line",
            Name = "Default Line Chart",
            ChartConfiguration = lineConfig,
            CreatedAt = DateTime.UtcNow
        };

        var barConfig = _configurationService.CreateConfigurationFromTemplate(ChartType.Bar);
        _templates["default_bar"] = new ChartTemplate
        {
            Id = "default_bar",
            Name = "Default Bar Chart",
            ChartConfiguration = barConfig,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Default templates initialized");
    }
}
