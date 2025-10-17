# FractalDataWorks.Services.Connections.MsSql

> ⚠️ **PROJECT STATUS: TEMPORARILY REMOVED**
>
> This project was temporarily removed from the solution due to architectural incompatibilities with the new two-tier command architecture. It will be rebuilt with:
> - Proper distinction between IConnectionCommand (service-level) and IDataCommand (data-level)
> - Updated ServiceBase constructor requiring logger parameter
> - Fixed configuration initialization patterns (init-only properties)
>
> **This documentation is currently OUTDATED and will be updated when the project is rebuilt.**

SQL Server implementation of external connection services for the FractalDataWorks platform.

## Overview

This project provides a complete implementation of the external connections abstractions for Microsoft SQL Server. It includes both service-level connection management and individual connection implementations with full transaction support, schema discovery, and command translation.

## Key Components

### Core Classes

#### `MsSqlService`
- **Type**: `sealed class`
- **Interfaces**: `ServiceBase<IGenericConnectionCommand, MsSqlConfiguration, MsSqlService>`, `IGenericConnectionDataService`, `IDisposable`
- **Purpose**: Manages multiple SQL Server connections and handles external connection commands
- **Key Features**:
  - Stateless connection management with automatic cleanup
  - Command routing (create, discovery, management operations)
  - Connection pooling and health checking
  - Comprehensive logging using high-performance LoggerMessage delegates

#### `MsSqlGenericConnection`
- **Type**: `sealed class`
- **Interfaces**: `IGenericConnection<MsSqlConfiguration>`, `IExternalDataConnection<MsSqlConfiguration>`
- **Purpose**: Stateless SQL Server connection implementation
- **Key Features**:
  - Per-execution connection creation (no persistent connections)
  - Automatic transaction management
  - Schema discovery for tables and views
  - Command execution with type-safe result mapping
  - Connection testing and metadata collection

#### `MsSqlConfiguration`
- **Type**: `sealed class`
- **Base Class**: `ConfigurationBase<MsSqlConfiguration>`
- **Interfaces**: `IGenericConnectionConfiguration`
- **Purpose**: Comprehensive configuration for SQL Server connections
- **Key Properties**:
  - Connection string management with sanitization
  - Timeout settings (command and connection)
  - Schema mapping configuration
  - Connection pooling options
  - Transaction settings with isolation level control
  - Retry logic configuration
  - SQL logging controls

#### `MsSqlCommandTranslator`
- **Type**: `internal sealed class`
- **Purpose**: Translates universal data commands to parameterized SQL Server statements
- **Supported Commands**:
  - Query (with WHERE, ORDER BY, paging)
  - Count, Exists
  - Insert, BulkInsert
  - Update, Delete
  - Upsert (using MERGE), BulkUpsert
- **Safety Features**:
  - All SQL is parameterized to prevent injection
  - Expression tree translation for WHERE clauses
  - Type-safe parameter binding

### Supporting Types

#### `MsSqlConnectionType` and `MsSqlConnectionMetadata`
- Connection type definitions and metadata collection
- Server information, capabilities, and custom properties

#### `SqlTranslationResult`
- Contains translated SQL and parameters for command execution

#### `ExpressionTranslator`
- Translates LINQ expressions to SQL WHERE clauses

### Command Implementations
Located in the `Commands` namespace:
- `MsSqlConnectionTestCommand`
- `MsSqlGenericConnectionCreateCommand`
- `MsSqlGenericConnectionDiscoveryCommand`
- `MsSqlGenericConnectionManagementCommand`

## Dependencies

### Project References
- `FractalDataWorks.Services.DataGateway.Abstractions` - Data command abstractions
- `FractalDataWorks.Services.Connections.Abstractions` - External connection contracts
- `FractalDataWorks.EnhancedEnums` - Enhanced enum support
- `FractalDataWorks.EnhancedEnums.SourceGenerators` - Build-time enum generation

### Package References
- `Microsoft.Data.SqlClient` - SQL Server connectivity
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI support
- `Microsoft.Extensions.Logging.Abstractions` - Logging contracts

## Usage Patterns

### Basic Service Registration
```csharp
services.Configure<MsSqlConfiguration>(config =>
{
    config.ConnectionString = "Server=...;Database=...";
    config.CommandTimeoutSeconds = 30;
    config.UseTransactions = true;
});
services.AddSingleton<MsSqlService>();
```

### Connection Creation and Usage
```csharp
var service = serviceProvider.GetRequiredService<MsSqlService>();

// Create connection
var createCommand = new MsSqlGenericConnectionCreateCommand
{
    ConnectionName = "main-db",
    ConnectionConfiguration = new MsSqlConfiguration
    {
        ConnectionString = connectionString
    }
};
var result = await service.Execute<string>(createCommand);

// Execute queries
var queryCommand = new DataQueryCommand<MyEntity>();
var queryResult = await connection.Execute<IEnumerable<MyEntity>>(queryCommand);
```

### Configuration Options
```csharp
var config = new MsSqlConfiguration
{
    ConnectionString = connectionString,
    CommandTimeoutSeconds = 60,
    UseTransactions = true,
    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
    EnableConnectionPooling = true,
    MaxPoolSize = 100,
    EnableRetryLogic = true,
    MaxRetryAttempts = 3,
    DefaultSchema = "dbo",
    SchemaMappings = new Dictionary<string, string>
    {
        ["Users"] = "identity.users",
        ["Orders"] = "sales.orders"
    }
};
```

## Architecture Notes

### Stateless Design
The implementation uses a stateless approach where:
- Connections are created per Execute call
- No persistent connection state is maintained
- Automatic resource cleanup after each operation
- Thread-safe by design

### Transaction Handling
- Optional transaction wrapping per Execute call
- Configurable isolation levels
- Automatic commit/rollback based on operation success
- Transaction-aware command execution

### Schema Discovery
- Discovers tables and views from system catalogs
- Returns structured DataContainer representations
- Supports schema-qualified object names
- Configurable schema mappings

## Code Coverage Exclusions

The following code should be excluded from coverage testing as it requires real SQL Server connections:

### Integration-Only Methods
- `MsSqlGenericConnection.ExecuteWithConnection<T>()` - Real SQL execution
- `MsSqlGenericConnection.CollectMetadataAsync()` - Server metadata collection
- `MsSqlGenericConnection.MapDataReaderToResults<T>()` - SqlDataReader mapping
- `MsSqlGenericConnection.MapReaderToDataRecord()` - Data record mapping
- `MsSqlGenericConnection.MapReaderToObject()` - Object mapping via reflection
- `MsSqlGenericConnection.DiscoverTablesAndViewsAsync()` - Schema discovery queries
- `MsSqlCommandTranslator.CreateSingleEntityUpsertCommand()` - Placeholder throwing NotSupportedException

These methods are marked with `[ExcludeFromCodeCoverage]` attributes and require integration testing with actual SQL Server instances.

## Known Limitations

1. **Bulk Operations**: BulkUpsert operations generate multiple MERGE statements instead of optimized bulk operations
2. **Expression Translation**: Limited LINQ expression support in WHERE clauses
3. **Command Creation**: CreateSingleEntityUpsertCommand throws NotSupportedException (placeholder implementation)

## Security Features

- Connection string sanitization for logging (removes passwords, user IDs)
- Parameterized queries prevent SQL injection
- Configuration validation at construction time
- Secure credential handling through connection strings

## Logging

High-performance logging using LoggerMessage.Define delegates for:
- Connection lifecycle events
- Command execution tracking
- Transaction commit/rollback events
- Schema discovery operations
- Error conditions and exceptions

All sensitive information is excluded from logs through connection string sanitization.