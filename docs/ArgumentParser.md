# ArgumentParser
The `ArgumentParser` class is designed to handle the parsing and validation of command-line arguments, providing a simple and efficient way to manage input parameters in C# applications. It allows developers to define required and optional arguments, parse their values, and validate their presence.

## API
* `public ArgumentParser`: The constructor for the `ArgumentParser` class, used to create a new instance.
* `public Dictionary<string, string> Parse`: Parses the command-line arguments and returns a dictionary containing the parsed argument values. The dictionary keys are the argument names, and the values are the corresponding argument values.
* `public bool ValidateRequired`: Validates whether all required arguments are present. Returns `true` if all required arguments are present, and `false` otherwise.
* `public string GetValue(string argumentName)`: Retrieves the value of a specific argument. The `argumentName` parameter specifies the name of the argument to retrieve the value for. Returns the value of the argument if it exists, or `null` if it does not.
* `public List<string> ParseList(string argumentName)`: Parses a list of values for a specific argument. The `argumentName` parameter specifies the name of the argument to parse the list for. Returns a list of values for the argument if it exists, or an empty list if it does not.

## Usage
The following examples demonstrate how to use the `ArgumentParser` class:
```csharp
// Example 1: Parsing simple arguments
var parser = new ArgumentParser();
var arguments = new[] { "--input", "input.txt", "--output", "output.txt" };
var parsedArguments = parser.Parse(arguments);
Console.WriteLine(parsedArguments["input"]);  // Output: input.txt
Console.WriteLine(parsedArguments["output"]); // Output: output.txt

// Example 2: Validating required arguments
var parser2 = new ArgumentParser();
var arguments2 = new[] { "--input", "input.txt" };
if (!parser2.ValidateRequired)
{
    Console.WriteLine("Error: Required argument '--output' is missing.");
}
else
{
    var parsedArguments2 = parser2.Parse(arguments2);
    Console.WriteLine(parsedArguments2["input"]);  // Output: input.txt
}
```

## Notes
When using the `ArgumentParser` class, note that the `Parse` method will throw an exception if the argument syntax is invalid. The `ValidateRequired` method will return `false` if any required arguments are missing, but will not throw an exception. The `GetValue` and `ParseList` methods will return `null` or an empty list if the specified argument does not exist. The `ArgumentParser` class is not thread-safe, as it uses instance state to store the parsed arguments. Therefore, it is recommended to create a new instance of the `ArgumentParser` class for each thread or concurrent operation.
