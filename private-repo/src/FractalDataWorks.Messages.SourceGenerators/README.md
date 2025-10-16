# FractalDataWorks Messages Source Generator

This source generator creates factory methods for message types that inherit from `MessageTemplate<TSeverity>`. It discovers all message classes and generates static collection classes with factory methods for each constructor.

## How to Create Message Types

### 1. Create a Message Class

Message classes must inherit from a base message class (like `ServiceMessage` or `FactoryMessage`) and use the `[Message]` attribute:

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message indicating that service creation failed.
/// </summary>
[Message("ServiceCreationFailed")]
public sealed class ServiceCreationFailedMessage : FactoryMessage
{
    // Parameterless constructor
    public ServiceCreationFailedMessage() 
        : base(2001, "ServiceCreationFailed", MessageSeverity.Error, 
               "Failed to create service", "SERVICE_CREATION_FAILED") { }

    // Constructor with parameters - will generate overloaded factory method
    public ServiceCreationFailedMessage(string serviceType)
        : base(2001, "ServiceCreationFailed", MessageSeverity.Error, 
               $"Failed to create service of type {serviceType}", "SERVICE_CREATION_FAILED") { }

    // Another constructor - will generate another overloaded factory method
    public ServiceCreationFailedMessage(string serviceType, Exception exception)
        : base(2001, "ServiceCreationFailed", MessageSeverity.Error, 
               $"Failed to create service of type {serviceType}: {exception.Message}", "SERVICE_CREATION_FAILED") { }
}
```

**Key Points:**
- Use `[Message("YourMessageName")]` attribute from `FractalDataWorks.Messages.Attributes`
- Inherit from your base message class (`ServiceMessage`, `FactoryMessage`, etc.)
- Each **public constructor** will generate a corresponding factory method
- Constructor parameters become factory method parameters

### 2. Create a Message Collection Base

Create a collection base class that defines which messages to include and what return type to use:

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Collection definition to generate FactoryMessages static class.

/// </summary>
[MessageCollection("FactoryMessages", ReturnType = typeof(IServiceMessage))]
public abstract class FactoryMessageCollectionBase : MessageCollectionBase<FactoryMessage>
{

}
```

**Key Points:**
- Use `[MessageCollection("CollectionName", ReturnType = typeof(YourInterface))]` attribute
- Inherit from `MessageCollectionBase<T>` where `T` is your base message type
- The generic parameter `T` determines which message types are included
- `ReturnType` specifies what interface/type the factory methods return
- If no `ReturnType` is specified, defaults to `IFractalMessage`

## Generated Output

The source generator will create a static class with factory methods:

```csharp
public abstract class FactoryMessages
{
    // Parameterless factory method
    public static IServiceMessage ServiceCreationFailed() => new ServiceCreationFailedMessage();
    
    // Factory method with one parameter
    public static IServiceMessage ServiceCreationFailed(string serviceType) => new ServiceCreationFailedMessage(serviceType);
    
    // Factory method with two parameters  
    public static IServiceMessage ServiceCreationFailed(string serviceType, Exception exception) => new ServiceCreationFailedMessage(serviceType, exception);
}
```

## Usage Examples

```csharp
// Using the generated factory methods
var message1 = FactoryMessages.ServiceCreationFailed();
var message2 = FactoryMessages.ServiceCreationFailed("UserService");
var message3 = FactoryMessages.ServiceCreationFailed("UserService", new InvalidOperationException("Database unavailable"));

// All return IServiceMessage (or whatever ReturnType you specified)
IServiceMessage result = FactoryMessages.ServiceCreationFailed("OrderService");
```

## File Structure Example

```
src/
  FractalDataWorks.Services/
    Messages/
      ServiceMessage.cs                    // Base class for service messages
      ServiceMessageCollectionBase.cs     // Collection definition with [MessageCollection]
      
      Service/                            // Folder for service-related messages
        InvalidCommandTypeMessage.cs      // [Message] class inheriting from ServiceMessage
        ValidationFailedMessage.cs        // [Message] class inheriting from ServiceMessage
        
      Factory/                           // Folder for factory-related messages  
        FactoryMessage.cs                 // Base class for factory messages
        FactoryMessageCollectionBase.cs   // Collection definition with [MessageCollection]
        ServiceCreationFailedMessage.cs   // [Message] class inheriting from FactoryMessage
        CouldNotCreateObjectMessage.cs    // [Message] class inheriting from FactoryMessage
```

## Key Attributes

### `[Message("MessageName")]` - Applied to Message Classes
- **Purpose**: Identifies a class as a message type for the source generator
- **Required**: Yes, for all message classes
- **Parameter**: String name of the message (used for method naming)

### `[MessageCollection("CollectionName", ReturnType = typeof(IYourInterface))]` - Applied to Collection Base Classes  
- **Purpose**: Defines a collection of messages and configures the generated output
- **Required**: Yes, for collection base classes
- **Parameters**:
  - `"CollectionName"`: Name of the generated static class (e.g., "FactoryMessages")
  - `ReturnType`: Interface/type that factory methods should return (optional, defaults to IFractalMessage)

## Discovery Rules

The source generator discovers message types by:
1. Finding all classes with `[MessageCollection]` attributes  
2. Extracting the generic type parameter `T` from `MessageCollectionBase<T>`
3. Scanning for all concrete classes that inherit from `T`
4. Creating factory methods for each public constructor in those classes

## Best Practices

1. **Organize by purpose**: Group related messages in folders (Service/, Factory/, etc.)
2. **Meaningful names**: Use descriptive names for both messages and collections
3. **Constructor design**: Each constructor should represent a meaningful variation of the message
4. **Return interfaces**: Use specific interfaces (like `IServiceMessage`) rather than base classes for better type safety
5. **Documentation**: Document your message classes and their constructors clearly

## Troubleshooting

- **No methods generated**: Check that your message classes inherit from the correct base type specified in `MessageCollectionBase<T>`
- **Missing constructors**: Ensure constructors are `public` - private/internal constructors are ignored
- **Wrong return type**: Verify the `ReturnType` parameter in your `[MessageCollection]` attribute
- **Build errors**: Make sure you reference `FractalDataWorks.Messages.SourceGenerators` in your project file