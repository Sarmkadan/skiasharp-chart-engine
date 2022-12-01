// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Fluent API for building and applying validation rules to objects.
/// Supports custom validation predicates and error message composition.
/// </summary>
public class ValidationRuleBuilder<T>
{
    private readonly List<ValidationRule<T>> _rules;
    private readonly ILogger<ValidationRuleBuilder<T>> _logger;

    public ValidationRuleBuilder(ILogger<ValidationRuleBuilder<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rules = new List<ValidationRule<T>>();
    }

    // Add a validation rule
    public ValidationRuleBuilder<T> AddRule(
        Func<T, bool> predicate,
        string errorMessage,
        string ruleName = null)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        _rules.Add(new ValidationRule<T>
        {
            Predicate = predicate,
            ErrorMessage = errorMessage,
            RuleName = ruleName ?? Guid.NewGuid().ToString()
        });

        _logger.LogDebug("Validation rule added: {RuleName}", ruleName);
        return this;
    }

    // Add required field rule
    public ValidationRuleBuilder<T> IsRequired(
        Func<T, object> accessor,
        string fieldName)
    {
        AddRule(
            obj => accessor(obj) != null,
            $"{fieldName} is required",
            $"Required_{fieldName}");

        return this;
    }

    // Add range validation rule
    public ValidationRuleBuilder<T> IsInRange(
        Func<T, int> accessor,
        int min,
        int max,
        string fieldName)
    {
        AddRule(
            obj => accessor(obj) >= min && accessor(obj) <= max,
            $"{fieldName} must be between {min} and {max}",
            $"Range_{fieldName}");

        return this;
    }

    // Add length validation rule
    public ValidationRuleBuilder<T> HasLength(
        Func<T, string> accessor,
        int minLength,
        int maxLength,
        string fieldName)
    {
        AddRule(
            obj =>
            {
                var value = accessor(obj);
                return value != null && value.Length >= minLength && value.Length <= maxLength;
            },
            $"{fieldName} must be between {minLength} and {maxLength} characters",
            $"Length_{fieldName}");

        return this;
    }

    // Add custom predicate rule
    public ValidationRuleBuilder<T> When(
        Func<T, bool> predicate,
        string errorMessage,
        string ruleName = null)
    {
        return AddRule(predicate, errorMessage, ruleName);
    }

    // Validate object against all rules
    public ValidationResult Validate(T obj)
    {
        try
        {
            var result = new ValidationResult();

            foreach (var rule in _rules)
            {
                try
                {
                    if (!rule.Predicate(obj))
                    {
                        result.Errors.Add(rule.ErrorMessage);
                        _logger.LogDebug("Validation failed for rule: {RuleName}", rule.RuleName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing validation rule: {RuleName}", rule.RuleName);
                    result.Errors.Add($"Validation error in {rule.RuleName}: {ex.Message}");
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during validation");
            return new ValidationResult { IsValid = false, Errors = new List<string> { ex.Message } };
        }
    }

    // Get rule count
    public int GetRuleCount() => _rules.Count;

    // Clear all rules
    public void ClearRules()
    {
        _rules.Clear();
        _logger.LogDebug("All validation rules cleared");
    }
}

/// <summary>
/// Represents a single validation rule.
/// </summary>
public class ValidationRule<T>
{
    public Func<T, bool> Predicate { get; set; }
    public string ErrorMessage { get; set; }
    public string RuleName { get; set; }
}

/// <summary>
/// Result of validation containing errors and status.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new List<string>();

    public override string ToString()
    {
        return IsValid ? "Valid" : $"Invalid: {string.Join("; ", Errors)}";
    }
}
