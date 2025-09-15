# Messages Framework

**Status: PRODUCTION READY**

The Messages framework provides a source generator-based system for creating type-safe message collections with factory methods. It enables centralized message management with automatic code generation for consistent message creation patterns.

## Overview

The Messages framework consists of:
- **Message Templates**: Base classes for different message types
- **Message Classes**: Concrete message implementations with multiple constructors
- **Message Collections**: Generated static factory classes 
- **Source Generator**: Automatic code generation for collections

## Core Components

### 1. Message Template Base Classes

#### ServiceMessage
Used for service-related messages.

```csharp
public abstract class ServiceMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    protected ServiceMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "Services", message, code, null, null)
    {
    }
}
```

#### FactoryMessage  
Used for factory and creation-related messages.

```csharp
public abstract class FactoryMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    protected FactoryMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "Factory", message, code, null, null)
    {
    }
}
```

### 2. Creating Message Classes

Message classes must:
- Inherit from a message template (ServiceMessage, FactoryMessage, etc.)
- Include the `[Message]` attribute with a unique name
- Provide multiple constructors for different use cases

#### Example: InvalidCommandTypeMessage

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message indicating that an invalid command type was provided.
/// </summary>
[Message("InvalidCommandType")]
public sealed class InvalidCommandTypeMessage : ServiceMessage
{
    /// <summary>
    /// Initializes a new instance with default message.
    /// </summary>
    public InvalidCommandTypeMessage() 
        : base(1001, "InvalidCommandType", MessageSeverity.Error, 
               "Invalid command type provided", "INVALID_COMMAND_TYPE") { }

    /// <summary>
    /// Initializes a new instance with the invalid command type name.
    /// </summary>
    /// <param name="commandType">The name of the invalid command type.</param>
    public InvalidCommandTypeMessage(string commandType) 
        : base(1001, "InvalidCommandType", MessageSeverity.Error, 
               $"Invalid command type: {commandType}", "INVALID_COMMAND_TYPE") { }
}
```

#### Example: ValidationFailedMessage

```csharp
[Message("ValidationFailed")]
public sealed class ValidationFailedMessage : ServiceMessage
{
    public ValidationFailedMessage() 
        : base(1002, "ValidationFailed", MessageSeverity.Error, 
               "Validation failed", "VALIDATION_FAILED") { }

    public ValidationFailedMessage(string errors) 
        : base(1002, "ValidationFailed", MessageSeverity.Error, 
               $"Validation failed: {errors}", "VALIDATION_FAILED") { }

    public ValidationFailedMessage(string fieldName, string error) 
        : base(1002, "ValidationFailed", MessageSeverity.Error, 
               $"Validation failed for {fieldName}: {error}", "VALIDATION_FAILED") { }
}
```

### 3. Creating Message Collections

Message collections define which messages to include and specify the return type.

#### Example: ServiceMessageCollectionBase

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Collection definition to generate ServiceMessages static class.
/// The Messages source generator will populate this automatically.
/// </summary>
[MessageCollection("ServiceMessages", ReturnType = typeof(IServiceMessage))]
public abstract class ServiceMessageCollectionBase : MessageCollectionBase<ServiceMessage>
{
    // Source generator will populate this automatically
}
```

#### Example: FactoryMessageCollectionBase

```csharp
[MessageCollection("FactoryMessages", ReturnType = typeof(IServiceMessage))]
public abstract class FactoryMessageCollectionBase : MessageCollectionBase<FactoryMessage>
{
    // Source generator will populate this automatically
}
```

## Generated Output

The source generator creates static factory classes with overloaded methods for each constructor:

### Generated ServiceMessages Class

```csharp
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Provides a collection of ServiceMessage message values.
/// </summary>
public abstract class ServiceMessages
{
    /// <summary>
    /// Creates a new instance of the InvalidCommandType message value.
    /// </summary>
    public static IServiceMessage InvalidCommandType() => new InvalidCommandTypeMessage();

    /// <summary>
    /// Creates a new instance of the InvalidCommandType message value with 1 parameter.
    /// </summary>
    /// <param name="commandType">The name of the invalid command type.</param>
    public static IServiceMessage InvalidCommandType(string commandType) => new InvalidCommandTypeMessage(commandType);

    /// <summary>
    /// Creates a new instance of the ValidationFailed message value.
    /// </summary>
    public static IServiceMessage ValidationFailed() => new ValidationFailedMessage();

    /// <summary>
    /// Creates a new instance of the ValidationFailed message value with 1 parameter.
    /// </summary>
    /// <param name="errors">The validation errors that occurred.</param>
    public static IServiceMessage ValidationFailed(string errors) => new ValidationFailedMessage(errors);

    /// <summary>
    /// Creates a new instance of the ValidationFailed message value with 2 parameters.
    /// </summary>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="error">The validation error message.</param>
    public static IServiceMessage ValidationFailed(string fieldName, string error) => new ValidationFailedMessage(fieldName, error);
}
```

## Usage Examples

### Basic Usage

```csharp
// Use parameterless factory methods
var invalidCommand = ServiceMessages.InvalidCommandType();
var validationError = ServiceMessages.ValidationFailed();

// Use parameterized factory methods
var specificInvalidCommand = ServiceMessages.InvalidCommandType("CreateUserCommand");
var detailedValidationError = ServiceMessages.ValidationFailed("Email", "Invalid email format");
var multiFieldError = ServiceMessages.ValidationFailed("Username and Email are required");
```

### In Service Methods

```csharp
public async Task<IFdwResult<TOut>> Execute<TOut>(ICommand command, CancellationToken cancellationToken)
{
    if (command is not TCommand cmd)
    {
        return FdwResult<TOut>.Failure(ServiceMessages.InvalidCommandType(command.GetType().Name));
    }

    var validationResult = await ValidateCommand(cmd, cancellationToken);
    if (!validationResult.IsValid)
    {
        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        return FdwResult<TOut>.Failure(ServiceMessages.ValidationFailed(errors));
    }

    // Continue with execution...
}
```

## Key Features

### 1. Type Safety
- All factory methods return strongly-typed interfaces (IServiceMessage, IFractalMessage)
- Compile-time verification of message usage
- IntelliSense support for all message methods

### 2. Constructor Overloads
- Each constructor in a message class generates a corresponding factory method
- Maintains same parameter names and types
- Automatic XML documentation generation

### 3. Namespace Management
- Generated classes include appropriate using statements
- Return types use short names (IServiceMessage vs FractalDataWorks.Services.Abstractions.IServiceMessage)
- Clean, readable generated code

### 4. Flexible Return Types
- Collections can specify custom return types via ReturnType attribute parameter
- Falls back to IFractalMessage if no return type specified
- Supports interface-based polymorphism

## Attribute Reference

### MessageAttribute
Applied to message classes to identify them for source generation.

```csharp
[Message("MessageName")]
public sealed class MyMessage : ServiceMessage { }
```

### MessageCollectionAttribute
Applied to collection base classes to define generated collections.

```csharp
[MessageCollection("CollectionName", ReturnType = typeof(IServiceMessage))]
public abstract class MyCollectionBase : MessageCollectionBase<ServiceMessage> { }
```

**Parameters:**
- `name` (required): Name of the generated static class
- `ReturnType` (optional): Type returned by factory methods, defaults to IFractalMessage

## Best Practices

### 1. Message Naming
- Use descriptive, action-oriented names: `ValidationFailed`, `ConfigurationNotFound`
- Include context when necessary: `ServiceCreationFailed`, `DatabaseConnectionFailed`
- Use consistent naming patterns within related messages

### 2. Constructor Design
- Always provide a parameterless constructor with a sensible default message
- Add parameterized constructors for common use cases
- Use meaningful parameter names that will appear in generated documentation

### 3. Message IDs
- Use consistent ID ranges for different message types
- Service messages: 1000-1999
- Factory messages: 2000-2999
- Configuration messages: 3000-3999

### 4. Error Codes
- Use consistent, searchable error codes
- Follow format: CATEGORY_SPECIFIC_ERROR (e.g., "SERVICE_CREATION_FAILED")
- Include in documentation and logging systems

## Project Setup

### 1. Add Project References
```xml
<ItemGroup>
  <!-- Messages framework core -->
  <ProjectReference Include="..\FractalDataWorks.Messages\FractalDataWorks.Messages.csproj" />
  
  <!-- Source generator - MUST be configured as Analyzer -->
  <ProjectReference Include="..\FractalDataWorks.Messages.SourceGenerators\FractalDataWorks.Messages.SourceGenerators.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
  
  <!-- If creating service messages, also add -->
  <ProjectReference Include="..\FractalDataWorks.Services.Abstractions\FractalDataWorks.Services.Abstractions.csproj" />
</ItemGroup>
```

**Important:** The `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"` attributes are required for the source generator to work.

### 2. Create Message Classes
Implement message classes inheriting from appropriate base classes with Message attributes.

### 3. Create Collection Definitions
Define collection base classes with MessageCollection attributes.

### 4. Build Project
The source generator runs automatically during build, creating factory classes.

**For detailed setup instructions, see:** [messages/SetupGuide.md](messages/SetupGuide.md)

## Troubleshooting

### Source Generator Not Running
- Ensure FractalDataWorks.Messages.SourceGenerators package is referenced
- Check that collection classes have MessageCollection attribute
- Verify message classes inherit from correct base classes

### Missing Factory Methods
- Confirm message classes are in same assembly as collection definition
- Check that constructors are public
- Verify Message attribute is present on message classes

### Wrong Return Types
- Specify ReturnType parameter in MessageCollection attribute
- Ensure return type interface is accessible in generated code namespace

## Migration Guide

### From Enhanced Enums to Messages
1. Remove `[EnumOption]` attributes, replace with `[Message]`
2. Change inheritance from EnumOptionBase to ServiceMessage/FactoryMessage
3. Update collection definitions to use MessageCollection instead of EnumCollection
4. Update usage from static properties to factory methods

This framework provides a robust foundation for centralized message management with compile-time safety and automatic code generation.