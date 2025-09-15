# FractalDataWorks.Configuration.Abstractions

## Overview

The FractalDataWorks.Configuration.Abstractions library provides the foundational interfaces and contracts for configuration management within the FractalDataWorks framework. This library establishes a unified approach to configuration handling with support for validation, change tracking, multiple source types, and both read and write operations.

## Key Features

- **Unified Configuration Interface**: All configurations implement `IFractalConfiguration` for consistent behavior
- **FluentValidation Integration**: Built-in validation support using FluentValidation
- **Change Tracking**: Event-driven notifications when configurations change
- **Multiple Source Types**: Support for files, databases, environment variables, and more
- **Type Safety**: Generic interfaces for type-safe configuration operations
- **Result Pattern**: Uses `IFdwResult<T>` for robust error handling
- **Enhanced Enums**: Type-safe enumeration of change types and source types

## Core Interfaces

### IFractalConfiguration

The base interface that all configuration objects must implement:

```csharp
public interface IFractalConfiguration
{
    int Id { get; }                    // Unique identifier
    string Name { get; }               // Configuration name for lookup
    string SectionName { get; }        // appsettings.json section name
    bool IsEnabled { get; }            // Enable/disable flag
    
    IFdwResult<ValidationResult> Validate(); // FluentValidation integration
}
```

**Key Behaviors:**
- Every configuration has a unique `Id` for database operations
- `Name` provides human-readable identification and lookup capability
- `SectionName` maps to configuration sections in appsettings.json
- `IsEnabled` allows configurations to be temporarily disabled
- `Validate()` returns FluentValidation results wrapped in `IFdwResult<T>`

### IConfigurationRegistry

Generic registry for managing configuration objects:

```csharp
// Non-generic registry for type-based operations
public interface IConfigurationRegistry
{
    void Register<T>(T configuration) where T : IFractalConfiguration;
    T? Get<T>() where T : IFractalConfiguration;
    bool IsRegistered<T>() where T : IFractalConfiguration;
}

// Typed registry for specific configuration types
public interface IConfigurationRegistry<TConfiguration> where TConfiguration : IFractalConfiguration
{
    TConfiguration? Get(int id);
    TConfiguration? GetByName(string name);
    IEnumerable<TConfiguration> GetAll();
    bool TryGet(int id, out TConfiguration? configuration);
    bool Contains(int id);
    bool ContainsByName(string name);
}
```

**Usage Patterns:**
- Use non-generic registry for type-based configuration management
- Use typed registry for working with specific configuration types
- Both `Get(int id)` and `GetByName(string name)` operations supported
- Existence checks available via `Contains` and `ContainsByName`

### IFractalConfigurationProvider

Asynchronous configuration provider with full CRUD operations:

```csharp
// Generic provider for any configuration type
public interface IFractalConfigurationProvider
{
    Task<IFdwResult<TConfiguration>> Get<TConfiguration>(int id);
    Task<IFdwResult<TConfiguration>> Get<TConfiguration>(string name);
    Task<IFdwResult<IEnumerable<TConfiguration>>> GetAll<TConfiguration>();
    Task<IFdwResult<IEnumerable<TConfiguration>>> GetEnabled<TConfiguration>();
    Task<IFdwResult<NonResult>> Reload();
    
    IFractalConfigurationSource Source { get; }
}

// Typed provider for specific configuration types
public interface IFractalConfigurationProvider<TConfiguration>
{
    Task<IFdwResult<TConfiguration>> Get(int id);
    Task<IFdwResult<TConfiguration>> Get(string name);
    Task<IFdwResult<IEnumerable<TConfiguration>>> GetAll();
    Task<IFdwResult<IEnumerable<TConfiguration>>> GetEnabled();
    Task<IFdwResult<TConfiguration>> Save(TConfiguration configuration);
    Task<IFdwResult<NonResult>> Delete(int id);
}
```

**Key Behaviors:**
- All operations are asynchronous and return `IFdwResult<T>`
- `GetEnabled()` filters for configurations where `IsEnabled = true`
- Typed provider supports full CRUD operations (Save/Delete)
- Generic provider is read-only with reload capability
- Each provider is associated with a specific `IFractalConfigurationSource`

### IFractalConfigurationSource

Defines configuration storage backends:

```csharp
public interface IFractalConfigurationSource
{
    string Name { get; }                    // Source identifier
    bool IsWritable { get; }               // Supports save/delete operations
    bool SupportsReload { get; }           // Supports change notifications
    
    Task<IFdwResult<IEnumerable<TConfiguration>>> Load<TConfiguration>();
    Task<IFdwResult<TConfiguration>> Save<TConfiguration>(TConfiguration configuration);
    Task<IFdwResult<NonResult>> Delete<TConfiguration>(int id);
    
    event EventHandler<ConfigurationSourceChangedEventArgs>? Changed;
}
```

**Source Capabilities:**
- **IsWritable**: Determines if `Save()` and `Delete()` are supported
- **SupportsReload**: Indicates if the source can detect changes and raise events
- **Changed Event**: Notifies when configurations are added, updated, deleted, or reloaded
- Sources can be read-only (files) or read-write (databases)

## Enhanced Enums

The library uses Enhanced Enums for type-safe enumeration of configuration concepts.

### Configuration Change Types

Available change types for tracking configuration modifications:

- **Added** (ID: 1): "A configuration was added"
- **Updated** (ID: 2): "A configuration was updated"  
- **Deleted** (ID: 3): "A configuration was deleted"
- **Reloaded** (ID: 4): "The configuration source was reloaded"

### Configuration Source Types

Available source types for different configuration storage:

- **File** (ID: 1): "Configuration from a file"
- **Environment** (ID: 2): "Configuration from environment variables"
- **Database** (ID: 3): "Configuration from a database" 
- **Remote** (ID: 4): "Configuration from a remote service"
- **CommandLine** (ID: 5): "Configuration from command line arguments"
- **Memory** (ID: 6): "Configuration from in-memory storage"
- **Custom** (ID: 7): "Custom configuration source"

### Using Enhanced Enums

```csharp
// Access specific change types
var addedType = new Added();
var updatedType = new Updated();

// Use in event args
var eventArgs = new ConfigurationSourceChangedEventArgs(
    changeType: new Added(),
    configurationType: typeof(MyConfiguration),
    configurationId: 123
);
```

## Event Handling

### ConfigurationSourceChangedEventArgs

Event arguments for configuration change notifications:

```csharp
public class ConfigurationSourceChangedEventArgs : EventArgs
{
    public ConfigurationChangeTypeBase ChangeType { get; }    // What changed
    public Type ConfigurationType { get; }                    // Which config type
    public int? ConfigurationId { get; }                      // Specific config ID (optional)
    public DateTime ChangedAt { get; }                        // When it changed (UTC)
}
```

**Usage Example:**
```csharp
configurationSource.Changed += (sender, args) => {
    Console.WriteLine($"Configuration {args.ConfigurationType.Name} was {args.ChangeType.Name} at {args.ChangedAt}");
    if (args.ConfigurationId.HasValue)
        Console.WriteLine($"Configuration ID: {args.ConfigurationId}");
};
```

## Specialized Interfaces

### IRetryPolicyConfiguration

Defines retry behavior for resilient operations:

```csharp
public interface IRetryPolicyConfiguration
{
    int MaxRetries { get; }           // Maximum retry attempts
    int InitialDelayMs { get; }       // Initial delay in milliseconds  
    double BackoffMultiplier { get; } // Exponential backoff multiplier
    int MaxDelayMs { get; }           // Maximum delay cap
}
```

**Note**: This interface is in the `FractalDataWorks.Services.Configuration` namespace rather than the main abstractions namespace.

## Dependencies

- **FluentValidation**: For configuration validation
- **FractalDataWorks.Results**: For result pattern implementation
- **FractalDataWorks.EnhancedEnums**: For type-safe enumeration support

## Implementation Guidelines

### Configuration Classes

When implementing `IFractalConfiguration`:

1. **Provide Unique IDs**: Ensure each configuration instance has a unique `Id`
2. **Implement Validation**: Use FluentValidation validators in the `Validate()` method
3. **Set Appropriate SectionName**: Map to your appsettings.json structure
4. **Handle Enable/Disable**: Respect the `IsEnabled` property in your business logic

### Configuration Providers

When implementing `IFractalConfigurationProvider`:

1. **Handle Errors Gracefully**: Always return `IFdwResult<T>` with appropriate error information
2. **Respect IsEnabled Filter**: `GetEnabled()` should only return configurations where `IsEnabled = true`
3. **Validate Before Save**: Call `Validate()` on configurations before persisting
4. **Maintain Consistency**: Ensure `Get(int id)` and `Get(string name)` return the same configuration

### Configuration Sources

When implementing `IFractalConfigurationSource`:

1. **Set Capabilities Correctly**: Accurately report `IsWritable` and `SupportsReload`
2. **Raise Change Events**: Fire `Changed` events when configurations are modified
3. **Handle Type Safety**: Ensure generic methods work correctly with different configuration types
4. **Implement Atomic Operations**: Save/Delete operations should be transactional when possible

## Code Coverage Exclusions

The following code should be excluded from code coverage testing:

### ConfigurationSourceChangedEventArgs
- **File**: `C:\development\FractalDataWorks\tmp\developer-kit\src\FractalDataWorks.Configuration.Abstractions\ConfigurationSourceChangedEventArgs.cs`
- **Reason**: Simple event args DTO with no business logic to test
- **Annotation**: `[ExcludeFromCodeCoverage]` and `<ExcludeFromTest>Simple event args DTO with no business logic to test</ExcludeFromTest>`

### Enhanced Enum Implementations
All Enhanced Enum concrete classes should be excluded as they contain no business logic:
- All classes in `EnhancedEnums/ConfigurationChangeTypes/` (except interfaces and base classes)
- All classes in `EnhancedEnums/ConfigurationSourceTypes/` (except interfaces and base classes)

## Breaking Changes

This library represents a refactored configuration system with the following changes from previous versions:

1. **Enhanced Enum Integration**: Configuration change types and source types now use Enhanced Enums for type safety
2. **Unified IFractalConfiguration**: All configurations now implement a common base interface
3. **Result Pattern**: All provider operations return `IFdwResult<T>` instead of throwing exceptions
4. **Async-First Design**: All provider operations are asynchronous
5. **Validation Integration**: Built-in FluentValidation support for all configurations
6. **Event-Driven Changes**: Configuration sources now provide change notification events

## Target Framework

- **.NET 10.0**: This library targets the latest .NET runtime for optimal performance and language features