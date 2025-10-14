# Inverted Translator Architecture - ULTRATHINK

## The Problem You Identified

**My Original Design (WRONG)**:
```
Translators registered separately in IoC
    ↓
Connection types separate from translators
    ↓
Problem 1: One connection type (HTTP) can support multiple command languages (REST, GraphQL, gRPC)
Problem 2: Can't easily swap translator implementations (SQL Kata vs raw SQL)
Problem 3: Tight coupling to IoC registration
Problem 4: Future-proofing nightmare (every new translator = new ServiceTypeOption)
```

**Your Insight (CORRECT)**:
```
Connections BRING their own translators
    ↓
Translators live WITH connections (implementation detail)
    ↓
Configuration selects which translator language to use
    ↓
Benefit: Can use HttpClientFactory for ALL HTTP-based translators (REST, GraphQL, gRPC)
Benefit: Can swap translator implementations (T-SQL vs SQL Kata) without touching connection code
Benefit: New command languages just drop in (no registration ceremony)
```

## Real-World Scenario: HTTP Connection

**Your Example**: HTTP connection could speak multiple languages:

```
HTTP Connection (one physical connection)
    ├── REST Translator (OData-style queries)
    ├── GraphQL Translator (GraphQL queries)
    ├── gRPC Translator (Protocol Buffers)
    └── [Future] JSON-RPC Translator
    └── [Future] SOAP Translator (yes, really)

Configuration picks which one:
{
    "ConnectionName": "ProductApi",
    "ConnectionType": "Http",
    "BaseUrl": "https://api.example.com",
    "TranslatorLanguage": "GraphQL"  ← User chooses!
}
```

**Same HttpClientFactory, different command language!**

## Another Real-World Scenario: SQL Connection

```
SQL Connection (one physical connection)
    ├── T-SQL Translator (raw SQL Server syntax)
    ├── SQL Kata Translator (uses SQL Kata library)
    ├── Dapper Translator (uses Dapper)
    └── [Future] EF Core Translator (generates EF Core expressions)

Configuration picks:
{
    "ConnectionName": "CustomerDb",
    "ConnectionType": "Sql",
    "ConnectionString": "...",
    "TranslatorLanguage": "SqlKata"  ← User chooses implementation!
}
```

**Same connection, different query builder technology!**

## Corrected Architecture: Connections Own Translators

### 1. Translators Live With Connection Implementations

**New Project Structure**:
```
FractalDataWorks.Services.Connections.MsSql/
    ├── MsSqlConnection.cs
    ├── MsSqlConnectionType.cs
    ├── MsSqlConfiguration.cs
    └── Translators/
        ├── TSqlTranslator.cs           ← Direct T-SQL generation
        └── SqlKataTranslator.cs        ← Uses SQL Kata library

FractalDataWorks.Services.Connections.Http/
    ├── HttpConnection.cs
    ├── HttpConnectionType.cs
    ├── HttpConfiguration.cs
    └── Translators/
        ├── RestTranslator.cs           ← REST/OData
        ├── GraphQLTranslator.cs        ← GraphQL
        └── GrpcTranslator.cs           ← gRPC

FractalDataWorks.Services.Connections.File/
    ├── FileConnection.cs
    ├── FileConnectionType.cs
    ├── FileConfiguration.cs
    └── Translators/
        ├── CsvTranslator.cs
        ├── JsonTranslator.cs
        └── ParquetTranslator.cs
```

**Key Point**: Translators are NOT separate projects - they're part of connection implementation projects!

### 2. Connection Types Register Their Translators

```csharp
namespace FractalDataWorks.Services.Connections.Http;

[ServiceTypeOption(typeof(ConnectionTypes), "Http")]
public sealed class HttpConnectionType :
    ConnectionTypeBase<HttpConnection, HttpConfiguration, IHttpConnectionFactory>,
    IConnectionType
{
    public static HttpConnectionType Instance { get; } = new();

    private HttpConnectionType() : base(id: 5, name: "Http", category: "Web") { }

    public override void Register(IServiceCollection services)
    {
        // Register connection
        services.AddScoped<HttpConnection>();
        services.AddScoped<IHttpConnectionFactory, HttpConnectionFactory>();

        // Register HttpClientFactory (shared across all HTTP translators!)
        services.AddHttpClient("FractalDataWorks");

        // Register translators (connection brings its own!)
        DataCommandTranslators.Register("Rest", new RestTranslator());
        DataCommandTranslators.Register("GraphQL", new GraphQLTranslator());
        DataCommandTranslators.Register("Grpc", new GrpcTranslator());
    }

    // Connection knows which translators it supports
    public override IReadOnlyList<string> SupportedTranslators => ["Rest", "GraphQL", "Grpc"];
}
```

### 3. Abstract Translators Collection (Not ServiceTypeCollection)

**Instead of ServiceTypeCollection, use HYBRID pattern**:

```csharp
namespace FractalDataWorks.Data.Commands.Abstractions;

/// <summary>
/// Abstract collection of data command translators.
/// Combines compile-time discovery (TypeCollection) with runtime registration.
/// Connections register their translators at startup.
/// </summary>
[TypeCollection(typeof(DataCommandTranslatorBase), typeof(IDataCommandTranslator), typeof(DataCommandTranslators))]
public abstract partial class DataCommandTranslators : TypeCollectionBase<DataCommandTranslatorBase, IDataCommandTranslator>
{
    // Source generator creates compile-time discovered translators
    // (for translators defined with [TypeOption])

    // ALSO: Runtime registration for connection-provided translators
    private static readonly ConcurrentDictionary<string, IDataCommandTranslator> _runtimeTranslators = new();

    /// <summary>
    /// Register translator at runtime (called by connection types during registration).
    /// </summary>
    public static void Register(string name, IDataCommandTranslator translator)
    {
        _runtimeTranslators[name] = translator;
        DataCommandTranslatorsLog.TranslatorRegistered(Logger, name, translator.GetType().Name);
    }

    /// <summary>
    /// Get translator by name.
    /// Checks compile-time translators first, then runtime-registered translators.
    /// </summary>
    public static IDataCommandTranslator? GetTranslator(string name)
    {
        // Check compile-time discovered translators first
        var translator = GetByName(name);
        if (translator != null) return translator;

        // Check runtime-registered translators
        return _runtimeTranslators.TryGetValue(name, out var runtimeTranslator)
            ? runtimeTranslator
            : null;
    }

    /// <summary>
    /// Get all available translators (compile-time + runtime).
    /// </summary>
    public static IEnumerable<IDataCommandTranslator> GetAllTranslators()
    {
        return All().Concat(_runtimeTranslators.Values);
    }

    /// <summary>
    /// Get all translator names.
    /// </summary>
    public static IEnumerable<string> GetTranslatorNames()
    {
        return All().Select(t => t.Name).Concat(_runtimeTranslators.Keys);
    }
}
```

### 4. Configuration Includes Translator Selection

```csharp
namespace FractalDataWorks.Services.Connections.Abstractions;

public interface IConnectionConfiguration
{
    string ConnectionName { get; }
    string ConnectionType { get; }

    /// <summary>
    /// Which translator language to use for this connection.
    /// Connection type determines available translators.
    /// </summary>
    string TranslatorLanguage { get; set; }
}

// Example: HTTP configuration
public class HttpConfiguration : IConnectionConfiguration
{
    public string ConnectionName { get; set; } = "";
    public string ConnectionType => "Http";

    public string BaseUrl { get; set; } = "";
    public string TranslatorLanguage { get; set; } = "Rest"; // Default to REST

    // HTTP-specific settings
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
}

// Example: SQL configuration
public class MsSqlConfiguration : IConnectionConfiguration
{
    public string ConnectionName { get; set; } = "";
    public string ConnectionType => "Sql";

    public string ConnectionString { get; set; } = "";
    public string TranslatorLanguage { get; set; } = "TSql"; // Default to raw T-SQL

    // Could also be: "SqlKata", "Dapper", "EFCore"
}
```

### 5. Connection Provides Translator at Execution Time

```csharp
namespace FractalDataWorks.Services.Connections.Http;

public sealed class HttpConnection : ConnectionBase<HttpConfiguration>, IDataConnection
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpConfiguration _configuration;

    public HttpConnection(
        HttpConfiguration configuration,
        IHttpClientFactory httpClientFactory)
        : base(configuration)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
        IDataCommand<TResult> command,
        CancellationToken ct = default)
    {
        // Get translator based on configuration
        var translator = DataCommandTranslators.GetTranslator(_configuration.TranslatorLanguage);
        if (translator == null)
        {
            return GenericResult<TResult>.Failure(
                ConnectionMessages.TranslatorNotFound.WithData(_configuration.TranslatorLanguage));
        }

        // Translate command to HTTP request
        var translationResult = await translator.TranslateAsync(command, GetContainerContext(), ct);
        if (!translationResult.IsSuccess)
            return GenericResult<TResult>.Failure(translationResult.Message);

        var httpRequest = (HttpConnectionCommand)translationResult.Value;

        // Execute HTTP request (shared HttpClient for all translators!)
        var httpClient = _httpClientFactory.CreateClient("FractalDataWorks");
        var response = await httpClient.SendAsync(httpRequest.BuildHttpRequestMessage(), ct);

        // Parse response
        var content = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<TResult>(content);

        return GenericResult<TResult>.Success(result!);
    }
}
```

## Benefits of Inverted Architecture

### 1. One Physical Connection, Multiple Command Languages

```csharp
// Same HTTP connection, different translator
var restConfig = new HttpConfiguration
{
    ConnectionName = "ProductApi",
    BaseUrl = "https://api.example.com",
    TranslatorLanguage = "Rest"  // OData queries
};

var graphqlConfig = new HttpConfiguration
{
    ConnectionName = "ProductGraphQL",
    BaseUrl = "https://api.example.com/graphql",
    TranslatorLanguage = "GraphQL"  // GraphQL queries
};

// Both use HttpConnection and HttpClientFactory
// Just different translators!
```

### 2. Swap Translator Implementation Without Touching Connection

```csharp
// Start with raw T-SQL
var sqlConfig = new MsSqlConfiguration
{
    ConnectionName = "CustomerDb",
    ConnectionString = "...",
    TranslatorLanguage = "TSql"
};

// Later: Switch to SQL Kata (no code changes, just config!)
var sqlConfig = new MsSqlConfiguration
{
    ConnectionName = "CustomerDb",
    ConnectionString = "...",
    TranslatorLanguage = "SqlKata"  // Now using SQL Kata
};

// Connection code doesn't change!
// Translator is swapped at runtime based on config
```

### 3. Future-Proof: New Languages Drop In

```csharp
// Today: REST and GraphQL
// Tomorrow: gRPC drops in

namespace FractalDataWorks.Services.Connections.Http.Translators;

public class GrpcTranslator : DataCommandTranslatorBase
{
    // New translator implementation
}

// Register in HttpConnectionType
public override void Register(IServiceCollection services)
{
    // ... existing registrations
    DataCommandTranslators.Register("Grpc", new GrpcTranslator());  // Just add it!
}

// User config:
{
    "TranslatorLanguage": "Grpc"  // New option available!
}
```

### 4. Shared Infrastructure (HttpClientFactory, Connection Pools)

```csharp
// ALL HTTP-based translators share the same HttpClientFactory
services.AddHttpClient("FractalDataWorks")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Shared configuration: connection pooling, timeouts, etc.
    });

// REST translator uses it
// GraphQL translator uses it
// gRPC translator uses it
// Future translators use it

// ONE HttpClient configuration, ALL HTTP translators benefit!
```

## Naming and Hierarchy

### Option 1: Commands Under Data (Follows Existing Pattern)

```
FractalDataWorks.Data.Commands.Abstractions
FractalDataWorks.Data.Commands
FractalDataWorks.Data.DataSets.Abstractions
FractalDataWorks.Data.DataSets

Pro: Consistent with FractalDataWorks.Data.DataStores
Pro: Commands are data-specific operations
Con: Verbose (Data.Commands)
```

### Option 2: Commands As Top-Level Concern

```
FractalDataWorks.Commands.Data.Abstractions
FractalDataWorks.Commands.Data
FractalDataWorks.Data.DataSets.Abstractions
FractalDataWorks.Data.DataSets

Pro: If commands expand beyond data (future: service commands, workflow commands)
Pro: Shorter when importing: using FractalDataWorks.Commands.Data;
Con: Inconsistent with existing Data.* naming
```

### Option 3: Keep DataCommands But Shorten

```
FractalDataWorks.DataCommands.Abstractions
FractalDataWorks.DataCommands
FractalDataWorks.DataSets.Abstractions
FractalDataWorks.DataSets

Pro: Clearest naming (DataCommands = commands for data)
Pro: Shorter than Data.Commands
Pro: Parallel with DataSets, DataStores
Con: Top-level namespace gets crowded
```

### RECOMMENDATION: Option 3 (DataCommands)

**Rationale**:
- **Clearest intent**: "DataCommands" immediately tells you what it is
- **Parallel naming**: DataCommands, DataSets, DataStores all at same level
- **Shorter imports**: `using FractalDataWorks.DataCommands;`
- **Future-proof**: If we need other command types, we can add `ServiceCommands`, `WorkflowCommands`, etc.

**Final Structure**:
```
FractalDataWorks.DataCommands.Abstractions/
    ├── Commands/          (IDataCommand, QueryCommand, etc.)
    ├── Operators/         (FilterOperators, LogicalOperator, etc.)
    ├── Expressions/       (IFilterExpression, IProjectionExpression, etc.)
    └── Translators/       (IDataCommandTranslator - abstract only)

FractalDataWorks.DataCommands/
    ├── Commands/          (QueryCommand<T> implementation)
    ├── Operators/         (EqualOperator, ContainsOperator, etc.)
    ├── Expressions/       (FilterExpression implementation)
    └── Builders/          (LinqDataCommandBuilder)

FractalDataWorks.Services.Connections.MsSql/
    ├── MsSqlConnection.cs
    ├── MsSqlConnectionType.cs
    └── Translators/       ← Translators live here!
        ├── TSqlTranslator.cs
        └── SqlKataTranslator.cs

FractalDataWorks.Services.Connections.Http/
    ├── HttpConnection.cs
    ├── HttpConnectionType.cs
    └── Translators/       ← Translators live here!
        ├── RestTranslator.cs
        ├── GraphQLTranslator.cs
        └── GrpcTranslator.cs
```

## Complete Flow with Inverted Architecture

```
User Code
    │
    ├─ Configuration loaded
    │  {
    │      "ConnectionName": "ProductApi",
    │      "ConnectionType": "Http",
    │      "TranslatorLanguage": "GraphQL"  ← User picks translator
    │  }
    │
    ├─ _context.Products.Where(p => p.IsActive)
    │  ↓
    │  LinqDataCommandBuilder.FromQueryable(query)
    │  ↓
    │  QueryCommand<Product> { Filter = ... }
    │
    ├─ await _context.ExecuteAsync(command)
    │  ↓
    │  Gets connection: HttpConnection
    │  ↓
    │  HttpConnection looks at configuration.TranslatorLanguage = "GraphQL"
    │  ↓
    │  Gets translator: DataCommandTranslators.GetTranslator("GraphQL")
    │  ↓
    │  GraphQLTranslator.TranslateAsync(command)
    │  ↓
    │  Builds: HttpConnectionCommand {
    │      Query = "query { products(where: { isActive: true }) { id name } }"
    │  }
    │  ↓
    │  HttpConnection executes via HttpClientFactory
    │  ↓
    │  Returns: IGenericResult<IEnumerable<Product>>
    │
    └─ result.Value (typed!)
```

## Migration Path: Adding SQL Kata Support

**Step 1**: Create translator in connection project
```csharp
// FractalDataWorks.Services.Connections.MsSql/Translators/SqlKataTranslator.cs

public class SqlKataTranslator : DataCommandTranslatorBase
{
    private readonly SqlKata.Compilers.SqlServerCompiler _compiler = new();

    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext context,
        CancellationToken ct)
    {
        var query = new SqlKata.Query(command.ContainerName);

        // Use SQL Kata fluent API
        if (command is QueryCommand queryCmd && queryCmd.Filter != null)
        {
            foreach (var condition in queryCmd.Filter.Conditions)
            {
                query.Where(condition.PropertyName, condition.Operator.SqlOperator, condition.Value);
            }
        }

        // Compile to SQL
        var compiled = _compiler.Compile(query);

        return GenericResult<IConnectionCommand>.Success(new SqlConnectionCommand
        {
            SqlText = compiled.Sql,
            Parameters = compiled.NamedBindings
        });
    }
}
```

**Step 2**: Register in connection type
```csharp
public override void Register(IServiceCollection services)
{
    services.AddScoped<MsSqlConnection>();

    // Register BOTH translators
    DataCommandTranslators.Register("TSql", new TSqlTranslator());
    DataCommandTranslators.Register("SqlKata", new SqlKataTranslator());  // New!
}

public override IReadOnlyList<string> SupportedTranslators => ["TSql", "SqlKata"];
```

**Step 3**: User picks in configuration
```json
{
    "ConnectionName": "CustomerDb",
    "ConnectionType": "Sql",
    "ConnectionString": "...",
    "TranslatorLanguage": "SqlKata"
}
```

**Done!** No changes to:
- Connection code
- Command definitions
- User's LINQ queries
- DataContext
- Anything else

**Just added a new translator and updated config.**

## Summary: Key Changes from Original Design

| Aspect | Original Design | Inverted Design |
|--------|----------------|-----------------|
| **Translator Location** | Separate project | Inside connection projects |
| **Registration** | ServiceTypeCollection | Runtime registration by connection |
| **Coupling** | Loose (IoC-based) | Tight (connection owns translators) |
| **Configuration** | Not configurable | User picks translator language |
| **Future-proofing** | New translator = new project | New translator = new class in connection |
| **Shared Infrastructure** | Each translator separate | Connection manages shared resources |
| **Example** | SqlTranslator (separate) | MsSql/Translators/TSqlTranslator.cs |

## Conclusion

Your insight is spot-on: **Connections should bring their own translators**.

Benefits:
1. ✅ One connection type, multiple command languages (HTTP: REST/GraphQL/gRPC)
2. ✅ Swap implementations easily (T-SQL vs SQL Kata)
3. ✅ Shared infrastructure (HttpClientFactory for all HTTP translators)
4. ✅ Configuration-driven selection
5. ✅ Future-proof (new translators just drop in)
6. ✅ Simpler mental model (translator is part of connection, not separate concern)

**Recommended naming**: `FractalDataWorks.DataCommands.Abstractions` - clean, parallel with DataSets/DataStores.

**Translator location**: `FractalDataWorks.Services.Connections.{Type}/Translators/` - part of connection implementation.

This is a much better architecture!
