// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Animation configuration for chart rendering
/// </summary>
public class AnimationSettings
{
    public bool Enabled { get; set; } = true;

    public int DurationMs { get; set; } = 500;

    public int FrameRate { get; set; } = 60;

    public EasingFunction EasingType { get; set; } = EasingFunction.EaseInOutQuad;

    public bool AnimateOnLoad { get; set; } = true;

    public bool AnimateOnUpdate { get; set; } = true;

    public double StartOpacity { get; set; } = 0;

    public double EndOpacity { get; set; } = 1;

    public AnimationSettings() { }

    public AnimationSettings(int durationMs, int frameRate = 60)
    {
        DurationMs = durationMs;
        FrameRate = frameRate;
    }

    public void Validate()
    {
        if (DurationMs < 0)
            throw new InvalidOperationException("Duration cannot be negative");

        if (FrameRate < 1)
            throw new InvalidOperationException("FrameRate must be at least 1");

        if (StartOpacity < 0 || StartOpacity > 1)
            throw new InvalidOperationException("StartOpacity must be between 0 and 1");

        if (EndOpacity < 0 || EndOpacity > 1)
            throw new InvalidOperationException("EndOpacity must be between 0 and 1");
    }

    public int GetTotalFrames() => (int)(DurationMs * FrameRate / 1000.0);

    public double GetProgress(int currentFrame)
    {
        if (GetTotalFrames() == 0)
            return 1;

        return Math.Min(1, currentFrame / (double)GetTotalFrames());
    }

    public AnimationSettings Clone()
    {
        return new AnimationSettings
        {
            Enabled = Enabled,
            DurationMs = DurationMs,
            FrameRate = FrameRate,
            EasingType = EasingType,
            AnimateOnLoad = AnimateOnLoad,
            AnimateOnUpdate = AnimateOnUpdate,
            StartOpacity = StartOpacity,
            EndOpacity = EndOpacity
        };
    }

    public override string ToString() => $"AnimationSettings(Duration={DurationMs}ms, FrameRate={FrameRate})";
}

/// <summary>
/// Easing function enumeration for animations
/// </summary>
public enum EasingFunction
{
    Linear = 0,
    EaseInQuad = 1,
    EaseOutQuad = 2,
    EaseInOutQuad = 3,
    EaseInCubic = 4,
    EaseOutCubic = 5,
    EaseInOutCubic = 6,
    EaseInExpo = 7,
    EaseOutExpo = 8,
    EaseInOutExpo = 9
}

/// <summary>
/// Easing function calculator
/// </summary>
public static class EasingCalculator
{
    public static double Calculate(EasingFunction function, double progress)
    {
        if (progress < 0) progress = 0;
        if (progress > 1) progress = 1;

        return function switch
        {
            EasingFunction.Linear => progress,
            EasingFunction.EaseInQuad => progress * progress,
            EasingFunction.EaseOutQuad => progress * (2 - progress),
            EasingFunction.EaseInOutQuad => progress < 0.5
                ? 2 * progress * progress
                : -1 + (4 - 2 * progress) * progress,
            EasingFunction.EaseInCubic => progress * progress * progress,
            EasingFunction.EaseOutCubic => 1 + (--progress) * progress * progress,
            EasingFunction.EaseInOutCubic => progress < 0.5
                ? 4 * progress * progress * progress
                : 1 + (--progress) * 2 * (progress * progress + 1),
            EasingFunction.EaseInExpo => progress == 0 ? 0 : Math.Pow(2, 10 * progress - 10),
            EasingFunction.EaseOutExpo => progress == 1 ? 1 : 1 - Math.Pow(2, -10 * progress),
            EasingFunction.EaseInOutExpo => progress == 0 ? 0 : progress == 1 ? 1 :
                progress < 0.5 ? Math.Pow(2, 20 * progress - 10) / 2 :
                (2 - Math.Pow(2, -20 * progress + 10)) / 2,
            _ => progress
        };
    }
}
