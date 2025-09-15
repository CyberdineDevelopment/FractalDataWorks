# FractalDataWorks.Services.Connections

## Overview

The Connections framework provides ServiceType auto-discovery for database and service connections with universal command patterns that work across all backends.

## Features

- **ServiceType Auto-Discovery**: Add connection packages and they're automatically registered
- **Universal Commands**: Same interface works with SQL Server, PostgreSQL, MongoDB, REST APIs, etc.
- **Dynamic Connection Creation**: Connections created via factories, not DI registration
- **Source-Generated Collections**: High-performance type lookup with compile-time validation

## Quick Start

### 1. Install Packages

```xml
<ProjectReference Include="..\FractalDataWorks.Services.Connections\FractalDataWorks.Services.Connections.csproj" />
<ProjectReference Include="..\FractalDataWorks.Services.Connections.MsSql\FractalDataWorks.Services.Connections.MsSql.csproj" />
```

### 2. Register Services

```csharp
// Program.cs - Zero-configuration registration
builder.Services.AddScoped<IFdwConnectionProvider, FdwConnectionProvider>();

// Single line registers ALL discovered connection types
ConnectionTypes.Register(builder.Services);
```

### 3. Configure Connections

```json
{
  "Connections": {
    "Database": {
      "ConnectionType": "MsSql",
      "ConnectionId": "MainDatabase",
      "ConnectionString": "Server=localhost;Database=MyApp;Integrated Security=true;"
    }
  }
}
```

### 4. Use Universal Commands

```csharp
public class UserService
{
    private readonly IFdwConnectionProvider _connectionProvider;

    public UserService(IFdwConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IFdwResult<List<User>>> GetActiveUsersAsync()
    {
        var connectionResult = await _connectionProvider.GetConnection("Database");
        if (!connectionResult.IsSuccess)
            return FdwResult<List<User>>.Failure(connectionResult.Error);

        using var connection = connectionResult.Value;

        // Universal command - works with any backend
        var command = new QueryCommand("SELECT * FROM Users WHERE Active = @active")
            .WithParameter("active", true);

        return await connection.Execute<List<User>>(command);
    }
}
```

## Available Connection Types

| Package | Connection Type | Purpose |
|---------|----------------|---------|
| `FractalDataWorks.Services.Connections.MsSql` | MsSql | SQL Server databases |
| `FractalDataWorks.Services.Connections.PostgreSql` | PostgreSql | PostgreSQL databases |
| `FractalDataWorks.Services.Connections.Rest` | Rest | REST API endpoints |

## How Auto-Discovery Works

1. **Source Generator Scans**: `[ServiceTypeCollection]` attribute triggers compile-time discovery
2. **Finds Implementations**: Scans referenced assemblies for types inheriting from `ConnectionTypeBase`
3. **Generates Collections**: Creates `ConnectionTypes.All`, `ConnectionTypes.Name()`, etc.
4. **Self-Registration**: Each connection type handles its own DI registration

## Adding Custom Connection Types

```csharp
// 1. Create your connection type (singleton pattern)
public sealed class MongoDbConnectionType : ConnectionTypeBase<IFdwConnection, MongoDbConfiguration, IMongoDbConnectionFactory>
{
    public static MongoDbConnectionType Instance { get; } = new();

    private MongoDbConnectionType() : base(3, "MongoDb", "Database Connections") { }

    public override Type FactoryType => typeof(IMongoDbConnectionFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMongoDbConnectionFactory, MongoDbConnectionFactory>();
        services.AddScoped<MongoDbCommandTranslator>();
    }
}

// 2. Add package reference - source generator automatically discovers it
// 3. ConnectionTypes.Register(services) will include it automatically
```

## Architecture Benefits

- **Zero Configuration**: Add package reference, get functionality
- **Type Safety**: Compile-time validation of connection types
- **Performance**: Source-generated collections use FrozenDictionary
- **Scalability**: Each connection type manages its own dependencies
- **Testability**: Interface-based design supports easy mocking

For complete architecture details, see [Services.Abstractions README](../FractalDataWorks.Services.Abstractions/README.md).