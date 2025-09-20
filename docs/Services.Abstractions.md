# FractalDataWorks.Services.Abstractions Documentation

## Overview

The FractalDataWorks.Services.Abstractions library defines the core contracts and interfaces for the entire service framework. It establishes the foundation for command-based service architectures with comprehensive abstraction layers that ensure consistency and extensibility across all service implementations.

## Core Interfaces

### IFdwService

The base interface for all services in the framework:

```csharp
public interface IFdwService
{
    string Id { get; }           // Unique service instance identifier
    string ServiceType { get; }  // Display name of the service
    bool IsAvailable { get; }    // Current availability status
}
```

### IFdwService<TCommand>

Extends the base interface with command execution capabilities:

```csharp
public interface IFdwService<TCommand> where TCommand : ICommand
{
    Task<IFdwResult> Execute(TCommand command);
}
```

### IFdwService<TCommand, TConfiguration>

Adds configuration support to command-based services:

```csharp
public interface IFdwService<TCommand, TConfiguration> : IFdwService<TCommand>
    where TCommand : ICommand
    where TConfiguration : IFdwConfiguration
{
    string Name { get; }
    TConfiguration Configuration { get; }
    Task<IFdwResult<T>> Execute<T>(TCommand command);
    Task<IFdwResult<TOut>> Execute<TOut>(TCommand command, CancellationToken cancellationToken);
    Task<IFdwResult> Execute(TCommand command, CancellationToken cancellationToken);
}
```

### IFdwService<TCommand, TConfiguration, TService>

The complete service interface with full type identification:

```csharp
public interface IFdwService<TCommand, TConfiguration, TService>
    : IFdwService<TCommand, TConfiguration>
    where TCommand : ICommand
    where TConfiguration : IFdwConfiguration
    where TService : class
{
    // TService is used for type identification and logging
    // No additional members - provides complete type safety
}
```

## Command System

### ICommand

Base interface for all commands:

```csharp
public interface ICommand
{
    // Marker interface for command pattern implementation
    // Concrete commands add properties for command data
}
```

### ICommandBuilder

Builder pattern for complex command construction:

```csharp
public interface ICommandBuilder<TCommand> where TCommand : ICommand
{
    TCommand Build();
    ICommandBuilder<TCommand> WithParameter(string name, object value);
}
```

### ICommandResult

Result pattern for command execution:

```csharp
public interface ICommandResult
{
    bool IsSuccess { get; }
    string Message { get; }
    Exception Error { get; }
}
```

### ICommandTypeMetrics

Performance metrics for command execution:

```csharp
public interface ICommandTypeMetrics
{
    string CommandType { get; }
    long ExecutionCount { get; }
    double AverageExecutionTime { get; }
    double MaxExecutionTime { get; }
}
```

## Service Factory System

### IServiceFactory

Base factory interface for service creation:

```csharp
public interface IServiceFactory
{
    IFdwResult<IFdwService> Create(IFdwConfiguration configuration);
    IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService;
}
```

### IServiceFactory<TService>

Typed factory for specific service types:

```csharp
public interface IServiceFactory<TService> : IServiceFactory
    where TService : class
{
    IFdwResult<TService> Create(IFdwConfiguration configuration);
}
```

### IServiceFactory<TService, TConfiguration>

Complete factory with configuration type safety:

```csharp
public interface IServiceFactory<TService, TConfiguration> : IServiceFactory<TService>
    where TService : class
    where TConfiguration : class, IFdwConfiguration
{
    IFdwResult<TService> Create(TConfiguration configuration);
}
```

## Service Lifetime Management

### ServiceLifetimeBase

Abstract base for service lifetime definitions:

```csharp
public abstract class ServiceLifetimeBase
{
    public abstract ServiceLifetime Lifetime { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
}
```

### Built-in Lifetime Options

1. **SingletonServiceLifetimeOption**: Single instance for application lifetime
2. **ScopedServiceLifetimeOption**: One instance per scope/request
3. **TransientServiceLifetimeOption**: New instance for each request

### ServiceLifetimeCollectionBase

Base class for source-generated lifetime collections:

```csharp
public abstract class ServiceLifetimeCollectionBase
{
    // Source generator populates with all lifetime options
}
```

## Provider and Validation

### IFdwServiceProvider (IRecServiceProvider)

Service provider interface for service resolution:

```csharp
public interface IFdwServiceProvider
{
    T GetService<T>() where T : IFdwService;
    IFdwService GetService(Type serviceType);
    IEnumerable<T> GetServices<T>() where T : IFdwService;
}
```

### IFdwValidator (IRecValidator)

Validation interface for service configurations:

```csharp
public interface IFdwValidator<T>
{
    ValidationResult Validate(T instance);
    Task<ValidationResult> ValidateAsync(T instance);
}
```

## Message System

### IServiceMessage

Base interface for service messages:

```csharp
public interface IServiceMessage
{
    string Code { get; }
    string Message { get; }
    MessageSeverity Severity { get; }
    DateTimeOffset Timestamp { get; }
}
```

### ServiceMessageBase

Abstract base class for message implementations:

```csharp
public abstract class ServiceMessageBase : IServiceMessage
{
    protected ServiceMessageBase(string code, string message, MessageSeverity severity)
    {
        Code = code;
        Message = message;
        Severity = severity;
        Timestamp = DateTimeOffset.UtcNow;
    }
}
```

## Data Commands

### IDataCommand

Specialized command interface for data operations:

```csharp
public interface IDataCommand : ICommand
{
    string Operation { get; }  // CRUD operation type
    string Entity { get; }     // Target entity/table
    object Data { get; }       // Operation data
}
```

## Configuration

### IServiceConfiguration

Base configuration interface:

```csharp
public interface IServiceConfiguration : IFdwConfiguration
{
    string Name { get; }
    bool Enabled { get; }
    Dictionary<string, object> Settings { get; }
}
```

## Key Design Principles

1. **Interface Segregation**: Multiple focused interfaces instead of one large interface
2. **Generic Constraints**: Type safety through generic type parameters
3. **Async-First**: All operations return Tasks for async execution
4. **Result Pattern**: No exceptions for control flow
5. **Cancellation Support**: CancellationToken throughout async operations

## Extension Points

The abstractions library provides multiple extension points:

1. Custom command types by implementing `ICommand`
2. Service lifetime options by extending `ServiceLifetimeBase`
3. Validation strategies via `IFdwValidator`
4. Message types through `IServiceMessage`
5. Provider implementations of `IFdwServiceProvider`

## Thread Safety Guarantees

- All interfaces are designed for thread-safe implementation
- No mutable shared state in abstractions
- Immutable message objects
- Configuration objects should be immutable after creation

## Best Practices

1. Program against interfaces, not implementations
2. Use the most specific interface that meets your needs
3. Leverage generic constraints for compile-time type safety
4. Implement proper null checking in concrete classes
5. Follow the established naming conventions (IFdw prefix)

## Integration Requirements

Implementations must reference:
- `FractalDataWorks.Configuration.Abstractions` for IFdwConfiguration
- `FractalDataWorks.Results` for IFdwResult types
- `Microsoft.Extensions.DependencyInjection.Abstractions` for DI integration