# FractalDataWorks.Messages

This package provides the foundation for structured, type-safe messaging within the FractalDataWorks framework using the Enhanced Enums pattern.

## Overview

The Messages package enables consistent, discoverable, and type-safe messaging across all FractalDataWorks components. Messages are used for structured result information in `GenericResult<T>` operations, not for logging.

## Key Components

### IGenericMessage Interface

The core interface that all framework messages implement:

```csharp
public interface IGenericMessage : IEnumOption
{
    string Message { get; }              // Human-readable message text
    string? Code { get; }                // Unique identifier for programmatic handling
    string? Source { get; }              // Component that generated the message
}
```

This interface inherits from `IEnumOption` providing:
- `int Id` - Unique identifier for this enum value  
- `string Name` - Display name or string representation

### IGenericMessage<TSeverity> Interface

Generic interface for messages with strongly typed severity:

```csharp
public interface IGenericMessage<TSeverity> : IGenericMessage where TSeverity : Enum
{
    TSeverity Severity { get; }          // Severity level of the message
}
```

### MessageSeverity Enum

Defines the severity levels for framework messages:

```csharp
public enum MessageSeverity
{
    Information = 0,    // Context or status updates
    Warning = 1,        // Potential issues that don't prevent operation
    Error = 2,          // Failures or critical problems
    Critical = 3        // System-level failures
}
```

### GenericMessage Class

Concrete implementation of IGenericMessage for general-purpose messaging:

```csharp
public class GenericMessage : IGenericMessage
{
    public MessageSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Source { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "GenericMessage";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public IDictionary<string, object> Metadata { get; set; }

    // Constructors for various initialization patterns
    public GenericMessage()
    public GenericMessage(string message)
    public GenericMessage(MessageSeverity severity, string message, string? code = null, string? source = null)
}
```

### MessageTemplate<TSeverity> Abstract Class

The base class for framework messages with formatting and metadata capabilities:

```csharp
public abstract class MessageTemplate<TSeverity> : IMessageIdentifier, IGenericMessage<TSeverity>
    where TSeverity : Enum
{
    public int Id { get; }
    public string Name { get; }
    public TSeverity Severity { get; }
    public string Message { get; }
    public string? Code { get; }
    public string? Source { get; }
    public string OriginatedIn { get; }
    public DateTime Timestamp { get; }
    public IDictionary<string, object?>? Details { get; }
    public object? Data { get; }

    // Format message with parameters
    public virtual string Format(params object[] args)
    
    // Create new instance with different severity (must be overridden)
    public virtual MessageTemplate<TSeverity> WithSeverity(TSeverity severity)
}
```

### MessageCollectionBase<T>

Base class for message collections that provides core functionality:

```csharp
public abstract class MessageCollectionBase<T> where T : class, IGenericMessage
{
    // Static collection methods
    public static ImmutableArray<T> All()
    public static T Empty()
    public static T? GetByName(string? name)
    public static T? GetById(int id)
    public static bool TryGetByName(string? name, out T? value)
    public static bool TryGetById(int id, out T? value)
    public static IEnumerable<T> AsEnumerable()
    public static int Count { get; }
    public static bool Any()
    public static T GetByIndex(int index)
}
```

### Attributes

#### MessageAttribute

Marks message classes for Enhanced Enum generation:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MessageAttribute : Attribute
{
    public string? CollectionName { get; set; }
    public string? Name { get; set; }
    public Type? ReturnType { get; set; }
    public string? ReturnTypeNamespace { get; set; }
    public bool IncludeInGlobalCollection { get; set; } = true;
}
```

#### MessageCollectionAttribute

Marks a class as a message collection base type:

```csharp
public sealed class MessageCollectionAttribute : Attribute
{
    public MessageCollectionAttribute(string name)
    public string Name { get; }
    public Type ReturnType { get; set; }
}
```

#### GlobalMessageCollectionAttribute

Marks a message collection for global cross-assembly discovery:

```csharp
public sealed class GlobalMessageCollectionAttribute : Attribute
{
    public GlobalMessageCollectionAttribute(string name)
    public string Name { get; }
    public Type ReturnType { get; set; }
}
```

## Implementation Examples

### Basic Message Usage

```csharp
// Simple message creation
var message = new GenericMessage("Operation completed successfully");

// Message with severity and metadata
var errorMessage = new GenericMessage(
    MessageSeverity.Error, 
    "Validation failed", 
    "VAL001", 
    "UserService"
);

// Access message properties
Console.WriteLine($"{errorMessage.Severity}: {errorMessage.Message}");
Console.WriteLine($"Code: {errorMessage.Code}, Source: {errorMessage.Source}");
```

### Custom Message Template

```csharp
public class ValidationMessage : MessageTemplate<MessageSeverity>
{
    public ValidationMessage(int id, string name, string message, string? code = null)
        : base(id, name, MessageSeverity.Warning, message, code, "Validation")
    {
    }

    public override MessageTemplate<MessageSeverity> WithSeverity(MessageSeverity severity)
    {
        // Implementation depends on specific message type
        throw new NotSupportedException("Severity changes not supported for ValidationMessage");
    }
}
```

### Message Collections

```csharp
[MessageCollection("ValidationMessages")]
public abstract class ValidationMessageCollection : MessageCollectionBase<ValidationMessage>
{
    // Source generator will populate collection methods
}

// Usage:
var allMessages = ValidationMessageCollection.All();
var messageById = ValidationMessageCollection.GetById(1001);
var messageByName = ValidationMessageCollection.GetByName("InvalidEmail");
```

## Best Practices

### Message Design
- **Clear, actionable messages**: Write messages that help users understand what happened and what to do
- **Consistent formatting**: Use standard .NET format strings for the Format() method
- **Appropriate severity**: Choose severity levels that match the actual impact
- **Unique codes**: Use meaningful, unique codes for programmatic handling
- **Thread-safe operations**: All message operations should be thread-safe

### Usage Guidelines
- **For results, not logging**: Messages are for `GenericResult<T>` operations, not logging
- **Immutable design**: Prefer immutable message instances where possible
- **Metadata support**: Use Details dictionary and Data property for structured additional information
- **Timestamp tracking**: All MessageTemplate instances include automatic UTC timestamp creation

## Code Coverage Exclusions

The following code should be excluded from coverage testing:

### Attributes (Configuration Only)
- `GlobalMessageCollectionAttribute` - Marked with `[ExcludeFromCodeCoverage]`
- Constructor parameter validation in attributes (infrastructure code)

### Generated Code
- Source generator output files
- Auto-generated assembly info files
- Build-time generated files in obj/ directories

### Infrastructure Code
- Internal initialization methods in `MessageCollectionBase<T>`
- Dictionary initialization for high-performance lookups
- Assembly loading and reflection helpers

## Dependencies

- **FractalDataWorks.EnhancedEnums**: Enhanced Enum pattern and base interfaces
- **System.Collections.Immutable**: For efficient collection storage
- **System.Collections.Frozen** (NET8_0_OR_GREATER): For optimized lookups

## Integration

Messages integrate seamlessly with other FractalDataWorks components:

- **Results**: Used in `GenericResult<T>` for structured error/success information
- **Services**: Service operations return messages for validation failures, errors
- **Enhanced Enums**: Automatic collection generation and discovery
- **Source Generation**: Compile-time code generation for collections and factory methods