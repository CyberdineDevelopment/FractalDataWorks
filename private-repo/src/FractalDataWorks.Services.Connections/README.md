# FractalDataWorks.Services.Connections

Unified connection management framework providing provider-based abstractions for external connections with automatic discovery and registration through source generation.

## Overview

FractalDataWorks.Services.Connections implements a provider-based model for managing connections to external systems (databases, APIs, message queues, etc.) with:

- Automatic discovery of connection types
- Provider-based connection resolution
- Configuration-driven connection creation
- State management and lifecycle control
- Source-generated type collections

## Features

- **GenericConnectionProvider** - Central provider for creating and managing connections
- **ConnectionTypes Collection** - Source-generated collection of all connection types
- **Configuration Integration** - Support for appsettings.json and configuration objects
- **State Management** - Built-in connection state tracking
- **Extensible Architecture** - Easy addition of new connection types

## Installation

```xml
<PackageReference Include="FractalDataWorks.Services.Connections" Version="1.0.0" />
```

## Quick Start

### 1. Register Services

```csharp
// Register the connection provider
services.AddSingleton<IGenericConnectionProvider, GenericConnectionProvider>();

// Register all discovered connection types
ConnectionTypes.Register(services);
```

### 2. Configure Connections

```json
{
  "Connections": {
    "MainDatabase": {
      "ConnectionType": "MsSql",
      "ConnectionString": "Server=localhost;Database=MyApp;Integrated Security=true;",
      "CommandTimeout": 30
    },
    "CacheServer": {
      "ConnectionType": "Redis",
      "ConnectionString": "localhost:6379",
      "DefaultDatabase": 0
    }
  }
}
```

### 3. Use Connections

```csharp
public class DataService
{
    private readonly IGenericConnectionProvider _connectionProvider;

    public DataService(IGenericConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IGenericResult<List<User>>> GetUsersAsync()
    {
        // Get connection by configuration name
        var connectionResult = await _connectionProvider.GetConnection("MainDatabase");
        if (connectionResult.Error)
            return GenericResult<List<User>>.Failure(connectionResult.Message);

        using var connection = connectionResult.Value;

        // Open connection
        var openResult = await connection.OpenAsync();
        if (openResult.Error)
            return GenericResult<List<User>>.Failure(openResult.Message);

        // Execute data command through the connection
        // Note: Data commands (IDataCommand) are executed through connections
        var dataCommand = new DataQueryCommand<User>
        {
            ConnectionName = "MainDatabase",
            Query = u => u.IsActive,
            CommandType = "Query"
        };
        return await connection.Execute<List<User>>(dataCommand);
    }
}
```

## Key Components

### GenericConnectionProvider

The main provider for connection management:

```csharp
public interface IGenericConnectionProvider
{
    Task<IGenericResult<IGenericConnection>> GetConnection(IConnectionConfiguration configuration);
    Task<IGenericResult<IGenericConnection>> GetConnection(int configurationId);
    Task<IGenericResult<IGenericConnection>> GetConnection(string configurationName);
}
```

### ConnectionTypes (Source-Generated)

Automatically discovered collection of connection types:

```csharp
// Generated static class
public static partial class ConnectionTypes
{
    public static IReadOnlyList<IConnectionType> All { get; }
    public static IConnectionType Name(string name);
    public static IConnectionType Id(int id);

    public static void Register(IServiceCollection services);
    public static void Register(IServiceCollection services,
        Action<ConnectionRegistrationOptions> configure);
}
```

### ConnectionRegistrationOptions

Custom registration configuration:

```csharp
ConnectionTypes.Register(services, options =>
{
    options.Configure("MsSql", (services, connectionType) =>
    {
        // Custom SQL Server configuration
        services.AddDbContext<MyDbContext>();
    });
});
```

## Connection Type Discovery

The framework uses source generation to automatically discover connection types:

1. **Compile-Time Discovery** - Source generator scans for ConnectionTypeBase derivatives
2. **Cross-Assembly Support** - Connection types can be in different assemblies
3. **Automatic Registration** - Each type self-registers its dependencies
4. **Zero Configuration** - Add package, get functionality

## Creating Custom Connection Types

To add a new connection type:

### 1. Create the Connection Type

```csharp
public sealed class MongoDbConnectionType :
    ConnectionTypeBase<IGenericConnection, MongoDbConfiguration, IMongoDbConnectionFactory>
{
    public static MongoDbConnectionType Instance { get; } = new();

    private MongoDbConnectionType() : base(10, "MongoDb", "NoSQL Databases") { }

    public override Type FactoryType => typeof(IMongoDbConnectionFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMongoDbConnectionFactory, MongoDbConnectionFactory>();
        services.AddScoped<MongoDbConnection>();
        services.AddScoped<MongoDbCommandTranslator>();
    }

    public override void Configure(IConfiguration configuration)
    {
        // Validate MongoDB-specific configuration
    }
}
```

### 2. Implement the Connection

```csharp
public class MongoDbConnection :
    ConnectionServiceBase<IConnectionCommand, MongoDbConfiguration, MongoDbConnection>
{
    public MongoDbConnection(ILogger<MongoDbConnection> logger, MongoDbConfiguration configuration)
        : base(logger, configuration)
    {
    }

    protected override async Task<IGenericResult> OpenCoreAsync()
    {
        // MongoDB connection logic
    }

    public override async Task<IGenericResult<T>> Execute<T>(IConnectionCommand command)
    {
        // Execute MongoDB commands
    }
}
```

### 3. Package and Deploy

The source generator automatically discovers the new type when the package is referenced.

## Connection Lifecycle

1. **Creation** - Provider creates connection via factory
2. **Configuration** - Configuration validated and applied
3. **Opening** - Connection.OpenAsync() called
4. **Usage** - Commands executed through connection
5. **Closing** - Connection.CloseAsync() for graceful shutdown
6. **Disposal** - Resources cleaned up

## State Management

Connections track their state through IConnectionState:

- `Created` - Initial state
- `Opening` - Transitioning to open
- `Open` - Ready for operations
- `Executing` - Command in progress
- `Closing` - Transitioning to closed
- `Closed` - Successfully closed
- `Broken` - Error state
- `Disposed` - Final state

## Command Architecture

The connection system uses a **two-tier command architecture** to separate connection management from data operations:

### Service Commands (IConnectionCommand)
Service-level commands for connection lifecycle management:
- **Purpose**: Manage connection creation, testing, and discovery
- **Key Properties**:
  - `CommandId` - Unique command identifier
  - `CreatedAt` - Timestamp
  - `CommandType` - Type of connection operation
- **Examples**: `CreateConnectionCommand`, `TestConnectionCommand`, `DiscoveryCommand`

### Data Commands (IDataCommand)
Data-level commands executed through established connections:
- **Purpose**: Execute operations against data sources
- **Key Properties**:
  - `ConnectionName` - Target connection identifier
  - `Query` - LINQ expression for filtering/querying
  - `CommandType` - Type of data operation
  - `TargetContainer` - Table/collection/resource name
  - `Metadata` - Additional command metadata
  - `Timeout` - Operation timeout
- **Examples**: `DataQueryCommand`, `DataInsertCommand`, `DataUpdateCommand`

### Usage Pattern
```csharp
// 1. Use service commands to manage connection lifecycle
var createCmd = new CreateConnectionCommand
{
    ConnectionName = "main-db",
    Configuration = dbConfig
};
var connResult = await connectionProvider.Execute(createCmd);

// 2. Use data commands through established connections
var queryCmd = new DataQueryCommand<User>
{
    ConnectionName = "main-db",
    Query = u => u.IsActive && u.Role == "Admin",
    CommandType = "Query"
};
var dataResult = await connection.Execute<List<User>>(queryCmd);
```

**Important**: Service commands manage the connection infrastructure, while data commands operate through the connections. Never confuse the two hierarchies.

## Logging

Comprehensive structured logging:

```csharp
// Connection operations
GenericConnectionProviderLog.GettingConnection(logger, connectionType);
GenericConnectionProviderLog.ConnectionCreated(logger, connectionType);
GenericConnectionProviderLog.ConnectionCreationFailed(logger, connectionType, error);
```

## Dependencies

- `FractalDataWorks.Services.Connections.Abstractions` - Connection interfaces
- `FractalDataWorks.ServiceTypes` - Service type discovery
- `FractalDataWorks.ServiceTypes.SourceGenerators` - Source generation
- `Microsoft.Extensions.Configuration` - Configuration support
- `Microsoft.Extensions.DependencyInjection` - DI integration
- `Microsoft.Extensions.Logging` - Logging

## Best Practices

1. **Always check connection state** before operations
2. **Use using statements** for proper disposal
3. **Handle connection failures** gracefully
4. **Configure appropriate timeouts** for operations
5. **Use configuration names** for connection resolution
6. **Implement retry logic** for transient failures

## Documentation

- [Connections Architecture](../../docs/Connections.md)
- [Developer Guide](../../docs/DeveloperGuide-ServiceSetup.md)
- [API Reference](https://docs.FractalDataWorks.com/api/connections)

## License

Copyright © FractalDataWorks Electric Cooperative. All rights reserved.