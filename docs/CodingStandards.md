# FractalDataWorks Framework Coding Standards and Best Practices

## Core Philosophy
The FractalDataWorks Framework follows Railway-Oriented Programming (ROP) principles within an Object-Oriented design. This means we use OOP patterns with functional error handling, avoiding exceptions for anticipated conditions.

## Naming Conventions

### General Rules
- **PascalCase** for public types, methods, properties, and constants
- **camelCase** for parameters and local variables  
- **_camelCase** (with underscore prefix) for private fields
- **NEVER use snake_case** in C# code

### Async Methods
- **DO NOT** append `Async` suffix to method names
- The return type `Task<T>` or `Task` is sufficient to indicate async behavior
- Example: `GetService()` not `GetServiceAsync()`

```csharp
// ✅ GOOD
public Task<IFdwResult<TService>> GetService(string name)

// ❌ BAD
public Task<IFdwResult<TService>> GetServiceAsync(string name)
```

## String Comparisons

### Always Use Explicit StringComparison
- Default to `StringComparer.Ordinal` for dictionaries and collections
- Use `StringComparison.OrdinalIgnoreCase` for case-insensitive comparisons
- Never rely on culture-specific comparisons unless explicitly required

```csharp
// ✅ GOOD
var dictionary = new Dictionary<string, TValue>(StringComparer.Ordinal);
if (string.Equals(a, b, StringComparison.Ordinal))

// ❌ BAD
var dictionary = new Dictionary<string, TValue>();
if (a == b)
```

## Collection Performance

### Prefer Count Over Any() When Collection is Materialized
```csharp
// ✅ GOOD - When you have a List, Array, or other ICollection
if (items.Count > 0)

// ❌ BAD - Any() creates an enumerator unnecessarily
if (items.Any())
```

## Null Handling

### Don't Add Redundant Null Checks
- Trust the compiler's nullable reference type annotations
- Don't check for null when the type system guarantees non-null

```csharp
// ✅ GOOD - Let the compiler enforce non-null
public void Process(TConfiguration configuration) // non-nullable parameter
{
    var result = configuration.Validate(); // No null check needed
}

// ❌ BAD - Redundant null check
public void Process(TConfiguration configuration)
{
    if (configuration == null) // Unnecessary - compiler ensures non-null
        throw new ArgumentNullException(nameof(configuration));
}
```

### Use Null-Coalescing Operators
```csharp
// ✅ GOOD
_logger ??= NullLogger<T>.Instance;

// ❌ BAD
if (_logger == null)
    _logger = NullLogger<T>.Instance;
```

### Use default Instead of null for Generic Types (CS0403)
Generic type parameters could be value types, so always use `default` instead of `null` when returning or assigning default values.

```csharp
// ✅ GOOD - Works for both reference and value types
public T? GetValue<T>(int id)
{
    return _dictionary.TryGetValue(id, out var value) ? value : default;
}

// ❌ BAD - Fails when T is a value type
public T? GetValue<T>(int id)
{
    return _dictionary.TryGetValue(id, out var value) ? value : null; // CS0403 error
}

// ✅ GOOD - Use default(T) for clarity if preferred
public T? GetValue<T>(int id)
{
    return _dictionary.TryGetValue(id, out var value) ? value : default(T);
}
```

This is especially important for:
- Generic return values
- Generic field/property initialization
- Conditional expressions with generics
- Generic method parameters with default values

## Regular Expressions and Security

### Prevent ReDoS (Regular Expression Denial of Service)
- **AVOID** regex for simple string operations
- Use manual string processing when possible
- If regex is necessary, use timeout parameters

```csharp
// ✅ GOOD - Manual string processing
var index = input.IndexOf(delimiter, StringComparison.Ordinal);
if (index >= 0)
{
    var prefix = input.Substring(0, index);
    var suffix = input.Substring(index + delimiter.Length);
}

// ❌ BAD - Regex for simple operations (potential ReDoS)
var match = Regex.Match(input, @"(.*)delimiter(.*)");
```

## Method Complexity

### Keep Methods Small
- Methods should be under 60 lines (MA0051)
- Extract complex logic into well-named private methods
- Single Responsibility Principle: one method, one purpose

## Static Methods

### Make Methods Static When Appropriate
- If a method doesn't access instance data, make it static (CA1822)
- Improves performance and clarifies intent

```csharp
// ✅ GOOD
private static IFdwResult<T> ValidateInput(string input)

// ❌ BAD - Doesn't use instance members but isn't static
private IFdwResult<T> ValidateInput(string input)
```

## Class Design

### Seal Classes When Possible
- Seal classes that have no derived types (CA1852)
- Improves performance and clarifies design intent

```csharp
// ✅ GOOD
public sealed class ServiceFactory : ServiceFactoryBase<TService>

// ❌ BAD - Not sealed but has no derivatives
public class ServiceFactory : ServiceFactoryBase<TService>
```

## File Organization

### One Primary Type Per File
- File name must match the primary type name (MA0048)
- Exception: Multiple generic variations of the same type
- Nested private classes are acceptable

```csharp
// ✅ GOOD - ServiceType.cs
public class ServiceType<TService, TConfiguration, TFactory>
{
    private class InternalHelper { } // OK - private nested class
}

// ❌ BAD - Multiple unrelated public types in one file
public class ServiceType { }
public class ServiceFactory { } // Should be in ServiceFactory.cs
```

## Exception Handling

### Railway-Oriented Programming
- **NEVER** throw exceptions for anticipated conditions
- **ALWAYS** return `IFdwResult<T>` for fallible operations
- Exceptions are ONLY for true system failures (OutOfMemoryException, StackOverflowException)

```csharp
// ✅ GOOD - Return Result for anticipated failures
public IFdwResult<TService> CreateService(TConfiguration config)
{
    var validation = config.Validate();
    if (!validation.IsValid)
        return FdwResult<TService>.Failure(
            ServiceMessages.ValidationFailed(validation.Errors));
    
    // ... create service
    return FdwResult<TService>.Success(service);
}

// ❌ BAD - Throwing for anticipated conditions
public TService CreateService(TConfiguration config)
{
    var validation = config.Validate();
    if (!validation.IsValid)
        throw new ValidationException(validation.Errors); // Don't throw!
    
    // ... create service
    return service;
}
```

### Catch Specific Conditions
- Only catch exceptions you can handle
- Never catch `Exception` except for logging/cleanup with re-throw
- Use exception filters when appropriate

```csharp
// ✅ GOOD - Specific handling with filters
catch (IOException ex) when (ex.HResult == 0x80070020)
{
    // Handle specific sharing violation
}

// ❌ BAD - Catching too broadly
catch (Exception ex)
{
    // Swallows all exceptions including system failures
}
```

## Dependency Injection

### Constructor Injection
- Prefer constructor injection over property injection
- Make dependencies explicit and immutable
- Use `ILogger<T>` pattern for logging

```csharp
// ✅ GOOD
public class ServiceFactory
{
    private readonly ILogger<ServiceFactory> _logger;
    
    public ServiceFactory(ILogger<ServiceFactory> logger)
    {
        _logger = logger;
    }
}
```

## Source Generation

### Use Source-Generated Logging
- Prefer `LoggerMessage` attribute over string interpolation
- Better performance and structured logging support

```csharp
// ✅ GOOD
[LoggerMessage(EventId = 1001, Level = LogLevel.Information, 
    Message = "Service {ServiceName} created")]
public static partial void ServiceCreated(ILogger logger, string serviceName);

// ❌ BAD
logger.LogInformation($"Service {serviceName} created");
```

### Use Messages Framework
- All user-facing messages should use the Messages framework
- Enables consistency and localization
- Provides structured error information

## Package Management

### Never Add Package References Manually
- **ALWAYS** use `dotnet add package`
- Use `--prerelease` flag when needed
- Central package management means manual version specification breaks builds

```bash
# ✅ GOOD
dotnet add package Serilog --prerelease

# ❌ BAD - Manual edit of .csproj
<PackageReference Include="Serilog" Version="3.0.0" />
```

## Namespace Declaration

### Use File-Scoped Namespaces
```csharp
// ✅ GOOD
using System;

namespace FractalDataWorks.Services;

public class ServiceFactory
{
}

// ❌ BAD - Block-scoped namespace
using System;

namespace FractalDataWorks.Services
{
    public class ServiceFactory
    {
    }
}
```

## PowerShell Invocation

### Security Best Practices
```powershell
# ✅ GOOD
powershell -NoProfile -Command "Get-Process"

# ❌ BAD - Bypassing execution policy
powershell -ExecutionPolicy Bypass -Command "Get-Process"
```

## Testing Standards

### xUnit.v3 with Shouldly
- Use xUnit.v3 for all unit tests
- Use Shouldly for assertions
- NO underscores in test method names
- One assertion/concept per test
- Write diagnostic information on failure

```csharp
// ✅ GOOD
[Fact]
public void ServiceCreationWithValidConfigurationShouldSucceed()
{
    var result = factory.Create(validConfig);
    result.IsSuccess.ShouldBeTrue();
    _output.WriteLine($"Service created: {result.Value?.Id}");
}

// ❌ BAD
[Fact]
public void Service_Creation_Should_Work_And_Validate_And_Log()
{
    // Multiple concerns in one test
}
```

## Comments and Documentation

### XML Documentation
- All public APIs must have XML documentation
- Include `<summary>`, `<param>`, `<returns>` tags
- Add `<remarks>` for complex behaviors

### Code Comments
- **DO NOT** add comments unless explicitly requested
- Code should be self-documenting through good naming
- If comments are needed, explain WHY not WHAT

```csharp
// ✅ GOOD - Self-documenting code
public IFdwResult<TService> CreateServiceWithRetry(TConfiguration config, int maxAttempts)

// ❌ BAD - Comment explains what the code already says
// This method creates a service
public IFdwResult<TService> Create(TConfiguration c) // Bad naming needs comment
```

## Performance Considerations

### Use FastGenericNew for Object Creation
- Prefer `FastNew.TryCreateInstance<T>()` over `Activator.CreateInstance`
- Better performance for generic type instantiation

### Avoid LINQ for Simple Operations
- Use direct loops for performance-critical code
- LINQ is acceptable for readability in non-critical paths

## Git Commit Standards

### No Attribution in Commits
- Never include "Generated by", "Created with", or tool attribution
- Focus on WHAT changed and WHY
- Group related changes in logical commits

```bash
# ✅ GOOD
git commit -m "Add ServiceType base class with factory creation support"

# ❌ BAD
git commit -m "Add ServiceType base class 🤖 Generated with Claude"
```