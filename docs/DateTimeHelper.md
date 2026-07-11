# DateTimeHelper

`DateTimeHelper` provides utility methods for common `DateTime` manipulations, timestamp conversions, and business-logic related calculations required when working with time-series and date-based data in charting applications.

## API

### FormatForAxis(DateTime dateTime)
Formats a `DateTime` object into a string suitable for display on a chart axis.
*   **Parameters:** `dateTime` - The `DateTime` to format.
*   **Returns:** A `string` representation of the date.

### FromUnixTimestamp(long unixTimestamp)
Converts a Unix timestamp (seconds since the Unix epoch) to a `DateTime` object.
*   **Parameters:** `unixTimestamp` - The timestamp to convert.
*   **Returns:** A `DateTime` object in UTC.

### ToUnixTimestamp(DateTime dateTime)
Converts a `DateTime` object to a Unix timestamp (seconds since the Unix epoch).
*   **Parameters:** `dateTime` - The `DateTime` to convert.
*   **Returns:** A `long` representing the Unix timestamp.

### GetBusinessDaysBetween(DateTime startDate, DateTime endDate)
Calculates the number of business days (Monday through Friday) between two dates, inclusive of the start date and exclusive of the end date.
*   **Parameters:** `startDate` - The start date. `endDate` - The end date.
*   **Returns:** An `int` representing the count of business days.

### GetPeriodStart(DateTime dateTime)
Returns the starting `DateTime` of the period containing the provided date (e.g., start of the month or year).
*   **Parameters:** `dateTime` - The reference date.
*   **Returns:** A `DateTime` representing the start of the period.

### GetPeriodEnd(DateTime dateTime)
Returns the ending `DateTime` of the period containing the provided date.
*   **Parameters:** `dateTime` - The reference date.
*   **Returns:** A `DateTime` representing the end of the period.

### GetWeekNumber(DateTime dateTime)
Calculates the ISO 8601 week number for the provided date.
*   **Parameters:** `dateTime` - The date to evaluate.
*   **Returns:** An `int` representing the week number (1-53).

### IsWeekend(DateTime dateTime)
Determines whether the provided date falls on a weekend (Saturday or Sunday).
*   **Parameters:** `dateTime` - The date to check.
*   **Returns:** `true` if the date is a weekend; otherwise, `false`.

### GetNextDayOfWeek(DateTime dateTime, DayOfWeek dayOfWeek)
Finds the next occurrence of a specific day of the week after the provided date.
*   **Parameters:** `dateTime` - The reference date. `dayOfWeek` - The target `DayOfWeek`.
*   **Returns:** A `DateTime` for the next occurrence.

### FormatTimespan(TimeSpan timeSpan)
Formats a `TimeSpan` into a human-readable string.
*   **Parameters:** `timeSpan` - The `TimeSpan` to format.
*   **Returns:** A `string` representation of the duration.

### GetAgeInYears(DateTime birthDate)
Calculates age in full years based on the provided birth date and the current system date.
*   **Parameters:** `birthDate` - The birth date.
*   **Returns:** An `int` representing the age in years.

### IsSameDay(DateTime date1, DateTime date2)
Determines if two `DateTime` instances represent the same calendar day.
*   **Parameters:** `date1` - The first date. `date2` - The second date.
*   **Returns:** `true` if both dates are on the same day; otherwise, `false`.

## Usage

```csharp
// Example 1: Calculating business days for chart data aggregation
DateTime start = new DateTime(2025, 01, 01);
DateTime end = new DateTime(2025, 01, 15);
int businessDays = DateTimeHelper.GetBusinessDaysBetween(start, end);

Console.WriteLine($"Business days in period: {businessDays}");
```

```csharp
// Example 2: Normalizing chart data timestamps
DateTime dataPointDate = DateTime.Now;
long unixTimestamp = DateTimeHelper.ToUnixTimestamp(dataPointDate);

// ... later, converting back for display
DateTime displayDate = DateTimeHelper.FromUnixTimestamp(unixTimestamp);
```

## Notes

*   **Thread Safety:** `DateTimeHelper` methods are implemented as static, stateless operations. They are fully thread-safe and can be called concurrently from multiple threads without external synchronization.
*   **Time Zones:** Methods accepting `DateTime` generally expect local or UTC time depending on the system context. Care should be taken when mixing time zones to ensure consistency before invoking these methods.
*   **Edge Cases:** For methods like `GetAgeInYears` or `GetBusinessDaysBetween`, ensure that `startDate` is less than or equal to `endDate`. Behavior when `startDate` is greater than `endDate` is not explicitly guaranteed and may return negative values or zero depending on the specific implementation.
