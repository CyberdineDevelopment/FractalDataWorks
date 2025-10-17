# FractalDataWorks.DependencyInjection

Dependency injection abstractions and extensions for the FractalDataWorks framework.

## Current Status

**This project is currently empty and serves as a placeholder for future dependency injection functionality.**

The project contains:
- `AssemblyInfo.cs` - Contains commented-out smart generator attributes:
  ```csharp
  // using FractalDataWorks.SmartGenerators;
  // 
  // [assembly: EnableAssemblyScanner]
  ```
- `FractalDataWorks.DependencyInjection.csproj` - Basic project file with reference to `FractalDataWorks.Configuration`
- No actual implementation classes, interfaces, or executable code

## Overview

FractalDataWorks.DependencyInjection is planned to provide:
- Container-agnostic DI abstractions
- Service registration helpers
- Module/plugin system support
- Service discovery patterns
- Lifetime management utilities

## Planned Components (Not Yet Implemented)

### IServiceModule

Module pattern for organizing service registrations:
```csharp
public interface IServiceModule
{
    string Name { get; }
    string Version { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
```

### Service Registration Extensions

Fluent registration helpers:
```csharp
public static class ServiceCollectionExtensions
{
    // Register all FractalDataWorks services
    public static IServiceCollection AddFractalDataWorks(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<FractalDataWorksOptions>? configure = null)
    {
        var options = new FractalDataWorksOptions();
        configure?.Invoke(options);
        
        // Register core services
        services.AddGenericCore();
        services.AddGenericConfiguration(configuration);
        services.AddGenericServices();
        
        // Register optional modules
        if (options.UseConnections)
            services.AddGenericConnections();
        if (options.UseData)
            services.AddGenericData();
        
        return services;
    }
    
    // Register services with configuration
    public static IServiceCollection AddGenericService<TService, TImplementation, TConfiguration>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class, IFractalService<TConfiguration>
        where TImplementation : class, TService
        where TConfiguration : class, IFractalConfiguration
    {
        services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
        services.AddConfigurationRegistry<TConfiguration>();
        return services;
    }
}
```

### Configuration Registry Registration

```csharp
public static class ConfigurationRegistrationExtensions
{
    public static IServiceCollection AddConfigurationRegistry<T>(
        this IServiceCollection services,
        Func<IServiceProvider, IConfigurationRegistry<T>>? factory = null)
        where T : class, IFractalConfiguration
    {
        if (factory != null)
        {
            services.AddSingleton(factory);
        }
        else
        {
            services.AddSingleton<IConfigurationRegistry<T>>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var configs = configuration.GetSection(typeof(T).Name).Get<List<T>>() ?? new List<T>();
                return new InMemoryConfigurationRegistry<T>(configs);
            });
        }
        
        return services;
    }
}
```

### Service Discovery

```csharp
public interface IServiceDiscovery
{
    IEnumerable<Type> DiscoverServices(Assembly assembly);
    IEnumerable<Type> DiscoverServices(string assemblyPattern);
    void RegisterDiscoveredServices(IServiceCollection services);
}

public class ConventionBasedServiceDiscovery : IServiceDiscovery
{
    public IEnumerable<Type> DiscoverServices(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IFractalService<>)));
    }
}
```

## Planned Features

### Module System

```csharp
public abstract class ServiceModuleBase : IServiceModule
{
    public abstract string Name { get; }
    public virtual string Version => "1.0.0";
    
    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services);
        RegisterConfigurations(services, configuration);
        RegisterValidators(services);
    }
    
    protected abstract void RegisterServices(IServiceCollection services);
    protected virtual void RegisterConfigurations(IServiceCollection services, IConfiguration configuration) { }
    protected virtual void RegisterValidators(IServiceCollection services) { }
}

// Example module
public class CustomerServiceModule : ServiceModuleBase
{
    public override string Name => "Customer Services";
    
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
    }
    
    protected override void RegisterConfigurations(IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigurationRegistry<CustomerConfiguration>();
    }
}
```

### Service Factory Pattern

```csharp
public interface IServiceFactory<TService> where TService : IFractalService
{
    TService CreateService(string name);
    TService CreateService(int configurationId);
}

public class ServiceFactory<TService, TConfiguration> : IServiceFactory<TService>
    where TService : IFractalService<TConfiguration>
    where TConfiguration : class, IFractalConfiguration
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationRegistry<TConfiguration> _configurations;
    
    public TService CreateService(int configurationId)
    {
        var config = _configurations.Get(configurationId);
        if (config == null)
            throw new InvalidOperationException($"Configuration {configurationId} not found");
            
        return ActivatorUtilities.CreateInstance<TService>(_serviceProvider, config);
    }
}
```

### Decorator Pattern Support

```csharp
public static class DecoratorExtensions
{
    public static IServiceCollection Decorate<TInterface, TDecorator>(
        this IServiceCollection services)
        where TInterface : class
        where TDecorator : class, TInterface
    {
        var wrappedDescriptor = services.FirstOrDefault(
            s => s.ServiceType == typeof(TInterface));
            
        if (wrappedDescriptor == null)
            throw new InvalidOperationException($"Service {typeof(TInterface).Name} not registered");
            
        var objectFactory = ActivatorUtilities.CreateFactory(
            typeof(TDecorator),
            new[] { typeof(TInterface) });
            
        services.Replace(ServiceDescriptor.Describe(
            typeof(TInterface),
            s => (TInterface)objectFactory(s, new[] { s.CreateInstance(wrappedDescriptor) }),
            wrappedDescriptor.Lifetime));
            
        return services;
    }
}
```

## Usage Examples (Planned)

### Basic Registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddFractalDataWorks(Configuration, options =>
    {
        options.UseConnections = true;
        options.UseData = true;
        options.ValidateOnStartup = true;
    });
    
    // Register custom services
    services.AddGenericService<IOrderService, OrderService, OrderConfiguration>();
}
```

### Module Registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Discover and register modules
    var modules = DiscoverModules("MyApp.*.dll");
    foreach (var module in modules)
    {
        module.ConfigureServices(services, Configuration);
    }
}

private IEnumerable<IServiceModule> DiscoverModules(string pattern)
{
    var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, pattern)
        .Select(Assembly.LoadFrom);
        
    return assemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(IServiceModule).IsAssignableFrom(t) && !t.IsAbstract)
        .Select(t => (IServiceModule)Activator.CreateInstance(t)!)
        .OrderBy(m => m.Name);
}
```

### Service Decoration
```csharp
services.AddScoped<ICustomerService, CustomerService>();
services.Decorate<ICustomerService, CachedCustomerService>();
services.Decorate<ICustomerService, LoggingCustomerService>();

// Results in: LoggingCustomerService -> CachedCustomerService -> CustomerService
```

## Project Structure

```
src/FractalDataWorks.DependencyInjection/
├── AssemblyInfo.cs                 # Currently commented out
├── FractalDataWorks.DependencyInjection.csproj
└── README.md                       # This file
```

## Dependencies

Current project dependencies:
- `FractalDataWorks.Configuration` - Referenced but not actively used in current empty state

Planned dependencies (not yet added):
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- FractalDataWorks core abstractions (when available)

## Implementation Status

| Component | Status | Notes |
|-----------|--------|-------|
| IServiceModule | Planned | Interface design documented but not implemented |
| ServiceCollectionExtensions | Planned | Extension methods for fluent registration |
| ConfigurationRegistrationExtensions | Planned | Configuration registry helpers |
| IServiceDiscovery | Planned | Convention-based service discovery |
| ServiceModuleBase | Planned | Abstract base for modules |
| IServiceFactory | Planned | Factory pattern support |
| Decorator Extensions | Planned | Service decoration patterns |

## Coverage Exclusions

Since this project is currently empty, there are no code coverage exclusions needed. When implementation begins, consider excluding:
- Generated code (if any source generators are used)
- Assembly-level attributes
- Pure data transfer objects without logic

## Development Notes

This project is in the early planning/scaffolding phase. The existing documentation represents intended design that has not been implemented. Before beginning development:

1. Review the planned interfaces and patterns
2. Consider integration with existing FractalDataWorks framework patterns
3. Ensure alignment with FractalDataWorks.Configuration abstractions
4. Plan for Enhanced Enum integration if applicable

## Next Steps

To begin implementation:
1. Uncomment and configure `AssemblyInfo.cs` if assembly scanning is needed
2. Implement core `IServiceModule` interface
3. Add basic `ServiceCollectionExtensions` methods
4. Create unit tests for implemented functionality
5. Update this documentation to reflect actual implementation

## Contributing

This package is accepting contributions for:
- Core interface definitions
- Service registration helpers  
- Module system implementation
- Service discovery mechanisms
- Container adapters for other DI frameworks
- Unit and integration tests