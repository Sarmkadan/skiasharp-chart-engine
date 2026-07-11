# TransitionTimelineExtensions

Extension methods for `TransitionTimeline` that provide common timeline manipulation operations such as keyframe insertion, transition appending, time shifting, duration analysis, and timeline reversal.

## API

### `AddKeyframeAt(TransitionTimeline timeline, double time, object keyframe)`

Inserts a new keyframe at the specified time in the timeline. If a keyframe already exists at the given time, it is replaced. The timeline's segments are automatically adjusted to accommodate the new keyframe.

- **Parameters**
  - `timeline`: The `TransitionTimeline` to modify.
  - `time`: The time position (in timeline units) where the keyframe should be inserted.
  - `keyframe`: The keyframe object to insert.
- **Return Value**: The modified `TransitionTimeline` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `timeline` is `null`. Throws `ArgumentOutOfRangeException` if `time` is negative.

---

### `AppendTransitionAt(TransitionTimeline timeline, double duration, object transition)`

Appends a new transition segment to the end of the timeline with the specified duration. The new segment starts at the current end time of the timeline and spans the given duration.

- **Parameters**
  - `timeline`: The `TransitionTimeline` to extend.
  - `duration`: The duration of the new transition segment (must be positive).
  - `transition`: The transition object to append.
- **Return Value**: The modified `TransitionTimeline` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `timeline` is `null`. Throws `ArgumentOutOfRangeException` if `duration` is not positive.

---
### `ShiftTime(TransitionTimeline timeline, double delta)`

Adjusts all keyframe times in the timeline by the specified delta. Positive values shift keyframes forward in time; negative values shift them backward. Segments are recalculated accordingly.

- **Parameters**
  - `timeline`: The `TransitionTimeline` to adjust.
  - `delta`: The time shift amount (can be positive or negative).
- **Return Value**: The modified `TransitionTimeline` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `timeline` is `null`.

---
### `GetSegmentDurations(TransitionTimeline timeline)`

Computes and returns the durations of all segments in the timeline. The result is an array where each element corresponds to the duration of the segment between two consecutive keyframes.

- **Parameters**
  - `timeline`: The `TransitionTimeline` to analyze.
- **Return Value**: An array of `double` values representing the duration of each segment. The length of the array is `timeline.Keyframes.Count - 1`.
- **Exceptions**: Throws `ArgumentNullException` if `timeline` is `null`.

---
### `Reverse(TransitionTimeline timeline)`

Reverses the timeline by flipping the order of keyframes and adjusting segment durations accordingly. The first keyframe becomes the last, and vice versa, with all transitions played in reverse order.

- **Parameters**
  - `timeline`: The `TransitionTimeline` to reverse.
- **Return Value**: The modified `TransitionTimeline` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `timeline` is `null`. Throws `InvalidOperationException` if the timeline has fewer than two keyframes.

## Usage
