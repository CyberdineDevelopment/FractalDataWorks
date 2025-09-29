# FractalDataWorks.ServiceTypes Documentation

## Overview

The FractalDataWorks.ServiceTypes library provides a powerful abstraction for defining and discovering service types within a plugin architecture. It enables automatic discovery, registration, and configuration of services across multiple assemblies through source generation, creating a flexible and extensible service ecosystem.

## Core Components

### ServiceTypeBase<TService, TConfiguration, TFactory>

The foundational abstract class for service type definitions:

```csharp
public abstract class ServiceTypeBase<TService, TConfiguration, TFactory>
    where TService : class        // Minimal constraint - just reference type
    where TConfiguration : class  // Minimal constraint - just reference type
    where TFactory : class        // Minimal constraint - just reference type
{
    public int Id { get; }
    public string Name { get; }
    public virtual string Category { get; }

    [TypeLookup("GetByServiceType")]
    public Type ServiceType => typeof(TService);

    public Type ConfigurationType => typeof(TConfiguration);
    public Type FactoryType => typeof(TFactory);

    public abstract string SectionName { get; }
    public abstract string DisplayName { get; }
    public abstract string Description { get; }

    public abstract void Register(IServiceCollection services);
    public abstract void Configure(IConfiguration configuration);

    protected ServiceTypeBase(int id, string name, string? category = null);
}
```

**Key Features:**
- Generic type parameters for complete type safety
- Category-based hierarchical organization
- Automatic type discovery via source generation
- Integration with dependency injection
- Configuration management support

### IServiceType

Core interface for service types:

```csharp
public interface IServiceType
{
    int Id { get; }
    string Name { get; }
    string Category { get; }
    Type ServiceType { get; }
    Type ConfigurationType { get; }
    Type FactoryType { get; }
    string SectionName { get; }
    string DisplayName { get; }
    string Description { get; }

    void Register(IServiceCollection services);
    void Configure(IConfiguration configuration);
}
```

### ServiceTypeCollectionBase

Base class for source-generated collections:

```csharp
public abstract class ServiceTypeCollectionBase
{
    // Source generator populates with discovered service types
}
```

## Attributes

### ServiceTypeCollectionAttribute

Marks a class for service type collection generation:

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ServiceTypeCollectionAttribute : Attribute
{
    public string InterfaceName { get; }
    public string CollectionName { get; }

    public ServiceTypeCollectionAttribute(string interfaceName, string collectionName);
}
```

**Usage:**
```csharp
[ServiceTypeCollection("IConnectionType", "ConnectionTypes")]
public static partial class ConnectionTypes
{
    // Source generator provides implementation
}
```

### TypeLookupAttribute

Generates specialized lookup methods:

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class TypeLookupAttribute : Attribute
{
    public string MethodName { get; }

    public TypeLookupAttribute(string methodName);
}
```

## Service Type Pattern

### Define a Service Type

```csharp
public sealed class EmailServiceType : ServiceTypeBase<IEmailService, EmailConfiguration, IEmailServiceFactory>
{
    public static EmailServiceType Instance { get; } = new();

    private EmailServiceType() : base(1, "Email", "Communication") { }

    public override string SectionName => "Email";
    public override string DisplayName => "Email Service";
    public override string Description => "Provides email sending capabilities";

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IEmailServiceFactory, EmailServiceFactory>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<EmailValidator>();
    }

    public override void Configure(IConfiguration configuration)
    {
        var config = configuration.GetSection(SectionName).Get<EmailConfiguration>();
        if (config != null)
        {
            // Validate or prepare configuration
            config.Validate();
        }
    }
}
```

### Create the Collection

```csharp
[ServiceTypeCollection("IServiceType", "ServiceTypes")]
public static partial class ServiceTypes
{
    // Generated methods:
    // - public static IReadOnlyList<IServiceType> All { get; }
    // - public static IServiceType GetById(int id)
    // - public static IServiceType GetByName(string name)
    // - public static IEnumerable<IServiceType> GetByCategory(string category)
    // - public static void RegisterAll(IServiceCollection services)
}
```

## Registration Patterns

### Automatic Registration

Register all discovered service types:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Registers all discovered service types
        ServiceTypes.RegisterAll(services);
    }
}
```

### Selective Registration

Register specific service types:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register only specific categories
    var communicationServices = ServiceTypes.GetByCategory("Communication");
    foreach (var serviceType in communicationServices)
    {
        serviceType.Register(services);
    }

    // Or register by name
    var emailService = ServiceTypes.GetByName("Email");
    emailService?.Register(services);
}
```

### Custom Registration

Override registration for specific scenarios:

```csharp
ServiceTypes.RegisterAll(services, options =>
{
    options.BeforeRegister = (serviceType, services) =>
    {
        // Custom logic before registration
        Console.WriteLine($"Registering {serviceType.DisplayName}");
    };

    options.AfterRegister = (serviceType, services) =>
    {
        // Custom logic after registration
        if (serviceType.Name == "Email")
        {
            services.AddSingleton<IEmailQueue, InMemoryEmailQueue>();
        }
    };
});
```

## Configuration Management

### Configuration Structure

```json
{
  "ServiceTypes": {
    "Email": {
      "SmtpServer": "smtp.example.com",
      "Port": 587,
      "EnableSsl": true
    },
    "Storage": {
      "ConnectionString": "DefaultEndpointsProtocol=https;...",
      "ContainerName": "uploads"
    }
  }
}
```

### Configuration Loading

```csharp
public class ServiceTypeConfigurationLoader
{
    public void LoadConfigurations(IConfiguration configuration)
    {
        foreach (var serviceType in ServiceTypes.All)
        {
            var section = configuration.GetSection($"ServiceTypes:{serviceType.SectionName}");
            if (section.Exists())
            {
                serviceType.Configure(configuration);
            }
        }
    }
}
```

## Plugin Architecture

### Plugin Discovery

Service types enable plugin-based architectures:

```csharp
public class PluginLoader
{
    public void LoadPlugins(string pluginDirectory)
    {
        var pluginFiles = Directory.GetFiles(pluginDirectory, "*.Plugin.dll");

        foreach (var file in pluginFiles)
        {
            var assembly = Assembly.LoadFrom(file);
            // ServiceTypes automatically discovers types in loaded assemblies
        }

        // Re-trigger registration for newly discovered types
        ServiceTypes.RefreshCollection();
    }
}
```

### Plugin Implementation

Create a plugin by implementing service types:

```csharp
// In MyCompany.Plugin.dll
public sealed class CustomStorageType : ServiceTypeBase<IStorageService, CustomStorageConfig, IStorageFactory>
{
    public static CustomStorageType Instance { get; } = new();

    private CustomStorageType() : base(100, "CustomStorage", "Storage") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IStorageService, CustomStorageService>();
    }
}
```

## Categories and Organization

### Hierarchical Categories

Organize service types hierarchically:

```csharp
public abstract class DatabaseServiceType : ServiceTypeBase<IDatabaseService, DatabaseConfig, IDatabaseFactory>
{
    protected DatabaseServiceType(int id, string name)
        : base(id, name, "Data/Database") { }
}

public abstract class CacheServiceType : ServiceTypeBase<ICacheService, CacheConfig, ICacheFactory>
{
    protected CacheServiceType(int id, string name)
        : base(id, name, "Data/Cache") { }
}
```

### Category-Based Operations

```csharp
// Get all data-related services
var dataServices = ServiceTypes.GetByCategory("Data");

// Get specific subcategories
var databaseServices = ServiceTypes.All
    .Where(st => st.Category.StartsWith("Data/Database"));

// Register by category
foreach (var category in ServiceTypes.Categories)
{
    Console.WriteLine($"Registering category: {category}");
    var services = ServiceTypes.GetByCategory(category);
    // Custom registration logic per category
}
```

## Factory Pattern Integration

### Service Factory Creation

```csharp
public interface IServiceTypeFactory
{
    IService CreateService(IServiceType serviceType, IConfiguration configuration);
}

public class ServiceTypeFactory : IServiceTypeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public IService CreateService(IServiceType serviceType, IConfiguration configuration)
    {
        // Get the factory type
        var factory = _serviceProvider.GetService(serviceType.FactoryType);

        // Get configuration
        var config = configuration.GetSection(serviceType.SectionName)
            .Get(serviceType.ConfigurationType);

        // Create service instance
        return factory.Create(config);
    }
}
```

## Metadata and Discovery

### Service Capabilities

```csharp
public abstract class ServiceTypeWithCapabilities : ServiceTypeBase<IService, IConfiguration, IFactory>
{
    public abstract ServiceCapabilities Capabilities { get; }
}

[Flags]
public enum ServiceCapabilities
{
    None = 0,
    Async = 1,
    Batch = 2,
    Transaction = 4,
    Retry = 8
}
```

### Runtime Discovery

```csharp
public class ServiceDiscovery
{
    public void DiscoverCapabilities()
    {
        foreach (var serviceType in ServiceTypes.All)
        {
            Console.WriteLine($"{serviceType.DisplayName}:");
            Console.WriteLine($"  Category: {serviceType.Category}");
            Console.WriteLine($"  Configuration: {serviceType.ConfigurationType.Name}");
            Console.WriteLine($"  Factory: {serviceType.FactoryType.Name}");

            if (serviceType is ServiceTypeWithCapabilities withCaps)
            {
                Console.WriteLine($"  Capabilities: {withCaps.Capabilities}");
            }
        }
    }
}
```

## Source Generation

### ServiceTypeCollectionGenerator

The source generator:

1. **Discovers Types**: Finds all ServiceTypeBase derivatives
2. **Generates Collections**: Creates static collection classes
3. **Provides Lookups**: Generates efficient lookup methods
4. **Cross-Assembly**: Discovers types across assemblies

### Generated Code Example

```csharp
public static partial class ServiceTypes
{
    private static readonly Dictionary<int, IServiceType> _byId;
    private static readonly Dictionary<string, IServiceType> _byName;
    private static readonly Dictionary<Type, IServiceType> _byServiceType;
    private static readonly IReadOnlyList<IServiceType> _all;

    static ServiceTypes()
    {
        var items = new IServiceType[]
        {
            EmailServiceType.Instance,
            StorageServiceType.Instance,
            CacheServiceType.Instance
        };

        _all = items;
        _byId = items.ToDictionary(x => x.Id);
        _byName = items.ToDictionary(x => x.Name);
        _byServiceType = items.ToDictionary(x => x.ServiceType);
    }

    public static IServiceType GetByServiceType(Type serviceType)
        => _byServiceType.TryGetValue(serviceType, out var result) ? result : null;

    public static void RegisterAll(IServiceCollection services)
    {
        foreach (var serviceType in _all)
        {
            serviceType.Register(services);
        }
    }
}
```

## Constraint Hierarchy in ServiceTypes

### ServiceTypeCollectionBase - Most Restrictive

The collection base class enforces full interface compliance:

```csharp
public abstract class ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : class, IServiceType<TService, TConfiguration, TFactory>
    where TGeneric : IServiceType<TService, TConfiguration, TFactory>
    where TService : class, IGenericService                              // Must be a service
    where TConfiguration : class, IGenericConfiguration                  // Must be config
    where TFactory : class, IServiceFactory<TService, TConfiguration> // Must be factory
```

This ensures type collections maintain full type safety and interface compliance.

### Domain-Specific ServiceTypes

Domain implementations add their specific constraints:

```csharp
// Connection domain example
public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory>
    : ServiceTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IGenericConnection  // Domain-specific interface
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
```

### Design Rationale

1. **Base flexibility**: ServiceTypeBase uses minimal constraints for maximum reusability
2. **Collection strictness**: Collections enforce full interface compliance for type safety
3. **Domain specificity**: Each domain adds its required constraints
4. **Progressive enhancement**: Constraints become more specific as you move toward implementation

## Best Practices

1. **Singleton Pattern**: Service types should be singletons
2. **Unique IDs**: Ensure IDs are globally unique
3. **Meaningful Categories**: Use hierarchical categories
4. **Configuration Validation**: Validate in Configure method
5. **Lazy Registration**: Register services only when needed
6. **Documentation**: Document service capabilities
7. **Version Compatibility**: Consider versioning strategies

## Integration Examples

### With MediatR

```csharp
public abstract class HandlerServiceType : ServiceTypeBase<IRequestHandler, HandlerConfig, IHandlerFactory>
{
    public override void Register(IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(ServiceType);
        });
    }
}
```

### With AutoMapper

```csharp
public sealed class MappingServiceType : ServiceTypeBase<IMapper, MappingConfig, IMappingFactory>
{
    public override void Register(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
    }
}
```

### With Health Checks

```csharp
public abstract class HealthCheckServiceType : ServiceTypeBase<IHealthCheck, HealthCheckConfig, IHealthCheckFactory>
{
    public override void Register(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddTypeActivatedCheck<ServiceHealthCheck>(
                Name,
                failureStatus: HealthStatus.Degraded,
                tags: new[] { Category });
    }
}
```

## Advanced Scenarios

### Multi-Tenant Services

```csharp
public abstract class TenantServiceType : ServiceTypeBase<ITenantService, TenantConfig, ITenantFactory>
{
    public abstract string TenantIdClaim { get; }
    public abstract IsolationLevel IsolationLevel { get; }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped(provider =>
        {
            var context = provider.GetService<IHttpContextAccessor>();
            var tenantId = context.HttpContext.User.FindFirst(TenantIdClaim)?.Value;
            return CreateForTenant(tenantId);
        });
    }
}
```

### Feature Flags

```csharp
public abstract class FeatureFlaggedServiceType : ServiceTypeBase<IService, IConfiguration, IFactory>
{
    public abstract string FeatureFlag { get; }

    public override void Register(IServiceCollection services)
    {
        if (IsFeatureEnabled(FeatureFlag))
        {
            base.Register(services);
        }
    }
}
```