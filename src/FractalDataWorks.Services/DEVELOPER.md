# FractalDataWorks Services Architecture

## Overview

The FractalDataWorks Services framework provides a comprehensive, type-safe, and high-performance architecture for building service-oriented applications. It combines several powerful patterns:

- **Service Factory Pattern** for efficient service instantiation
- **Provider Pattern** for service orchestration
- **Enhanced Enums** for type-safe collections and options
- **Source Generation** for compile-time optimization
- **Railway-Oriented Programming** for consistent error handling

## Core Architecture Components

### 1. Service Hierarchy

The service architecture follows a layered inheritance pattern:

```
IGenericService (Base Interface)
    ↓
ServiceBase<TCommand, TConfiguration, TService> (Abstract Base)
    ↓
Domain-Specific Base (e.g., ConnectionServiceBase)
    ↓
Concrete Implementation (e.g., MsSqlService)
```

#### Base Service Interface (`IGenericService`)

Located in `FractalDataWorks.Abstractions/Services/Abstractions/IGenericService.cs`

**Note:** The interface location is split across two projects to manage dependencies:
- `IGenericService` (base) - in FractalDataWorks.Abstractions (no FluentValidation dependency, supports netstandard2.0)
- `IGenericService<T>` variants - also in FractalDataWorks.Abstractions

```csharp
public interface IGenericService
{
    string Id { get; }
    string ServiceType { get; }  // Note: string, not IServiceType
    bool IsAvailable { get; }

public interface IGenericService<TCommand, TConfiguration> : IGenericService
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
{
    IGenericResult<ICommandResult> Execute(TCommand command);
    Task<IGenericResult<ICommandResult>> ExecuteAsync(TCommand command);
}
```

#### Abstract Service Base

Located in `FractalDataWorks.Services/ServiceBase.cs`

Provides:
- Configuration management
- Logging infrastructure
- Command execution framework
- Service lifecycle management

### 2. Factory Pattern

The factory pattern provides high-performance service instantiation with type safety.

#### Factory Hierarchy

```
IServiceFactory (Non-generic base)
    ↓
IServiceFactory<TService> (Service-specific)
    ↓
IServiceFactory<TService, TConfiguration> (Fully typed)
```

#### Implementation Example

```csharp
public class GenericServiceFactory<TService, TConfiguration>
    : ServiceFactory<TService, TConfiguration>
    where TService : class, IGenericService
    where TConfiguration : class, IGenericConfiguration
{
    public override IGenericResult<TService> Create(TConfiguration configuration)
    {
        // Uses FastGenericNew for performance
        if (FastNew.TryCreateInstance<TService, TConfiguration>(configuration, out var service))
        {
            return GenericResult<TService>.Success(service);
        }

        // Fallback to Activator for edge cases
        // ...
    }
}
```

### 3. Provider Pattern

Providers orchestrate service creation by managing factory selection and configuration binding.

#### Provider Responsibilities

1. **Configuration Resolution** - From multiple sources (appsettings, database, etc.)
2. **Factory Selection** - Based on service type
3. **Instance Management** - Lifecycle and caching
4. **Error Aggregation** - Consistent error handling

#### Example: Connection Provider

```csharp
public sealed class GenericConnectionProvider : IGenericConnectionProvider
{
    public async Task<IGenericResult<IGenericConnection>> GetConnection(string configurationName)
    {
        // 1. Load configuration from appsettings
        var config = _configuration.GetSection($"Connections:{configurationName}");

        // 2. Determine connection type
        var connectionType = config["ConnectionType"];

        // 3. Resolve appropriate factory from DI
        var factory = _serviceProvider.GetService<IConnectionFactory>();

        // 4. Create connection instance
        return await factory.CreateConnectionAsync(configuration);
    }
}
```

## Enhanced Enums Pattern

Enhanced Enums provide compile-time safe, high-performance enum-like types with additional capabilities.

### Structure

```csharp
// Base class definition
public abstract class ServiceLifetimes : EnhancedEnum<ServiceLifetimes>
{
    // Source generator creates singleton instances
}

// Concrete implementations
public sealed class Singleton : ServiceLifetimes
{
    public override string Name => "Singleton";
    public override int Value => 1;
}

public sealed class Scoped : ServiceLifetimes
{
    public override string Name => "Scoped";
    public override int Value => 2;
}
```

### Source-Generated Collections

The source generator creates high-performance lookup collections:

```csharp
// Generated code
public abstract partial class ServiceLifetimes
{
    private static readonly Dictionary<string, ServiceLifetimes> _byName = new()
    {
        ["Singleton"] = Singleton.Instance,
        ["Scoped"] = Scoped.Instance,
        ["Transient"] = Transient.Instance
    };

    public static ServiceLifetimes FromName(string name) => _byName[name];
    public static bool TryFromName(string name, out ServiceLifetimes value)
        => _byName.TryGetValue(name, out value);
}
```

### Benefits

- **Type Safety** - No magic strings or integers
- **Performance** - Dictionary lookups are O(1)
- **Extensibility** - Add new values without modifying base
- **IntelliSense** - Full IDE support
- **Serialization** - Built-in JSON/XML support

## ServiceType and TypeCollection Pattern

ServiceTypes define metadata and registration logic for services, while TypeCollections provide centralized type discovery.

### ServiceType Pattern

Each service domain has a type definition:

```csharp
public sealed class MsSqlConnectionType : ConnectionTypeBase<MsSqlService, MsSqlConfiguration, MsSqlConnectionFactory>
{
    private static readonly Lazy<MsSqlConnectionType> _instance = new(() => new MsSqlConnectionType());
    public static MsSqlConnectionType Instance => _instance.Value;

    public override string Name => "MsSql";
    public override string DisplayName => "Microsoft SQL Server";
    public override IServiceLifetime ServiceLifetime => ServiceLifetimes.Scoped;

    public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register factory
        services.AddScoped<IConnectionFactory, MsSqlConnectionFactory>();

        // Register service
        services.AddScoped<MsSqlService>();

        // Register supporting services
        services.AddScoped<ISqlTranslator, TSqlTranslator>();
    }
}
```

### TypeCollection Pattern

Collections are source-generated from base classes marked with `[TypeCollection]`:

```csharp
[TypeCollection(typeof(IConnectionType), GenerateInterface = true)]
public abstract class ConnectionTypeCollectionBase : TypeCollectionBase<IConnectionType>
{
    // Source generator creates ConnectionTypes class
}
```

Generated collection:

```csharp
// Source-generated
public sealed class ConnectionTypes : ConnectionTypeCollectionBase
{
    private static readonly Dictionary<string, IConnectionType> _types = new()
    {
        ["MsSql"] = MsSqlConnectionType.Instance,
        ["PostgreSql"] = PostgreSqlConnectionType.Instance,
        ["MySql"] = MySqlConnectionType.Instance
    };

    public static IConnectionType GetByName(string name) => _types[name];
    public static IEnumerable<IConnectionType> GetAll() => _types.Values;
}
```

## Two-Project Architecture for Dependency Management

The framework uses a deliberate two-project split to manage dependencies:

### FractalDataWorks.Abstractions
- **Target Frameworks:** netstandard2.0, net10.0
- **No FluentValidation dependency**
- **Contains:** Base interfaces (IGenericService, ICommand)
- **RootNamespace:** FractalDataWorks
- **Purpose:** Maximum compatibility with older frameworks

### FractalDataWorks.Services.Abstractions
- **Can have FluentValidation dependency**
- **Contains:** Enhanced interfaces (ICommand<T>)
- **Purpose:** Rich functionality for modern frameworks

Both projects use the same namespace (`FractalDataWorks.Services.Abstractions`) for seamless usage, but maintain separate assemblies for dependency isolation.

## Progressive Constraint Hierarchy

The framework implements increasingly specific constraints as you move from abstractions to implementations:

### Level 1: Framework Abstractions (Most Flexible)
```csharp
// No constraints or minimal constraints
public interface IServiceFactory<TService> // No constraints
public interface IServiceFactory<TService, TConfiguration>
    where TConfiguration : IGenericConfiguration // Only config constrained
```

### Level 2: Concrete Framework (Basic Constraints)
```csharp
public abstract class ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
    where TService : class  // Just reference type
```

### Level 3: Domain Abstractions (Domain-Specific)
```csharp
public abstract class ConnectionServiceBase<TCommand, TConfiguration, TService>
    where TCommand : IConnectionCommand  // Specific command type
    where TConfiguration : class, IConnectionConfiguration
    where TService : class
```

### Level 4: Domain Implementations (Most Specific)
```csharp
public class MsSqlConnection : ConnectionServiceBase<MsSqlCommand, MsSqlConfiguration, MsSqlConnection>
// All concrete types, no generics
```

This pattern provides:
- Maximum flexibility at abstraction layers
- Domain-specific enforcement where needed
- Type safety that increases with specificity
- Ability to integrate non-standard implementations

## Implementation Guide

### Creating a New Service Domain

1. **Define Abstractions**

```csharp
// IMyDomainService.cs
public interface IMyDomainService : IGenericService<MyCommand, MyConfiguration>
{
    // Domain-specific methods
}

// MyDomainServiceBase.cs
public abstract class MyDomainServiceBase<TCommand, TConfiguration, TService>
    : ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : ICommand
    where TConfiguration : IMyDomainConfiguration
    where TService : IMyDomainService
{
    // Domain-specific base implementation
}
```

2. **Create ServiceType Collection**

```csharp
[TypeCollection(typeof(IMyDomainType), GenerateInterface = true)]
public abstract class MyDomainTypeCollectionBase : TypeCollectionBase<IMyDomainType>
{
}
```

3. **Implement Concrete Service**

```csharp
public sealed class ConcreteService : MyDomainServiceBase<MyCommand, MyConfiguration, ConcreteService>
{
    public ConcreteService(MyConfiguration configuration, ILogger<ConcreteService> logger)
        : base(configuration, logger)
    {
    }

    protected override IGenericResult<ICommandResult> ExecuteCore(MyCommand command)
    {
        // Implementation
    }
}
```

4. **Create ServiceType**

```csharp
public sealed class ConcreteServiceType : MyDomainTypeBase<ConcreteService, MyConfiguration, ConcreteFactory>
{
    private static readonly Lazy<ConcreteServiceType> _instance = new(() => new ConcreteServiceType());
    public static ConcreteServiceType Instance => _instance.Value;

    public override string Name => "Concrete";
    public override IServiceLifetime ServiceLifetime => ServiceLifetimes.Scoped;

    public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMyDomainFactory, ConcreteFactory>();
        services.AddScoped<ConcreteService>();
    }
}
```

5. **Register in DI Container**

```csharp
// In Startup.cs or Program.cs
services.AddMyDomainServices(configuration, options =>
{
    options.RegisterType(ConcreteServiceType.Instance);
});
```

## Message Pattern

Services use strongly-typed messages for communication, also implemented as Enhanced Enums:

```csharp
public abstract class ServiceMessages : MessageBase<ServiceMessages>
{
    // Base for all service messages
}

public sealed class ServiceCreatedMessage : ServiceMessages
{
    public override string Code => "SVC001";
    public override string Template => "Service {ServiceName} created successfully";
    public override MessageSeverity Severity => MessageSeverity.Information;
}
```

## Performance Considerations

### FastGenericNew

The framework uses `FastGenericNew` for high-performance object instantiation:

- **10-100x faster** than Activator.CreateInstance
- **No boxing** for value type parameters
- **Cached delegates** for repeated instantiation
- **Type-safe** at compile time

### Source Generation

Source generators provide:

- **Zero runtime reflection** for type discovery
- **Compile-time validation** of type hierarchies
- **Optimal data structures** (dictionaries, arrays)
- **Reduced memory allocation** through singletons

### Railway-Oriented Programming

All operations return `IGenericResult<T>` instead of throwing exceptions:

```csharp
public IGenericResult<TService> Create(TConfiguration config)
{
    return Validate(config)
        .Bind(c => CreateInstance(c))
        .Bind(s => Initialize(s))
        .Map(s => LogSuccess(s));
}
```

Benefits:
- **No exception overhead** for expected failures
- **Composable operations** through Bind/Map
- **Explicit error handling** in type system

## GenericServiceFactory Pattern

The framework provides `GenericServiceFactory<TService, TConfiguration>` for services that follow standard instantiation patterns:

### When to Use GenericServiceFactory

Use the generic factory for services that:
- Constructor accepts only `(ILogger<TService>, TConfiguration)`
- No complex initialization required
- No external resource management needed
- Standard dependency injection patterns

### Implementation Example

```csharp
// ServiceType uses GenericServiceFactory directly
public sealed class EmailServiceType : ServiceTypeBase<EmailService, GenericServiceFactory<EmailService, EmailConfiguration, EmailConfiguration>>
{
    public override void Register(IServiceCollection services)
    {
        // Register the generic factory - no custom factory code needed
        services.AddScoped<GenericServiceFactory<EmailService, EmailConfiguration>>();
        services.AddScoped<EmailService>();
    }
}

// Service follows standard constructor pattern
public class EmailService : ServiceBase<EmailCommand, EmailConfiguration, EmailService>
{
    public EmailService(ILogger<EmailService> logger, EmailConfiguration configuration)
        : base(logger, configuration)
    {
        // Standard initialization only
    }
}
```

### When to Create Custom Factories

Only create custom factory classes when you need:
- Connection pooling requirements
- HttpClient management
- External service integration
- Resource lifecycle management
- Complex validation beyond configuration
- Special initialization procedures

### Custom Factory Example

```csharp
public class HttpConnectionFactory : ConnectionFactoryBase<HttpConnection, HttpConfiguration>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpConnectionFactory(IHttpClientFactory httpClientFactory, ILogger<HttpConnectionFactory> logger)
        : base(logger)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override IGenericResult<HttpConnection> Create(HttpConfiguration configuration)
    {
        // Custom logic: HttpClient setup, connection pooling, etc.
        var httpClient = _httpClientFactory.CreateClient(configuration.ClientName);

        // Custom instantiation
        return GenericResult<HttpConnection>.Success(new HttpConnection(configuration, httpClient));
    }
}
```

**Design Principle**: Use GenericServiceFactory for the majority of services. Only implement custom factories when you have specialized instantiation requirements that cannot be handled through standard dependency injection.

## Mandatory Implementation Patterns

All services in the framework MUST follow these patterns for consistency and proper integration:

### 1. Constructor Pattern (CRITICAL)

**Services MUST use this exact constructor signature for GenericServiceFactory compatibility:**

```csharp
public class MyService : ServiceBase<MyCommand, MyConfiguration, MyService>
{
    // REQUIRED: Exactly this constructor signature
    public MyService(ILogger<MyService> logger, MyConfiguration configuration)
        : base(logger, configuration)
    {
        // Standard initialization only
        // NO additional dependencies here - use custom factory if needed
    }
}
```

**Violations that break GenericServiceFactory:**
- Additional constructor parameters
- ILoggerFactory instead of ILogger<TService>
- Missing base() call
- Complex initialization logic

### 2. Source-Generated Logging Pattern (REQUIRED)

**Every service domain MUST implement source-generated logging:**

```csharp
// Logging/MyServiceLog.cs
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage(Justification = "Source-generated logging class")]
public static partial class MyServiceLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
                   Message = "Service operation started for {ServiceName} with {ParameterCount} parameters")]
    public static partial void OperationStarted(ILogger logger, string serviceName, int parameterCount);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error,
                   Message = "Service operation failed for {ServiceName}: {ErrorMessage}")]
    public static partial void OperationFailed(ILogger logger, string serviceName, string errorMessage, Exception exception);
}
```

**Benefits:**
- Zero allocation logging
- Compile-time safety
- Structured logging compatibility
- Performance optimization

### 3. Structured Messages Pattern (REQUIRED)

**Every service domain MUST implement structured messages:**

```csharp
// Messages/MyServiceMessage.cs
[MessageCollection("MyServiceMessages")]
public abstract class MyServiceMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    protected MyServiceMessage(int id, string name, MessageSeverity severity, string message, string? code = null)
        : base(id, name, severity, "MyService", message, code, null, null) { }
}

// Messages/OperationStartedMessage.cs
[Message("OperationStartedMessage")]
public sealed class OperationStartedMessage : MyServiceMessage
{
    public OperationStartedMessage() : base(1001, "OperationStarted", MessageSeverity.Information,
                                           "Service operation started", "OPERATION_STARTED") { }

    public OperationStartedMessage(string serviceName)
        : base(1001, "OperationStarted", MessageSeverity.Information,
               $"Service operation started for {serviceName}", "OPERATION_STARTED") { }
}

// Messages/MyServiceMessageCollectionBase.cs
[MessageCollection("MyServiceMessages", ReturnType = typeof(IServiceMessage))]
public abstract class MyServiceMessageCollectionBase : MessageCollectionBase<MyServiceMessage> { }
```

**Source Generation Result:**
- Creates `MyServiceMessages.OperationStarted()` static methods
- Type-safe message creation
- Consistent error codes and categorization

### 4. Result Pattern (MANDATORY)

**Services MUST return IGenericResult types, never throw exceptions for business logic:**

```csharp
public override async Task<IGenericResult> Execute(MyCommand command)
{
    try
    {
        // 1. Log operation start with source-generated logging
        MyServiceLog.OperationStarted(Logger, Name, command.Parameters?.Count ?? 0);

        // 2. Execute business logic
        var result = await ExecuteBusinessLogicAsync(command);

        // 3. Log success and return structured message
        MyServiceLog.OperationCompleted(Logger, Name, result.RecordsProcessed);
        var successMessage = MyServiceMessages.OperationCompleted(result.RecordsProcessed);
        return GenericResult.Success(successMessage);
    }
    catch (Exception ex)
    {
        // 4. Log failure with source-generated logging
        MyServiceLog.OperationFailed(Logger, Name, ex.Message, ex);

        // 5. Return failure with structured message (never throw)
        var errorMessage = MyServiceMessages.OperationFailed(ex.Message);
        return GenericResult.Failure(errorMessage);
    }
}
```

**Key Requirements:**
- Use `Logger` property from ServiceBase (not injected ILogger)
- Always use source-generated logging methods
- Return structured IServiceMessage objects in Results
- Never throw exceptions for business failures
- Use proper exception handling for unexpected errors

### 5. Implementation Project Structure

**Every implementation project (e.g., Services.Connections.MsSql) MUST have:**

```
├── Services/
│   └── MyService.cs                    # Service implementation
├── Configuration/
│   └── MyConfiguration.cs             # Configuration class
├── Commands/
│   └── MyCommand.cs                    # Command definitions
├── Logging/
│   └── MyServiceLog.cs                 # Source-generated logging
├── Messages/
│   ├── MyServiceMessage.cs             # Message base class
│   ├── Operations/
│   │   ├── OperationStartedMessage.cs  # Individual messages
│   │   └── OperationFailedMessage.cs
│   └── MyServiceMessageCollectionBase.cs  # Collection for source generation
├── Factories/
│   └── MyServiceFactory.cs             # Custom factory (if needed)
└── ServiceTypes/
    └── MyServiceType.cs                # ServiceType definition
```

**Compliance Check:**
- Constructor follows exact pattern ✓
- Source-generated logging implemented ✓
- Structured messages implemented ✓
- Result pattern followed ✓
- No business logic exceptions ✓

## Testing

### Unit Testing Services

```csharp
public class MyServiceTests
{
    [Fact]
    public void Execute_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var config = new MyConfiguration { /* ... */ };
        var factory = new GenericServiceFactory<MyService, MyConfiguration>();
        var serviceResult = factory.Create(config);

        serviceResult.IsSuccess.ShouldBeTrue();
        var service = serviceResult.Value;

        // Act
        var result = service.Execute(new MyCommand());

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}
```

### Testing Type Collections

```csharp
[Fact]
public void ConnectionTypes_ContainsExpectedTypes()
{
    // Act
    var types = ConnectionTypes.GetAll();

    // Assert
    types.ShouldContain(t => t.Name == "MsSql");
    types.ShouldContain(t => t.Name == "PostgreSql");
}
```

## Best Practices

1. **Always use typed factories** instead of service locator pattern
2. **Implement IDisposable** properly for services with resources
3. **Use async methods** for I/O operations
4. **Validate configuration** in factory, not service constructor
5. **Log using source-generated methods** for performance
6. **Return Results** instead of throwing for expected failures
7. **Keep service constructors simple** - complex init in factory
8. **Use Enhanced Enums** instead of string constants
9. **Register types at startup** for fail-fast behavior
10. **Write tests for factories** separately from services

## Common Patterns

### Singleton Service with Factory

```csharp
public class SingletonServiceFactory : ServiceFactory<SingletonService, SingletonConfig>
{
    private static readonly Lazy<SingletonService> _instance =
        new(() => new SingletonService());

    public override IGenericResult<SingletonService> Create(SingletonConfig config)
    {
        return GenericResult<SingletonService>.Success(_instance.Value);
    }
}
```

### Service with Dependencies

```csharp
public class ServiceWithDeps : ServiceBase<Command, Config, ServiceWithDeps>
{
    private readonly IRepository _repository;
    private readonly ICache _cache;

    public ServiceWithDeps(
        Config configuration,
        IRepository repository,
        ICache cache,
        ILogger<ServiceWithDeps> logger)
        : base(configuration, logger)
    {
        _repository = repository;
        _cache = cache;
    }
}
```

### Async Command Execution

```csharp
protected override async Task<IGenericResult<ICommandResult>> ExecuteCoreAsync(MyCommand command)
{
    var data = await _repository.GetAsync(command.Id);

    return data
        .Bind(d => ProcessData(d))
        .MapAsync(async r => await SaveResult(r));
}
```

## Troubleshooting

### Service Creation Failures

Check:
1. Configuration validation in factory
2. Constructor parameter matching
3. DI registration for dependencies
4. FastGenericNew compatibility

### Type Collection Issues

Verify:
1. Base class has `[TypeCollection]` attribute
2. Implementations are in referenced assemblies
3. Source generator is included in project
4. No circular dependencies

### Performance Issues

Profile:
1. Factory instantiation overhead
2. Enhanced Enum lookups (should be O(1))
3. Message formatting allocations
4. Async operation scheduling

## Further Reading

- [Enhanced Enums Documentation](../FractalDataWorks.EnhancedEnums/README.md)
- [Source Generator Documentation](../FractalDataWorks.Collections.SourceGenerators/README.md)
- [Configuration Documentation](../FractalDataWorks.Configuration/README.md)
- [Railway-Oriented Programming](../FractalDataWorks.Results/README.md)