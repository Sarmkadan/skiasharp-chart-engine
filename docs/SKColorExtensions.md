# SKColorExtensions

`SKColorExtensions` provides helpers for changing and formatting SkiaSharp `SKColor` values. The methods return new values and do not modify the original color.

```csharp
using SkiaSharp;
using SkiasharpChartEngine.Extensions;
```

## WithOpacity

```csharp
public static SKColor WithOpacity(this SKColor color, float opacity)
```

Returns a color with the same red, green, and blue channels and a new alpha channel. `opacity` is clamped to the range `0.0f` through `1.0f`; `0.0f` is fully transparent and `1.0f` is fully opaque.

```csharp
var blue = new SKColor(30, 144, 255);
var translucentBlue = blue.WithOpacity(0.5f);

Console.WriteLine(translucentBlue.Alpha);   // 127
Console.WriteLine(translucentBlue.ToHex()); // #7F1E90FF
```

## Darken

```csharp
public static SKColor Darken(this SKColor color, float amount)
```

Returns a darker color by multiplying each RGB channel by `1 - amount`. The amount is clamped to `0.0f` through `1.0f`, and the original alpha channel is preserved. An amount of `0.0f` leaves the color unchanged; `1.0f` produces black.

```csharp
var color = new SKColor(100, 150, 200, 220);
var darker = color.Darken(0.25f);

Console.WriteLine(darker.ToHex()); // #DC4B7096
```

## Lighten

```csharp
public static SKColor Lighten(this SKColor color, float amount)
```

Returns a lighter color by moving each RGB channel toward `255`. The amount is clamped to `0.0f` through `1.0f`, and the original alpha channel is preserved. An amount of `0.0f` leaves the color unchanged; `1.0f` produces white.

```csharp
var color = new SKColor(100, 150, 200, 220);
var lighter = color.Lighten(0.25f);

Console.WriteLine(lighter.ToHex()); // #DC8AB0D5
```

## ToHex

```csharp
public static string ToHex(this SKColor color)
```

Formats an opaque color as `#RRGGBB`. If alpha is less than `255`, it uses `#AARRGGBB`. Hexadecimal letters are uppercase.

```csharp
var opaque = new SKColor(255, 87, 51);
var transparent = new SKColor(255, 87, 51, 128);

Console.WriteLine(opaque.ToHex());      // #FF5733
Console.WriteLine(transparent.ToHex()); // #80FF5733
```

## TryParseHex

```csharp
public static bool TryParseHex(string hex, out SKColor color)
```

Attempts to parse `RRGGBB` or `AARRGGBB`, with an optional leading `#`. Parsing is case-insensitive. On failure, the method returns `false` and sets `color` to its default value.

```csharp
if (SKColorExtensions.TryParseHex("#336699", out var opaque))
{
    Console.WriteLine(opaque.Red);   // 51
    Console.WriteLine(opaque.Green); // 102
    Console.WriteLine(opaque.Blue);  // 153
    Console.WriteLine(opaque.Alpha); // 255
}

if (SKColorExtensions.TryParseHex("80336699", out var translucent))
{
    Console.WriteLine(translucent.Alpha); // 128
    Console.WriteLine(translucent.ToHex()); // #80336699
}

bool parsed = SKColorExtensions.TryParseHex("not-a-color", out _);
Console.WriteLine(parsed); // False
```

`TryParseHex` does not accept three- or four-digit shorthand, RGBA ordering, surrounding whitespace, CSS color names, or functional color notation such as `rgb(...)`.

## Notes

- `WithOpacity`, `Darken`, and `Lighten` truncate fractional channel values when converting them to bytes.
- `Darken` and `Lighten` retain the source color's alpha channel.
- `ToHex` and `TryParseHex` use alpha-first ordering (`#AARRGGBB`) for eight-digit values.
