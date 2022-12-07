// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// DateTime utilities for chart data and axis formatting
/// Provides methods for date/time formatting and calculations
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Formats datetime for chart axis labels
    /// Intelligently chooses format based on time span
    /// </summary>
    public static string FormatForAxis(DateTime dateTime, DateTime minDate, DateTime maxDate)
    {
        var timeSpan = maxDate - minDate;

        return timeSpan.TotalDays switch
        {
            < 1 => dateTime.ToString("HH:mm"),
            < 7 => dateTime.ToString("ddd HH:mm"),
            < 30 => dateTime.ToString("MMM dd"),
            < 365 => dateTime.ToString("MMM yyyy"),
            _ => dateTime.ToString("yyyy")
        };
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime
    /// </summary>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
    }

    /// <summary>
    /// Converts DateTime to Unix timestamp
    /// </summary>
    public static long ToUnixTimestamp(DateTime dateTime)
    {
        return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    /// <summary>
    /// Calculates business days between two dates (excludes weekends)
    /// </summary>
    public static int GetBusinessDaysBetween(DateTime startDate, DateTime endDate)
    {
        var totalDays = (int)(endDate - startDate).TotalDays;
        int businessDays = 0;

        for (int i = 0; i <= totalDays; i++)
        {
            var day = startDate.AddDays(i);
            if (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
            {
                businessDays++;
            }
        }

        return businessDays;
    }

    /// <summary>
    /// Gets the start of the period (day, week, month, year)
    /// </summary>
    public static DateTime GetPeriodStart(DateTime date, DateTimePeriod period)
    {
        return period switch
        {
            DateTimePeriod.Day => date.Date,
            DateTimePeriod.Week => date.Date.AddDays(-(int)date.DayOfWeek),
            DateTimePeriod.Month => new DateTime(date.Year, date.Month, 1),
            DateTimePeriod.Year => new DateTime(date.Year, 1, 1),
            DateTimePeriod.Hour => date.AddSeconds(-date.Second).AddMilliseconds(-date.Millisecond),
            _ => date
        };
    }

    /// <summary>
    /// Gets the end of the period
    /// </summary>
    public static DateTime GetPeriodEnd(DateTime date, DateTimePeriod period)
    {
        return period switch
        {
            DateTimePeriod.Day => date.Date.AddDays(1).AddTicks(-1),
            DateTimePeriod.Week => date.Date.AddDays(7 - (int)date.DayOfWeek).AddTicks(-1),
            DateTimePeriod.Month => new DateTime(date.Year, date.Month, 1).AddMonths(1).AddTicks(-1),
            DateTimePeriod.Year => new DateTime(date.Year, 12, 31, 23, 59, 59, 999),
            DateTimePeriod.Hour => date.AddHours(1).AddTicks(-1),
            _ => date
        };
    }

    /// <summary>
    /// Gets the week number of the year
    /// </summary>
    public static int GetWeekNumber(DateTime date)
    {
        var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date,
            System.Globalization.CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);
    }

    /// <summary>
    /// Checks if a date falls on a weekend
    /// </summary>
    public static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Gets the next occurrence of a day of week
    /// </summary>
    public static DateTime GetNextDayOfWeek(DateTime date, DayOfWeek dayOfWeek)
    {
        var daysToAdd = (int)dayOfWeek - (int)date.DayOfWeek;
        if (daysToAdd <= 0)
        {
            daysToAdd += 7;
        }

        return date.AddDays(daysToAdd);
    }

    /// <summary>
    /// Formats a timespan in human-readable format
    /// </summary>
    public static string FormatTimespan(TimeSpan timespan)
    {
        if (timespan.TotalSeconds < 60)
            return $"{(int)timespan.TotalSeconds}s";

        if (timespan.TotalMinutes < 60)
            return $"{(int)timespan.TotalMinutes}m {timespan.Seconds}s";

        if (timespan.TotalHours < 24)
            return $"{(int)timespan.TotalHours}h {timespan.Minutes}m";

        return $"{(int)timespan.TotalDays}d {timespan.Hours}h";
    }

    /// <summary>
    /// Gets the age in years from a birthdate
    /// </summary>
    public static int GetAgeInYears(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Checks if two dates are on the same day
    /// </summary>
    public static bool IsSameDay(DateTime date1, DateTime date2)
    {
        return date1.Date == date2.Date;
    }
}

/// <summary>
/// Enumeration of date/time periods
/// </summary>
public enum DateTimePeriod
{
    Hour,
    Day,
    Week,
    Month,
    Year
}
