# Final Naming and Generic Connection Architecture

## Naming Decision: FractalDataWorks.Commands.Data

### Why NOT FractalDataWorks.DataCommands

**Problem**: DataCommands as sibling to Commands creates confusion:
```
FractalDataWorks.Commands/          ← IGenericCommand lives here
FractalDataWorks.DataCommands/      ← IDataCommand lives here
```

**Issues**:
1. ❌ IDataCommand SHOULD implement IGenericCommand (they're not siblings!)
2. ❌ DataCommands should be submittable anywhere IGenericCommand is accepted
3. ❌ Inconsistent with existing architecture (Commands is already established)
4. ❌ Implies data commands are separate concept, not specialized commands

### Why FractalDataWorks.Commands.Data

**Correct Hierarchy**:
```
FractalDataWorks.Commands/                     ← Base command abstractions
    ├── IGenericCommand                        ← Base for ALL commands
    └── ... (existing command infrastructure)

FractalDataWorks.Commands.Data/                ← Data-specific commands
    ├── IDataCommand : IGenericCommand         ← Extends base command!
    ├── IDataCommand<TResult> : IDataCommand
    └── IDataCommand<TResult,TInput> : IDataCommand<TResult>

FractalDataWorks.Commands.Service/             ← Future: Service commands
FractalDataWorks.Commands.Workflow/            ← Future: Workflow commands
```

**Benefits**:
1. ✅ Clear hierarchy: Data commands ARE commands (inherit IGenericCommand)
2. ✅ Data commands submittable anywhere IGenericCommand is accepted
3. ✅ Parallel structure for future command types (Service, Workflow, etc.)
4. ✅ Consistent with existing Commands namespace
5. ✅ Clear intent: Commands.Data = "commands for data operations"

### Alternative: FractalDataWorks.Data.Commands

**Also valid hierarchy**:
```
FractalDataWorks.Data/                         ← All data-related abstractions
    ├── DataStores/                            ← Data storage
    ├── DataSets/                              ← Data sets
    └── Commands/                              ← Data commands
        ├── IDataCommand : IGenericCommand
        └── ...
```

**Comparison**:

| Aspect | Commands.Data | Data.Commands |
|--------|---------------|---------------|
| **Grouping** | By command type | By data domain |
| **Future commands** | Commands.Service, Commands.Workflow | Unclear where Service commands go |
| **Parallel with** | Commands namespace | Data.DataStores, Data.DataSets |
| **Discoverability** | All commands under Commands.* | Commands scattered across domains |
| **Intent** | Command-first thinking | Data-first thinking |

**RECOMMENDATION: `FractalDataWorks.Commands.Data`**

**Rationale**:
- Command types should be grouped together (Commands.*)
- Enables future: Commands.Service, Commands.Workflow, Commands.Integration
- Data commands extend IGenericCommand (clear hierarchy)
- Commands are the primary abstraction, data is the specialization

## Generic Connection Architecture: Connection<TTranslator>

### The Insight

**Your point**: Even as late as DataGateway, translator type can be specified in configuration.

**Current Problem**:
```csharp
// Connection doesn't know translator type at compile time
public class HttpConnection
{
    public async Task ExecuteAsync(IDataCommand command)
    {
        // Must look up translator by string name at runtime
        var translator = DataCommandTranslators.GetTranslator(_config.TranslatorLanguage);
        // No type safety!
    }
}
```

**Solution**: Make connection generic over translator type:
```csharp
public class HttpConnection<TTranslator> : ConnectionBase<HttpConfiguration>
    where TTranslator : IDataCommandTranslator
{
    private readonly TTranslator _translator;

    public HttpConnection(HttpConfiguration config, TTranslator translator)
        : base(config)
    {
        _translator = translator;  // Type-safe!
    }

    public async Task ExecuteAsync<TResult>(IDataCommand<TResult> command, CancellationToken ct)
    {
        // Translator is strongly typed - no dictionary lookup!
        var result = await _translator.TranslateAsync(command, GetContainerContext(), ct);
        // ...
    }
}
```

### Configuration Specifies Translator Type

**Configuration**:
```json
{
    "ConnectionName": "ProductApi",
    "ConnectionType": "Http",
    "TranslatorType": "GraphQL",  // Specifies TTranslator
    "BaseUrl": "https://api.example.com"
}
```

**DataGateway resolves at runtime**:
```csharp
public class DataGateway
{
    public async Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
        IDataCommand<TResult> command,
        string connectionName,
        CancellationToken ct)
    {
        // Get configuration
        var config = await _configService.GetConfigurationAsync<HttpConfiguration>(connectionName);

        // Resolve translator type from configuration
        var translatorType = ResolveTranslatorType(config.TranslatorType);
        // translatorType might be typeof(GraphQLTranslator)

        // Resolve connection with translator type
        var connectionType = typeof(HttpConnection<>).MakeGenericType(translatorType);
        var connection = (IDataConnection)_serviceProvider.GetRequiredService(connectionType);

        // Execute command
        return await connection.ExecuteAsync(command, ct);
    }
}
```

### Registration With Generic Translator

```csharp
[ServiceTypeOption(typeof(ConnectionTypes), "Http")]
public sealed class HttpConnectionType :
    ConnectionTypeBase<HttpConnection<RestTranslator>, HttpConfiguration, IHttpConnectionFactory>,
    IConnectionType
{
    public static HttpConnectionType Instance { get; } = new();

    private HttpConnectionType() : base(id: 5, name: "Http", category: "Web") { }

    public override void Register(IServiceCollection services)
    {
        // Register connection with EACH translator type
        services.AddScoped<HttpConnection<RestTranslator>>();
        services.AddScoped<HttpConnection<GraphQLTranslator>>();
        services.AddScoped<HttpConnection<GrpcTranslator>>();

        // Register translators
        services.AddSingleton<RestTranslator>();
        services.AddSingleton<GraphQLTranslator>();
        services.AddSingleton<GrpcTranslator>();

        // Shared infrastructure
        services.AddHttpClient("FractalDataWorks");
    }

    // Connection supports multiple translator types
    public override IReadOnlyList<Type> SupportedTranslatorTypes =>
    [
        typeof(RestTranslator),
        typeof(GraphQLTranslator),
        typeof(GrpcTranslator)
    ];
}
```

### Benefits of Generic Connection

1. ✅ **Type safety**: Translator type known at compile time (for specific instance)
2. ✅ **Performance**: No dictionary lookups, no string comparisons
3. ✅ **IntelliSense**: Connection knows translator's specific methods/properties
4. ✅ **Configuration-driven**: DataGateway resolves generic type from config
5. ✅ **Flexibility**: Same connection class, different translator types

## What About SqlCommandTranslator?

**Problem**: `SqlCommand` is a .NET Framework type (System.Data.SqlClient.SqlCommand)

**Options**:

### Option 1: Avoid Name Collision with Prefix

```csharp
// ✅ CORRECT: Prefix to avoid collision
public class TSqlCommandTranslator : IDataCommandTranslator { }
public class SqlKataCommandTranslator : IDataCommandTranslator { }

// In MsSql connection project:
FractalDataWorks.Services.Connections.MsSql/Translators/
    ├── TSqlCommandTranslator.cs           // Direct T-SQL
    └── SqlKataCommandTranslator.cs        // SQL Kata library
```

### Option 2: Domain-Specific Naming

```csharp
// ✅ CORRECT: Emphasize what it translates TO
public class TSqlTranslator : IDataCommandTranslator { }  // Translates TO T-SQL syntax
public class SqlKataTranslator : IDataCommandTranslator { } // Translates USING SQL Kata

// Clear intent - translates DataCommand → T-SQL
```

### Option 3: Use Namespace Qualification

```csharp
// Fully qualified avoids ambiguity
using FractalSqlCommand = FractalDataWorks.Commands.Data.SqlCommandTranslator;
using SystemSqlCommand = System.Data.SqlClient.SqlCommand;
```

**RECOMMENDATION: Option 2 (Domain-Specific Naming)**

Use names that describe WHAT the translator targets:
- `TSqlTranslator` - Generates T-SQL syntax
- `SqlKataTranslator` - Uses SQL Kata library to generate SQL
- `RestTranslator` - Generates REST/OData requests
- `GraphQLTranslator` - Generates GraphQL queries

Avoid "CommandTranslator" suffix - just use "Translator" (it's already in `Translators/` folder).

## Final Project Structure

```
FractalDataWorks.Commands/                      (Existing - base commands)
    ├── IGenericCommand.cs
    └── ... (existing command infrastructure)

FractalDataWorks.Commands.Data.Abstractions/    (NEW - data command abstractions)
    ├── Commands/
    │   ├── IDataCommand.cs                     (extends IGenericCommand)
    │   ├── IDataCommand{TResult}.cs
    │   ├── IDataCommand{TResult,TInput}.cs
    │   ├── DataCommandBase.cs
    │   └── DataCommands.cs (TypeCollection)
    ├── Operators/
    │   ├── FilterOperatorBase.cs
    │   └── FilterOperators.cs (TypeCollection)
    ├── Expressions/
    │   ├── IFilterExpression.cs
    │   ├── IProjectionExpression.cs
    │   └── ...
    └── Translators/
        ├── IDataCommandTranslator.cs
        └── DataCommandTranslators.cs (Hybrid collection)

FractalDataWorks.Commands.Data/                 (NEW - data command implementations)
    ├── Commands/
    │   ├── QueryCommand{T}.cs
    │   ├── InsertCommand{T}.cs
    │   └── ...
    ├── Operators/
    │   ├── EqualOperator.cs
    │   └── ...
    ├── Expressions/
    │   └── FilterExpression.cs
    └── Builders/
        └── LinqDataCommandBuilder.cs

FractalDataWorks.Services.Connections.MsSql/    (UPDATED - connection with translators)
    ├── MsSqlConnection{TTranslator}.cs         (Generic over translator!)
    ├── MsSqlConnectionType.cs
    ├── MsSqlConfiguration.cs
    └── Translators/
        ├── TSqlTranslator.cs                   (Direct T-SQL generation)
        └── SqlKataTranslator.cs                (SQL Kata library)

FractalDataWorks.Services.Connections.Http/     (UPDATED - connection with translators)
    ├── HttpConnection{TTranslator}.cs          (Generic over translator!)
    ├── HttpConnectionType.cs
    ├── HttpConfiguration.cs
    └── Translators/
        ├── RestTranslator.cs                   (REST/OData)
        ├── GraphQLTranslator.cs                (GraphQL)
        └── GrpcTranslator.cs                   (gRPC)

FractalDataWorks.Data.DataSets.Abstractions/    (Data context layer)
    ├── DataContext.cs
    └── IDataSet{T}.cs

FractalDataWorks.Data.DataSets/                 (Data context implementation)
    ├── DataSet{T}.cs
    └── FractalDataWorksQueryProvider.cs
```

## IDataCommand Extends IGenericCommand

**Critical**: DataCommands must extend IGenericCommand for polymorphism:

```csharp
namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Data command interface - extends IGenericCommand.
/// Can be submitted anywhere IGenericCommand is accepted!
/// </summary>
public interface IDataCommand : IGenericCommand
{
    string ContainerName { get; }
    IReadOnlyDictionary<string, object> Metadata { get; }
}

public interface IDataCommand<TResult> : IDataCommand
{
    // Generic version
}

public interface IDataCommand<TResult, TInput> : IDataCommand<TResult>
{
    TInput Data { get; }
}
```

**Usage**:
```csharp
// DataGateway accepts IGenericCommand (polymorphic!)
public async Task<IGenericResult> ExecuteAsync(IGenericCommand command)
{
    // Works with IDataCommand because it extends IGenericCommand
    if (command is IDataCommand dataCommand)
    {
        // Handle data command
    }
    else if (command is IServiceCommand serviceCommand)
    {
        // Handle service command
    }
}

// Connection-specific
public async Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
    IDataCommand<TResult> command)  // Specific to data commands
{
    // Type-safe data command execution
}
```

## Configuration Example with Generic Connection

```json
{
    "Connections": [
        {
            "ConnectionName": "ProductApi",
            "ConnectionType": "Http",
            "TranslatorType": "GraphQL",
            "BaseUrl": "https://api.example.com/graphql",
            "Timeout": "00:00:30"
        },
        {
            "ConnectionName": "ProductRest",
            "ConnectionType": "Http",
            "TranslatorType": "Rest",
            "BaseUrl": "https://api.example.com/odata",
            "Timeout": "00:00:30"
        },
        {
            "ConnectionName": "CustomerDb",
            "ConnectionType": "Sql",
            "TranslatorType": "SqlKata",
            "ConnectionString": "Server=...;Database=Customers;"
        },
        {
            "ConnectionName": "LegacyDb",
            "ConnectionType": "Sql",
            "TranslatorType": "TSql",
            "ConnectionString": "Server=...;Database=Legacy;"
        }
    ]
}
```

**DataGateway Resolution**:
```csharp
// User requests "ProductApi" connection
var config = GetConfiguration("ProductApi");
// ConnectionType = "Http", TranslatorType = "GraphQL"

// Resolve generic types
var translatorType = typeof(GraphQLTranslator);
var connectionType = typeof(HttpConnection<GraphQLTranslator>);

// Get from IoC
var connection = serviceProvider.GetRequiredService<HttpConnection<GraphQLTranslator>>();
// Type-safe! No runtime lookups!

// Execute
await connection.ExecuteAsync(command);
```

## Summary of Changes

### Naming
- ✅ Use `FractalDataWorks.Commands.Data` (not DataCommands)
- ✅ IDataCommand extends IGenericCommand (inheritance hierarchy)
- ✅ Translators named by target: TSqlTranslator, GraphQLTranslator (not CommandTranslator)

### Generic Connections
- ✅ Connection<TTranslator> for compile-time type safety
- ✅ Configuration specifies translator type
- ✅ DataGateway resolves generic types at runtime
- ✅ IoC registers Connection<T> for each supported translator

### Benefits
1. ✅ Data commands submittable anywhere IGenericCommand accepted
2. ✅ Type-safe translator resolution
3. ✅ Configuration-driven translator selection
4. ✅ Clear namespace hierarchy (Commands.Data, Commands.Service, Commands.Workflow)
5. ✅ Future-proof architecture

This is the final, correct architecture!
