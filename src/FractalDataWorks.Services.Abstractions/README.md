# FractalDataWorks.Services.Abstractions

## Overview

FractalDataWorks.Services.Abstractions defines the complete contract for the ServiceType architecture - a pattern for building auto-discoverable, self-registering services with unified command interfaces. This is the definitive guide for where every component belongs in the architecture.

## Service Architecture Components

### 1. SERVICE INTERFACES (Goes in: Service.Abstractions)

Services provide unified command execution across different providers (SQL, HTTP, MongoDB, etc.).

```csharp
/// <summary>
/// Base interface for all FractalDataWorks services
/// </summary>
public interface IFractalService
{
    string Id { get; }
    string ServiceType { get; }
    bool IsAvailable { get; }
}

/// <summary>
/// Generic service interface for command execution
/// </summary>
public interface IFractalService<TCommand> : IFractalService
    where TCommand : ICommand
{
    Task<IFractalResult<TResult>> Execute<TResult>(TCommand command, CancellationToken cancellationToken = default);
}
```

**RULE**: Service interfaces define the contract but contain NO implementation.

### 2. COMMAND INTERFACES (Goes in: Service.Abstractions)

Commands provide universal interfaces that work across all service implementations.

```csharp
/// <summary>
/// Universal command interface with audit fields
/// </summary>
public interface ICommand
{
    Guid CommandId { get; }
    Guid CorrelationId { get; }
    DateTimeOffset Timestamp { get; }
    IEnumerable<string> Validate();
}

/// <summary>
/// Universal data command - same interface for SQL, MongoDB, REST, GraphQL
/// </summary>
public interface IDataCommand : ICommand
{
    string CommandType { get; }    // "Query", "Insert", "Update", "Delete", "BulkInsert"
    string EntityName { get; }     // Table/Collection/Endpoint name
    IReadOnlyDictionary<string, object> Parameters { get; }
    IReadOnlyDictionary<string, object> Filters { get; }   // WHERE conditions
    IReadOnlyDictionary<string, object> Values { get; }    // SET/INSERT values
}
```

**RULE**: Commands define WHAT to do, not HOW to do it. Each service translates commands to provider-specific operations.

### 3. SERVICETYPE ARCHITECTURE (Goes in: ServiceTypes package)

ServiceTypes provide metadata and DI registration for auto-discovery.

```csharp
/// <summary>
/// Base ServiceType - all service types inherit from this
/// </summary>
public abstract class ServiceTypeBase<TService, TConfiguration, TFactory> : IServiceType
    where TService : class, IFractalService
    where TConfiguration : class, IFractalConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    protected ServiceTypeBase(int id, string name, string category)
    {
        Id = id;
        Name = name;
        Category = category;
    }

    public int Id { get; }
    public string Name { get; }
    public string Category { get; }

    /// <summary>
    /// Register this service type's dependencies with DI
    /// </summary>
    public abstract void Register(IServiceCollection services);

    /// <summary>
    /// Configure this service type
    /// </summary>
    public abstract void Configure(IConfiguration configuration);

    public abstract Type FactoryType { get; }
}
```

**RULE**: ServiceTypes handle metadata and DI registration. They live in implementation packages, not abstractions.

### 4. CONNECTION TYPES (Goes in: Connections.MsSql, Connections.PostgreSql, etc.)

Specialized ServiceTypes for data connections.

```csharp
/// <summary>
/// SQL Server connection type - auto-discovered by source generator
/// </summary>
public sealed class MsSqlConnectionType : ConnectionTypeBase<IMsSqlConnection, MsSqlConfiguration, IMsSqlConnectionFactory>
{
    public static MsSqlConnectionType Instance { get; } = new();

    private MsSqlConnectionType() : base(1, "MsSql", "Database Connections") { }

    public override Type FactoryType => typeof(IMsSqlConnectionFactory);

    public override void Register(IServiceCollection services)
    {
        // Register the factory
        services.AddScoped<IMsSqlConnectionFactory, MsSqlConnectionFactory>();

        // Register the command translator (provider-specific)
        services.AddScoped<MsSqlCommandTranslator>();

        // Register the expression translator (provider-specific)
        services.AddScoped<MsSqlExpressionTranslator>();

        // Register any other provider-specific services
        services.AddScoped<MsSqlBulkOperations>();
    }
}
```

**RULE**: Each connection package contains its own ConnectionType. Adding the package automatically makes it discoverable.

### 5. TRANSLATORS (Goes in: Provider-specific packages)

Translators convert universal commands to provider-specific operations.

```csharp
/// <summary>
/// Translates IDataCommand to SQL Server T-SQL
/// </summary>
public class MsSqlCommandTranslator : ICommandTranslator
{
    public string Translate(IDataCommand command)
    {
        return command.CommandType switch
        {
            "Query" => $"SELECT * FROM {command.EntityName} WHERE {BuildWhereClause(command.Filters)}",
            "Insert" => $"INSERT INTO {command.EntityName} ({BuildColumns(command.Values)}) VALUES ({BuildValues(command.Values)})",
            "Update" => $"UPDATE {command.EntityName} SET {BuildSetClause(command.Values)} WHERE {BuildWhereClause(command.Filters)}",
            "Delete" => $"DELETE FROM {command.EntityName} WHERE {BuildWhereClause(command.Filters)}",
            "BulkInsert" => BuildBulkInsert(command),
            _ => throw new NotSupportedException($"Command type {command.CommandType}")
        };
    }
}

/// <summary>
/// Translates IDataCommand to MongoDB operations
/// </summary>
public class MongoCommandTranslator : ICommandTranslator
{
    public BsonDocument Translate(IDataCommand command)
    {
        return command.CommandType switch
        {
            "Query" => new BsonDocument("find", command.EntityName).Add("filter", BuildMongoFilter(command.Filters)),
            "Insert" => new BsonDocument("insert", command.EntityName).Add("documents", BuildMongoDocument(command.Values)),
            "Update" => new BsonDocument("update", command.EntityName).Add("updates", BuildMongoUpdate(command.Values, command.Filters)),
            // etc...
        };
    }
}
```

**RULE**: Each provider implements its own translator. Same command interface, different output.

### 6. FACTORIES (Goes in: Provider-specific packages)

Factories create service instances with proper dependencies.

```csharp
/// <summary>
/// Creates SQL Server connection instances
/// </summary>
public class MsSqlConnectionFactory : IConnectionFactory<IMsSqlConnection, MsSqlConfiguration>
{
    private readonly MsSqlCommandTranslator _translator;
    private readonly ILogger<MsSqlConnection> _logger;

    public MsSqlConnectionFactory(MsSqlCommandTranslator translator, ILogger<MsSqlConnection> logger)
    {
        _translator = translator;
        _logger = logger;
    }

    public async Task<IMsSqlConnection> Create(MsSqlConfiguration config, CancellationToken ct = default)
    {
        return new MsSqlConnection(config, _translator, _logger);
    }
}
```

**RULE**: Factories use DI to inject translators and other dependencies into service instances.

### 7. SERVICE IMPLEMENTATIONS (Goes in: Provider-specific packages)

The actual service that executes commands using the translator.

```csharp
/// <summary>
/// SQL Server connection implementation
/// </summary>
public class MsSqlConnection : IMsSqlConnection
{
    private readonly MsSqlConfiguration _config;
    private readonly MsSqlCommandTranslator _translator;
    private readonly ILogger _logger;

    public async Task<IFractalResult<TResult>> Execute<TResult>(IDataCommand command, CancellationToken ct = default)
    {
        // 1. Translate universal command to SQL
        var sql = _translator.Translate(command);

        // 2. Execute SQL
        using var connection = new SqlConnection(_config.ConnectionString);
        var result = await connection.QueryAsync<TResult>(sql, command.Parameters);

        // 3. Return universal result
        return FractalResult.Success(result);
    }
}
```

**RULE**: Services use their translator to convert universal commands to provider-specific operations.

### 8. COLLECTIONS (Goes in: Domain packages like Connections)

ServiceType collections provide auto-discovery and registration.

```csharp
/// <summary>
/// Auto-generated collection of all connection types
/// </summary>
[ServiceTypeCollection("IConnectionType", "ConnectionTypes")]
public static partial class ConnectionTypes
{
    /// <summary>
    /// Register all discovered connection types
    /// </summary>
    public static void Register(IServiceCollection services)
    {
        foreach (var connectionType in All)  // All is source-generated
        {
            connectionType.Register(services);
        }
    }

    /// <summary>
    /// Register with custom configuration
    /// </summary>
    public static void Register(IServiceCollection services, Action<ConnectionRegistrationOptions> configure)
    {
        var options = new ConnectionRegistrationOptions();
        configure(options);

        foreach (var connectionType in All)
        {
            connectionType.Register(services);

            // Apply custom configuration if specified
            if (options.CustomConfigurations.TryGetValue(connectionType.Name, out var customConfig))
            {
                customConfig(services, connectionType);
            }
        }
    }
}
```

**RULE**: Collections live in domain packages and are generated by source generators.

### 9. PROVIDERS (Goes in: Domain packages like Connections)

Providers offer runtime service resolution using the collections.

```csharp
/// <summary>
/// Runtime connection provider
/// </summary>
public class ConnectionProvider : IConnectionProvider
{
    private readonly IConnectionFactoryProvider _factoryProvider;
    private readonly IConfiguration _configuration;

    public async Task<IFdwConnection> GetConnection(string connectionName)
    {
        // 1. Look up configuration
        var config = _configuration.GetSection($"Connections:{connectionName}");
        var typeName = config["Type"]; // e.g., "MsSql"

        // 2. Find connection type from generated collection
        var connectionType = ConnectionTypes.Name(typeName);

        // 3. Get factory and create connection
        var factory = _factoryProvider.GetFactory(connectionType);
        return await factory.Create(config);
    }
}
```

**RULE**: Providers use generated collections for runtime resolution.

## Component Placement Rules

### ✅ ABSTRACTIONS Package Contains:
- `IFractalService` and related interfaces
- `ICommand`, `IDataCommand` and command interfaces
- `IServiceFactory` and factory interfaces
- `ICommandTranslator` interfaces
- **NO** implementations, **NO** ServiceTypes

### ✅ SERVICETYPES Package Contains:
- `ServiceTypeBase<TService, TConfiguration, TFactory>`
- `ServiceTypeCollectionBase<T>`
- Source generator attributes
- **NO** concrete ServiceTypes (those go in implementation packages)

### ✅ DOMAIN Packages (Connections, Authentication, etc.) Contains:
- ServiceType collections (`ConnectionTypes`)
- Domain providers (`ConnectionProvider`)
- Collection registration methods
- **NO** concrete implementations

### ✅ IMPLEMENTATION Packages (Connections.MsSql, Authentication.AzureEntra, etc.) Contains:
- Concrete ServiceTypes (`MsSqlConnectionType`)
- Service implementations (`MsSqlConnection`)
- Translators (`MsSqlCommandTranslator`)
- Factories (`MsSqlConnectionFactory`)
- Configurations (`MsSqlConfiguration`)

## Auto-Discovery Flow

```
1. Add package reference to MsSql package
   └── Contains MsSqlConnectionType.Instance

2. Source generator scans all referenced assemblies
   └── Finds MsSqlConnectionType inheriting from ConnectionTypeBase

3. Generates ConnectionTypes.All collection
   └── Includes MsSqlConnectionType.Instance automatically

4. Call ConnectionTypes.Register(services)
   └── Loops through All, calls each connectionType.Register(services)

5. MsSqlConnectionType.Register() called
   └── Registers MsSqlConnectionFactory, MsSqlCommandTranslator, etc.

6. Runtime: ConnectionProvider.GetConnection("primary-db")
   └── Uses generated collection to find and resolve connection
```

## Universal Command Pattern

The beauty of this architecture is the universal command interface:

```csharp
// Same command works with ANY connection type
var command = new QueryCommand("Users", filters: new { Status = "Active" });

// SQL Server - translates to: SELECT * FROM Users WHERE Status = @Status
var sqlUsers = await sqlConnection.Execute<List<User>>(command);

// MongoDB - translates to: db.Users.find({ Status: "Active" })
var mongoUsers = await mongoConnection.Execute<List<User>>(command);

// REST API - translates to: GET /api/users?Status=Active
var restUsers = await restConnection.Execute<List<User>>(command);
```

**KEY INSIGHT**: Commands define WHAT to do. Translators define HOW to do it for each provider.

## Source Generator Requirements

For auto-discovery to work, packages must reference:

```xml
<ProjectReference Include="FractalDataWorks.ServiceTypes.SourceGenerators"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

The source generator:
1. Finds classes with `[ServiceTypeCollection]` attribute
2. Scans all assemblies for types inheriting from the specified base type
3. Generates collections with `.All`, `.Name()`, `.Id()` methods
4. Creates high-performance lookup methods using FrozenDictionary

## Summary

| Component | Package Location | Purpose |
|-----------|-----------------|---------|
| Service Interfaces | Services.Abstractions | Define contracts |
| Command Interfaces | Services.Abstractions | Universal command API |
| ServiceTypeBase | ServiceTypes | Base class for metadata |
| Collections | Domain packages | Auto-discovery and registration |
| Providers | Domain packages | Runtime resolution |
| ServiceTypes | Implementation packages | Metadata + DI registration |
| Services | Implementation packages | Actual functionality |
| Translators | Implementation packages | Provider-specific conversion |
| Factories | Implementation packages | Service creation |

This architecture enables:
- ✅ **Auto-discovery** - Add package, get functionality
- ✅ **Universal APIs** - Same command interface everywhere
- ✅ **Provider-specific optimization** - Each translator optimizes for its provider
- ✅ **Self-registration** - No manual DI configuration
- ✅ **Type safety** - Compile-time validation
- ✅ **Performance** - Generated collections use FrozenDictionary