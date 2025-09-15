# Messages Framework - Quick Start Guide

This guide will help you set up and use the Messages framework in 5 minutes.

## Step 1: Add Package References

Add the Messages packages to your project:

```xml
<PackageReference Include="FractalDataWorks.Messages" />
<PackageReference Include="FractalDataWorks.Messages.SourceGenerators" />
<PackageReference Include="FractalDataWorks.Services.Abstractions" />
```

## Step 2: Create Your First Message

Create a message class inheriting from `ServiceMessage`:

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace MyProject.Messages;

[Message("UserNotFound")]
public sealed class UserNotFoundMessage : ServiceMessage
{
    // Parameterless constructor
    public UserNotFoundMessage() 
        : base(1001, "UserNotFound", MessageSeverity.Error, 
               "User not found", "USER_NOT_FOUND") { }

    // Constructor with user ID
    public UserNotFoundMessage(int userId) 
        : base(1001, "UserNotFound", MessageSeverity.Error, 
               $"User with ID {userId} not found", "USER_NOT_FOUND") { }

    // Constructor with username
    public UserNotFoundMessage(string username) 
        : base(1001, "UserNotFound", MessageSeverity.Error, 
               $"User '{username}' not found", "USER_NOT_FOUND") { }
}
```

## Step 3: Create a Message Collection

Define which messages to include and the return type:

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace MyProject.Messages;

[MessageCollection("AppMessages", ReturnType = typeof(IServiceMessage))]
public abstract class AppMessageCollectionBase : MessageCollectionBase<ServiceMessage>
{
    // Source generator will populate this automatically
}
```

## Step 4: Build Your Project

The source generator runs automatically during build. After building, you'll have a generated `AppMessages` class.

## Step 5: Use Your Messages

```csharp
public class UserService
{
    public async Task<User> GetUserAsync(int userId)
    {
        var user = await repository.GetUserAsync(userId);
        if (user == null)
        {
            // Use generated factory methods
            throw new UserNotFoundException(AppMessages.UserNotFound(userId));
        }
        return user;
    }

    public async Task<User> GetUserByNameAsync(string username)
    {
        var user = await repository.GetUserByNameAsync(username);
        if (user == null)
        {
            // Different overload for username
            throw new UserNotFoundException(AppMessages.UserNotFound(username));
        }
        return user;
    }
}
```

## Generated Output

The source generator creates this for you:

```csharp
namespace MyProject.Messages;

public abstract class AppMessages
{
    public static IServiceMessage UserNotFound() => new UserNotFoundMessage();
    public static IServiceMessage UserNotFound(int userId) => new UserNotFoundMessage(userId);
    public static IServiceMessage UserNotFound(string username) => new UserNotFoundMessage(username);
}
```

## Key Benefits

- ✅ **Type Safety**: Compile-time checking of message usage
- ✅ **IntelliSense**: Full IDE support with parameter hints
- ✅ **Automatic Generation**: No manual factory method writing
- ✅ **Multiple Overloads**: One method per constructor automatically
- ✅ **Clean Code**: Generated code uses proper namespaces and documentation

## Next Steps

- Read the full [Messages Documentation](../Messages.md)
- Learn about [Message Patterns](MessagePatterns.md)  
- Explore [Advanced Configuration](AdvancedConfiguration.md)
- See [Integration Examples](IntegrationExamples.md)