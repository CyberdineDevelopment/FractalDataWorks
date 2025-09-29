# ServiceTypes Implementation Guide

Complete guide showing the full implementation chain from service types to collections, including configuration storage and retrieval patterns.

## Project Structure and File Layout

```
FractalDataWorks.DeveloperKit/
├── src/
│   ├── FractalDataWorks.Abstractions/                          # Core interfaces
│   │   ├── FractalDataWorks.Abstractions.csproj
│   │   ├── IEnumOption.cs                                      # Base enum interface
│   │   └── IServiceType.cs                                     # Service type interfaces
│   │
│   ├── FractalDataWorks.ServiceTypes/                          # Core framework
│   │   ├── FractalDataWorks.ServiceTypes.csproj
│   │   ├── README.md                                          # Framework overview
│   │   ├── IMPLEMENTATION-GUIDE.md                           # This guide
│   │   ├── ServiceTypeBase.cs                                # Base implementation classes
│   │   ├── ServiceTypeCollectionBase.cs                      # Collection base class
│   │   └── Attributes/
│   │       ├── ServiceTypeCollectionAttribute.cs             # Marks collections for generation
│   │       └── TypeLookupAttribute.cs                        # Marks properties for lookup methods
│   │
│   ├── FractalDataWorks.ServiceTypes.SourceGenerators/         # Code generation
│   │   ├── FractalDataWorks.ServiceTypes.SourceGenerators.csproj
│   │   └── Generators/
│   │       └── ServiceTypeCollectionGenerator.cs             # Generates high-performance collections
│   │
│   ├── FractalDataWorks.Services.Abstractions/                 # Service framework base
│   │   ├── FractalDataWorks.Services.Abstractions.csproj
│   │   ├── IGenericService.cs                                     # Base service interface
│   │   ├── IServiceConfiguration.cs                          # Base configuration interface
│   │   ├── IServiceFactory.cs                                # Base factory interface
│   │   ├── ServiceLifetimeBase.cs                            # Service lifetime Enhanced Enum
│   │   └── ServiceLifetimeCollectionBase.cs                  # Service lifetime collection
│   │
│   ├── FractalDataWorks.Services.Connections.Abstractions/     # Connection framework
│   │   ├── FractalDataWorks.Services.Connections.Abstractions.csproj
│   │   ├── IConnectionType.cs                                # Connection service type interfaces
│   │   ├── ConnectionTypeBase.cs                             # Connection base implementation
│   │   ├── ConnectionTypeCollectionBase.cs                   # Connection collection base
│   │   ├── ConnectionTypesBase.cs                            # Concrete collection [ServiceTypeCollection]
│   │   ├── IGenericConnection.cs                                 # Base connection interface
│   │   ├── IConnectionConfiguration.cs                       # Base connection config
│   │   ├── IConnectionFactory.cs                             # Base connection factory
│   │   ├── States/
│   │   │   ├── ConnectionStateBase.cs                        # Connection state Enhanced Enum
│   │   │   ├── OpenConnectionState.cs                        # Specific states
│   │   │   ├── ClosedConnectionState.cs
│   │   │   └── ... (other connection states)
│   │   └── Messages/
│   │       ├── ConnectionMessage.cs                          # Connection message Enhanced Enum
│   │       ├── ConnectionFailedMessage.cs                     # Specific messages
│   │       └── ... (other connection messages)
│   │
│   ├── FractalDataWorks.Services.Connections.MsSql/           # Concrete SQL Server implementation
│   │   ├── FractalDataWorks.Services.Connections.MsSql.csproj
│   │   ├── MsSqlConnectionType.cs                            # Concrete service type (singleton)
│   │   ├── MsSqlConfiguration.cs                             # SQL Server configuration
│   │   ├── IMsSqlConnectionFactory.cs                        # SQL Server factory interface
│   │   ├── MsSqlConnectionFactory.cs                         # SQL Server factory implementation
│   │   ├── MsSqlService.cs                                   # SQL Server connection service
│   │   ├── MsSqlCommandTranslator.cs                         # SQL command translation
│   │   └── Commands/
│   │       ├── MsSqlConnectionTestCommand.cs                 # Test connection command
│   │       ├── MsSqlExternalConnectionCreateCommand.cs       # Create connection command
│   │       └── ... (other SQL Server commands)
│   │
│   ├── FractalDataWorks.Services.Connections.Rest/            # Concrete REST API implementation
│   │   ├── FractalDataWorks.Services.Connections.Rest.csproj
│   │   ├── RestConnectionType.cs                             # REST service type (singleton)
│   │   ├── RestConfiguration.cs                              # REST API configuration
│   │   ├── IRestConnectionFactory.cs                         # REST factory interface
│   │   ├── RestConnectionFactory.cs                          # REST factory implementation
│   │   └── RestService.cs                                    # REST connection service
│   │
│   └── ... (other service domain implementations)
│
├── samples/                                                    # Example applications
│   ├── ConnectionExample/
│   │   ├── ConnectionExample.csproj
│   │   ├── Program.cs                                        # Shows service type usage
│   │   ├── appsettings.json                                  # Configuration for all connection types
│   │   └── Services/
│   │       ├── ConnectionManagerService.cs                   # Service using IConnectionType
│   │       └── ConnectionConfigurationService.cs             # Dynamic config loading
│   │
│   └── PluginExample/
│       ├── PluginExample.csproj
│       ├── Program.cs                                        # Plugin discovery example
│       └── appsettings.json
│
└── Directory.Build.props                                       # Global build properties
```

## File Dependencies and References

```
Project Reference Flow:
FractalDataWorks.Abstractions  (foundational interfaces)
    ↑ (referenced by)
├── FractalDataWorks.ServiceTypes  (core framework)
├── FractalDataWorks.EnhancedEnums  (Enhanced Enum implementation)
└── FractalDataWorks.Services.Abstractions  (service base interfaces)
    ↑ (referenced by)
    └── FractalDataWorks.Services.Connections.Abstractions  (connection framework)
        ↑ (referenced by)
        ├── FractalDataWorks.Services.Connections.MsSql  (concrete SQL Server)
        ├── FractalDataWorks.Services.Connections.Rest  (concrete REST API)
        └── ... (other connection implementations)

Source Generator Dependencies:
FractalDataWorks.ServiceTypes.SourceGenerators
├── References: Microsoft.CodeAnalysis.CSharp
└── Generates code for projects that reference ServiceTypes with [ServiceTypeCollection] attributes
```

## 1. Connection Service Type Implementation Chain

### 1.1 Base Abstractions Layer

#### IConnectionType Interface
```csharp
// File: FractalDataWorks.Services.Connections.Abstractions/IConnectionType.cs
namespace FractalDataWorks.Services.Connections.Abstractions;

// Generic interface with connection-specific constraints
public interface IConnectionType<TService, TConfiguration, TFactory> : IServiceType<TService, TConfiguration, TFactory>
    where TService : class, IGenericConnection
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    // Connection-specific methods can be added here
}

// Non-generic storage interface
public interface IConnectionType : IServiceType
{
    // Non-generic connection-specific methods can be added here
}
```

#### ConnectionTypeBase Implementation
```csharp
// File: FractalDataWorks.Services.Connections.Abstractions/ConnectionTypeBase.cs
namespace FractalDataWorks.Services.Connections.Abstractions;

public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory> : 
    ServiceTypeBase<TService, TConfiguration, TFactory>,
    IConnectionType<TService, TConfiguration, TFactory>,
    IConnectionType
    where TService : class, IGenericConnection
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    protected ConnectionTypeBase(int id, string name, string? category = null)
        : base(id, name, category ?? "Connection")
    {
    }
}
```

#### ConnectionTypeCollectionBase
```csharp
// File: FractalDataWorks.Services.Connections.Abstractions/ConnectionTypeCollectionBase.cs
namespace FractalDataWorks.Services.Connections.Abstractions;

public abstract class ConnectionTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory> :
    ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : ConnectionTypeBase<TService, TConfiguration, TFactory>
    where TGeneric : ConnectionTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IGenericConnection
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    // Source generator will populate this collection
}
```

#### Concrete Collection with Source Generation
```csharp
// File: FractalDataWorks.Services.Connections.Abstractions/ConnectionTypesBase.cs
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

[ServiceTypeCollection("ConnectionTypeBase", "ConnectionTypes")]
public partial class ConnectionTypesBase : 
    ConnectionTypeCollectionBase<
        ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>,
        ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>,
        IGenericConnection,
        IConnectionConfiguration,
        IConnectionFactory<IGenericConnection, IConnectionConfiguration>>
{
    // Source generator will populate:
    // - Static instances for each discovered connection type
    // - High-performance FrozenDictionary lookups
    // - All() property with all connection types
    // - GetById(), GetByName(), GetByServiceType() methods
}
```

### 1.2 Concrete Implementation Layer

#### MsSql Configuration
```csharp
// File: FractalDataWorks.Services.Connections.MsSql/MsSqlConfiguration.cs
namespace FractalDataWorks.Services.Connections.MsSql;

public sealed class MsSqlConfiguration : IConnectionConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
    public bool EnableConnectionPooling { get; set; } = true;
    public int MaxPoolSize { get; set; } = 100;
}
```

#### MsSql Factory Interface
```csharp
// File: FractalDataWorks.Services.Connections.MsSql/IMsSqlConnectionFactory.cs
namespace FractalDataWorks.Services.Connections.MsSql;

public interface IMsSqlConnectionFactory : IConnectionFactory<IGenericConnection, MsSqlConfiguration>
{
    Task<IGenericConnection> CreateConnectionAsync(MsSqlConfiguration configuration, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(MsSqlConfiguration configuration, CancellationToken cancellationToken = default);
}
```

#### MsSql Service Type Implementation
```csharp
// File: FractalDataWorks.Services.Connections.MsSql/MsSqlConnectionType.cs
namespace FractalDataWorks.Services.Connections.MsSql;

public sealed class MsSqlConnectionType : 
    ConnectionTypeBase<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>
{
    // Singleton pattern
    public static MsSqlConnectionType Instance { get; } = new();
    
    private MsSqlConnectionType() : base(
        id: 1, 
        name: "MsSql", 
        category: "Database")
    {
    }
    
    // Required abstract property implementations
    public override string SectionName => "MsSql";
    public override string DisplayName => "Microsoft SQL Server";
    public override string Description => "High-performance connection to Microsoft SQL Server databases with advanced query translation, connection pooling, and transaction management.";
    
    // Service registration
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMsSqlConnectionFactory, MsSqlConnectionFactory>();
        services.AddScoped<MsSqlService>();
        services.AddScoped<MsSqlCommandTranslator>();
        services.AddScoped<ExpressionTranslator>();
    }
    
    // Configuration validation and setup
    public override void Configure(IConfiguration configuration)
    {
        var config = configuration.GetSection(SectionName).Get<MsSqlConfiguration>();
        
        if (config == null)
        {
            throw new InvalidOperationException($"MsSql configuration section '{SectionName}' not found");
        }
        
        if (string.IsNullOrWhiteSpace(config.ConnectionString))
        {
            throw new InvalidOperationException("MsSql ConnectionString is required");
        }
        
        // Additional validation logic
        ValidateConfiguration(config);
    }
    
    private static void ValidateConfiguration(MsSqlConfiguration config)
    {
        if (config.CommandTimeout < 0)
            throw new ArgumentException("CommandTimeout must be non-negative");
            
        if (config.MaxRetryAttempts < 0)
            throw new ArgumentException("MaxRetryAttempts must be non-negative");
            
        if (config.MaxPoolSize <= 0)
            throw new ArgumentException("MaxPoolSize must be positive");
    }
}
```

## 2. Configuration Storage and Retrieval

### 2.1 Configuration File Structure
```json
// File: appsettings.json
{
  "MsSql": {
    "ConnectionString": "Server=(local);Database=MyDb;Integrated Security=true;",
    "CommandTimeout": 60,
    "MaxRetryAttempts": 3,
    "EnableConnectionPooling": true,
    "MaxPoolSize": 50
  },
  "PostgreSql": {
    "ConnectionString": "Host=localhost;Database=MyDb;Username=user;Password=pass;",
    "CommandTimeout": 30,
    "MaxRetryAttempts": 2,
    "EnableConnectionPooling": true,
    "MaxPoolSize": 25
  },
  "RestApi": {
    "BaseUrl": "https://api.example.com",
    "ApiKey": "your-api-key",
    "Timeout": 30,
    "MaxRetryAttempts": 3
  }
}
```

### 2.2 Dynamic Configuration Loading by Service Type
```csharp
// Service that loads configuration for any connection type
public class ConnectionConfigurationService
{
    private readonly IConfiguration _configuration;
    
    public ConnectionConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    // Load configuration for specific connection type using interface metadata
    public T LoadConfiguration<T>(IConnectionType connectionType) where T : class, IConnectionConfiguration
    {
        // Use interface metadata to discover section name and configuration type
        var sectionName = connectionType.SectionName;
        var configurationType = connectionType.ConfigurationType;
        
        // Validate that T matches the discovered configuration type
        if (typeof(T) != configurationType)
        {
            throw new ArgumentException($"Generic type {typeof(T).Name} does not match connection type's configuration type {configurationType.Name}");
        }
        
        // Load and bind configuration
        var configSection = _configuration.GetSection(sectionName);
        var config = configSection.Get<T>();
        
        if (config == null)
        {
            throw new InvalidOperationException($"Configuration section '{sectionName}' not found or invalid for {connectionType.DisplayName}");
        }
        
        return config;
    }
    
    // Load configuration without knowing the type at compile time
    public object LoadConfiguration(IConnectionType connectionType)
    {
        var sectionName = connectionType.SectionName;
        var configurationType = connectionType.ConfigurationType;
        
        var configSection = _configuration.GetSection(sectionName);
        var config = configSection.Get(configurationType);
        
        if (config == null)
        {
            throw new InvalidOperationException($"Configuration section '{sectionName}' not found for {connectionType.DisplayName}");
        }
        
        return config;
    }
    
    // Validate all registered connection configurations
    public void ValidateAllConfigurations(IEnumerable<IConnectionType> connectionTypes)
    {
        foreach (var connectionType in connectionTypes)
        {
            try
            {
                // Load configuration to trigger validation
                var config = LoadConfiguration(connectionType);
                
                // Call the connection type's Configure method for validation
                connectionType.Configure(_configuration);
                
                Console.WriteLine($"✓ {connectionType.DisplayName} configuration is valid");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ {connectionType.DisplayName} configuration error: {ex.Message}");
                throw;
            }
        }
    }
}
```

### 2.3 Service Registration and Discovery
```csharp
// Extension method for registering all connection types
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectionTypes(this IServiceCollection services, IConfiguration configuration)
    {
        // Get all discovered connection types
        var connectionTypes = ConnectionTypesBase.All;
        
        foreach (var connectionType in connectionTypes)
        {
            // Register the connection type instance
            services.AddSingleton<IConnectionType>(connectionType);
            
            // Configure the connection type
            connectionType.Configure(configuration);
            
            // Register services required by this connection type
            connectionType.Register(services);
        }
        
        // Register configuration service
        services.AddSingleton<ConnectionConfigurationService>();
        
        return services;
    }
}

// Usage in Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register all connection types and their dependencies
builder.Services.AddConnectionTypes(builder.Configuration);

var app = builder.Build();
```

### 2.4 Runtime Service Type Discovery and Usage
```csharp
// Service that uses connection types dynamically
public class ConnectionManagerService
{
    private readonly IEnumerable<IConnectionType> _connectionTypes;
    private readonly ConnectionConfigurationService _configService;
    private readonly IServiceProvider _serviceProvider;
    
    public ConnectionManagerService(
        IEnumerable<IConnectionType> connectionTypes,
        ConnectionConfigurationService configService,
        IServiceProvider serviceProvider)
    {
        _connectionTypes = connectionTypes;
        _configService = configService;
        _serviceProvider = serviceProvider;
    }
    
    // Get connection type by name
    public IConnectionType? GetConnectionType(string name)
    {
        return _connectionTypes.FirstOrDefault(ct => ct.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    // Get all connection types in a specific category
    public IEnumerable<IConnectionType> GetConnectionTypesByCategory(string category)
    {
        return _connectionTypes.Where(ct => ct.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }
    
    // Create connection dynamically based on connection type
    public async Task<IGenericConnection> CreateConnectionAsync(string connectionTypeName)
    {
        var connectionType = GetConnectionType(connectionTypeName);
        if (connectionType == null)
        {
            throw new ArgumentException($"Connection type '{connectionTypeName}' not found");
        }
        
        // Load configuration using interface metadata
        var config = _configService.LoadConfiguration(connectionType);
        
        // Get factory using interface metadata
        var factoryType = connectionType.FactoryType;
        var factory = _serviceProvider.GetRequiredService(factoryType);
        
        // Use reflection to call CreateConnectionAsync (or implement a common interface)
        if (factory is IConnectionFactory<IGenericConnection, IConnectionConfiguration> genericFactory)
        {
            return await genericFactory.CreateConnectionAsync((IConnectionConfiguration)config);
        }
        
        throw new InvalidOperationException($"Factory {factoryType.Name} does not implement expected interface");
    }
    
    // List all available connection types with their metadata
    public void ListAvailableConnections()
    {
        Console.WriteLine("Available Connection Types:");
        Console.WriteLine("=" * 50);
        
        foreach (var connectionType in _connectionTypes.OrderBy(ct => ct.Category).ThenBy(ct => ct.Name))
        {
            Console.WriteLine($"Name: {connectionType.Name}");
            Console.WriteLine($"Category: {connectionType.Category}");
            Console.WriteLine($"Display Name: {connectionType.DisplayName}");
            Console.WriteLine($"Description: {connectionType.Description}");
            Console.WriteLine($"Service Type: {connectionType.ServiceType.Name}");
            Console.WriteLine($"Configuration Type: {connectionType.ConfigurationType.Name}");
            Console.WriteLine($"Factory Type: {connectionType.FactoryType.Name}");
            Console.WriteLine($"Configuration Section: {connectionType.SectionName}");
            Console.WriteLine();
        }
    }
}
```

## 3. Collection Usage Patterns

### 3.1 High-Performance Lookups (Generated by Source Generator)
```csharp
// The source generator creates these methods in ConnectionTypesBase
public partial class ConnectionTypesBase
{
    // Generated static instances
    public static readonly MsSqlConnectionType MsSql = MsSqlConnectionType.Instance;
    public static readonly PostgreSqlConnectionType PostgreSql = PostgreSqlConnectionType.Instance;
    public static readonly RestConnectionType Rest = RestConnectionType.Instance;
    
    // Generated collection
    private static readonly FrozenDictionary<int, IConnectionType> _byId = new Dictionary<int, IConnectionType>
    {
        { 1, MsSql },
        { 2, PostgreSql },
        { 3, Rest }
    }.ToFrozenDictionary();
    
    private static readonly FrozenDictionary<string, IConnectionType> _byName = new Dictionary<string, IConnectionType>
    {
        { "MsSql", MsSql },
        { "PostgreSql", PostgreSql },
        { "Rest", Rest }
    }.ToFrozenDictionary();
    
    // Generated lookup methods - O(1) performance
    public override IConnectionType GetById(int id) => _byId.GetValueOrDefault(id) ?? Empty;
    public override IConnectionType GetByName(string name) => _byName.GetValueOrDefault(name) ?? Empty;
    
    // Generated All property
    public override IReadOnlyList<IConnectionType> All { get; } = new List<IConnectionType>
    {
        MsSql, PostgreSql, Rest
    }.AsReadOnly();
}
```

### 3.2 Collection Usage Examples
```csharp
public class ConnectionTypeExamples
{
    public void DemonstrateCollectionUsage()
    {
        // Static access to connection types collection
        var connectionTypes = new ConnectionTypesBase();
        
        // Get all connection types
        var allConnections = connectionTypes.All;
        Console.WriteLine($"Found {allConnections.Count} connection types");
        
        // Fast lookups - O(1) performance
        var sqlServer = connectionTypes.GetById(1);
        var postgres = connectionTypes.GetByName("PostgreSql");
        
        // Filter by service type
        var databaseConnections = connectionTypes.GetByServiceType(typeof(IGenericConnection));
        
        // Filter by category
        var databaseTypes = allConnections.Where(ct => ct.Category == "Database");
        var apiTypes = allConnections.Where(ct => ct.Category == "API");
        
        // Iterate through all connection types
        foreach (var connectionType in allConnections)
        {
            Console.WriteLine($"{connectionType.DisplayName}: {connectionType.Description}");
            Console.WriteLine($"  Section: {connectionType.SectionName}");
            Console.WriteLine($"  Service: {connectionType.ServiceType.Name}");
            Console.WriteLine($"  Config: {connectionType.ConfigurationType.Name}");
            Console.WriteLine();
        }
    }
    
    // Example of polymorphic usage
    public void ProcessConnectionType(IConnectionType connectionType)
    {
        // Can work with any connection type without knowing its concrete implementation
        Console.WriteLine($"Processing {connectionType.DisplayName}");
        Console.WriteLine($"Category: {connectionType.Category}");
        Console.WriteLine($"Requires config type: {connectionType.ConfigurationType.Name}");
        
        // Type-specific logic can be handled through the interface
        if (connectionType.Category == "Database")
        {
            Console.WriteLine("This is a database connection - enabling transaction support");
        }
        else if (connectionType.Category == "API")
        {
            Console.WriteLine("This is an API connection - enabling retry policies");
        }
    }
}
```

## 4. Complete Implementation Flow

### 4.1 From Definition to Usage
```
1. Define domain interfaces (IConnectionType)
2. Create base classes (ConnectionTypeBase)
3. Create collection base (ConnectionTypeCollectionBase)
4. Mark collection for source generation ([ServiceTypeCollection])
5. Implement concrete service type (MsSqlConnectionType)
6. Source generator discovers and populates collection
7. Register services and validate configuration
8. Use through interfaces for runtime polymorphism
```

### 4.2 Configuration Flow
```
1. Define configuration class (MsSqlConfiguration)
2. Add configuration section to appsettings.json
3. ServiceType.Configure() validates configuration
4. ConfigurationService loads dynamically using interface metadata
5. Factory uses configuration to create service instances
```

### 4.3 Dependency Injection Flow
```
1. Register IConnectionType instances (not concrete types)
2. Inject IConnectionType or IEnumerable<IConnectionType>
3. Use interface metadata to discover types and create instances
4. Enables hot-swapping and plugin architectures
```

This complete implementation guide shows how the ServiceTypes framework enables a fully dynamic, type-safe plugin architecture where service types can be discovered, configured, and used entirely through interfaces while maintaining compile-time safety through progressive generic constraints.