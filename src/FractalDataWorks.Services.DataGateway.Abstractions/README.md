# FractalDataWorks.Services.DataGateway.Abstractions

## Overview

The **FractalDataWorks.Services.DataGateway.Abstractions** project defines the contracts, models, and base types for the FractalDataWorks data provider system. This project contains all the interfaces, abstract classes, commands, and data models that enable provider-agnostic data operations across different data stores (SQL databases, NoSQL databases, file systems, REST APIs, etc.).

## Purpose

This abstractions library provides:

- Universal command abstractions that work across all data providers
- Data models for representing schema, containers, and records
- Configuration abstractions for data store management
- Enhanced enums for data store types and capabilities
- Message definitions for data provider operations
- Transaction and connection abstractions
- Validation frameworks for data provider configurations

## Key Components

### Command System

#### `DataCommandBase`
**File**: `Commands/DataCommandBase.cs`

Abstract base class for all data commands providing provider-agnostic data operations.

**Key Features**:
- Universal command structure with correlation IDs and timestamps
- Parameter and metadata management with type-safe accessors
- Fluent command building with `WithConnection()`, `WithTarget()`, `WithTimeout()`
- Built-in validation using FluentValidation
- Abstract `CreateCopy()` method for immutable command patterns

**Properties**:
- `CommandId` - Unique identifier for the command
- `CorrelationId` - Correlation identifier for tracking
- `ConnectionName` - Target connection name
- `TargetContainer` - Target data container path
- `Parameters` - Command parameters dictionary
- `Metadata` - Additional command metadata
- `Timeout` - Execution timeout
- `IsDataModifying` - Indicates if command modifies data

#### Specific Command Types

**Query Commands**:
- `QueryCommand<TEntity>` - LINQ-based data queries
- `CountCommand<TEntity>` - Record counting operations
- `ExistsCommand<TEntity>` - Existence checks

**Modification Commands**:
- `InsertCommand<TEntity>` - Single record insertion
- `BulkInsertCommand<TEntity>` - Bulk record insertion
- `UpdateCommand<TEntity>` - Record updates with predicates
- `PartialUpdateCommand<TEntity>` - Field-specific updates
- `DeleteCommand<TEntity>` - Record deletion
- `TruncateCommand<TEntity>` - Container truncation
- `UpsertCommand<TEntity>` - Insert-or-update operations
- `BulkUpsertCommand<TEntity>` - Bulk upsert operations

### Data Models

#### `DataContainer`
**File**: `Models/DataContainer.cs`

Represents a data container (table, collection, file, etc.) with metadata.

**Properties**:
- Schema information (name, type, path)
- Container metadata and capabilities
- Child container references
- Access permissions and constraints

#### `DataPath`
**File**: `Models/DataPath.cs`

Hierarchical path representation for navigating data structures.

**Features**:
- Hierarchical path navigation (server.database.schema.table)
- Platform-agnostic path representation
- Path parsing and validation
- Wildcard and pattern matching support

#### `DataRecord`
**File**: `Models/DataRecord.cs`

Represents a single data record with typed field access.

**Features**:
- Type-safe field access
- Metadata preservation
- Change tracking capabilities
- Serialization support

#### `DataStore`
**File**: `Models/DataStore.cs`

Represents a complete data store with its capabilities and configuration.

#### `Datum` and `DatumCategory`
**Files**: `Models/Datum.cs`, `Models/DatumCategory.cs`

Represent individual data elements and their categorization for schema discovery and mapping.

### Enhanced Enums

#### Data Store Types
**Directory**: `EnhancedEnums/DataStoreTypes/`

Concrete implementations of data store types with specific capabilities:

- `SqlServerDataStoreType` - Microsoft SQL Server
- `PostgreSqlDataStoreType` - PostgreSQL  
- `MySqlDataStoreType` - MySQL
- `MongoDbDataStoreType` - MongoDB
- `RedisDataStoreType` - Redis
- `FileSystemDataStoreType` - File system
- `RestApiDataStoreType` - REST APIs
- `SftpDataStoreType` - SFTP connections

**Features**:
- Provider-specific connection strings and capabilities
- Supported operation sets per provider
- Performance characteristics and limitations
- Authentication and security settings

#### `DataStoreTypesCollection`
**File**: `EnhancedEnums/DataStoreTypes/DataStoreTypesCollection.cs`

Source generator target for creating static collections of data store types.

### Configuration System

#### `DataStoreConfiguration`
**File**: `Configuration/DataStoreConfiguration.cs`

Main configuration class for data store settings.

**Features**:
- Connection string management
- Provider-specific settings
- Performance tuning parameters
- Security configuration

#### `DataStoreConfigurationValidator`
**File**: `Configuration/DataStoreConfigurationValidator.cs`

FluentValidation validator for data store configurations.

#### Specialized Configuration Classes

- `ContainerAccessConfiguration` - Container access permissions
- `ConventionSettings` - Naming conventions and mappings
- `ConnectionPoolingSettings` - Connection pooling configuration
- `HealthCheckSettings` - Health check configuration
- `SchemaDiscoverySettings` - Schema discovery configuration
- `DataContainerMapping` - Container mapping rules
- `DatumMapping` - Field mapping specifications
- `DatumCategorizationStrategy` - Data categorization rules

**Enums**:
- `CacheStrategy` - Cache strategy options
- `CategorizationMode` - Data categorization modes

### Core Interfaces

#### `IDataCommand` and `IDataCommand<TResult>`
**File**: `IDataCommand.cs`

Core interfaces for all data commands with execution metadata and validation.

#### `IDataService` and `IDataServiceFactory`
**Files**: `IDataService.cs`, `IDataServiceFactory.cs`

Service abstractions for data operations and service creation.

#### `IExternalDataConnection<TConfiguration>`
**File**: `IExternalDataConnection.cs`

Interface for external data connections with typed configuration.

**Methods**:
- `Execute<T>(command, cancellationToken)` - Execute commands
- `DiscoverSchema(startPath, cancellationToken)` - Schema discovery
- `TestConnection(cancellationToken)` - Connection health testing
- `GetConnectionInfo(cancellationToken)` - Connection metadata

#### `IDataTransaction`
**File**: `IDataTransaction.cs`

Transaction abstraction supporting different isolation levels and states.

**Features**:
- `FractalTransactionIsolationLevel` enumeration
- `FractalTransactionState` state management
- Commit/rollback operations
- Nested transaction support

#### `IDataGatewaysConfiguration`
**File**: `IDataGatewaysConfiguration.cs`

Configuration interface for data provider services.

### Service Type System

#### `DataGatewayServiceType<T, TService, TConfig, TFactory>`
**File**: `DataGatewayServiceType.cs`

Generic base class for defining data provider service types.

#### `DataGatewayServiceTypes<TCollection, TFactory>`
**File**: `DataGatewayServiceTypes.cs`

Base class for enhanced enum collections of service types.

#### `IDataGatewayServiceType`
**File**: `IDataGatewayServiceType.cs`

Interface for data provider service type definitions.

### Message System

#### `DataGatewayMessage`
**File**: `Messages/DataGatewayMessage.cs`

Base class for all data provider messages.

#### Specific Message Types

- `ConnectionFailedMessage` - Connection failure notifications
- `QueryFailedMessage` - Query execution failures  
- `TransactionRollbackMessage` - Transaction rollback events

#### `DataGatewayMessageCollectionBase`
**File**: `Messages/DataGatewayMessageCollectionBase.cs`

Base class for message collections using source generators.

### Additional Interfaces and Models

#### `IBatchResult`
**File**: `IBatchResult.cs`

Interface for representing batch operation results.

#### `IFractalProviderMetrics`
**File**: `IFractalProviderMetrics.cs`

Interface for data provider performance metrics.

#### `IExternalDataConnectionFactory`
**File**: `IExternalDataConnectionFactory.cs`

Factory interface for creating external data connections.

## Dependencies

### Project References
- `FractalDataWorks.Data` - Core data abstractions
- `FractalDataWorks.Services` - Base service framework
- `FractalDataWorks.Services.Connections.Abstractions` - External connection contracts
- `FractalDataWorks.EnhancedEnums` - Enhanced enumeration system
- `FractalDataWorks.EnhancedEnums.SourceGenerators` - Code generation for enums

## Usage Patterns

### Creating Custom Commands

```csharp
public class CustomQueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public string SqlQuery { get; }
    
    public CustomQueryCommand(string connectionName, string sqlQuery) 
        : base("CustomQuery", connectionName)
    {
        SqlQuery = sqlQuery;
    }
    
    public override bool IsDataModifying => false;
    
    protected override DataCommandBase CreateCopy(
        string? connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, object> metadata,
        TimeSpan? timeout)
    {
        return new CustomQueryCommand<T>(connectionName ?? "", SqlQuery)
        {
            TargetContainer = targetContainer,
            Timeout = timeout
        };
    }
}
```

### Implementing Data Store Types

```csharp
public sealed class CustomDataStoreType : DataStoreTypeBase<CustomDataStoreType>, 
    IEnumOption<CustomDataStoreType>
{
    public CustomDataStoreType() : base(
        id: 100,
        name: "Custom",
        description: "Custom data store",
        connectionStringTemplate: "Server={server};Database={database};",
        supportedOperations: ["Query", "Insert", "Update"],
        defaultPort: 1433,
        requiresAuthentication: true,
        supportsTransactions: true,
        supportsBulkOperations: false,
        maxConnectionPoolSize: 50)
    {
    }
}
```

### Using Configuration Classes

```csharp
public class MyDataConfiguration : DataStoreConfiguration
{
    public string CustomSetting { get; set; } = "";
    
    // Override validation
    protected override void ConfigureValidation(
        DataStoreConfigurationValidator validator)
    {
        base.ConfigureValidation(validator);
        
        validator.RuleFor(x => x.CustomSetting)
            .NotEmpty()
            .WithMessage("Custom setting is required");
    }
}
```

### Working with Data Models

```csharp
// Create data path
var tablePath = DataPath.Create("Server1", "Database1", "Schema1", "TableName");

// Navigate data containers
public async Task ExploreSchema(IExternalDataConnection connection)
{
    var containers = await connection.DiscoverSchema(tablePath);
    
    foreach (var container in containers.Value)
    {
        Console.WriteLine($"Container: {container.Name}");
        Console.WriteLine($"Type: {container.ContainerType}");
        Console.WriteLine($"Path: {container.Path}");
        
        // Process child containers
        foreach (var child in container.Children)
        {
            // Process child container
        }
    }
}
```

## Code Coverage Exclusions

The following code should be excluded from coverage testing:

### Source Generator Targets
- All classes ending with `Collection.cs` (e.g., `DataStoreTypesCollection.cs`)
- All classes ending with `CollectionBase.cs`

### Enhanced Enum Implementations  
- All concrete data store type classes in `EnhancedEnums/DataStoreTypes/`
- All message classes in `Messages/` directory
- `DataGatewayServiceType.cs` and derived classes

### Configuration Validators
- All `*Validator.cs` files (tested through configuration validation)
- `*Settings.cs` configuration classes (simple property containers)

### Model Classes
- Simple data model classes with only properties (`DataRecord`, `DataContainer`, etc.)
- `ToString()` implementations
- Property getter/setter implementations

### Interface Definitions
- All `I*.cs` interface files (no implementation to test)
- Abstract base classes with no implementation logic

## Implementation Notes

### Command Design Pattern
All commands follow an immutable design pattern:
- Commands are created once and not modified
- Use `WithConnection()`, `WithTarget()` methods to create new instances
- Thread-safe by design due to immutability

### Enhanced Enums
Enhanced enums use source generators to create:
- Static collections of all instances
- Type-safe accessors by ID and name  
- Compile-time validation of enum completeness

### Validation Strategy
The library uses FluentValidation throughout:
- Commands validate themselves before execution
- Configurations have dedicated validator classes
- Validation results use `IFdwResult<ValidationResult>` pattern

### Provider Agnostic Design
All abstractions are designed to work across different providers:
- LINQ expressions translate to provider-specific queries
- DataPath works with hierarchical and flat data stores
- Commands abstract away provider-specific operations

### Thread Safety
All abstractions assume concurrent usage:
- Commands are immutable after creation
- Configuration objects should be thread-safe
- Connection interfaces support concurrent command execution

## Target Framework
- **NET 10.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Disabled