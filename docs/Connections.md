# FractalDataWorks.Services.Connections Documentation

## Overview

The FractalDataWorks.Services.Connections library provides a unified abstraction for managing external connections (databases, APIs, message queues, etc.) within the service framework. It implements a provider-based model with automatic discovery and registration of connection types through source generation.

## Core Components

### IGenericConnection

The base connection interface that all connections must implement:

```csharp
// Located in: Services.Connections.Abstractions/IGenericConnection.cs
public interface IGenericConnection : IDisposable, IGenericService
{
    // Inherits from IGenericService for consistent service patterns
}
```

### GenericConnectionProvider

The central provider for creating and managing connections:

```csharp
public sealed class GenericConnectionProvider : IGenericConnectionProvider
{
    public async Task<IGenericResult<IGenericConnection>> GetConnection(IConnectionConfiguration configuration);
    public async Task<IGenericResult<IGenericConnection>> GetConnection(int configurationId);
    public async Task<IGenericResult<IGenericConnection>> GetConnection(string configurationName);
}
```

**Features:**
- Configuration-based connection creation
- Automatic factory resolution via ConnectionTypes
- Support for multiple configuration sources (objects, IDs, names)
- Comprehensive error handling and logging
- Integration with appsettings.json

### ConnectionTypes (Source-Generated)

Static class generated from all ConnectionTypeBase implementations:

```csharp
[ServiceTypeCollection("IConnectionType", "ConnectionTypes")]
public static partial class ConnectionTypes
{
    // Generated properties and methods:
    public static IReadOnlyList<IConnectionType> All { get; }
    public static IConnectionType Name(string name);
    public static IConnectionType Id(int id);

    // Registration methods:
    public static void Register(IServiceCollection services);
    public static void Register(IServiceCollection services, Action<ConnectionRegistrationOptions> configure);
}
```

The source generator automatically discovers all types inheriting from `ConnectionTypeBase` across all referenced assemblies.

### ConnectionRegistrationOptions

Configuration for custom connection type registration:

```csharp
public class ConnectionRegistrationOptions
{
    public Dictionary<string, Action<IServiceCollection, IConnectionType>> CustomConfigurations { get; }

    public void Configure(string connectionTypeName, Action<IServiceCollection, IConnectionType> configure)
    {
        CustomConfigurations[connectionTypeName] = configure;
    }
}
```

## Logging Infrastructure

### GenericConnectionProviderLog

Structured logging for connection operations:

- `GettingConnection`: Connection creation attempts
- `ConnectionCreated`: Successful connection creation
- `ConnectionCreationFailed`: Creation failures with error details
- `UnknownConnectionType`: Invalid connection type requests
- `NoFactoryRegistered`: Missing factory registrations
- `ConfigurationSectionNotFound`: Missing configuration sections

All logging uses high-performance LoggerMessage delegates for minimal overhead.

## Connection Type Discovery

The framework uses source generation to automatically discover connection types:

1. **Attribute-Based Discovery**: Classes marked with attributes are discovered
2. **Inheritance-Based Discovery**: All `ConnectionTypeBase` derivatives are found
3. **Cross-Assembly Support**: Connection types can be in different assemblies
4. **Compile-Time Generation**: All discovery happens at compile time

## Progressive Constraint Hierarchy

The Connections library follows the framework's progressive constraint pattern:

### Connection Abstractions
```csharp
// Base connection service with domain-specific constraints
public abstract class ConnectionServiceBase<TCommand, TConfiguration, TService>
    where TCommand : IConnectionCommand  // Must be connection-specific command
    where TConfiguration : class, IConnectionConfiguration  // Connection configuration
    where TService : class  // Flexible service type

// Connection type with full constraints
public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IGenericConnection  // Must implement IGenericConnection
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
```

### Concrete Implementations
```csharp
// Example: MsSql implementation (all concrete types)
public class MsSqlConnection : ConnectionServiceBase<MsSqlCommand, MsSqlConfiguration, MsSqlConnection>
{
    // No generics at implementation level - full type safety
}
```

This ensures:
- Connection services must use connection-specific commands
- All connections implement IGenericConnection
- Type safety increases from abstraction to implementation

## Registration Process

### Automatic Registration

```csharp
// In Startup.cs or Program.cs
services.AddConnections(); // Extension method that calls ConnectionTypes.Register
```

### Custom Registration

```csharp
ConnectionTypes.Register(services, options =>
{
    options.Configure("MsSql", (services, connectionType) =>
    {
        // Custom SQL Server configuration
        services.AddDbContext<MyDbContext>();
    });

    options.Configure("Redis", (services, connectionType) =>
    {
        // Custom Redis configuration
        services.AddStackExchangeRedisCache();
    });
});
```

## Configuration Integration

### appsettings.json Structure

```json
{
  "Connections": {
    "MainDatabase": {
      "ConnectionType": "MsSql",
      "ConnectionString": "Server=...;Database=...",
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

### Configuration Binding

The provider automatically:
1. Loads the configuration section
2. Determines the connection type
3. Binds to the appropriate configuration class
4. Validates the configuration
5. Creates the connection using the registered factory

## Factory Resolution

The provider uses a multi-step resolution process:

1. **Type Lookup**: Find connection type in ConnectionTypes collection
2. **Factory Resolution**: Get factory from DI container
3. **Instance Creation**: Use factory to create connection
4. **State Management**: Track connection state transitions

## Error Handling

All operations return `IGenericResult<T>` with detailed error information:

- Configuration validation failures
- Missing connection types
- Unregistered factories
- Connection creation exceptions
- Timeout and network errors

## Extension Methods

### ConnectionServiceExtensions

Provides fluent registration APIs:

```csharp
public static class ConnectionServiceExtensions
{
    public static IServiceCollection AddConnections(this IServiceCollection services);
    public static IServiceCollection AddConnectionProvider(this IServiceCollection services);
    public static IServiceCollection AddConnectionType<T>(this IServiceCollection services)
        where T : IConnectionType;
}
```

## Thread Safety

- `ConnectionTypes` static class is thread-safe
- `GenericConnectionProvider` is thread-safe for all operations
- Individual connections maintain their own thread safety
- Factory operations are thread-safe

## Performance Considerations

- Connection type lookups use dictionary lookups (O(1))
- Factory resolution leverages DI container caching
- Logging uses high-performance structured logging
- Source generation eliminates runtime reflection

## Best Practices

1. **Register all connection types at startup**: Use `ConnectionTypes.Register()`
2. **Use configuration names**: Reference connections by name from appsettings
3. **Implement proper disposal**: Connections implement IDisposable
4. **Handle connection states**: Check IsAvailable before operations
5. **Use async methods**: All operations are async-first

## Integration with MsSql

The MsSql implementation is provided as an example:
- `MsSqlConnectionType`: Defines the SQL Server connection type
- Registers factory, translator, and supporting services
- Integrates with Entity Framework Core if available
- Provides connection pooling configuration

## Extensibility

To add new connection types:

1. Create a class inheriting from `ConnectionTypeBase`
2. Implement required abstract members
3. Create corresponding configuration and factory classes
4. The source generator automatically discovers and includes it

## Connection Lifecycle

1. **Creation**: Provider creates connection via factory
2. **Opening**: Connection transitions to Open state
3. **Usage**: Execute commands while in Open state
4. **Closing**: Graceful shutdown with state transition
5. **Disposal**: Clean up resources

## Monitoring and Diagnostics

Built-in support for:
- Connection state tracking
- Operation timing and metrics
- Detailed error logging
- Configuration validation logging
- Factory registration verification

## Migration from Direct Connections

The framework provides a migration path from direct connection usage:

1. Wrap existing connections in adapters
2. Implement IGenericConnection interface
3. Register with ConnectionTypes
4. Gradually migrate to full abstraction