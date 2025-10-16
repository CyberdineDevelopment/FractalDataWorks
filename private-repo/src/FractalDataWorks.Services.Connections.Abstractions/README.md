# FractalDataWorks.Services.Connections.Abstractions

**Minimal external connection service abstractions for the FractalDataWorks Framework.**

## Purpose

This package provides the foundational interfaces for external connection services in the FractalDataWorks ecosystem. It defines the domain boundary for connection management through minimal, clean abstractions.

## Architecture

The external connections abstractions follow the framework's **enhanced service pattern**:

- **Core Interface**: `IGenericConnectionService` extends `IServiceType`
- **Command Contract**: `IConnectionCommand` defines connection command structure
- **Configuration Base**: `IGenericConnectionConfiguration` provides configuration contract
- **Base Classes**: Add type constraints without implementation logic

## Command Architecture

This package defines two distinct command hierarchies for the two-tier architecture:

### IConnectionCommand (Service-Level)
Service-level commands for connection management:
- **Purpose**: Creating and configuring connections, testing connectivity, discovery operations, connection lifecycle management
- **Key Properties**:
  - `CommandId` - Unique command identifier
  - `CreatedAt` - Timestamp
  - `CommandType` - Type of connection operation
- **Scope**: Connection service infrastructure

### IDataCommand (Data-Level)
Data-level commands executed through connections (defined in `FractalDataWorks.Data.Abstractions`):
- **Purpose**: Query operations, insert/update/delete operations, bulk operations, schema discovery
- **Key Properties**:
  - `ConnectionName` - Target connection identifier
  - `Query` - LINQ expression for filtering
  - `CommandType` - Type of data operation
  - `TargetContainer` - Table/collection name
  - `Metadata` - Additional command metadata
  - `Timeout` - Operation timeout
- **Scope**: Data operations through established connections

**Important**: These are separate command hierarchies. Service commands manage connections, while data commands operate through established connections. Do not confuse the two.

## Key Interfaces

### Core Service Interface
```csharp
public interface IGenericConnectionService : IServiceType
{
    // Service discovery and capability information
    string[] SupportedDataStores { get; }
    string ProviderName { get; }
    Type ConnectionType { get; }
    Type ConfigurationType { get; }
    IReadOnlyList<string> SupportedConnectionModes { get; }
    int Priority { get; }
    
    // Factory creation for connections
    Task<IGenericResult<IGenericConnectionFactory>> CreateConnectionFactoryAsync(
        IServiceProvider serviceProvider);
    
    // Validation and metadata
    IGenericResult ValidateCapability(string dataStore, string? connectionMode = null);
    Task<IGenericResult<IProviderMetadata>> GetProviderMetadataAsync();
}
```

### Generic Service Interface
```csharp
public interface IGenericConnectionService<TConfiguration> : IGenericConnectionService
    where TConfiguration : IGenericConnectionConfiguration, new()
{
    // Type-safe factory creation
    Task<IGenericResult<IGenericConnectionFactory<TConfiguration, IGenericConnection<TConfiguration>>>> 
        CreateTypedConnectionFactoryAsync(IServiceProvider serviceProvider);
        
    // Configuration validation
    IGenericResult ValidateConfiguration(TConfiguration configuration);
}
```

### Connection Interface
```csharp
public interface IGenericConnection
{
    // Connection lifecycle and state management
    string ConnectionId { get; }
    string ProviderName { get; }
    GenericConnectionState State { get; }
    string ConnectionString { get; }
    
    // Connection operations
    Task<IGenericResult> OpenAsync();
    Task<IGenericResult> CloseAsync();
    Task<IGenericResult> TestConnectionAsync();
    Task<IGenericResult<IConnectionMetadata>> GetMetadataAsync();
}
```

### Factory Interface
```csharp
public interface IGenericConnectionFactory
{
    // Connection creation and validation
    string ProviderName { get; }
    IReadOnlyList<string> SupportedConnectionTypes { get; }
    Type ConfigurationType { get; }
    
    Task<IGenericResult<IGenericConnection>> CreateConnectionAsync(FractalConfigurationBase configuration);
    Task<IGenericResult> ValidateConfigurationAsync(FractalConfigurationBase configuration);
    Task<IGenericResult> TestConnectivityAsync(FractalConfigurationBase configuration);
}
```

## Base Classes

The package includes base classes that **only add generic type constraints**:

- `GenericConnectionProviderBase` - Provides service type implementation with metadata
- Base classes are **logging-enabled** but contain no business logic

**Important**: Base classes contain **no implementation logic**. They exist solely to provide service type enumeration support and type constraints.

## Core Types

### Connection State Management
```csharp
public enum GenericConnectionState
{
    Closed, Opening, Open, Closing, Broken, Connecting, Executing
}
```

### Connection Metadata
```csharp
public interface IConnectionMetadata
{
    // Connection information and capabilities
    // Version, features, performance characteristics
}

public interface IProviderMetadata  
{
    // Provider capabilities and information
    // Version, supported features, limitations
}
```

## Enhanced Service Pattern

Unlike other abstractions, external connections use an **enhanced service pattern**:

- **Rich Interface**: More than just command processing
- **Service Discovery**: Built-in capability advertisement
- **Factory Pattern**: Separate connection creation from service
- **Metadata Support**: Runtime capability querying
- **Type Safety**: Generic constraints for configurations

This pattern is necessary because connections are **infrastructure services** that need:
- Runtime capability discovery
- Dynamic service selection
- Type-safe configuration handling
- Connection lifecycle management

## Usage

Concrete implementations should:

1. **Implement IGenericConnectionService** with capability metadata
2. **Inherit from GenericConnectionProviderBase** for service enumeration
3. **Create connection factory** that produces actual connections
4. **Define connection types** with proper state management
5. **Provide configuration classes** for connection parameters

## Generic Constraints

The type hierarchy flows from service discovery to typed implementation:

```
IServiceType
    ↓
IGenericConnectionService
    ↓
IGenericConnectionService<TConfiguration>
    ↓  
GenericConnectionProviderBase
    ↓
ConcreteConnectionService<SpecificConfig>
```

## Framework Integration

This abstraction integrates with other FractalDataWorks services:

- **DataGateways**: Provides database connections for data services
- **SecretManager**: Retrieves connection strings and credentials
- **Authentication**: Handles connection authentication
- **Configuration**: Manages connection settings and parameters

## Design Philosophy

These abstractions balance **minimal design** with **practical needs**:

- ✅ Define domain boundaries through interfaces
- ✅ Provide rich service discovery capabilities
- ✅ Enable dynamic service selection at runtime
- ✅ Support multiple connection types per service
- ❌ No implementation logic in abstractions
- ❌ No complex inheritance hierarchies
- ❌ No framework-specific coupling beyond service types

## Evolution

This package will grow organically as the framework evolves:

- New connection types added when needed
- Enhanced metadata interfaces for better discovery
- Additional factory patterns for complex scenarios
- Backward compatibility maintained for existing implementations

The enhanced service design provides the flexibility needed for infrastructure services while maintaining the framework's consistency and type safety principles.