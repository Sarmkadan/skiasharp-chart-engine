# XmlChartSerializer

A utility class for serializing `Chart` objects to XML and deserializing XML back into `Chart` objects. It provides methods for validating XML content, formatting XML for readability, extracting specific elements, and merging multiple chart definitions.

## API

### `public XmlChartSerializer()`

Initializes a new instance of the `XmlChartSerializer` class. This constructor has no parameters and prepares the serializer for use.

### `public string Serialize(Chart chart)`

Serializes the provided `Chart` object into an XML string.

- **Parameters**
  - `chart` – The `Chart` instance to serialize. Must not be `null`.
- **Return value**
  - A string containing the XML representation of the chart.
- **Exceptions**
  - Throws `ArgumentNullException` if `chart` is `null`.

### `public Chart? Deserialize(string xml)`

Deserializes an XML string into a `Chart` object.

- **Parameters**
  - `xml` – The XML string to deserialize. Must not be `null` or empty.
- **Return value**
  - A `Chart` instance if deserialization succeeds; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `xml` is `null`.
  - Throws `ArgumentException` if `xml` is empty or whitespace.

### `public bool IsValidXml(string xml)`

Determines whether the provided XML string is valid and can be deserialized by this serializer.

- **Parameters**
  - `xml` – The XML string to validate. Must not be `null`.
- **Return value**
  - `true` if the XML is valid and deserializable; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `xml` is `null`.

### `public string PrettyPrint(string xml)`

Formats the provided XML string with indentation and line breaks for improved readability.

- **Parameters**
  - `xml` – The XML string to format. Must not be `null`.
- **Return value**
  - A new string containing the pretty-printed XML.
- **Exceptions**
  - Throws `ArgumentNullException` if `xml` is `null`.
  - Throws `XmlException` if the input is not valid XML.

### `public string? ExtractElement(string xml, string elementName)`

Extracts the content of a specific XML element from the provided XML string.

- **Parameters**
  - `xml` – The XML string to search. Must not be `null`.
  - `elementName` – The name of the element to extract. Must not be `null`.
- **Return value**
  - A string containing the inner XML of the first matching element, or `null` if the element is not found.
- **Exceptions**
  - Throws `ArgumentNullException` if either `xml` or `elementName` is `null`.

### `public string? MergeCharts(string xml, string additionalXml)`

Merges the content of an additional XML string into the provided XML string, combining chart definitions.

- **Parameters**
  - `xml` – The base XML string to merge into. Must not be `null`.
  - `additionalXml` – The XML string containing additional chart data to merge. Must not be `null`.
- **Return value**
  - A new string containing the merged XML, or `null` if merging fails.
- **Exceptions**
  - Throws `ArgumentNullException` if either `xml` or `additionalXml` is `null`.
  - Throws `XmlException` if either input is not valid XML.

## Usage

### Example 1: Serialize and Deserialize a Chart
