# VersionCompatibilityChecker

The `VersionCompatibilityChecker` class provides a mechanism to verify that a specific application or data version aligns with defined minimum and maximum supported version constraints. It facilitates automated checks to determine compatibility status and identify whether data migration is required, ensuring that the system operates within supported version ranges.

## API

### Constructors

*   **`VersionCompatibilityChecker()`**
    Initializes a new instance of the `VersionCompatibilityChecker` class.

### Properties

*   **`bool IsCompatible`**
    Gets a value indicating whether the current version is within the supported range (between `MinSupportedVersion` and `MaxSupportedVersion`, inclusive).

*   **`bool MigrationNeeded`**
    Gets a value indicating whether a migration process is required based on the version disparity.

*   **`string CurrentVersion`**
    Gets the current version string associated with the checker.

*   **`string MinSupportedVersion`**
    Gets the minimum supported version string.

*   **`string MaxSupportedVersion`**
    Gets the maximum supported version string.

*   **`DateTime CheckedAt`**
    Gets the timestamp indicating when the compatibility check was last performed.

### Methods

*   **`string GetCurrentVersion()`**
    Returns the string representation of the current version.

*   **`VersionInfo GetVersionInfo()`**
    Returns a `VersionInfo` object containing detailed information about the version configuration.

*   **`bool ValidateVersionSequence(string version1, string version2)`**
    Validates whether the provided sequence of two versions is in a valid chronological or logical order. Returns `true` if valid; otherwise, `false`.

*   **`string ToString()`**
    Returns a string representation of the current `VersionCompatibilityChecker` instance, including version details and compatibility status.

## Usage

### Basic Compatibility Verification
```csharp
var checker = new VersionCompatibilityChecker(currentVersion, "1.0.0", "2.5.0");

if (checker.IsCompatible)
{
    Console.WriteLine("Version is supported.");
}
else if (checker.MigrationNeeded)
{
    Console.WriteLine("Migration required.");
}
```

### Validating a Version Sequence
```csharp
var checker = new VersionCompatibilityChecker();
bool isSequenceValid = checker.ValidateVersionSequence("1.0.0", "1.1.0");

if (isSequenceValid)
{
    // Proceed with version upgrade logic
}
```

## Notes

*   **Edge Cases:** If `MinSupportedVersion` or `MaxSupportedVersion` are not properly initialized or are null/empty, compatibility checks may throw a `NullReferenceException` or return unexpected results. Ensure all version strings adhere to a standard semantic versioning format if the underlying implementation depends on string-based comparison.
*   **Thread Safety:** Instances of `VersionCompatibilityChecker` are generally thread-safe for read-only operations. However, if the instance properties are modified concurrently from multiple threads, synchronization mechanisms should be implemented externally.
*   **Version Format:** The accuracy of `ValidateVersionSequence` and `IsCompatible` depends on the format of the version strings provided. Ensure that all versions being compared are formatted consistently (e.g., "major.minor.patch") to avoid incorrect logical comparisons.
