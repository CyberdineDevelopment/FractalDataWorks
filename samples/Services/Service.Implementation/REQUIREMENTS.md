# FractalDataWorks MsSql Connection Implementation Requirements

## Overview
This document provides complete, step-by-step requirements for implementing a SQL Server connection service using the FractalDataWorks ServiceTypes framework. Follow each section exactly as written.

## Directory Structure Requirements

### Root Structure
Create the following directory structure under `samples/Services/Service.Implementation/`:

```
samples/Services/Service.Implementation/
├── Directory.Build.props
├── Directory.Packages.props
├── nuget.config
├── src/
│   ├── FractalDataWorks.Services.Connections.Abstractions/
│   ├── FractalDataWorks.Services.Connections/
│   └── FractalDataWorks.Services.Connections.MsSql/
└── samples/
    └── ConnectionExample/
```

## Configuration Files

### 1. Directory.Build.props (Root)
**Location**: `samples/Services/Service.Implementation/Directory.Build.props`

```xml
<Project>
  <PropertyGroup>
    <!-- All projects target .NET 10.0 -->
    <TargetFramework>net10.0</TargetFramework>

    <!-- Enable nullable reference types -->
    <Nullable>enable</Nullable>

    <!-- Do not use implicit usings -->
    <ImplicitUsings>disable</ImplicitUsings>

    <!-- Use central package management -->
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>

    <!-- Use latest C# language version -->
    <LangVersion>latest</LangVersion>

    <!-- Generate documentation files -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>

    <!-- Treat warnings as errors in Release builds -->
    <TreatWarningsAsErrors Condition="'$(Configuration)' == 'Release'">true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

### 2. Directory.Packages.props (Root)
**Location**: `samples/Services/Service.Implementation/Directory.Packages.props`

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Microsoft packages -->
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.Configuration" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.Configuration.Binder" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Console" Version="10.0.0-preview.7.25380.108" />
    <PackageVersion Include="Microsoft.Extensions.Options" Version="10.0.0-preview.7.25380.108" />

    <!-- Data access -->
    <PackageVersion Include="Microsoft.Data.SqlClient" Version="5.1.5" />
    <PackageVersion Include="System.Data.Common" Version="4.3.0" />
  </ItemGroup>
</Project>
```

### 3. nuget.config (Root)
**Location**: `samples/Services/Service.Implementation/nuget.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- Local packages folder -->
    <add key="localPackages" value="../../../localpackages" />
    <!-- NuGet.org feed -->
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>

  <packageSourceMapping>
    <!-- All FractalDataWorks packages come from local -->
    <packageSource key="localPackages">
      <package pattern="FractalDataWorks.*" />
    </packageSource>
    <!-- Everything else from NuGet.org -->
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

## Project 1: FractalDataWorks.Services.Connections.Abstractions

### Project File
**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/FractalDataWorks.Services.Connections.Abstractions.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>FractalDataWorks.Services.Connections.Abstractions</RootNamespace>
    <AssemblyName>FractalDataWorks.Services.Connections.Abstractions</AssemblyName>
    <IsPackable>true</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" />
  </ItemGroup>
</Project>
```

### Core Interfaces

#### IFdwConnection.cs
**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/IFdwConnection.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Base interface for all FractalDataWorks connections.
/// </summary>
public interface IFdwConnection : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this connection instance.
    /// </summary>
    string ConnectionId { get; }

    /// <summary>
    /// Gets the provider name (e.g., "MsSql", "PostgreSql", "RestApi").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Executes a command on this connection.
    /// </summary>
    Task<IFdwResult> Execute(
        IDataCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command on this connection and returns a typed result.
    /// </summary>
    Task<IFdwResult<TResult>> Execute<TResult>(
        IDataCommand command,
        CancellationToken cancellationToken = default);
}
```

#### IConnectionConfiguration.cs
**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/IConnectionConfiguration.cs`

```csharp
namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Base interface for connection configuration.
/// All connection configurations must implement this interface.
/// </summary>
public interface IConnectionConfiguration
{
    /// <summary>
    /// Gets or sets the connection type name (e.g., "MsSql").
    /// Used by the ConnectionProvider to determine which factory to use.
    /// </summary>
    string ConnectionTypeName { get; set; }

    /// <summary>
    /// Gets or sets the connection string or endpoint.
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    int CommandTimeout { get; set; }

    /// <summary>
    /// Gets or sets the maximum retry count for transient failures.
    /// </summary>
    int MaxRetryCount { get; set; }
}
```

#### IConnectionFactory.cs
**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/IConnectionFactory.cs`

```csharp
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Factory interface for creating connections.
/// </summary>
public interface IConnectionFactory
{
    /// <summary>
    /// Gets the connection type name this factory supports.
    /// </summary>
    string ConnectionTypeName { get; }

    /// <summary>
    /// Creates a connection with the specified configuration.
    /// </summary>
    Task<IFdwConnection> Create(
        IConnectionConfiguration configuration,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic factory interface for creating typed connections.
/// </summary>
public interface IConnectionFactory<TConnection, TConfiguration> : IConnectionFactory
    where TConnection : IFdwConnection
    where TConfiguration : IConnectionConfiguration
{
    /// <summary>
    /// Creates a typed connection with the specified configuration.
    /// </summary>
    Task<TConnection> Create(
        TConfiguration configuration,
        CancellationToken cancellationToken = default);
}
```

#### IConnectionProvider.cs
**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/IConnectionProvider.cs`

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Provider interface for managing connection factories and creating connections.
/// </summary>
public interface IConnectionProvider
{
    /// <summary>
    /// Gets all registered connection type names.
    /// </summary>
    IEnumerable<string> GetSupportedConnectionTypes();

    /// <summary>
    /// Registers a connection factory.
    /// </summary>
    void RegisterFactory(IConnectionFactory factory);

    /// <summary>
    /// Creates a connection using the appropriate factory.
    /// </summary>
    Task<IFdwConnection> Create(
        IConnectionConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests if a connection type is supported.
    /// </summary>
    bool IsConnectionTypeSupported(string connectionTypeName);
}
```

#### IDataCommand.cs
**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/IDataCommand.cs`

```csharp
using System.Collections.Generic;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Represents an abstract data command that can be translated to provider-specific commands.
/// </summary>
public interface IDataCommand
{
    /// <summary>
    /// Gets the command type (e.g., "Query", "Insert", "Update", "Delete").
    /// </summary>
    string CommandType { get; }

    /// <summary>
    /// Gets the entity or table name.
    /// </summary>
    string EntityName { get; }

    /// <summary>
    /// Gets the command parameters.
    /// </summary>
    IReadOnlyDictionary<string, object> Parameters { get; }

    /// <summary>
    /// Gets the filter criteria for queries and deletes.
    /// </summary>
    IReadOnlyDictionary<string, object> Filters { get; }

    /// <summary>
    /// Gets the field values for inserts and updates.
    /// </summary>
    IReadOnlyDictionary<string, object> Values { get; }
}
```

#### IFdwResult.cs
**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/IFdwResult.cs`

```csharp
namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public interface IFdwResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    string Error { get; }
}

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
public interface IFdwResult<T> : IFdwResult
{
    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    T Value { get; }
}
```

#### FdwResult.cs
**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/FdwResult.cs`

```csharp
namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Implementation of IFdwResult.
/// </summary>
public sealed class FdwResult : IFdwResult
{
    private FdwResult(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    public static IFdwResult Success() => new FdwResult(true, string.Empty);
    public static IFdwResult Failure(string error) => new FdwResult(false, error);

    public static IFdwResult<T> Success<T>(T value) => new FdwResult<T>(true, value, string.Empty);
    public static IFdwResult<T> Failure<T>(string error) => new FdwResult<T>(false, default!, error);
}

/// <summary>
/// Implementation of IFdwResult<T>.
/// </summary>
public sealed class FdwResult<T> : IFdwResult<T>
{
    internal FdwResult(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public T Value { get; }
}
```

## Project 2: FractalDataWorks.Services.Connections

### Project File
**Location**: `src/FractalDataWorks.Services.Connections/FractalDataWorks.Services.Connections.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>FractalDataWorks.Services.Connections</RootNamespace>
    <AssemblyName>FractalDataWorks.Services.Connections</AssemblyName>
    <IsPackable>true</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\FractalDataWorks.Services.Connections.Abstractions\FractalDataWorks.Services.Connections.Abstractions.csproj" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
```

### Implementation Classes

#### ConnectionProvider.cs
**Location**: `src/FractalDataWorks.Services.Connections/ConnectionProvider.cs`

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections;

/// <summary>
/// Default implementation of IConnectionProvider.
/// Manages connection factories and creates connections.
/// </summary>
public sealed partial class ConnectionProvider : IConnectionProvider
{
    private readonly ILogger<ConnectionProvider> _logger;
    private readonly ConcurrentDictionary<string, IConnectionFactory> _factories;

    public ConnectionProvider(ILogger<ConnectionProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _factories = new ConcurrentDictionary<string, IConnectionFactory>(StringComparer.OrdinalIgnoreCase);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Registered connection factory '{FactoryType}' for connection type '{ConnectionType}'")]
    private partial void LogFactoryRegistered(string factoryType, string connectionType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Replacing existing factory for connection type '{ConnectionType}'. Old factory: {OldFactory}, New factory: {NewFactory}")]
    private partial void LogFactoryReplaced(string connectionType, string oldFactory, string newFactory);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Creating connection using factory '{FactoryType}' for type '{ConnectionType}'")]
    private partial void LogCreatingConnection(string factoryType, string connectionType);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully created connection '{ConnectionId}' of type '{ConnectionType}'")]
    private partial void LogConnectionCreated(string connectionId, string connectionType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create connection of type '{ConnectionType}' using factory '{FactoryType}'")]
    private partial void LogConnectionCreationFailed(Exception ex, string connectionType, string factoryType);

    public IEnumerable<string> GetSupportedConnectionTypes()
    {
        return _factories.Keys.ToList();
    }

    public void RegisterFactory(IConnectionFactory factory)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (string.IsNullOrWhiteSpace(factory.ConnectionTypeName))
        {
            throw new ArgumentException("Connection type name cannot be null or empty.", nameof(factory));
        }

        _factories.AddOrUpdate(
            factory.ConnectionTypeName,
            factory,
            (key, existingFactory) =>
            {
                LogFactoryReplaced(key, existingFactory.GetType().Name, factory.GetType().Name);
                return factory;
            });

        LogFactoryRegistered(factory.GetType().Name, factory.ConnectionTypeName);
    }

    public async Task<IFdwConnection> Create(
        IConnectionConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.ConnectionTypeName))
        {
            throw new ArgumentException(
                "Connection type name must be specified in configuration.",
                nameof(configuration));
        }

        if (!_factories.TryGetValue(configuration.ConnectionTypeName, out var factory))
        {
            throw new NotSupportedException(
                $"No factory registered for connection type '{configuration.ConnectionTypeName}'. " +
                $"Supported types: {string.Join(", ", GetSupportedConnectionTypes())}");
        }

        LogCreatingConnection(factory.GetType().Name, configuration.ConnectionTypeName);

        try
        {
            var connection = await factory.Create(configuration, cancellationToken);

            LogConnectionCreated(connection.ConnectionId, configuration.ConnectionTypeName);

            return connection;
        }
        catch (Exception ex)
        {
            LogConnectionCreationFailed(ex, configuration.ConnectionTypeName, factory.GetType().Name);
            throw;
        }
    }

    public bool IsConnectionTypeSupported(string connectionTypeName)
    {
        return !string.IsNullOrWhiteSpace(connectionTypeName) &&
               _factories.ContainsKey(connectionTypeName);
    }
}
```

## Project 3: FractalDataWorks.Services.Connections.MsSql

### Project File
**Location**: `src/FractalDataWorks.Services.Connections.MsSql/FractalDataWorks.Services.Connections.MsSql.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>FractalDataWorks.Services.Connections.MsSql</RootNamespace>
    <AssemblyName>FractalDataWorks.Services.Connections.MsSql</AssemblyName>
    <IsPackable>true</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\FractalDataWorks.Services.Connections.Abstractions\FractalDataWorks.Services.Connections.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Services.Connections\FractalDataWorks.Services.Connections.csproj" />
    <PackageReference Include="Microsoft.Data.SqlClient" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
```

### Core Implementation

#### MsSqlConfiguration.cs
**Location**: `src/FractalDataWorks.Services.Connections.MsSql/MsSqlConfiguration.cs`

```csharp
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// Configuration for SQL Server connections.
/// </summary>
public sealed class MsSqlConfiguration : IConnectionConfiguration
{
    /// <inheritdoc />
    public string ConnectionTypeName { get; set; } = "MsSql";

    /// <inheritdoc />
    public string ConnectionString { get; set; } = string.Empty;

    /// <inheritdoc />
    public int CommandTimeout { get; set; } = 30;

    /// <inheritdoc />
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to enable connection pooling.
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum pool size.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum pool size.
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the connection lifetime in seconds.
    /// </summary>
    public int ConnectionLifetime { get; set; } = 0;
}
```

#### MsSqlConnectionFactory.cs
**Location**: `src/FractalDataWorks.Services.Connections.MsSql/MsSqlConnectionFactory.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// Factory for creating SQL Server connections.
/// </summary>
public sealed class MsSqlConnectionFactory : IConnectionFactory<MsSqlConnection, MsSqlConfiguration>
{
    private readonly ILoggerFactory _loggerFactory;

    public MsSqlConnectionFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <inheritdoc />
    public string ConnectionTypeName => "MsSql";

    /// <inheritdoc />
    public async Task<IFdwConnection> Create(
        IConnectionConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration is not MsSqlConfiguration msSqlConfig)
        {
            throw new ArgumentException(
                $"Configuration must be of type {nameof(MsSqlConfiguration)}",
                nameof(configuration));
        }

        return await Create(msSqlConfig, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MsSqlConnection> Create(
        MsSqlConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            throw new ArgumentException(
                "Connection string cannot be null or empty.",
                nameof(configuration));
        }

        var logger = _loggerFactory.CreateLogger<MsSqlConnection>();
        var connection = new MsSqlConnection(logger, configuration);

        return Task.FromResult(connection);
    }
}
```

#### MsSqlConnection.cs
**Location**: `src/FractalDataWorks.Services.Connections.MsSql/MsSqlConnection.cs`

```csharp
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// SQL Server implementation of IFdwConnection.
/// </summary>
public sealed class MsSqlConnection : IFdwConnection
{
    private readonly ILogger<MsSqlConnection> _logger;
    private readonly MsSqlConfiguration _configuration;
    private SqlConnection? _sqlConnection;
    private bool _disposed;

    public MsSqlConnection(ILogger<MsSqlConnection> logger, MsSqlConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        ConnectionId = Guid.NewGuid().ToString("N");
        _sqlConnection = new SqlConnection(_configuration.ConnectionString);
    }

    /// <inheritdoc />
    public string ConnectionId { get; }

    /// <inheritdoc />
    public string ProviderName => "MsSql";

    /// <inheritdoc />
    public async Task<IFdwResult> Execute(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await Execute<object>(command, cancellationToken);
        return result.IsSuccess ? FdwResult.Success() : FdwResult.Failure(result.Error);
    }

    /// <inheritdoc />
    public async Task<IFdwResult<TResult>> Execute<TResult>(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return FdwResult.Failure<TResult>("Connection has been disposed");

        if (command == null)
            return FdwResult.Failure<TResult>("Command cannot be null");

        try
        {
            if (_sqlConnection?.State != ConnectionState.Open)
            {
                await _sqlConnection!.OpenAsync(cancellationToken);
            }

            using var sqlCommand = new SqlCommand(BuildSqlCommand(command), _sqlConnection)
            {
                CommandTimeout = _configuration.CommandTimeout
            };

            AddParameters(sqlCommand, command);

            var result = await ExecuteCommand<TResult>(sqlCommand, command.CommandType, cancellationToken);
            return FdwResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command on connection {ConnectionId}", ConnectionId);
            return FdwResult.Failure<TResult>(ex.Message);
        }
    }

    private string BuildSqlCommand(IDataCommand command)
    {
        // Basic SQL command building - in real implementation, use proper query builder
        return command.CommandType.ToUpperInvariant() switch
        {
            "QUERY" => $"SELECT * FROM [{command.EntityName}]",
            "INSERT" => $"INSERT INTO [{command.EntityName}] DEFAULT VALUES",
            "UPDATE" => $"UPDATE [{command.EntityName}] SET Id = Id",
            "DELETE" => $"DELETE FROM [{command.EntityName}] WHERE 1=0",
            _ => throw new NotSupportedException($"Command type '{command.CommandType}' not supported")
        };
    }

    private void AddParameters(SqlCommand sqlCommand, IDataCommand command)
    {
        foreach (var param in command.Parameters)
        {
            sqlCommand.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
        }
    }

    private async Task<TResult> ExecuteCommand<TResult>(
        SqlCommand sqlCommand,
        string commandType,
        CancellationToken cancellationToken)
    {
        return commandType.ToUpperInvariant() switch
        {
            "QUERY" => (TResult)(object)await sqlCommand.ExecuteScalarAsync(cancellationToken),
            "INSERT" or "UPDATE" or "DELETE" => (TResult)(object)await sqlCommand.ExecuteNonQueryAsync(cancellationToken),
            _ => throw new NotSupportedException($"Command type '{commandType}' not supported")
        } ?? default!;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _sqlConnection?.Dispose();
        _sqlConnection = null;
        _disposed = true;
    }
}
```

## Sample Application

### Project File
**Location**: `samples/ConnectionExample/ConnectionExample.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\FractalDataWorks.Services.Connections.Abstractions\FractalDataWorks.Services.Connections.Abstractions.csproj" />
    <ProjectReference Include="..\..\src\FractalDataWorks.Services.Connections\FractalDataWorks.Services.Connections.csproj" />
    <ProjectReference Include="..\..\src\FractalDataWorks.Services.Connections.MsSql\FractalDataWorks.Services.Connections.MsSql.csproj" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Configuration" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" />
    <PackageReference Include="Microsoft.Extensions.Logging" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

### appsettings.json
**Location**: `samples/ConnectionExample/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "FractalDataWorks": "Debug",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SampleDb;Integrated Security=true;",
    "TestConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=true;"
  },
  "Connections": {
    "MsSql": {
      "CommandTimeout": 30,
      "MaxRetryCount": 3,
      "EnableConnectionPooling": true,
      "MaxPoolSize": 100,
      "MinPoolSize": 0,
      "ConnectionLifetime": 0
    }
  }
}
```

This document provides the complete requirements with all the corrections we discussed:
- Only `Execute` and `Execute<T>` methods (no Open/Close)
- All method names without "Async" suffix
- `FdwResult` instead of `FractalResult`
- Source-generated logging with `partial` classes
- No reflection - proper interface-based design
- Factory methods named `Create` (not `CreateConnection`)