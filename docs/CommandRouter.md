# CommandRouter
The `CommandRouter` class is a central component in the skiasharp-chart-engine project, responsible for managing and executing commands. It provides a flexible way to register, route, and display help for various commands, allowing for a decoupled and extensible command handling system.

## API
* `public CommandRouter`: The constructor for the `CommandRouter` class, used to create a new instance.
* `public void RegisterCommand`: Registers a new command with the router. This method takes no parameters and returns no value, but it is expected that the command to be registered is provided through some other means, such as a parameter to the constructor or a separate initialization method. If the command is already registered, this method may throw an exception.
* `public async Task<int> RouteAsync`: Routes a command to its corresponding handler and executes it asynchronously. The method returns an integer value indicating the result of the command execution. If the command is not registered or an error occurs during execution, this method may throw an exception.
* `public void DisplayHelp`: Displays help information for all registered commands. This method takes no parameters and returns no value.
* `public IEnumerable<string> GetRegisteredCommands`: Returns a collection of strings representing the names of all registered commands.

## Usage
The following examples demonstrate how to use the `CommandRouter` class:
```csharp
// Example 1: Registering and routing a command
var router = new CommandRouter();
router.RegisterCommand();
var result = await router.RouteAsync();
Console.WriteLine($"Command executed with result: {result}");
```

```csharp
// Example 2: Displaying help and getting registered commands
var router = new CommandRouter();
router.RegisterCommand();
router.DisplayHelp();
var commands = router.GetRegisteredCommands();
Console.WriteLine("Registered commands:");
foreach (var command in commands)
{
    Console.WriteLine(command);
}
```

## Notes
The `CommandRouter` class is designed to be thread-safe, allowing multiple threads to access and manipulate the registered commands concurrently. However, the `RouteAsync` method may throw exceptions if the command execution fails or if the command is not registered. Additionally, the `RegisterCommand` method may throw exceptions if the command is already registered or if there is an error during the registration process. It is recommended to handle these exceptions properly to ensure robust and reliable command handling.
