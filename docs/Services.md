# FractalDataWorks.Services Documentation

## Overview

The FractalDataWorks.Services library provides the core foundation for building service-oriented architectures within the framework. It implements a sophisticated pattern that combines dependency injection, configuration management, and command execution with comprehensive type safety and Railway-Oriented Programming through the Result pattern.

## Core Components

### ServiceBase<TCommand, TConfiguration, TService>

The abstract base class that all services inherit from. It provides:

- **Generic Type Parameters:**
  - `TCommand`: The command type this service executes (`where TCommand : ICommand`)
  - `TConfiguration`: Configuration type for the service (`where TConfiguration : IFdwConfiguration`)
  - `TService`: The concrete service type for logging and identification (`where TService : class`)

- **Key Properties:**
  - `Id`: Unique identifier for the service instance (string)
  - `ServiceType`: The display name of the service type (string)
  - `IsAvailable`: Indicates if the service is ready for use (bool)
  - `Name`: Service name from configuration or type name
  - `Configuration`: Strongly-typed configuration instance

**Note:** The `TService` parameter uses a loose `class` constraint to allow maximum flexibility. Domain-specific implementations typically add more restrictive constraints.

- **Core Methods:**
  - `Execute(TCommand command)`: Execute commands with result pattern
  - `Execute<TOut>(TCommand command)`: Execute with generic return type
  - Overloads with `CancellationToken` support

### ServiceFactoryBase<TService, TConfiguration>

The factory pattern implementation for creating service instances:

- **Features:**
  - Uses `FastGenericNew` for high-performance instantiation
  - Automatic configuration validation via FluentValidation
  - Comprehensive logging with structured logging support
  - Type-safe service creation with proper error handling

- **Key Method:**
  - `Create(TConfiguration configuration)`: Creates service instances with validation

### ServiceFactoryProvider

Central provider for managing multiple service factories:

- Implements `IServiceFactoryProvider`
- Manages factory registration and retrieval
- Integrates with dependency injection container

## Factory Pattern Options

The framework provides two approaches for service instantiation:

### GenericServiceFactory (Default Pattern)

For services that can be instantiated using FastGenericNew with just configuration injection:

```csharp
public sealed class EmailServiceType : ServiceTypeBase<EmailService, EmailConfiguration, GenericServiceFactory<EmailService, EmailConfiguration>>
{
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<GenericServiceFactory<EmailService, EmailConfiguration>>();
        services.AddScoped<EmailService>();
    }
}
```

**Benefits:**
- No custom factory code required
- High-performance instantiation via FastGenericNew
- Automatic configuration validation
- Consistent error handling and logging

### Custom Factory Pattern

For services requiring complex instantiation logic:

```csharp
public sealed class HttpConnectionType : ConnectionTypeBase<HttpConnection, HttpConfiguration, HttpConnectionFactory>
{
    // Custom factory implementation for connection pooling, HttpClient management, etc.
}

public class HttpConnectionFactory : ConnectionFactoryBase<HttpConnection, HttpConfiguration>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public override IFdwResult<HttpConnection> Create(HttpConfiguration config)
    {
        // Custom logic: connection pooling, HttpClient setup, etc.
        var httpClient = _httpClientFactory.CreateClient(config.ClientName);
        return new HttpConnection(config, httpClient);
    }
}
```

**When to Use Custom Factories:**
- Connection pooling requirements
- HttpClient management
- External service integration
- Resource lifecycle management
- Complex initialization beyond standard dependency injection

**Use GenericServiceFactory for the majority of services. Only create custom factories when you need specialized instantiation logic.**

## Message System

The library includes a comprehensive messaging system for service operations:

### Message Categories

1. **Factory Messages**
   - `ServiceCreatedSuccessfullyMessage`
   - `ServiceCreationFailedMessage`
   - `FastGenericCreationFailedMessage`
   - `ServiceTypeCastFailedMessage`

2. **Configuration Messages**
   - `ConfigurationCannotBeNullMessage`
   - `ConfigurationNotInitializedMessage`

3. **Registration Messages**
   - `ServiceRegisteredMessage`
   - `RegistrationFailedMessage`
   - `ConfigurationRegistryNotFoundMessage`

4. **Service Messages**
   - `InvalidCommandMessage`
   - `NoServiceTypesRegisteredMessage`
   - `ValidationFailedMessage`
   - `RecordNotFoundMessage`

All message collections are generated using source generators for type safety and consistency.

## Logging Infrastructure

### Structured Logging Components

- **ServiceBaseLog**: Core service operation logging
- **ServiceFactoryLog**: Factory creation and instantiation logging
- **ServiceProviderBaseLog**: Provider-level operation logging
- **ServiceRegistrationLog**: Registration and DI container logging
- **PerformanceMetrics**: Performance measurement utilities

All logging uses Microsoft.Extensions.Logging with structured logging patterns for optimal observability.

## Registration and Dependency Injection

### ServiceRegistrationOptions

Configuration class for service registration with the DI container:

```csharp
public class ServiceRegistrationOptions
{
    public ServiceLifetime Lifetime { get; set; }
    public bool RegisterFactory { get; set; }
    public bool RegisterAsInterface { get; set; }
}
```

### Extension Methods

`ServiceFactoryRegistrationExtensions` provides fluent API for registration:

```csharp
services.AddServiceFactory<TService, TConfiguration, TFactory>(options => {
    options.Lifetime = ServiceLifetime.Scoped;
    options.RegisterFactory = true;
});
```

## Configuration Integration

Services integrate with Microsoft.Extensions.Configuration:

- Configuration validation via FluentValidation
- Support for appsettings.json binding
- Environment-specific configuration support
- Configuration hot-reload capabilities

## Enhanced Enum Integration

The library uses Enhanced Enums for:

- Service lifetime options (Singleton, Scoped, Transient)
- Configuration registry types
- Service states and status codes

This provides intellisense support and compile-time safety for all enumeration values.

## Result Pattern Implementation

All service operations return `IFdwResult` or `IFdwResult<T>`:

- Railway-Oriented Programming pattern
- Success/Failure states with messages
- Chainable operations
- No exceptions for control flow

## Progressive Constraint Pattern

The framework implements a deliberate progressive constraint hierarchy that provides maximum flexibility at abstraction layers while enforcing domain-specific rules at implementation layers:

### Constraint Hierarchy (Least to Most Restrictive)

1. **Framework Abstractions** (`FractalDataWorks.Services.Abstractions`)
   - `IServiceFactory<TService>`: No constraints on TService
   - `IServiceFactory<TService, TConfiguration>`: Only constrains TConfiguration

2. **Concrete Framework** (`FractalDataWorks.Services`)
   - `ServiceBase<TCommand, TConfiguration, TService>`: TService only requires `class`
   - `ServiceFactory<TService, TConfiguration>`: TService only requires `class`

3. **Domain Abstractions** (e.g., `Services.Connections.Abstractions`)
   - `ConnectionServiceBase`: Requires `IConnectionCommand`, `IConnectionConfiguration`
   - `ConnectionTypeBase`: Requires `IFdwConnection`, `IConnectionFactory`

4. **Implementations** (e.g., `Services.Connections.MsSql`)
   - Concrete types with no generics
   - Full type safety with specific implementations

This pattern allows:
- Framework reuse for non-standard scenarios
- Domain-specific enforcement where needed
- Progressive type safety as you move toward implementation
- Maximum flexibility without sacrificing safety

## Key Design Patterns

1. **Abstract Factory Pattern**: ServiceFactoryBase for creating services
2. **Template Method Pattern**: ServiceBase with virtual methods for customization
3. **Strategy Pattern**: Command execution with different command types
4. **Repository Pattern**: Service provider for service instance management
5. **Builder Pattern**: Fluent configuration APIs
6. **Progressive Constraints**: Increasing type safety from abstraction to implementation

## Thread Safety

- All base classes are thread-safe for read operations
- Service instances maintain their own state
- Factory operations are thread-safe
- Proper use of CancellationToken throughout

## Performance Optimizations

- FastGenericNew for zero-allocation object creation
- Cached compiled expressions for high-performance instantiation
- Structured logging with minimal allocation
- Lazy initialization where appropriate

## Best Practices

1. Always validate configuration in factory Create method
2. Use structured logging for all operations
3. Return Result types instead of throwing exceptions
4. Implement proper disposal patterns
5. Use CancellationToken for async operations
6. Register services with appropriate lifetimes

## Integration Points

The Services library integrates with:

- **Collections**: For type collections and lookups
- **Configuration**: For settings management
- **EnhancedEnums**: For type-safe enumerations
- **Messages**: For structured messaging
- **Results**: For Railway-Oriented Programming
- **ServiceTypes**: For service discovery and registration