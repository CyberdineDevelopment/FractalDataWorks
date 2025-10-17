# FractalDataWorks.ServiceTypes

A plugin architecture framework that provides type-safe service registration, discovery, and factory creation with high-performance lookups through a progressive constraint system.

## Architecture Overview

The ServiceTypes framework provides a structured approach to defining and managing service types using:

- **ServiceTypeBase hierarchy** - Abstract base classes for service implementations
- **IServiceType interfaces** - Storage contracts with progressive generic constraints  
- **ServiceTypeCollectionBase** - High-performance collections with O(1) lookups
- **Attributes** - Source generator integration for automatic discovery

## Core Components

### Dual-Purpose Interface System

The interface system serves **two critical purposes**:

#### Purpose 1: Generic Constraint Refinement
Domain-specific interfaces like `IConnectionType<TService, TConfiguration, TFactory>` constrain generics to specific types:

```csharp
// Generic interface constrains to connection-specific types
public interface IConnectionType<TService, TConfiguration, TFactory> : IServiceType<TService, TFactory, TConfiguration>
    where TService : class, IGenericConnection              // Must be connection service
    where TConfiguration : class, IConnectionConfiguration   // Must be connection config
    where TFactory : class, IConnectionFactory<TService, TConfiguration> // Must be connection factory

// Implementation gets constrained generics
public sealed class MsSqlConnectionType : 
    ConnectionTypeBase<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>,
    IConnectionType<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>
```

#### Purpose 2: Type Discovery and Metadata Access
Collections store the base interface but can discover generic types, configurations, and factories:

```csharp
// Store as base interface
IConnectionType connectionType = MsSqlConnectionType.Instance;

// Collection can discover the specific types
Type serviceType = connectionType.ServiceType;           // IGenericConnection
Type configurationType = connectionType.ConfigurationType; // MsSqlConfiguration  
Type factoryType = connectionType.FactoryType;           // IMsSqlConnectionFactory
string category = connectionType.Category;               // "Database"
```

### Interface Hierarchy

```csharp
IServiceType                                    // Base storage contract (Id, Name)
├── IServiceType<TService>                     // + Service type constraint
├── IServiceType<TService, TConfiguration>     // + Configuration constraint  
└── IServiceType<TService, TFactory, TConfiguration> // + Factory constraint

// Domain-specific interfaces add specific constraints
IConnectionType : IServiceType                 // Connection base
└── IConnectionType<TService, TConfiguration, TFactory> : IServiceType<TService, TFactory, TConfiguration>
```

### ServiceTypeBase Hierarchy

```csharp
ServiceTypeBase                                    // Abstract base implementation
├── ServiceTypeBase<TService>                     // + Service type
├── ServiceTypeBase<TService, TConfiguration>     // + Configuration
└── ServiceTypeBase<TService, TFactory, TConfiguration> // + Factory
```

### ServiceTypeCollectionBase

Generic collection providing:
- **Storage**: Base interfaces (`IServiceType`, `IConnectionType`)
- **Discovery**: Access to generic types, configurations, factories
- **Performance**: O(1) lookups by ID, name, service type, configuration type
- **Source generation**: Automatic population from discovered types

## Implementation Rules

### Rule 1: Dual Inheritance Pattern
**Every service type MUST inherit from base class AND implement interface**

```csharp
// ✅ CORRECT - Both base class and interface
public sealed class MsSqlConnectionType : 
    ConnectionTypeBase<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>,
    IConnectionType<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>,
    IConnectionType

// ❌ WRONG - Missing interface implementation
public sealed class BadConnectionType : 
    ConnectionTypeBase<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>
```

### Rule 2: Singleton Pattern with Static Instance
**Service types MUST use singleton pattern**

```csharp
public sealed class MsSqlConnectionType : ConnectionTypeBase<...>
{
    // ✅ CORRECT - Static singleton instance
    public static MsSqlConnectionType Instance { get; } = new();
    
    // ✅ CORRECT - Private constructor
    private MsSqlConnectionType() : base(id: 2, name: "MsSql", category: "Database") { }
}
```

### Rule 3: Unique Sequential IDs
**Each service type MUST have unique integer ID**

```csharp
// ✅ CORRECT - Unique IDs across the domain
private MsSqlConnectionType() : base(id: 1, name: "MsSql") { }
private PostgreSqlConnectionType() : base(id: 2, name: "PostgreSql") { }
private RestConnectionType() : base(id: 3, name: "Rest") { }

// ❌ WRONG - Duplicate IDs
private MsSqlConnectionType() : base(id: 1, name: "MsSql") { }
private PostgreSqlConnectionType() : base(id: 1, name: "PostgreSql") { } // Duplicate!
```

### Rule 4: Generic Type Constraints
**Generic parameters MUST follow framework constraints**

```csharp
// ✅ CORRECT - Proper constraints for connections
public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory> : 
    ServiceTypeBase<TService, TFactory, TConfiguration>
    where TService : class, IGenericConnection          // Must implement IGenericConnection
    where TConfiguration : class, IConnectionConfiguration // Must implement IConnectionConfiguration  
    where TFactory : class, IConnectionFactory<TService, TConfiguration> // Must implement factory interface
```

### Rule 5: ServiceTypeCollection Attribute
**Collections MUST use ServiceTypeCollection attribute with explicit targeting**

```csharp
// ✅ CORRECT - Attribute with base type, interface type, and collection type
[ServiceTypeCollection(typeof(ConnectionTypeBase<,,>), typeof(IConnectionType), typeof(ConnectionTypes))]
public partial class ConnectionTypesBase : ServiceTypeCollectionBase<...>
{
    // Source generator populates this class using explicit collection targeting
}
```

### Rule 6: TypeLookup Attribute for Properties
**Properties used for lookups MUST have TypeLookup attribute**

```csharp
public abstract class ServiceTypeBase
{
    // ✅ CORRECT - Generates GetByServiceType method
    [TypeLookup("GetByServiceType")]
    public abstract Type ServiceType { get; }
    
    // ✅ CORRECT - Generates GetByConfigurationType method  
    [TypeLookup("GetByConfigurationType")]
    public abstract Type ConfigurationType { get; }
}
```

### Rule 7: Required Abstract Method Implementations
**Service types MUST implement all abstract methods**

```csharp
public sealed class MsSqlConnectionType : ConnectionTypeBase<...>
{
    // ✅ REQUIRED - Configuration section name
    public override string SectionName => "MsSql";
    
    // ✅ REQUIRED - Display name for UI
    public override string DisplayName => "Microsoft SQL Server";
    
    // ✅ REQUIRED - Description of capabilities
    public override string Description => "High-performance connection to Microsoft SQL Server databases with query translation and connection pooling.";
    
    // ✅ REQUIRED - Register services with DI container
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMsSqlConnectionFactory, MsSqlConnectionFactory>();
        services.AddScoped<MsSqlService>();
        services.AddScoped<MsSqlCommandTranslator>();
    }
    
    // ✅ REQUIRED - Configure service at startup
    public override void Configure(IConfiguration configuration)
    {
        // Validate connection strings, configure pools, etc.
    }
}
```

### Rule 8: ServiceTypeOption Attribute for Explicit Collection Targeting
**Service type options MUST use ServiceTypeOption attribute with explicit collection targeting**

```csharp
// ✅ CORRECT - Attribute with explicit collection type and name
[ServiceTypeOption(typeof(ConnectionTypes), "MsSql")]
public sealed class MsSqlConnectionType :
    ConnectionTypeBase<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>,
    IConnectionType<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>
{
    public static MsSqlConnectionType Instance { get; } = new();
    private MsSqlConnectionType() : base(id: 2, name: "MsSql", category: "Database") { }
    // ... implementation
}

// ❌ WRONG - Missing ServiceTypeOption attribute
public sealed class BadConnectionType : ConnectionTypeBase<...>
{
    // Missing: [ServiceTypeOption(typeof(ConnectionTypes), "BadConnection")]
}
```

**Performance Benefits:**
- O(types_with_attribute) vs O(collections × assemblies × all_types) discovery
- Eliminates expensive inheritance scanning across all assemblies
- Direct collection type targeting for faster source generation

## Complete Example: SQL Server Connection Service

### Project Structure
```
FractalDataWorks.Services.Connections.Abstractions/
├── ConnectionTypeBase.cs           (Rule 1: Base class)
├── ConnectionTypeCollectionBase.cs (Rule 5: Collection base)
├── IConnectionType.cs             (Rule 1: Interface contract)
└── ConnectionTypesBase.cs         (Rule 5: Concrete collection)

FractalDataWorks.Services.Connections.MsSql/
├── MsSqlConnectionType.cs         (Rule 1-7: Complete implementation)
├── MsSqlConfiguration.cs          (Rule 4: Configuration constraint)
├── IMsSqlConnectionFactory.cs     (Rule 4: Factory constraint)
└── MsSqlService.cs               (Rule 4: Service constraint)
```

### 1. Abstract Base Class (in Abstractions project)

```csharp
// ConnectionTypeBase.cs - Rule 1 & 4: Base class with constraints
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.Connections.Abstractions;

public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory> : 
    ServiceTypeBase<TService, TFactory, TConfiguration>,    // Rule 1: Inherit base class
    IConnectionType<TService, TConfiguration, TFactory>,    // Rule 1: Implement generic interface
    IConnectionType                                         // Rule 1: Implement non-generic interface
    where TService : class, IGenericConnection              // Rule 4: Service constraint
    where TConfiguration : class, IConnectionConfiguration   // Rule 4: Configuration constraint  
    where TFactory : class, IConnectionFactory<TService, TConfiguration> // Rule 4: Factory constraint
{
    protected ConnectionTypeBase(int id, string name, string? category = null)
        : base(id, name, category ?? "Connection")          // Rule 3: Pass unique ID
    {
    }
}
```

### 2. Collection Base (in Abstractions project)

```csharp
// ConnectionTypeCollectionBase.cs - Rule 5: Collection with attribute
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

[ServiceTypeCollection(typeof(ConnectionTypeBase<,,>), typeof(IConnectionType), typeof(ConnectionTypes))]  // Rule 5: Collection attribute
public partial class ConnectionTypesBase : 
    ConnectionTypeCollectionBase<
        ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>,
        ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>,
        IGenericConnection,
        IConnectionConfiguration,
        IConnectionFactory<IGenericConnection, IConnectionConfiguration>>
{
    // Source generator will populate with discovered connection types
}
```

### 3. Concrete Implementation (in MsSql project)

```csharp
// MsSqlConnectionType.cs - Rules 1-8: Complete implementation
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.Connections.MsSql;

[ServiceTypeOption(typeof(ConnectionTypes), "MsSql")]                                   // Rule 8: ServiceTypeOption with collection targeting
public sealed class MsSqlConnectionType :
    ConnectionTypeBase<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>, // Rule 1: Base class
    IConnectionType<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>,    // Rule 1: Generic interface
    IConnectionType                                                                      // Rule 1: Non-generic interface
{
    public static MsSqlConnectionType Instance { get; } = new();                        // Rule 2: Singleton

    private MsSqlConnectionType() : base(id: 2, name: "MsSql", category: "Database")   // Rule 2 & 3: Private constructor with unique ID
    {
    }
    
    // Rule 7: Required property implementations
    public override string SectionName => "MsSql";
    public override string DisplayName => "Microsoft SQL Server";  
    public override string Description => "High-performance connection to Microsoft SQL Server databases.";
    
    // Rule 7: Required method implementations
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMsSqlConnectionFactory, MsSqlConnectionFactory>();
        services.AddScoped<MsSqlService>();
    }
    
    public override void Configure(IConfiguration configuration)
    {
        // SQL Server specific configuration
    }
}
```

### 4. Supporting Types (in MsSql project)

```csharp
// MsSqlConfiguration.cs - Rule 4: Configuration constraint satisfaction
public sealed class MsSqlConfiguration : IConnectionConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
}

// IMsSqlConnectionFactory.cs - Rule 4: Factory constraint satisfaction
public interface IMsSqlConnectionFactory : IConnectionFactory<IGenericConnection, MsSqlConfiguration>
{
    // SQL Server specific factory methods
}
```

## Usage Examples

### Interface-Based Storage and Constraints
```csharp
// Store as base interface for polymorphic collections
IConnectionType connectionType = MsSqlConnectionType.Instance;
List<IConnectionType> allConnections = new() { connectionType };

// Use constrained interface for type-safe operations
public void ConfigureConnection<T>(IConnectionType<IGenericConnection, T, IConnectionFactory<IGenericConnection, T>> connectionType)
    where T : class, IConnectionConfiguration
{
    // Compiler guarantees T is IConnectionConfiguration
    // Can safely cast connectionType.ConfigurationType to typeof(T)
}

// Interface enables dependency injection without concrete types
public class ConnectionManager
{
    private readonly IConnectionType _connectionType;
    
    public ConnectionManager(IConnectionType connectionType)  // Inject interface, not concrete type
    {
        _connectionType = connectionType;
    }
    
    public void CreateConnection()
    {
        // Access discovered metadata through interface
        var factoryType = _connectionType.FactoryType;        // IMsSqlConnectionFactory
        var configType = _connectionType.ConfigurationType;   // MsSqlConfiguration
        var factory = serviceProvider.GetService(factoryType);
    }
}
```

### Service Registration
```csharp
// Register all connection types discovered by source generator
services.AddServiceTypes<ConnectionTypesBase>();

// Register specific connection type
MsSqlConnectionType.Instance.Register(services);

// Register by interface for dependency injection
services.AddSingleton<IConnectionType>(MsSqlConnectionType.Instance);
```

### Service Discovery Through Collections
```csharp
// Get all connection types as interfaces
IReadOnlyList<IConnectionType> connectionTypes = ConnectionTypes.All;

// Get specific connection type by ID
IConnectionType sqlServer = ConnectionTypes.GetById(2);

// Get connection type by name  
IConnectionType sqlServer = ConnectionTypes.GetByName("MsSql");

// Discover types at runtime through interface metadata
foreach (var connectionType in connectionTypes)
{
    Type serviceType = connectionType.ServiceType;           // What service it provides
    Type configType = connectionType.ConfigurationType;     // What config it needs  
    Type factoryType = connectionType.FactoryType;          // What factory creates it
    string category = connectionType.Category;              // How it's categorized
    
    // Use reflection to create instances based on discovered metadata
    var factory = serviceProvider.GetService(factoryType);
    var config = configuration.GetSection(connectionType.SectionName).Get(configType);
}
```

### Configuration Binding with Interface Metadata
```csharp
// appsettings.json
{
  "MsSql": {
    "ConnectionString": "...",
    "CommandTimeout": 60
  }
}

// Bind configuration using interface metadata discovery
public void ConfigureFromInterface(IConnectionType connectionType)
{
    var sectionName = connectionType.SectionName;           // "MsSql"  
    var configurationType = connectionType.ConfigurationType; // typeof(MsSqlConfiguration)
    
    // Dynamic configuration binding based on discovered metadata
    var configSection = configuration.GetSection(sectionName);
    var configInstance = configSection.Get(configurationType);
    
    // Type-safe casting because interface contract guarantees compatibility
    if (configInstance is IConnectionConfiguration connectionConfig)
    {
        // Use the configuration
    }
}
```

## Rule Violations and Common Mistakes

### ❌ Missing Interface Implementation
```csharp
// WRONG - Only inherits base class
public sealed class BadType : ServiceTypeBase<IService, Factory, Config>
{
    // Missing: , IServiceType<IService, Factory, Config>, IServiceType
}
```

### ❌ Non-Singleton Pattern
```csharp
// WRONG - Public constructor allows multiple instances
public sealed class BadType : ServiceTypeBase<...>
{
    public BadType() : base(1, "Bad") { }  // Should be private
    // Missing: public static BadType Instance { get; } = new();
}
```

### ❌ Missing Required Attributes
```csharp
// WRONG - Missing ServiceTypeCollection attribute
public partial class BadCollection : ServiceTypeCollectionBase<...>
{
    // Missing: [ServiceTypeCollection(typeof(SomeTypeBase<,,>), typeof(ISomeType), typeof(SomeTypes))]
}

// WRONG - Missing ServiceTypeOption attribute
public sealed class BadServiceType : ServiceTypeBase<...>
{
    // Missing: [ServiceTypeOption(typeof(SomeTypes), "BadService")]
}
```

### ❌ Constraint Violations
```csharp
// WRONG - Service doesn't implement required interface
public sealed class BadConnection : 
    ConnectionTypeBase<SomeService, Config, Factory>  // SomeService must implement IGenericConnection
```

## Source Generator Integration

The framework uses source generators to automatically:

1. **Discover Service Types** - Find all classes inheriting from ServiceTypeBase
2. **Generate Collections** - Create high-performance FrozenDictionary lookups  
3. **Generate Lookup Methods** - Create methods based on TypeLookup attributes
4. **Validate Rules** - Ensure all rules are followed at compile-time

## Best Practices

1. **One Service Type Per File** - Keep service type definitions focused
2. **Descriptive Categories** - Use meaningful category names for organization
3. **Comprehensive Registration** - Register all required services in Register method
4. **Validation in Configure** - Validate configurations and fail fast
5. **Consistent Naming** - Use consistent naming patterns across service types
6. **Documentation** - Provide clear descriptions of capabilities and use cases