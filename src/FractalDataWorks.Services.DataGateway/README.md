# FractalDataWorks.Services.DataGateway

## Overview

The **FractalDataWorks.Services.DataGateway** project provides concrete implementation of data provider services that route data commands to named external connections. It serves as the primary entry point for data operations within the FractalDataWorks framework, abstracting away the complexity of managing multiple data connections and providing a unified interface for data operations across different data stores.

## Purpose

This project implements the core data provider functionality that:

- Routes provider-agnostic data commands to appropriate external connections
- Provides schema discovery capabilities across different data stores
- Manages connection health monitoring and metadata retrieval
- Implements consistent error handling and logging across all operations
- Supports SQL databases, NoSQL databases, file systems, REST APIs, and other external systems

## Key Components

### Core Services

#### `DataGatewayService`
**File**: `Services/DataGatewayService.cs`

The main service implementation that routes commands to named connections. Implements both `ServiceBase` and `IDataGateway` interfaces.

**Key Features**:
- Command validation and routing based on connection names
- Schema discovery across different data stores
- Connection health monitoring and availability checks
- Comprehensive logging with structured scoping
- Exception handling with proper error recovery

**Constructor Dependencies**:
- `ILogger<DataGatewayService>` - Service logging
- `DataStoreConfiguration` - Service configuration  
- `IExternalDataConnectionProvider` - Connection routing provider

#### `ExternalDataConnectionProvider`
**File**: `Services/ExternalDataConnectionProvider.cs`

Manages the registry of named external data connections and routes commands to appropriate connections.

**Key Features**:
- Thread-safe connection registry using `ConcurrentDictionary`
- Dynamic connection registration and unregistration
- Connection health testing before command execution
- Parallel metadata retrieval from all connections
- High-performance logging with `LoggerMessage.Define`

**Methods**:
- `ExecuteCommand<T>()` - Routes and executes data commands
- `DiscoverConnectionSchema()` - Discovers schema structure from connections
- `GetConnectionsMetadata()` - Retrieves metadata from all registered connections
- `IsConnectionAvailable()` - Tests connection availability
- `RegisterConnection()` - Adds new named connections
- `UnregisterConnection()` - Removes connections from registry

### Configuration and Registry

#### `DataStoreConfigurationRegistry`
**File**: `Configuration/DataStoreConfigurationRegistry.cs`

Registry for managing data store configurations across the system.

#### `DataGatewayServiceTypesCollection`
**File**: `DataGatewayServiceTypesCollection.cs`

Enhanced enum collection for data provider service types. Uses source generators to automatically create:
- Static `DataGatewayServiceTypes.SqlServer` accessor
- `DataGatewayServiceTypes.All` collection
- `GetById(int id)` and `GetByName(string name)` methods

#### `SqlDataGatewayServiceType`
**File**: `SqlDataGatewayServiceType.cs`

Concrete service type definition for SQL Server data providers.

**Configuration**:
- ID: 1, Name: "SqlServer"
- Provider: "System.Data.SqlClient"
- Default connection string template
- Supported data stores: MsSql, AzureSql, SqlServer
- Supported commands: Query, Insert, Update, Delete, BulkInsert, BulkUpsert, Exists, Count, Truncate
- Max batch size: 10,000
- Max connection pool size: 100
- Supports transactions, bulk operations, streaming, and schema discovery

### Command Factory

#### `DataCommands`
**File**: `Enums/DataCommands.cs`

Static factory class providing fluent syntax for creating provider-agnostic data commands.

**Query Operations**:
- `Query<TEntity>(predicate, connectionName)` - Filtered queries
- `QueryAll<TEntity>(connectionName)` - Retrieve all records
- `QueryById<TEntity, TId>(id, connectionName)` - Find by ID
- `Count<TEntity>(predicate, connectionName)` - Count records
- `Exists<TEntity>(predicate, connectionName)` - Check existence

**Modification Operations**:
- `Insert<TEntity>(entity, connectionName)` - Single insert
- `BulkInsert<TEntity>(entities, connectionName, batchSize)` - Bulk insert
- `Update<TEntity>(entity, predicate, connectionName)` - Update records
- `UpdateById<TEntity, TId>(entity, id, connectionName)` - Update by ID
- `PartialUpdate<TEntity>(updates, predicate, connectionName)` - Field updates
- `Delete<TEntity>(predicate, connectionName)` - Delete records
- `DeleteById<TEntity, TId>(id, connectionName)` - Delete by ID
- `Truncate<TEntity>(connectionName)` - Remove all records
- `Upsert<TEntity>(entity, conflictFields, connectionName)` - Insert or update
- `BulkUpsert<TEntity>(entities, conflictFields, connectionName)` - Bulk upsert

### Interfaces

#### `IDataGateway`
**File**: `Services/IDataGateway.cs`

Consumer-facing interface for data operations.

**Methods**:
- `Execute<T>(command, cancellationToken)` - Execute typed data commands
- `DiscoverSchema(connectionName, startPath, cancellationToken)` - Discover data structure
- `GetConnectionsInfo(cancellationToken)` - Retrieve connection metadata

#### `IExternalDataConnectionProvider`  
**File**: `Services/IExternalDataConnectionProvider.cs`

Interface for managing external data connections and command routing.

### Logging

#### `DataGatewayServiceLog`
**File**: `Logging/DataGatewayServiceLog.cs`

High-performance logging using `LoggerMessage.Define` for all data provider operations.

## Dependencies

### Project References
- `FractalDataWorks.Services` - Base service functionality
- `FractalDataWorks.Configuration` - Configuration management
- `FractalDataWorks.Services.DataGateway.Abstractions` - Contracts and models
- `FractalDataWorks.Services.Connections.Abstractions` - External connection interfaces

### Package References
- `Microsoft.Extensions.Logging` - Logging framework
- `Microsoft.Extensions.Options` - Configuration binding

## Usage Patterns

### Basic Data Operations

```csharp
// Inject the data provider service
public class MyService
{
    private readonly IDataGateway _dataProvider;
    
    public MyService(IDataGateway dataProvider)
    {
        _dataProvider = dataProvider;
    }
    
    // Query data
    public async Task<IFdwResult<List<Customer>>> GetActiveCustomers()
    {
        var command = DataCommands.Query<Customer>(c => c.IsActive)
            .WithConnection("ProductionDB");
            
        return await _dataProvider.Execute<List<Customer>>(command);
    }
    
    // Insert data
    public async Task<IFdwResult<int>> CreateCustomer(Customer customer)
    {
        var command = DataCommands.Insert(customer)
            .WithConnection("ProductionDB");
            
        return await _dataProvider.Execute<int>(command);
    }
    
    // Bulk operations
    public async Task<IFdwResult<int>> BulkLoadCustomers(List<Customer> customers)
    {
        var command = DataCommands.BulkInsert(customers, batchSize: 500)
            .WithConnection("ProductionDB");
            
        return await _dataProvider.Execute<int>(command);
    }
}
```

### Schema Discovery

```csharp
public async Task<IFdwResult<IEnumerable<DataContainer>>> ExploreDatabase()
{
    // Discover full schema
    var fullSchema = await _dataProvider.DiscoverSchema("ProductionDB");
    
    // Discover from specific path
    var salesSchema = await _dataProvider.DiscoverSchema(
        "ProductionDB", 
        DataPath.Create(".", "SalesDB")
    );
    
    return fullSchema;
}
```

### Connection Management

```csharp
public async Task<IFdwResult<IDictionary<string, object>>> CheckConnections()
{
    var connectionsInfo = await _dataProvider.GetConnectionsInfo();
    
    if (connectionsInfo.IsSuccess)
    {
        foreach (var connection in connectionsInfo.Value)
        {
            Console.WriteLine($"Connection: {connection.Key}");
            // Process connection metadata
        }
    }
    
    return connectionsInfo;
}
```

## Code Coverage Exclusions

The following code should be excluded from coverage testing:

### Infrastructure Code
- `DataGatewayServiceTypesCollection.cs` - Source generator target class
- `Logging/DataGatewayServiceLog.cs` - Static logging definitions
- `Configuration/DataStoreConfigurationRegistry.cs` - Configuration infrastructure

### Boilerplate Code
- Constructor parameter validation (`ArgumentNullException` throws)
- Property getter/setter implementations
- `ToString()` method implementations
- Static factory method implementations in `DataCommands.cs`

## Implementation Notes

### Service Registration
The service uses the standard FractalDataWorks service registration pattern. Register with DI container:

```csharp
services.AddScoped<IDataGateway, DataGatewayService>();
services.AddSingleton<IExternalDataConnectionProvider, ExternalDataConnectionProvider>();
```

### Connection Lifecycle
- Connections are registered at application startup
- Connection health is checked before each command execution
- Failed connections are reported but don't prevent other operations
- Connections can be dynamically added/removed during runtime

### Performance Considerations
- Uses `ConcurrentDictionary` for thread-safe connection management
- Implements high-performance logging with `LoggerMessage.Define`
- Supports configurable batch sizes for bulk operations
- Connection pooling is managed by individual connection providers

### Error Handling
- Comprehensive validation at command and connection levels
- Structured logging with correlation IDs for traceability
- Graceful degradation when connections are unavailable
- Detailed error messages with operation context

### Thread Safety
- All public APIs are thread-safe
- Connection registry supports concurrent access
- Command execution is isolated per operation
- Logging scopes are properly managed across threads

## Target Framework
- **NET 10.0**
- **Nullable Reference Types**: Enabled  
- **Implicit Usings**: Disabled