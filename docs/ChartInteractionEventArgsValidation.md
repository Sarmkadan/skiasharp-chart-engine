# ChartInteractionEventArgsValidation

The `ChartInteractionEventArgsValidation` is a static helper class that validates `ChartInteractionEventArgs` instances within the `skiasharp-chart-engine`. It offers three entry points with different reporting styles: a problem list for diagnostics (`Validate`), a boolean check for filtering (`IsValid`), and a guard that throws for fail-fast argument checking (`EnsureValid`). All three are extension methods on `ChartInteractionEventArgs`, so they can be called as instance methods on an event args object.

## API

*   **`Validate(this ChartInteractionEventArgs value)`**
    Validates an event args instance and returns every problem found.
    *   **Parameters:** `value` (`ChartInteractionEventArgs`) - The event arguments to validate.
    *   **Returns:** An `IReadOnlyList<string>` of human-readable messages. The list is empty when the instance is valid.
    *   **Exceptions:** `ArgumentNullException` when `value` is `null`.
    *   **Behavior:** All rules are evaluated, so one call can report several problems at once. Messages include the offending value where relevant (for example, `PointerX must be non-negative, but was -3.`).

*   **`IsValid(this ChartInteractionEventArgs value)`**
    Checks whether an event args instance is valid.
    *   **Parameters:** `value` (`ChartInteractionEventArgs`) - The event arguments to check.
    *   **Returns:** `true` if `Validate` reports no problems; otherwise, `false`.
    *   **Exceptions:** `ArgumentNullException` when `value` is `null`.

*   **`EnsureValid(this ChartInteractionEventArgs value)`**
    Throws if an event args instance is invalid. Intended for guarding public entry points.
    *   **Parameters:** `value` (`ChartInteractionEventArgs`) - The event arguments to validate.
    *   **Exceptions:** `ArgumentNullException` when `value` is `null`. `ArgumentException` when the instance is invalid; the message is `ChartInteractionEventArgs is invalid. Problems:` followed by each problem on its own line.

## Validation Rules

An instance is valid when all of the following hold:

1.  `PointerX` is greater than or equal to `0`.
2.  `PointerY` is greater than or equal to `0`.
3.  `Region` is not the default value of `ChartRegion` (see the note on `PlotArea` below).
4.  `TooltipText` is not `null`, and is not empty or whitespace-only.
5.  `Timestamp` is not the default `DateTime`.
6.  `Metadata` is not `null`.

`InteractionType`, `HitDataPoint`, `HitSeries`, and `SeriesIndex` are not checked. A miss (`HitDataPoint` and `HitSeries` are `null`, `SeriesIndex` is `-1`) is therefore valid.

## Usage

```csharp
// Example 1: Collecting every problem with an event for logging
IReadOnlyList<string> problems = args.Validate();
if (problems.Count > 0)
{
    _logger.LogWarning("Rejected interaction event: {Problems}", string.Join("; ", problems));
}

// Example 2: Filtering events before they reach handlers
var validEvents = pendingEvents.Where(e => e.IsValid()).ToList();

// Example 3: Guarding a public method argument
public void Publish(ChartInteractionEventArgs args)
{
    args.EnsureValid();
    _publisher.Raise(args);
}
```

## Notes

*   **Thread-Safety:** All methods are stateless and only read the instance they are given, so they are safe for concurrent use as long as the instance itself is not being mutated.
*   **Null Handling:** `Validate`, `IsValid`, and `EnsureValid` throw `ArgumentNullException` for `null` input rather than reporting it as a problem.
*   **`PlotArea` Region:** `ChartRegion.PlotArea` is the enum's default value (`0`), so an event whose `Region` was never assigned is indistinguishable from one hit in the plot area. The `Region` check therefore rejects `PlotArea` events, even though `PlotArea` is a legitimate hit region. Callers that raise plot-area interactions must be aware of this, or the check should be revisited.
*   **Timestamp Check:** `Timestamp` is a get-only property initialized to `DateTime.UtcNow`, so the check only fails for an instance whose timestamp is `default`. In normal construction this never happens.
*   **Metadata Check:** `Metadata` defaults to an empty dictionary, so the check only fails when a caller explicitly assigns `null`.
*   **Scope of Checks:** The validator does not inspect `HitDataPoint`, `HitSeries`, or the consistency between `SeriesIndex` and `HitSeries`. Those relationships are the responsibility of the code that performs the hit-test.
