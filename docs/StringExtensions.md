# StringExtensions

Provides a collection of pure‑function extension methods for `System.String` that perform common text transformations, validation, and formatting without allocating mutable state.

## API

### ToKebabCase
```csharp
public static string ToKebabCase(this string input)
```
- **Purpose:** Converts the input string to kebab‑case (lowercase words separated by hyphens).  
- **Parameters:** `input` – the string to convert.  
- **Return value:** A new string in kebab‑case; returns an empty string if `input` is empty.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ToSnakeCase
```csharp
public static string ToSnakeCase(this string input)
```
- **Purpose:** Converts the input string to snake_case (lowercase words separated by underscores).  
- **Parameters:** `input` – the string to convert.  
- **Return value:** A new string in snake_case; returns an empty string if `input` is empty.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ToPascalCase
```csharp
public static string ToPascalCase(this string input)
```
- **Purpose:** Converts the input string to PascalCase (each word capitalized, no separators).  
- **Parameters:** `input` – the string to convert.  
- **Return value:** A new string in PascalCase; returns an empty string if `input` is empty.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ToDoubleOrDefault
```csharp
public static double ToDoubleOrDefault(this string input, double defaultValue = 0.0)
```
- **Purpose:** Attempts to parse the input as a `double`; returns `defaultValue` on failure.  
- **Parameters:**  
  - `input` – the string to parse.  
  - `defaultValue` – value to return when parsing fails (default 0.0).  
- **Return value:** The parsed `double` or `defaultValue`.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ToIntOrDefault
```csharp
public static int ToIntOrDefault(this string input, int defaultValue = 0)
```
- **Purpose:** Attempts to parse the input as an `int`; returns `defaultValue` on failure.  
- **Parameters:**  
  - `input` – the string to parse.  
  - `defaultValue` – value to return when parsing fails (default 0).  
- **Return value:** The parsed `int` or `defaultValue`.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### Truncate
```csharp
public static string Truncate(this string input, int length)
```
- **Purpose:** Returns the input string shortened to at most `length` characters.  
- **Parameters:**  
  - `input` – the string to truncate.  
  - `length` – maximum length of the result; must be non‑negative.  
- **Return value:** `input` if its length ≤ `length`; otherwise the first `length` characters.  
- **Exceptions:**  
  - `ArgumentNullException` if `input` is `null`.  
  - `ArgumentOutOfRangeException` if `length` is negative.

### IsNumeric
```csharp
public static bool IsNumeric(this string input)
```
- **Purpose:** Determines whether the input consists solely of Unicode decimal digits.  
- **Parameters:** `input` – the string to test.  
- **Return value:** `true` if every character is a digit and the string is non‑empty; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### IsValidHexColor
```csharp
public static bool IsValidHexColor(this string input)
```
- **Purpose:** Checks whether the input represents a valid hexadecimal color (`#RGB`, `#RGBA`, `#RRGGBB`, or `#RRGGBBAA`).  
- **Parameters:** `input` – the string to test.  
- **Return value:** `true` if the string matches one of the accepted formats; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ExtractNumbers
```csharp
public static string ExtractNumbers(this string input)
```
- **Purpose:** Returns a new string containing only the digit characters from the input, in their original order.  
- **Parameters:** `input` – the string to process.  
- **Return value:** A string of extracted digits; empty if no digits are present.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### RemoveWhitespace
```csharp
public static string RemoveWhitespace(this string input)
```
- **Purpose:** Returns a copy of the input with all Unicode whitespace characters removed.  
- **Parameters:** `input` – the string to process.  
- **Return value:** The input string without any whitespace; empty if the input contained only whitespace.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### CapitalizeFirstLetter
```csharp
public static string CapitalizeFirstLetter(this string input)
```
- **Purpose:** Returns a string where the first character is converted to uppercase; the rest of the string is unchanged.  
- **Parameters:** `input` – the string to process.  
- **Return value:** The transformed string; returns an empty string if `input` is empty.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ContainsDigit
```csharp
public static bool ContainsDigit(this string input)
```
- **Purpose:** Indicates whether the input contains at least one Unicode decimal digit.  
- **Parameters:** `input` – the string to inspect.  
- **Return value:** `true` if any character is a digit; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### Repeat
```csharp
public static string Repeat(this string input, int count)
```
- **Purpose:** Returns a new string consisting of the input repeated `count` times concatenated together.  
- **Parameters:**  
  - `input` – the string to repeat.  
  - `count` – number of repetitions; must be non‑negative.  
- **Return value:** The repeated string; returns an empty string if `count` is 0 or `input` is empty.  
- **Exceptions:**  
  - `ArgumentNullException` if `input` is `null`.  
  - `ArgumentOutOfRangeException` if `count` is negative.

### Reverse
```csharp
public static string Reverse(this string input)
```
- **Purpose:** Returns a new string with the characters of the input in reverse order.  
- **Parameters:** `input` – the string to reverse.  
- **Return value:** The reversed string; returns an empty string if `input` is empty.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### IsPalindrome
```csharp
public static bool IsPalindrome(this string input)
```
- **Purpose:** Determines whether the input reads the same forwards and backwards (case‑sensitive).  
- **Parameters:** `input` – the string to test.  
- **Return value:** `true` if the string is a palindrome; otherwise `false`. Empty strings are considered palindromes.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

## Usage

```csharp
using static SkiasharpChartEngine.StringExtensions;

// Example 1: Normalizing a user‑provided identifier
string raw = "  user_ID  ";
string kebab = raw.Trim()
                  .ToSnakeCase()      // "user_id"
                  .ToKebabCase();     // "user-id"

// Example 2: Safe conversion of configuration values
string configValue = GetSetting("timeout"); // might be "42" or invalid
int timeout = configValue.ToIntOrDefault(30); // 30 if parsing fails
```

## Notes

- All methods are **pure**; they do not modify the input string and have no side effects, making them inherently thread‑safe.  
- Null inputs are not tolerated; each method throws `ArgumentNullException` to fail fast rather than returning a default value.  
- Empty strings are handled gracefully and typically return an empty string or `false`/`0` as appropriate.  
- Culture‑sensitive operations (e.g., `ToPascalCase`, `CapitalizeFirstLetter`) use the invariant culture implicitly via `char.ToUpper`/`char.ToLower` to avoid unexpected results across different locales.  
- The hex‑color validator accepts both short (`#RGB`) and long (`#RRGGBB`) forms with optional alpha channel; it does not permit CSS color names or functional notations (`rgb()`, `hsl()`).  
- Methods that accept a length or count (`Truncate`, `Repeat`) validate that the argument is non‑negative; negative values result in `ArgumentOutOfRangeException`.
