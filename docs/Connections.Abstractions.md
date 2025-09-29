# FractalDataWorks.Services.Connections.Abstractions Documentation

## Overview

The FractalDataWorks.Services.Connections.Abstractions library defines the comprehensive contract system for all connection implementations in the framework. It provides abstract base classes, interfaces, and infrastructure for building type-safe, state-managed connections to external systems with full integration into the service architecture.

## Core Base Classes

### ConnectionServiceBase<TCommand, TConfiguration, TService>

The foundational abstract class for all connection service implementations:

```csharp
public abstract class ConnectionServiceBase<TCommand, TConfiguration, TService>
    : IGenericService<TCommand, TConfiguration, TService>, IGenericConnection
    where TCommand : IConnectionCommand
    where TConfiguration : class, IConnectionConfiguration
    where TService : class
```

**Key Features:**
- Implements both service and connection interfaces
- State management with ConnectionStates
- Automatic GUID-based identification
- Template methods for customization
- Thread-safe state transitions

**Protected Virtual Methods:**
- `OpenCoreAsync()`: Override to implement connection opening logic
- `CloseCoreAsync()`: Override for connection closing logic
- `TestConnectionCoreAsync()`: Override for connection testing
- `GetMetadataCoreAsync()`: Override to provide metadata
- `Dispose(bool disposing)`: Override for resource cleanup

### ConnectionTypeBase<TService, TConfiguration, TFactory>

Abstract base for connection type definitions:

```csharp
public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory>
    where TService : class
    where TConfiguration : class
    where TFactory : class
{
    public int Id { get; }
    public string Name { get; }
    public string Category { get; }
    public abstract Type FactoryType { get; }
    public abstract void Register(IServiceCollection services);
    public abstract void Configure(IConfiguration configuration);
}
```

This base class integrates with the ServiceTypes source generator for automatic discovery.

## Connection Interfaces

### IGenericConnection

Core connection interface:

```csharp
public interface IGenericConnection : IDisposable
{
    string ConnectionId { get; }
    string ProviderName { get; }
    IConnectionState State { get; }
    string ConnectionString { get; }

    Task<IGenericResult> OpenAsync();
    Task<IGenericResult> CloseAsync();
    Task<IGenericResult> TestConnectionAsync();
    Task<IGenericResult<IConnectionMetadata>> GetMetadataAsync();
}
```

### IConnectionConfiguration

Base configuration interface:

```csharp
public interface IConnectionConfiguration : IGenericConfiguration
{
    string ConnectionType { get; }
    string ConnectionString { get; }
    int? CommandTimeout { get; }
    Dictionary<string, object> ExtendedProperties { get; }
}
```

### IConnectionFactory

Factory interface for connection creation:

```csharp
public interface IConnectionFactory
{
    Task<IGenericResult<IGenericConnection>> CreateConnectionAsync(IConnectionConfiguration configuration);
    bool CanCreate(IConnectionConfiguration configuration);
}
```

### IConnectionMetadata

Metadata about connection capabilities:

```csharp
public interface IConnectionMetadata
{
    string SystemName { get; }
    string Version { get; }
    string ServerInfo { get; }
    string DatabaseName { get; }
    IReadOnlyDictionary<string, object> Capabilities { get; }
    DateTimeOffset CollectedAt { get; }
    IReadOnlyDictionary<string, object> CustomProperties { get; }
}
```

## Command System

### IConnectionCommand

Base interface for connection commands:

```csharp
public interface IConnectionCommand : ICommand
{
    string CommandText { get; }
    CommandType CommandType { get; }
    int? Timeout { get; }
}
```

### Specialized Command Interfaces

**IConnectionCreateCommand**: For connection creation operations
```csharp
public interface IConnectionCreateCommand : IConnectionCommand
{
    string ResourceName { get; }
    Dictionary<string, object> CreationParameters { get; }
}
```

**IConnectionDiscoveryCommand**: For resource discovery
```csharp
public interface IConnectionDiscoveryCommand : IConnectionCommand
{
    ConnectionDiscoveryOptions Options { get; }
    string SearchPattern { get; }
}
```

**IConnectionManagementCommand**: For management operations
```csharp
public interface IConnectionManagementCommand : IConnectionCommand
{
    ConnectionManagementOperation Operation { get; }
    Dictionary<string, object> Parameters { get; }
}
```

### ConnectionDiscoveryOptions

Flags enum for discovery operations:

```csharp
[Flags]
public enum ConnectionDiscoveryOptions
{
    None = 0,
    IncludeSystemObjects = 1,
    IncludeUserObjects = 2,
    IncludeMetadata = 4,
    IncludePermissions = 8,
    DeepScan = 16
}
```

### ConnectionManagementOperation

Enumeration of management operations:

```csharp
public enum ConnectionManagementOperation
{
    Backup,
    Restore,
    Optimize,
    Analyze,
    Repair,
    Truncate,
    Vacuum
}
```

## Connection States

### IConnectionState

Interface for connection state representation:

```csharp
public interface IConnectionState
{
    int Id { get; }
    string Name { get; }
    bool IsOpen { get; }
    bool CanExecute { get; }
    bool IsTerminal { get; }
}
```

### ConnectionStateBase

Abstract base for state implementations:

```csharp
public abstract class ConnectionStateBase : IConnectionState
{
    protected ConnectionStateBase(int id, string name, bool isOpen, bool canExecute, bool isTerminal);
}
```

### Built-in Connection States

1. **CreatedConnectionState**: Initial state after creation
2. **OpeningConnectionState**: Transitioning to open
3. **OpenConnectionState**: Ready for operations
4. **ExecutingConnectionState**: Currently executing command
5. **ClosingConnectionState**: Transitioning to closed
6. **ClosedConnectionState**: Successfully closed
7. **BrokenConnectionState**: Error state
8. **DisposedConnectionState**: Final disposed state
9. **UnknownConnectionState**: Undefined state

### ConnectionStateCollectionBase

Base for source-generated state collections:

```csharp
public abstract class ConnectionStateCollectionBase
{
    // Source generator populates with all states
}
```

Generated `ConnectionStates` class provides static access to all states.

## Message System

### ConnectionMessage

Base class for connection-related messages:

```csharp
public abstract class ConnectionMessage : ServiceMessageBase
{
    public string ConnectionId { get; }
    public string ConnectionType { get; }
    protected ConnectionMessage(string code, string message, string connectionId, string connectionType);
}
```

### Specialized Messages

**ConnectionFailedMessage**: Connection failure information
```csharp
public class ConnectionFailedMessage : ConnectionMessage
{
    public Exception Exception { get; }
    public string FailureReason { get; }
}
```

**ConnectionTimeoutMessage**: Timeout notifications
```csharp
public class ConnectionTimeoutMessage : ConnectionMessage
{
    public int TimeoutSeconds { get; }
    public string Operation { get; }
}
```

### ConnectionMessageCollectionBase

Base for source-generated message collections:

```csharp
public abstract class ConnectionMessageCollectionBase
{
    // Generated with all message types
}
```

## Provider Interfaces

### IGenericConnectionProvider

Main provider interface:

```csharp
public interface IGenericConnectionProvider
{
    Task<IGenericResult<IGenericConnection>> GetConnection(IConnectionConfiguration configuration);
    Task<IGenericResult<IGenericConnection>> GetConnection(int configurationId);
    Task<IGenericResult<IGenericConnection>> GetConnection(string configurationName);
}
```

## Type System

### IConnectionType

Interface for connection type definitions:

```csharp
public interface IConnectionType
{
    int Id { get; }
    string Name { get; }
    string Category { get; }
    Type ServiceType { get; }
    Type ConfigurationType { get; }
    Type FactoryType { get; }
    void Register(IServiceCollection services);
}
```

### IConnectionTypeRegistration

Registration interface:

```csharp
public interface IConnectionTypeRegistration
{
    void RegisterConnectionType(IServiceCollection services);
    void ConfigureConnectionType(IConfiguration configuration);
}
```

## Data Service Interface

### IConnectionDataService

Specialized interface for data operations:

```csharp
public interface IConnectionDataService : IGenericConnection
{
    Task<IGenericResult<T>> QueryAsync<T>(string query, object parameters = null);
    Task<IGenericResult<IEnumerable<T>>> QueryMultipleAsync<T>(string query, object parameters = null);
    Task<IGenericResult<int>> ExecuteAsync(string command, object parameters = null);
    Task<IGenericResult> BulkInsertAsync<T>(IEnumerable<T> data, string tableName);
}
```

## Key Design Patterns

1. **State Pattern**: Connection state management
2. **Abstract Factory**: Connection and factory creation
3. **Template Method**: Customizable base operations
4. **Strategy Pattern**: Command execution strategies
5. **Provider Pattern**: Connection provider abstraction

## Thread Safety

- State transitions are thread-safe
- Message objects are immutable
- Base classes provide thread-safe property access
- Commands should be immutable after creation

## Best Practices

1. **Always check connection state** before operations
2. **Use async methods** for all I/O operations
3. **Implement proper disposal** in derived classes
4. **Validate configuration** in factory methods
5. **Log state transitions** for debugging
6. **Handle transient failures** with retry logic
7. **Implement connection pooling** where appropriate

## Extension Points

The library provides multiple extension points:

1. Custom connection states by extending `ConnectionStateBase`
2. New command types implementing `IConnectionCommand`
3. Metadata providers via `IConnectionMetadata`
4. Message types extending `ConnectionMessage`
5. Discovery strategies through `IConnectionDiscoveryCommand`

## Integration Requirements

Implementations must reference:
- `FractalDataWorks.Services.Abstractions` for service interfaces
- `FractalDataWorks.Configuration.Abstractions` for configuration
- `FractalDataWorks.Results` for result types
- `FractalDataWorks.Messages` for messaging