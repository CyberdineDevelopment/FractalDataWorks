# FractalDataWorks Developer Kit

A comprehensive framework for building scalable, maintainable data-driven applications with universal command patterns, dependency injection, and source generation.

## 🏗️ Architecture Overview

The FractalDataWorks Developer Kit provides a complete architecture for building enterprise applications with these core principles:

### Universal Command Pattern
**Single syntax, multiple backends** - Write your data commands once, execute anywhere:

```csharp
// Same command works across all backends
IConnectionCommand query = new QueryCommand("SELECT * FROM users WHERE active = @active")
    .WithParameter("active", true);

// PostgreSQL: Translates to `SELECT * FROM "users" WHERE "active" = $1`
// SQL Server: Translates to `SELECT * FROM [users] WHERE [active] = @active`
// MongoDB: Translates to `db.users.find({"active": true})`
```

### Dynamic Connection Management
Connections are **NOT** registered in dependency injection. Instead, they're created dynamically:

```csharp
// ✅ Correct - Register factories and providers
services.AddScoped<IFdwConnectionProvider, FdwConnectionProvider>();
PostgreSqlConnectionType.Instance.Register(services); // Registers factory, not connection

// ❌ Wrong - Don't register connections directly
services.AddScoped<PostgreSqlConnection>(); // Never do this
```

### Source-Generated Discovery
Type discovery happens at compile time for maximum performance:

```csharp
// Generated static class provides O(1) lookup
var connectionType = ConnectionTypes.PostgreSql; // No reflection
var factory = serviceProvider.GetService(connectionType.FactoryType);
```

## 🚀 Key Features

### 🔧 ServiceTypes Framework
Standalone plugin architecture with **progressive constraint refinement**:

```csharp
// Base interface - minimal constraints
public interface IServiceType<TService>
    where TService : IFractalService
{ }

// More specific interface adds constraints
public interface IConnectionType<TConnection, TConfiguration, TFactory>
    : IServiceType<TConnection>
    where TConnection : IFdwConnection
    where TConfiguration : IFractalConfiguration
    where TFactory : IServiceFactory<TConnection, TConfiguration>
{ }
```

### 📦 Enhanced Enums
Type-safe enumerations with source-generated collections:

```csharp
public sealed class ConnectionState : EnhancedEnumBase<ConnectionState>
{
    public static readonly ConnectionState Closed = new("Closed", 0);
    public static readonly ConnectionState Open = new("Open", 1);
    public static readonly ConnectionState Connecting = new("Connecting", 2);
}

// Source-generated collection provides O(1) operations
var state = ConnectionStates.ByName("Open"); // No LINQ, no reflection
```

### 🔀 Railway-Oriented Programming
All operations return `IFdwResult<T>` for robust error handling:

```csharp
var connectionResult = await connectionProvider.GetConnection(config);
if (connectionResult.IsSuccess)
{
    using var connection = connectionResult.Value;
    var queryResult = await connection.Execute<User>(command);
    // Chain operations safely
}
```

### ⚡ Source-Generated Logging
High-performance logging with compile-time optimization:

```csharp
[LoggerMessage(EventId = 3001, Level = LogLevel.Debug,
    Message = "Creating PostgreSQL connection: {ConnectionId}")]
public static partial void CreatingConnection(ILogger logger, string connectionId);
```

### 🏭 Factory Pattern Integration
Services use factories for creation, enabling dynamic instantiation:

```csharp
public interface IServiceFactory<TService, TConfiguration>
    where TService : IFractalService
    where TConfiguration : IFractalConfiguration
{
    Task<IFdwResult<TService>> CreateServiceAsync(TConfiguration configuration);
}
```

## 📁 Project Structure

```
FractalDataWorks.DeveloperKit/
├── src/                              # Core framework packages
│   ├── FractalDataWorks.Abstractions/ # Base interfaces and types
│   ├── FractalDataWorks.Results/      # Railway-oriented programming
│   ├── FractalDataWorks.Collections/  # High-performance collections
│   ├── FractalDataWorks.EnhancedEnums/ # Type-safe enumerations
│   ├── FractalDataWorks.Messages/     # Messaging framework
│   ├── FractalDataWorks.ServiceTypes/ # Plugin architecture
│   ├── FractalDataWorks.Configuration/ # Configuration management
│   ├── FractalDataWorks.Services/     # Service base classes
│   ├── FractalDataWorks.Data/         # Data access patterns
│   └── FractalDataWorks.Services.Connections/ # Connection framework
├── samples/                          # Working examples
│   └── Services/Service.Implementation/ # Complete PostgreSQL example
└── docs/                            # Architecture documentation
```

## 🎯 Complete Working Sample

The `samples/Services/Service.Implementation` directory contains a **complete, runnable example** demonstrating:

### PostgreSQL Connection Implementation

```csharp
// 1. Connection Type (Singleton)
public sealed class PostgreSqlConnectionType : ConnectionTypeBase<IFdwConnection, PostgreSqlConfiguration, IPostgreSqlConnectionFactory>
{
    public static PostgreSqlConnectionType Instance { get; } = new();

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IPostgreSqlConnectionFactory, PostgreSqlConnectionFactory>();
        services.AddScoped<PostgreSqlCommandTranslator>();
    }
}

// 2. Command Translator (Universal → SQL)
public class PostgreSqlCommandTranslator
{
    public PostgreSqlTranslationResult Translate(IConnectionCommand command)
    {
        return command switch
        {
            IConnectionQueryCommand => TranslateQuery(command),
            IConnectionCreateCommand => TranslateCreate(command),
            // Universal interface, PostgreSQL-specific implementation
        };
    }
}

// 3. Connection Service
public sealed class PostgreSqlConnection : ConnectionServiceBase<IConnectionCommand, PostgreSqlConfiguration, PostgreSqlConnection>
{
    public override async Task<IFdwResult<T>> Execute<T>(IConnectionCommand command)
    {
        // 1. Translate universal command to PostgreSQL SQL
        var sql = _translator.Translate(command);

        // 2. Execute against PostgreSQL
        // 3. Return via Railway-Oriented Programming
    }
}
```

### Service Registration Pattern

```csharp
// In Program.cs
services.AddScoped<IFdwConnectionProvider, FdwConnectionProvider>();

// Register connection types (factories, not connections)
PostgreSqlConnectionType.Instance.Register(services);

// In production, register all available connection types:
foreach (var connectionType in ConnectionTypes.All())
{
    connectionType.Register(services);
}
```

### Dynamic Connection Usage

```csharp
var config = new PostgreSqlConfiguration
{
    ConnectionId = "MyDatabase",
    ConnectionString = "Host=localhost;Database=sample;Username=user;Password=pass;"
};

// ConnectionProvider creates connections dynamically
var connectionResult = await connectionProvider.GetConnection(config);

if (connectionResult.IsSuccess)
{
    using var connection = connectionResult.Value;
    var result = await connection.Execute<User>(universalCommand);
}
```

## 🏃‍♂️ Quick Start

### 1. Build the Framework

```bash
# Build all core packages to local NuGet
./scripts/localpack.ps1
```

### 2. Run the Sample

```bash
cd samples/Services/Service.Implementation/src/SampleApp
dotnet run
```

### 3. Expected Output

The sample demonstrates:
- **Service Registration** - How factories and translators are registered
- **Connection Creation** - Dynamic connection creation via `ConnectionProvider`
- **Universal Commands** - Same interface across all backends
- **Source-Generated Logging** - High-performance logging
- **Railway-Oriented Programming** - All operations return `IFdwResult`
- **Connection Lifecycle** - Open, execute, test, close operations

## 🔧 Configuration Examples

### Connection Configuration (`appsettings.json`)

```json
{
  "Connections": {
    "PostgreSqlDemo": {
      "ConnectionType": "PostgreSql",
      "ConnectionId": "DemoDatabase",
      "ConnectionString": "Host=localhost;Database=sample;Username=demo;Password=demo;",
      "CommandTimeout": 30,
      "MaxPoolSize": 10,
      "EnableRetry": true,
      "MaxRetryAttempts": 3
    }
  }
}
```

### Service Registration

```csharp
// Register the connection provider
services.AddScoped<IFdwConnectionProvider, FdwConnectionProvider>();

// Register all available connection types
foreach (var connectionType in ConnectionTypes.All())
{
    connectionType.Register(services);
}
```

## 🎨 Design Patterns Demonstrated

### 1. **Universal Command Pattern**
- Single interface works across all persistence systems
- Backend-specific translators handle conversion
- Application code remains unchanged when switching databases

### 2. **Factory Pattern**
- Services created via factories, not directly from DI
- Enables dynamic service creation based on configuration
- Supports multiple configurations per service type

### 3. **Plugin Architecture**
- ServiceTypes enable standalone plugins
- Progressive constraint refinement
- Compile-time type discovery

### 4. **Railway-Oriented Programming**
- All operations return `IFdwResult<T>`
- Explicit error handling without exceptions
- Chainable operations with failure propagation

### 5. **Source Generation**
- Compile-time collection generation for performance
- No runtime reflection or LINQ overhead
- Type-safe code generation

## 📚 Architecture Benefits

### Performance
- **Source-generated collections** eliminate runtime overhead
- **Compiled logging** removes string formatting costs
- **Direct type resolution** avoids reflection

### Maintainability
- **Universal commands** eliminate database-specific code
- **Railway-oriented programming** makes error handling explicit
- **Progressive constraints** catch errors at compile time

### Scalability
- **Dynamic connection creation** scales with load
- **Factory pattern** supports connection pooling
- **Plugin architecture** enables modular growth

### Testability
- **Dependency injection** enables easy mocking
- **Interface-based design** supports test doubles
- **Railway results** make testing error paths simple

## 🔄 Connection Lifecycle

```
Configuration → ConnectionProvider → Factory → Connection → Translator → Backend
      ↓               ↓              ↓           ↓            ↓          ↓
   JSON/Code    Dynamic Lookup   Create     Execute     Universal   SQL/NoSQL
                                          Command      Command    Specific
```

## 🚀 Advanced Features

### DataGateway Integration
The connection architecture integrates with DataGateway and DataSet/DataContainer patterns:

```csharp
// Same DataCommand syntax regardless of persistence system
var command = new DataCommand("GetActiveUsers")
    .WithParameter("status", "Active");

// Works with SQL databases, NoSQL, APIs, files, etc.
var result = await dataGateway.Execute<User>(command);
```

### Multi-Backend Support
Add new backends by implementing the connection interface:

```csharp
public sealed class MongoDbConnectionType : ConnectionTypeBase<IFdwConnection, MongoDbConfiguration, IMongoDbConnectionFactory>
{
    // Universal commands automatically work with MongoDB
    // Translator converts to MongoDB queries
}
```

## 📋 Prerequisites

- **.NET 10.0 SDK** (RC or later)
- **PostgreSQL** (optional - sample handles connection failures gracefully)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow the established patterns and conventions
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

## 🏷️ Version

Current version: `0.0.0-alpha` (Development build)

---

**Built with ❤️ by the FractalDataWorks team**