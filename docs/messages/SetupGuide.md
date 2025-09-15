# Messages Framework Complete Setup Guide

This guide provides step-by-step instructions for setting up and using the Messages framework in your projects.

## Table of Contents
- [Project Setup](#project-setup)
- [Creating Messages](#creating-messages)
- [Using Generated Collections](#using-generated-collections)
- [Common Patterns](#common-patterns)
- [Troubleshooting](#troubleshooting)

## Project Setup

### Step 1: Add Project References

Add the following references to your `.csproj` file:

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

**Important Notes:**
- The `OutputItemType="Analyzer"` is **required** for the source generator to work
- The `ReferenceOutputAssembly="false"` prevents the analyzer from being included in output
- Without these attributes, the source generator will not run

### Step 2: Add Required Using Statements

In your message classes, add these using statements:

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions; // If using IServiceMessage
```

## Creating Messages

### Step 1: Create Base Message Class (Optional)

If you want domain-specific messages, create a base class:

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Services.Abstractions;

namespace YourProject.Messages;

/// <summary>
/// Base class for all messages in this domain.
/// </summary>
public abstract class DomainMessage : ServiceMessage
{
    protected DomainMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, message, code)
    {
    }
}
```

### Step 2: Create Message Classes

Create concrete message classes with the `[Message]` attribute:

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace YourProject.Messages;

/// <summary>
/// Message for validation failures.
/// </summary>
[Message("ValidationFailed")]
public sealed class ValidationFailedMessage : DomainMessage
{
    /// <summary>
    /// Creates a validation failed message with default text.
    /// </summary>
    public ValidationFailedMessage() 
        : base(1001, "ValidationFailed", MessageSeverity.Error, 
               "Validation failed", "VALIDATION_FAILED") { }

    /// <summary>
    /// Creates a validation failed message with field name.
    /// </summary>
    /// <param name="fieldName">The field that failed validation.</param>
    public ValidationFailedMessage(string fieldName) 
        : base(1001, "ValidationFailed", MessageSeverity.Error, 
               $"Validation failed for field: {fieldName}", "VALIDATION_FAILED") { }

    /// <summary>
    /// Creates a validation failed message with field and reason.
    /// </summary>
    /// <param name="fieldName">The field that failed validation.</param>
    /// <param name="reason">The reason for failure.</param>
    public ValidationFailedMessage(string fieldName, string reason) 
        : base(1001, "ValidationFailed", MessageSeverity.Error, 
               $"Validation failed for {fieldName}: {reason}", "VALIDATION_FAILED") { }
}
```

**Key Points:**
- Each public constructor generates a factory method
- Use XML documentation - it appears in IntelliSense
- Message IDs should be unique within your domain
- Use consistent error codes for searching/filtering

### Step 3: Create Message Collection

Create a collection base class to generate the factory class:

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace YourProject.Messages;

/// <summary>
/// Collection definition for domain messages.
/// The Messages source generator will populate this automatically.
/// </summary>
[MessageCollection("DomainMessages", ReturnType = typeof(IServiceMessage))]
public abstract class DomainMessageCollectionBase : MessageCollectionBase<DomainMessage>
{
    // Source generator will populate this automatically
    // Do not add any code here
}
```

**Important:**
- The class name (`DomainMessages`) becomes the generated static class name
- `ReturnType` determines what interface the factory methods return
- The generic parameter (`DomainMessage`) determines which messages are included

### Step 4: Build the Project

Build your project to trigger the source generator:

```bash
dotnet build
```

The generator creates a static class in `obj\Debug\net10.0\generated\FractalDataWorks.Messages.SourceGenerators\`:

```csharp
// Generated code
public static class DomainMessages
{
    public static IServiceMessage ValidationFailed() 
        => new ValidationFailedMessage();
    
    public static IServiceMessage ValidationFailed(string fieldName) 
        => new ValidationFailedMessage(fieldName);
    
    public static IServiceMessage ValidationFailed(string fieldName, string reason) 
        => new ValidationFailedMessage(fieldName, reason);
}
```

## Using Generated Collections

### Basic Usage

```csharp
// Use the generated factory methods
var message1 = DomainMessages.ValidationFailed();
var message2 = DomainMessages.ValidationFailed("Email");
var message3 = DomainMessages.ValidationFailed("Email", "Invalid format");

// All return the interface type specified in MessageCollection
IServiceMessage msg = DomainMessages.ValidationFailed("Username");
```

### With Result Pattern

```csharp
public async Task<IFdwResult<User>> CreateUser(CreateUserCommand command)
{
    // Validation
    if (string.IsNullOrEmpty(command.Email))
    {
        return FdwResult<User>.Failure(
            DomainMessages.ValidationFailed("Email", "Email is required"));
    }

    // Business logic
    try
    {
        var user = await _repository.CreateAsync(command);
        return FdwResult<User>.Success(user);
    }
    catch (DuplicateEmailException)
    {
        return FdwResult<User>.Failure(
            DomainMessages.DuplicateEmail(command.Email));
    }
}
```

### With Logging

```csharp
public void LogMessage(IServiceMessage message)
{
    var logLevel = message.Severity switch
    {
        MessageSeverity.Error => LogLevel.Error,
        MessageSeverity.Warning => LogLevel.Warning,
        _ => LogLevel.Information
    };

    _logger.Log(logLevel, "[{Code}] {Message}", message.Code, message.Message);
}
```

## Common Patterns

### Pattern 1: Domain-Specific Message Hierarchies

```
Messages/
  DomainMessage.cs                    // Base for all domain messages
  DomainMessageCollectionBase.cs      // Collection definition
  
  Validation/
    ValidationFailedMessage.cs        // Validation-specific messages
    RequiredFieldMissingMessage.cs
    InvalidFormatMessage.cs
    
  Business/
    DuplicateRecordMessage.cs        // Business rule messages
    InsufficientFundsMessage.cs
    QuotaExceededMessage.cs
```

### Pattern 2: Service Layer Messages

```csharp
// For service projects
[MessageCollection("ServiceMessages", ReturnType = typeof(IServiceMessage))]
public abstract class ServiceMessageCollectionBase : MessageCollectionBase<ServiceMessage>
{
}

// For factory patterns
[MessageCollection("FactoryMessages", ReturnType = typeof(IServiceMessage))]
public abstract class FactoryMessageCollectionBase : MessageCollectionBase<FactoryMessage>
{
}
```

### Pattern 3: Message ID Ranges

Organize message IDs by category:

```csharp
// Service messages: 1000-1999
public ServiceStartedMessage() : base(1001, ...);
public ServiceStoppedMessage() : base(1002, ...);

// Factory messages: 2000-2999
public CreationFailedMessage() : base(2001, ...);
public TypeNotFoundMessage() : base(2002, ...);

// Validation messages: 3000-3999
public ValidationFailedMessage() : base(3001, ...);
public RequiredFieldMessage() : base(3002, ...);
```

## Troubleshooting

### Source Generator Not Running

**Symptoms:** No factory methods generated, can't find generated class

**Solutions:**
1. Verify project references are correct:
   ```xml
   <ProjectReference Include="..\FractalDataWorks.Messages.SourceGenerators\FractalDataWorks.Messages.SourceGenerators.csproj" 
                     OutputItemType="Analyzer" 
                     ReferenceOutputAssembly="false" />
   ```

2. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build
   ```

3. Check for generated files:
   ```bash
   dir obj\Debug\net10.0\generated\FractalDataWorks.Messages.SourceGenerators\
   ```

### Wrong Return Type

**Symptoms:** Factory methods return wrong interface/type

**Solution:** Specify `ReturnType` in MessageCollection attribute:
```csharp
[MessageCollection("MyMessages", ReturnType = typeof(IServiceMessage))]
```

### Missing Factory Methods

**Symptoms:** Some constructors don't have factory methods

**Solutions:**
1. Ensure constructors are `public`
2. Check that message class has `[Message]` attribute
3. Verify message class inherits from base type in collection

### Compilation Errors in Generated Code

**Symptoms:** Build fails with errors in generated files

**Solutions:**
1. Check that all namespaces are properly imported
2. Verify ReturnType interface is accessible
3. Ensure no naming conflicts with generated class name

### IntelliSense Not Working

**Symptoms:** Can't see factory methods in IDE

**Solutions:**
1. Build project first
2. Restart IDE/OmniSharp
3. Check that generated files exist in obj folder

## Best Practices

1. **Always document constructors** - XML comments appear in IntelliSense
2. **Use meaningful parameter names** - They become part of the API
3. **Keep messages immutable** - Don't add setters to message properties
4. **Group related messages** - Use folders to organize by domain/feature
5. **Consistent error codes** - Makes searching logs easier
6. **Test message creation** - Ensure all constructors work as expected

## Example: Complete Service Implementation

```csharp
// OrderService.cs
public class OrderService : ServiceBase<OrderCommand, OrderConfiguration, OrderService>
{
    private readonly IOrderRepository _repository;

    public OrderService(
        ILogger<OrderService> logger,
        OrderConfiguration configuration,
        IOrderRepository repository)
        : base(logger, configuration)
    {
        _repository = repository;
    }

    protected override async Task<IFdwResult<T>> ExecuteCore<T>(OrderCommand command)
    {
        // Validation using messages
        if (command.Items?.Count == 0)
        {
            return FdwResult<T>.Failure(
                OrderMessages.ValidationFailed("Items", "Order must contain at least one item"));
        }

        try
        {
            // Business logic
            var order = await _repository.CreateOrderAsync(command);
            
            Logger.LogInformation("Order {OrderId} created successfully", order.Id);
            return FdwResult<T>.Success((T)(object)order);
        }
        catch (InsufficientStockException ex)
        {
            // Use specific message for business rule violation
            return FdwResult<T>.Failure(
                OrderMessages.InsufficientStock(ex.ProductId, ex.RequestedQuantity));
        }
        catch (Exception ex)
        {
            // Generic failure message
            Logger.LogError(ex, "Order creation failed");
            return FdwResult<T>.Failure(
                OrderMessages.OrderCreationFailed());
        }
    }
}
```

This completes the setup guide for the Messages framework. Follow these steps to implement type-safe, generated message collections in your projects.