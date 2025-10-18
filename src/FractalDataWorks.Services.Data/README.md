# FractalDataWorks.Services.Data

**Data gateway service for universal data command execution across any data store type.**

## Overview

This project provides the `DataGatewayService` - the central orchestration point for executing universal data commands against any type of data store (SQL, REST, GraphQL, File, etc.). The gateway implements the **translator pattern** to convert universal commands into domain-specific commands.

### Key Principle

**"Write once, run anywhere"** - Application code writes ONE command that works with ANY data store by simply changing the connection name in configuration.

## Architecture

```
Application Code
    ↓
IDataGateway.Execute(IDataCommand)
    ↓
DataGatewayService
    ├──> Gets IDataConnection by name
    ├──> Gets IDataStore from connection
    ├──> Gets IDataCommandTranslator based on DataStore.TranslatorType
    ├──> Translates IDataCommand → IConnectionCommand
    └──> Executes IConnectionCommand on connection
    ↓
IGenericResult<T>
```

## Core Components

### IDataGateway Interface

```csharp
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Data.Abstractions;

/// <summary>
/// Gateway service for executing universal data commands.
/// </summary>
public interface IDataGateway
{
    /// <summary>
    /// Executes a data command and returns a typed result.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="command">The universal data command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the typed data or failure information.</returns>
    Task<IGenericResult<T>> Execute<T>(IDataCommand command, CancellationToken cancellationToken = default);
}
```

### DataGatewayService Implementation

```csharp
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Commands.Data.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Data.Abstractions;
using FractalDataWorks.Services.Data.Logging;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// Default implementation of the DataGateway service.
/// Orchestrates command translation and execution using the translator pattern.
/// </summary>
public sealed class DataGatewayService : IDataGateway
{
    private readonly ILogger<DataGatewayService> _logger;
    private readonly IDataConnectionProvider _connectionProvider;
    private readonly IDataCommandTranslatorProvider _translatorProvider;

    public DataGatewayService(
        ILogger<DataGatewayService> logger,
        IDataConnectionProvider connectionProvider,
        IDataCommandTranslatorProvider translatorProvider)
    {
        _logger = logger;
        _connectionProvider = connectionProvider;
        _translatorProvider = translatorProvider;
    }

    public async Task<IGenericResult<T>> Execute<T>(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        DataGatewayLog.RoutingCommand(_logger, command.CommandType, command.ConnectionName);

        // 1. Get data connection by name
        var connectionResult = await _connectionProvider.GetConnection(command.ConnectionName)
            .ConfigureAwait(false);

        if (!connectionResult.IsSuccess || connectionResult.Value == null)
        {
            DataGatewayLog.ConnectionRetrievalFailed(_logger, command.ConnectionName);
            return GenericResult<T>.Failure($"Connection '{command.ConnectionName}' not found");
        }

        var connection = connectionResult.Value;

        // 2. Get DataStore from connection metadata
        if (connection.Metadata?.DataStore == null)
        {
            DataGatewayLog.DataStoreNotFound(_logger, command.ConnectionName);
            return GenericResult<T>.Failure(
                $"Connection '{command.ConnectionName}' has no associated DataStore");
        }

        var dataStore = connection.Metadata.DataStore;

        // 3. Get translator for the DataStore's domain
        var translatorResult = _translatorProvider.GetTranslator(dataStore.TranslatorType);
        if (!translatorResult.IsSuccess)
        {
            DataGatewayLog.TranslatorRetrievalFailed(_logger, dataStore.TranslatorType, command.ConnectionName);
            return GenericResult<T>.Failure(translatorResult.Message);
        }

        var translator = translatorResult.Value;

        // 4. Translate IDataCommand → IConnectionCommand
        DataGatewayLog.TranslatingCommand(_logger, command.ContainerName, dataStore.TranslatorType);
        var translatedResult = await translator.Translate(command, cancellationToken)
            .ConfigureAwait(false);

        if (!translatedResult.IsSuccess)
        {
            DataGatewayLog.TranslationFailed(_logger, command.ConnectionName, translatedResult.Message);
            return GenericResult<T>.Failure(translatedResult.Message);
        }

        var connectionCommand = translatedResult.Value;

        // 5. Execute IConnectionCommand on connection
        DataGatewayLog.ExecutingCommand(_logger, command.CommandType, command.ConnectionName);
        return await connection.Execute<T>(connectionCommand, cancellationToken)
            .ConfigureAwait(false);
    }
}
```

### DataGatewayConfiguration

```csharp
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// Configuration for the DataGateway service.
/// </summary>
public sealed class DataGatewayConfiguration : ConfigurationBase<DataGatewayConfiguration>
{
    public override string SectionName => "DataGateway";

    /// <summary>
    /// Default command timeout in seconds.
    /// </summary>
    public int DefaultCommandTimeout { get; set; } = 30;

    /// <summary>
    /// Whether to enable detailed translation logging.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Maximum retry attempts for transient failures.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}
```

## Execution Flow

### Step-by-Step Process

```
1. Application Creates Command
   ↓
   var command = new QueryDataCommand
   {
       ConnectionName = "PrimaryDB",
       ContainerName = "Users"
   };

2. Application Calls Gateway
   ↓
   var result = await dataGateway.Execute<User>(command);

3. Gateway Gets Connection
   ↓
   var connection = await connectionProvider.GetConnection("PrimaryDB");
   // Returns: MsSqlConnection

4. Gateway Gets DataStore from Connection
   ↓
   var dataStore = connection.Metadata.DataStore;
   // DataStore.TranslatorType = "MsSql"

5. Gateway Gets Translator
   ↓
   var translator = await translatorProvider.GetTranslator("MsSql");
   // Returns: MsSqlDataCommandTranslator

6. Gateway Translates Command
   ↓
   var sqlCommand = await translator.Translate(command);
   // IDataCommand → IConnectionCommand (SQL)
   // Result: SELECT * FROM [Users]

7. Gateway Executes Translated Command
   ↓
   var result = await connection.Execute<User>(sqlCommand);
   // MsSqlConnection executes SQL

8. Result Returned to Application
   ↓
   if (result.IsSuccess)
   {
       var users = result.Value;
   }
```

### Flow Diagram

```
┌──────────────────┐
│ Application Code │
└────────┬─────────┘
         │ IDataCommand
         ↓
┌──────────────────┐
│  DataGateway     │
│  Service         │
└────────┬─────────┘
         │
         ├─→ [1] GetConnection("PrimaryDB")
         │        ↓ Returns IDataConnection
         │
         ├─→ [2] Get DataStore from Connection.Metadata
         │        ↓ DataStore.TranslatorType = "MsSql"
         │
         ├─→ [3] GetTranslator("MsSql")
         │        ↓ Returns IDataCommandTranslator
         │
         ├─→ [4] Translate(IDataCommand)
         │        ↓ Returns IConnectionCommand (SQL)
         │
         └─→ [5] Execute(IConnectionCommand)
                  ↓ Returns IGenericResult<T>
```

## Usage Examples

### Example 1: Simple Query

```csharp
using FractalDataWorks.Commands.Data.Abstractions;
using FractalDataWorks.Services.Data.Abstractions;

public class UserService
{
    private readonly IDataGateway _dataGateway;

    public UserService(IDataGateway dataGateway)
    {
        _dataGateway = dataGateway;
    }

    public async Task<IGenericResult<IEnumerable<User>>> GetActiveUsers()
    {
        // Create universal command - works with ANY data store!
        var command = new QueryDataCommand
        {
            ConnectionName = "PrimaryDB",  // From appsettings.json
            ContainerName = "Users",        // Table/Endpoint/File name
            Filter = new FilterExpression
            {
                Conditions = new[]
                {
                    new FilterCondition
                    {
                        PropertyName = "IsActive",
                        Operator = FilterOperators.Equal,
                        Value = true
                    }
                }
            }
        };

        // Execute - DataGateway handles translation and execution
        return await _dataGateway.Execute<IEnumerable<User>>(command);
    }
}
```

### Example 2: Configuration-Driven Store Selection

```csharp
// appsettings.Development.json
{
  "Connections": {
    "PrimaryDB": {
      "Type": "MsSql",
      "ConnectionString": "Server=localhost;Database=DevDB;"
    }
  }
}

// appsettings.Production.json
{
  "Connections": {
    "PrimaryDB": {
      "Type": "Rest",
      "BaseUrl": "https://api.production.com"
    }
  }
}

// SAME CODE works in both environments!
public async Task<IGenericResult<User>> GetUser(int id)
{
    var command = new GetUserCommand
    {
        ConnectionName = "PrimaryDB",  // Routes to MsSql in dev, REST in prod
        ContainerName = "Users",
        UserId = id
    };

    return await _dataGateway.Execute<User>(command);
}
```

### Example 3: Multiple Data Stores in One Application

```csharp
public class DataService
{
    private readonly IDataGateway _dataGateway;

    // Query SQL database
    public async Task<User> GetUserFromDatabase(int id)
    {
        var command = new QueryDataCommand
        {
            ConnectionName = "SqlDatabase",  // SQL Server
            ContainerName = "Users"
        };
        var result = await _dataGateway.Execute<User>(command);
        return result.Value;
    }

    // Query REST API
    public async Task<Product> GetProductFromApi(string sku)
    {
        var command = new QueryDataCommand
        {
            ConnectionName = "ProductApi",   // REST API
            ContainerName = "products"
        };
        var result = await _dataGateway.Execute<Product>(command);
        return result.Value;
    }

    // Query File System
    public async Task<Config> GetConfigFromFile(string name)
    {
        var command = new QueryDataCommand
        {
            ConnectionName = "FileStore",    // File System
            ContainerName = "configs"
        };
        var result = await _dataGateway.Execute<Config>(command);
        return result.Value;
    }
}
```

## Registration and Configuration

### Service Registration

```csharp
using FractalDataWorks.Services.Data;
using Microsoft.Extensions.DependencyInjection;

// In Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register DataGateway and dependencies
    services.AddDataGateway();

    // Register connection types (includes translators)
    ConnectionTypes.RegisterAll(services);

    // Register specific connections if needed
    services.AddScoped<IDataConnection, MsSqlConnection>();
    services.AddScoped<IDataConnection, RestConnection>();
}
```

### Configuration File

```json
{
  "DataGateway": {
    "DefaultCommandTimeout": 30,
    "EnableDetailedLogging": true,
    "MaxRetryAttempts": 3
  },
  "Connections": {
    "PrimaryDB": {
      "Type": "MsSql",
      "ConnectionString": "Server=localhost;Database=MyApp;",
      "CommandTimeout": 60
    },
    "ProductApi": {
      "Type": "Rest",
      "BaseUrl": "https://api.products.com",
      "Headers": {
        "Authorization": "Bearer ${API_TOKEN}"
      }
    }
  },
  "DataStores": {
    "MsSql": {
      "Name": "PrimarySQL",
      "TranslatorType": "MsSql",
      "Settings": {
        "CommandTimeout": "30"
      }
    }
  }
}
```

## Logging

### Source-Generated Logging

```csharp
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Data.Logging;

public static partial class DataGatewayLog
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Routing {CommandType} command to connection '{ConnectionName}'")]
    public static partial void RoutingCommand(
        ILogger logger,
        string commandType,
        string connectionName);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Failed to retrieve connection '{ConnectionName}'")]
    public static partial void ConnectionRetrievalFailed(
        ILogger logger,
        string connectionName);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "DataStore not found for connection '{ConnectionName}'")]
    public static partial void DataStoreNotFound(
        ILogger logger,
        string connectionName);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "Failed to retrieve translator '{TranslatorType}' for connection '{ConnectionName}'")]
    public static partial void TranslatorRetrievalFailed(
        ILogger logger,
        string translatorType,
        string connectionName);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Debug,
        Message = "Translating command for container '{ContainerName}' using translator '{TranslatorType}'")]
    public static partial void TranslatingCommand(
        ILogger logger,
        string containerName,
        string translatorType);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Error,
        Message = "Translation failed for connection '{ConnectionName}': {Error}")]
    public static partial void TranslationFailed(
        ILogger logger,
        string connectionName,
        string error);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Debug,
        Message = "Executing {CommandType} command on connection '{ConnectionName}'")]
    public static partial void ExecutingCommand(
        ILogger logger,
        string commandType,
        string connectionName);
}
```

### Log Output Example

```
[INF] Routing Query command to connection 'PrimaryDB'
[DBG] Translating command for container 'Users' using translator 'MsSql'
[DBG] Executing Query command on connection 'PrimaryDB'
[INF] Command completed successfully in 45ms
```

## Best Practices

### Gateway Usage

✅ **DO**: Inject `IDataGateway` into services via DI
✅ **DO**: Use configuration for connection names (never hardcode)
✅ **DO**: Check result.IsSuccess before using result.Value
✅ **DO**: Use typed commands (IDataCommand<T>) for type safety

❌ **DON'T**: Create DataGateway instances directly
❌ **DON'T**: Hardcode connection names in commands
❌ **DON'T**: Ignore failure results
❌ **DON'T**: Assume translator will always succeed

### Error Handling

✅ **DO**: Return descriptive error messages
✅ **DO**: Log all failures with context
✅ **DO**: Use Result pattern (no exceptions)
✅ **DO**: Provide fallback strategies for transient failures

❌ **DON'T**: Throw exceptions from Execute method
❌ **DON'T**: Swallow errors silently
❌ **DON'T**: Return null values

### Performance

✅ **DO**: Use async/await properly
✅ **DO**: Configure appropriate command timeouts
✅ **DO**: Cache frequently accessed connections
✅ **DO**: Use cancellation tokens for long-running operations

❌ **DON'T**: Block async calls with .Result or .Wait()
❌ **DON'T**: Create new connections on every request
❌ **DON'T**: Ignore CancellationToken

## Testing

### Unit Tests

```csharp
using FractalDataWorks.Services.Data;
using FractalDataWorks.Commands.Data.Abstractions;
using Xunit;
using Moq;

public class DataGatewayServiceTests
{
    [Fact]
    public async Task Execute_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var mockConnectionProvider = new Mock<IDataConnectionProvider>();
        var mockTranslatorProvider = new Mock<IDataCommandTranslatorProvider>();
        var mockConnection = new Mock<IDataConnection>();
        var mockTranslator = new Mock<IDataCommandTranslator>();

        mockConnectionProvider
            .Setup(x => x.GetConnection("PrimaryDB"))
            .ReturnsAsync(GenericResult<IDataConnection>.Success(mockConnection.Object));

        mockTranslatorProvider
            .Setup(x => x.GetTranslator("MsSql"))
            .Returns(GenericResult<IDataCommandTranslator>.Success(mockTranslator.Object));

        var gateway = new DataGatewayService(
            Mock.Of<ILogger<DataGatewayService>>(),
            mockConnectionProvider.Object,
            mockTranslatorProvider.Object);

        var command = new QueryDataCommand
        {
            ConnectionName = "PrimaryDB",
            ContainerName = "Users"
        };

        // Act
        var result = await gateway.Execute<User>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_ConnectionNotFound_ReturnsFailure()
    {
        // Arrange
        var mockConnectionProvider = new Mock<IDataConnectionProvider>();
        mockConnectionProvider
            .Setup(x => x.GetConnection(It.IsAny<string>()))
            .ReturnsAsync(GenericResult<IDataConnection>.Failure("Connection not found"));

        var gateway = new DataGatewayService(
            Mock.Of<ILogger<DataGatewayService>>(),
            mockConnectionProvider.Object,
            Mock.Of<IDataCommandTranslatorProvider>());

        var command = new QueryDataCommand
        {
            ConnectionName = "InvalidConnection",
            ContainerName = "Users"
        };

        // Act
        var result = await gateway.Execute<User>(command);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Connection not found");
    }
}
```

## Target Frameworks

- .NET Standard 2.0
- .NET 10.0

## Dependencies

**NuGet Packages:**
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Configuration.Abstractions

**Project References:**
- FractalDataWorks.Commands.Data.Abstractions - IDataCommand, IDataCommandTranslator
- FractalDataWorks.Services.Connections.Abstractions - IDataConnection, IDataConnectionProvider
- FractalDataWorks.Results - Result pattern
- FractalDataWorks.Configuration - Configuration base classes

## Related Projects

- **FractalDataWorks.Commands.Data.Abstractions** - Data command interfaces and translator contracts
- **FractalDataWorks.Data.Translators** - Domain-specific translator implementations
- **FractalDataWorks.Services.Connections** - Connection implementations
- **FractalDataWorks.Services.Data.Abstractions** - Gateway and provider interfaces

---

**FractalDataWorks.Services.Data** - Universal data gateway for write-once, run-anywhere data operations.
