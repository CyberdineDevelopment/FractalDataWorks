# FractalDataWorks.Commands.Data.Abstractions

**Universal data command abstractions for the FractalDataWorks Developer Kit.**

## Overview

This project provides the core abstractions for universal data commands that work across any data store type (SQL, REST, GraphQL, File, etc.). The key principle is **"write once, run anywhere"** - the same `IDataCommand` can execute against different backend stores simply by changing configuration.

### Core Concepts

- **IDataCommand** - Universal command interface representing data operations
- **IDataCommandTranslator** - Converts universal commands to domain-specific commands (SQL, REST, etc.)
- **DataCommandTranslators** - Hybrid TypeCollection combining compile-time and runtime translator registration
- **IDataCommandTranslatorProvider** - DI-aware provider for resolving translators

## Architecture

```
IDataCommand (universal)
    ↓
IDataCommandTranslator
    ↓
IConnectionCommand (domain-specific)
```

### Flow Example

```csharp
// 1. User creates universal command
var command = new QueryDataCommand
{
    ConnectionName = "PrimaryDB",  // Which connection
    ContainerName = "Users",        // Which container (table/endpoint/file)
    // ... filter, projection, etc.
};

// 2. DataGateway gets translator based on connection's DataStore
var translator = await translatorProvider.GetTranslator("MsSql");

// 3. Translator converts to SQL
var sqlCommand = await translator.Translate(command);
// Result: SELECT * FROM [Users] WHERE ...

// 4. Connection executes SQL
var result = await connection.Execute<User>(sqlCommand);
```

## Core Interfaces

### IDataCommand

Base interface for all data commands. Extends `IGenericCommand` from the Commands.Abstractions project.

```csharp
public interface IDataCommand : IGenericCommand
{
    /// <summary>
    /// Gets the container name (table, collection, endpoint, file path).
    /// </summary>
    string ContainerName { get; }

    /// <summary>
    /// Gets metadata for the command (connection hints, caching, etc.).
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}
```

**Generic Variants:**
- `IDataCommand<TResult>` - Command with typed result
- `IDataCommand<TResult, TInput>` - Command with typed input and result

**Usage Examples:**

```csharp
// Simple query command
public sealed class GetUserCommand : IDataCommand<User>
{
    public string ConnectionName => "PrimaryDB";
    public string ContainerName => "Users";
    public int UserId { get; init; }
}

// Insert command
public sealed class CreateUserCommand : IDataCommand<int, UserDto>
{
    public string ConnectionName => "PrimaryDB";
    public string ContainerName => "Users";
    public UserDto User { get; init; }
}
```

### IDataCommandTranslator

Interface for translating universal commands to domain-specific commands.

```csharp
public interface IDataCommandTranslator
{
    /// <summary>
    /// Gets the domain name this translator targets (Sql, Rest, File, GraphQL, etc.).
    /// </summary>
    string DomainName { get; }

    /// <summary>
    /// Translates a data command to a connection-specific command.
    /// </summary>
    Task<IGenericResult<IConnectionCommand>> Translate(
        IDataCommand command,
        CancellationToken cancellationToken = default);
}
```

**Translator Examples:**

```csharp
// SQL Server Translator
public sealed class MsSqlDataCommandTranslator : IDataCommandTranslator
{
    public string DomainName => "MsSql";

    public Task<IGenericResult<IConnectionCommand>> Translate(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        // Convert command.ContainerName → SELECT * FROM [TableName]
        // Convert filters → WHERE clauses
        // Convert projections → SELECT columns
        // Return SQL ConnectionCommand
    }
}

// REST API Translator
public sealed class RestDataCommandTranslator : IDataCommandTranslator
{
    public string DomainName => "Rest";

    public Task<IGenericResult<IConnectionCommand>> Translate(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        // Convert command.ContainerName → GET /api/endpoint
        // Convert filters → OData $filter query params
        // Convert projections → $select query params
        // Return HTTP ConnectionCommand
    }
}
```

### DataCommandTranslators (TypeCollection)

Hybrid collection supporting both compile-time (via `[TypeOption]`) and runtime registration (via `Register()` method).

```csharp
[TypeCollection(typeof(DataCommandTranslatorBase), typeof(IDataCommandTranslator), typeof(DataCommandTranslators))]
public abstract partial class DataCommandTranslators : TypeCollectionBase<DataCommandTranslatorBase, IDataCommandTranslator>
{
    // Compile-time registration (via [TypeOption] attribute)
    // Source generator discovers translators and creates static properties

    // Runtime registration (called by connection types during registration)
    public static void Register(string name, Type translatorType)
    {
        // Connections register their translators here
        // Example: HttpConnection registers RestTranslator, GraphQLTranslator
    }

    // Get translator type by domain name
    public static Type? GetTranslatorType(string domainName)
    {
        // Checks both compile-time and runtime registered translators
    }
}
```

**Registration Examples:**

```csharp
// Compile-time registration (in translator project)
[TypeOption(typeof(DataCommandTranslators), "MsSql")]
public sealed class MsSqlTranslatorType : DataCommandTranslatorBase
{
    public static MsSqlTranslatorType Instance { get; } = new();
    private MsSqlTranslatorType() : base(1, "MsSql", "Sql") { }
}

// Runtime registration (in connection's ServiceTypeOption.Configure())
public override void Configure(IConfiguration configuration)
{
    // Register translator at runtime
    DataCommandTranslators.Register("MsSql", typeof(MsSqlDataCommandTranslator));

    // Register in DI
    services.AddScoped<IDataCommandTranslator, MsSqlDataCommandTranslator>();
}
```

### IDataCommandTranslatorProvider

Provider interface for resolving translators from DI.

```csharp
public interface IDataCommandTranslatorProvider
{
    /// <summary>
    /// Gets a translator for the specified domain.
    /// </summary>
    IGenericResult<IDataCommandTranslator> GetTranslator(string domainName);
}
```

**Implementation:**

```csharp
public sealed class DataCommandTranslatorProvider : IDataCommandTranslatorProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataCommandTranslatorProvider> _logger;

    public IGenericResult<IDataCommandTranslator> GetTranslator(string domainName)
    {
        // 1. Get translator type from TypeCollection
        var translatorType = DataCommandTranslators.GetTranslatorType(domainName);
        if (translatorType == null)
        {
            return GenericResult<IDataCommandTranslator>.Failure(
                $"No translator registered for domain: {domainName}");
        }

        // 2. Resolve from DI
        var translator = _serviceProvider.GetService(translatorType) as IDataCommandTranslator;
        if (translator == null)
        {
            return GenericResult<IDataCommandTranslator>.Failure(
                $"Translator for '{domainName}' not available in DI");
        }

        return GenericResult<IDataCommandTranslator>.Success(translator);
    }
}
```

## Project Structure

```
FractalDataWorks.Commands.Data.Abstractions/
├── Commands/
│   ├── IDataCommand.cs                    # Base command interface
│   ├── IDataCommand{TResult}.cs           # Generic command interface
│   ├── IDataCommand{TResult,TInput}.cs    # Generic command with input
│   ├── DataCommandBase.cs                 # Base command implementation
│   ├── DataCommandBase{TResult}.cs        # Generic base implementation
│   ├── DataCommandBase{TResult,TInput}.cs # Generic base with input
│   └── DataCommands.cs                    # TypeCollection for commands
├── Translators/
│   ├── IDataCommandTranslator.cs          # Translator interface
│   ├── DataCommandTranslatorBase.cs       # Base translator implementation
│   ├── DataCommandTranslators.cs          # Hybrid TypeCollection
│   ├── IDataCommandTranslatorProvider.cs  # Provider interface
│   ├── DataCommandTranslatorProvider.cs   # Provider implementation
│   └── Logging/
│       └── DataCommandTranslatorProviderLog.cs # Source-generated logging
├── Expressions/
│   ├── IFilterExpression.cs               # Filter expression interface
│   ├── IProjectionExpression.cs           # Projection interface
│   └── ... (other expression types)
├── Operators/
│   ├── FilterOperators.cs                 # TypeCollection of filter operators
│   ├── LogicalOperator.cs                 # AND/OR EnhancedEnum
│   └── SortDirection.cs                   # ASC/DESC EnhancedEnum
└── Messages/
    └── DataCommandMessages.cs             # Structured messages
```

## Usage Examples

### Example 1: Simple Query Command

```csharp
using FractalDataWorks.Commands.Data.Abstractions;

public sealed class GetUsersCommand : DataCommandBase<IEnumerable<User>>
{
    public GetUsersCommand(string connectionName)
        : base(connectionName, "Users")
    {
    }

    // Optional: Add filter metadata
    public string? NameFilter { get; init; }
}

// Execute via DataGateway
var command = new GetUsersCommand("PrimaryDB")
{
    NameFilter = "John"
};

var result = await dataGateway.Execute<IEnumerable<User>>(command);
if (result.IsSuccess)
{
    foreach (var user in result.Value)
    {
        Console.WriteLine($"User: {user.Name}");
    }
}
```

### Example 2: Translator Registration (in Connection ServiceTypeOption)

```csharp
[ServiceTypeOption(typeof(ConnectionTypes), "MsSql")]
public sealed class MsSqlConnectionType : ConnectionTypeBase<...>
{
    public override void Configure(IConfiguration configuration)
    {
        // Register translator at runtime
        DataCommandTranslators.Register("MsSql", typeof(MsSqlDataCommandTranslator));
    }

    public override void Register(IServiceCollection services)
    {
        // Register translator in DI
        services.AddScoped<IDataCommandTranslator, MsSqlDataCommandTranslator>();

        // Register connection
        services.AddScoped<IDataConnection, MsSqlConnection>();
    }
}
```

### Example 3: Custom Translator Implementation

```csharp
public sealed class CustomApiTranslator : IDataCommandTranslator
{
    private readonly ILogger<CustomApiTranslator> _logger;

    public CustomApiTranslator(ILogger<CustomApiTranslator> logger)
    {
        _logger = logger;
    }

    public string DomainName => "CustomApi";

    public Task<IGenericResult<IConnectionCommand>> Translate(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Build custom API request from command
            var apiUrl = $"/api/{command.ContainerName}";

            // Add filters from metadata
            if (command.Metadata.TryGetValue("Filter", out var filter))
            {
                apiUrl += $"?filter={filter}";
            }

            // Create HTTP connection command
            var connectionCommand = new HttpConnectionCommand
            {
                Method = "GET",
                Url = apiUrl
            };

            return Task.FromResult(GenericResult<IConnectionCommand>.Success(connectionCommand));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to translate command for {Container}", command.ContainerName);
            return Task.FromResult(GenericResult<IConnectionCommand>.Failure(ex.Message));
        }
    }
}
```

## Best Practices

### 1. Translator Design

✅ **DO**: Keep translators focused on one domain (SQL, REST, etc.)
✅ **DO**: Use source-generated logging for translation operations
✅ **DO**: Return descriptive failure messages
✅ **DO**: Validate commands before translation

❌ **DON'T**: Mix multiple domain translations in one translator
❌ **DON'T**: Throw exceptions - use Result pattern
❌ **DON'T**: Use switch statements - use TypeCollections/polymorphism

### 2. Command Design

✅ **DO**: Keep commands simple and focused
✅ **DO**: Use metadata for optional/hint information
✅ **DO**: Provide typed generic variants when possible

❌ **DON'T**: Put translation logic in commands
❌ **DON'T**: Reference domain-specific types (SQL, REST) in commands

### 3. Registration

✅ **DO**: Use `[TypeOption]` for compile-time translator discovery
✅ **DO**: Use `DataCommandTranslators.Register()` for connection-provided translators
✅ **DO**: Register translators in ServiceTypeOption.Configure()

❌ **DON'T**: Register translators manually in Startup.cs
❌ **DON'T**: Skip DI registration

## Integration with Other Projects

### With FractalDataWorks.Services.Data (DataGateway)

```csharp
// DataGateway uses IDataCommandTranslatorProvider
public class DataGatewayService : IDataGateway
{
    private readonly IDataCommandTranslatorProvider _translatorProvider;

    public async Task<IGenericResult<T>> Execute<T>(IDataCommand command)
    {
        // 1. Get connection
        var connection = await _connectionProvider.GetConnection(command.ConnectionName);

        // 2. Get translator based on connection's DataStore
        var translator = _translatorProvider.GetTranslator(connection.DataStore.TranslatorType);

        // 3. Translate IDataCommand → IConnectionCommand
        var connectionCommand = await translator.Translate(command);

        // 4. Execute
        return await connection.Execute<T>(connectionCommand);
    }
}
```

### With FractalDataWorks.Data.Translators

```csharp
// Translator implementations in separate project
public class MsSqlDataCommandTranslator : IDataCommandTranslator
{
    public string DomainName => "MsSql";

    public Task<IGenericResult<IConnectionCommand>> Translate(...)
    {
        // SQL-specific translation logic
    }
}
```

### With FractalDataWorks.Services.Connections

```csharp
// Connections register their translators during startup
public class MsSqlConnectionType : ConnectionTypeBase<...>
{
    public override void Configure(IConfiguration configuration)
    {
        // Register MsSql translator
        DataCommandTranslators.Register("MsSql", typeof(MsSqlDataCommandTranslator));
    }
}
```

## Target Frameworks

- .NET Standard 2.0
- .NET 10.0

## Dependencies

**NuGet Packages:**
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions

**Project References:**
- FractalDataWorks.Commands.Abstractions - Base command interfaces
- FractalDataWorks.Collections - TypeCollection support
- FractalDataWorks.Results - Result pattern
- FractalDataWorks.Services.Connections.Abstractions - IConnectionCommand

## Related Projects

- **FractalDataWorks.Data.Translators** - Concrete translator implementations (MsSql, Rest, GraphQL)
- **FractalDataWorks.Services.Data** - DataGateway service that orchestrates translation and execution
- **FractalDataWorks.Services.Connections** - Connection implementations that execute translated commands
- **FractalDataWorks.Data.DataStores.Abstractions** - DataStore abstractions that identify translator type

## Contributing

When adding new translator types:

1. Create implementation in `FractalDataWorks.Data.Translators` project
2. Implement `IDataCommandTranslator` interface
3. Use `[TypeOption(typeof(DataCommandTranslators), "YourDomain")]` for compile-time discovery
4. OR register at runtime: `DataCommandTranslators.Register("YourDomain", typeof(YourTranslator))`
5. Register in DI in connection's ServiceTypeOption
6. Add source-generated logging
7. Write unit tests

---

**FractalDataWorks.Commands.Data.Abstractions** - Universal data command abstractions for write-once, run-anywhere data operations.
