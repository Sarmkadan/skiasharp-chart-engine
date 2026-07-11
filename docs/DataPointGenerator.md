# DataPointGenerator

The `DataPointGenerator` is a utility class within the `skiasharp-chart-engine` designed to produce sets of `DataPoint` objects based on various mathematical functions. It simplifies the process of creating mock datasets for testing, visualization, and charting development by providing static methods to generate linear, sinusoidal, random, Gaussian, exponential, and polynomial data series.

## API

### GenerateLinearData
Generates a linear sequence of data points following the formula: $y = \text{slope} \times x + \text{intercept}$.

- **Parameters:**
  - `int count`: The number of data points to generate.
  - `double slope`: The slope of the line.
  - `double intercept`: The y-intercept of the line.
- **Returns:** A `List<DataPoint>` containing the generated linear series.

### GenerateSinusoidalData
Generates a sinusoidal sequence of data points following the formula: $y = \text{amplitude} \times \sin(\text{frequency} \times x)$.

- **Parameters:**
  - `int count`: The number of data points to generate.
  - `double frequency`: The frequency of the sine wave.
  - `double amplitude`: The amplitude of the sine wave.
- **Returns:** A `List<DataPoint>` containing the generated sinusoidal series.

### GenerateRandomData
Generates a sequence of data points with randomly distributed y-values within a specified range.

- **Parameters:**
  - `int count`: The number of data points to generate.
  - `double min`: The minimum y-value.
  - `double max`: The maximum y-value.
- **Returns:** A `List<DataPoint>` containing the generated random series.

### GenerateGaussianData
Generates a sequence of data points following a Gaussian (normal) distribution.

- **Parameters:**
  - `int count`: The number of data points to generate.
  - `double mean`: The mean ($\mu$) of the distribution.
  - `double stdDev`: The standard deviation ($\sigma$) of the distribution.
- **Returns:** A `List<DataPoint>` containing the generated Gaussian series.

### GenerateExponentialData
Generates a sequence of data points following the formula: $y = \text{baseValue} \times \text{exponent}^x$.

- **Parameters:**
  - `int count`: The number of data points to generate.
  - `double baseValue`: The initial value at $x=0$.
  - `double exponent`: The exponent factor.
- **Returns:** A `List<DataPoint>` containing the generated exponential series.

### GeneratePolynomialData
Generates a sequence of data points based on a polynomial function defined by the provided coefficients, where the index of the array corresponds to the power of $x$ (e.g., `coefficients[0]` is the constant term).

- **Parameters:**
  - `int count`: The number of data points to generate.
  - `double[] coefficients`: An array of coefficients for the polynomial.
- **Returns:** A `List<DataPoint>` containing the generated polynomial series.

## Usage

### Example 1: Creating a Simple Linear Dataset
```csharp
// Generate 100 points for a line: y = 2x + 5
List<DataPoint> linearData = DataPointGenerator.GenerateLinearData(100, 2.0, 5.0);
```

### Example 2: Creating a Quadratic Dataset
```csharp
// Generate 50 points for a parabola: y = 3x^2 + 0x + 1
double[] coefficients = { 1.0, 0.0, 3.0 };
List<DataPoint> polynomialData = DataPointGenerator.GeneratePolynomialData(50, coefficients);
```

## Notes

- **Edge Cases:** Passing a `count` less than or equal to zero will return an empty `List<DataPoint>`. For `GeneratePolynomialData`, providing an empty or null array for `coefficients` will result in a list of points where all y-values are zero.
- **Thread Safety:** The methods in `DataPointGenerator` are static and do not maintain internal state. They are thread-safe and can be called concurrently from multiple threads without additional synchronization.
- **Performance:** These generators create and populate a new `List<DataPoint>` on each call. For high-frequency updates, consider caching the results if the parameters do not change.
