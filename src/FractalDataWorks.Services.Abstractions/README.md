# FractalDataWorks.Services.Abstractions

Core service contracts and interfaces for the FractalDataWorks service framework. This library defines the foundational abstractions for command-based service architectures with comprehensive type safety and extensibility.

## Overview

FractalDataWorks.Services.Abstractions establishes the contract system for all services, providing:
- Service lifecycle interfaces
- Command pattern contracts
- Factory abstractions
- Configuration interfaces
- Service provider patterns
- Message system contracts

## Core Interfaces

### IFdwService

Base interface hierarchy for all services:

```csharp
// Base service interface
public interface IFdwService
{
    string Id { get; }
    string ServiceType { get; }
    bool IsAvailable { get; }
}

// Service with command execution
public interface IFdwService<TCommand> : IFdwService
    where TCommand : ICommand
{
    Task<IFdwResult> Execute(TCommand command);
}

// Service with configuration support
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

// Full service interface with type identification
public interface IFdwService<TCommand, TConfiguration, TService>
    : IFdwService<TCommand, TConfiguration>
    where TCommand : ICommand
    where TConfiguration : IFdwConfiguration
    where TService : class
{
    // TService provides type identification for logging and DI
}
```

## Command System

### ICommand

Base command interface:

```csharp
public interface ICommand
{
    // Marker interface - implementations add properties
}

public interface ICommandBuilder<TCommand> where TCommand : ICommand
{
    TCommand Build();
    ICommandBuilder<TCommand> WithParameter(string name, object value);
}

public interface ICommandResult
{
    bool IsSuccess { get; }
    string Message { get; }
    Exception Error { get; }
}

public interface IDataCommand : ICommand
{
    string Operation { get; }
    string Entity { get; }
    object Data { get; }
}
```

## Factory System

### IServiceFactory

Factory interfaces for service creation:

```csharp
// Base factory interface
public interface IServiceFactory
{
    IFdwResult<IFdwService> Create(IFdwConfiguration configuration);
    IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService;
}

// Typed factory for specific services
public interface IServiceFactory<TService> : IServiceFactory
    where TService : class
{
    IFdwResult<TService> Create(IFdwConfiguration configuration);
}

// Complete factory with configuration type
public interface IServiceFactory<TService, TConfiguration> : IServiceFactory<TService>
    where TService : class
    where TConfiguration : class, IFdwConfiguration
{
    IFdwResult<TService> Create(TConfiguration configuration);
}
```

## Service Lifetimes

### ServiceLifetimeBase

Base classes for service lifetime management:

```csharp
public abstract class ServiceLifetimeBase
{
    public abstract ServiceLifetime Lifetime { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
}

// Built-in lifetime options
public class SingletonServiceLifetimeOption : ServiceLifetimeBase { }
public class ScopedServiceLifetimeOption : ServiceLifetimeBase { }
public class TransientServiceLifetimeOption : ServiceLifetimeBase { }
```

## Configuration

### IServiceConfiguration

Configuration interface:

```csharp
public interface IServiceConfiguration : IFdwConfiguration
{
    string Name { get; }
    bool Enabled { get; }
    Dictionary<string, object> Settings { get; }
}
```

## Provider Pattern

### IFdwServiceProvider

Service provider for resolution:

```csharp
public interface IFdwServiceProvider
{
    T GetService<T>() where T : IFdwService;
    IFdwService GetService(Type serviceType);
    IEnumerable<T> GetServices<T>() where T : IFdwService;
}
```

## Validation

### IFdwValidator

Validation interface:

```csharp
public interface IFdwValidator<T>
{
    ValidationResult Validate(T instance);
    Task<ValidationResult> ValidateAsync(T instance);
}
```

## Message System

### IServiceMessage

Service message contract:

```csharp
public interface IServiceMessage
{
    string Code { get; }
    string Message { get; }
    MessageSeverity Severity { get; }
    DateTimeOffset Timestamp { get; }
}

public abstract class ServiceMessageBase : IServiceMessage
{
    protected ServiceMessageBase(string code, string message, MessageSeverity severity);
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Services.Abstractions" Version="1.0.0" />
```

## Dependencies

- `FractalDataWorks.Configuration.Abstractions` - Configuration contracts
- `FractalDataWorks.Results` - Result pattern types
- `FractalDataWorks.Messages` - Message infrastructure
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI integration

## Design Principles

1. **Interface Segregation** - Multiple focused interfaces instead of one large interface
2. **Generic Constraints** - Type safety through generic parameters
3. **Async-First** - All operations return Tasks
4. **Result Pattern** - No exceptions for control flow
5. **Cancellation Support** - CancellationToken throughout

## Best Practices

1. Program against interfaces, not implementations
2. Use the most specific interface that meets your needs
3. Leverage generic constraints for compile-time safety
4. Implement proper null checking in concrete classes
5. Follow the IFdw naming convention

## Extension Points

- Custom commands via `ICommand`
- Service lifetimes via `ServiceLifetimeBase`
- Validation strategies via `IFdwValidator`
- Message types via `IServiceMessage`
- Provider implementations via `IFdwServiceProvider`

## Documentation

- [Services Architecture](../../docs/Services.Abstractions.md)
- [Developer Guide](../../docs/DeveloperGuide-ServiceSetup.md)
- [API Reference](https://docs.fractaldataworks.com/api/services-abstractions)

## License

Copyright © FractalDataWorks Electric Cooperative. All rights reserved.