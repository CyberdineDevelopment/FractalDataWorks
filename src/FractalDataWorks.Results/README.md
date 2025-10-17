# FractalDataWorks.Results

A robust result pattern library that provides a standardized way to handle operation outcomes with detailed message support. This library implements the Result pattern to replace exceptions for expected failures and provide rich context about operation results.

## Project Overview

The FractalDataWorks.Results library provides:
- **IGenericResult** and **IGenericResult&lt;T&gt;** interfaces for result handling
- **GenericResult** and **GenericResult&lt;T&gt;** concrete implementations
- **NonResult** unit type for operations that don't return values
- Integration with **FractalDataWorks.Messages** for structured message handling
- Functional programming methods (Map, Match) for result composition

**Target Framework**: .NET Standard 2.0  
**Dependencies**: FractalDataWorks.Messages

## Key Types and Behavior

### IGenericResult Interface

Base interface for all result types with the following properties:

```csharp
public interface IGenericResult : IGenericResult
{
    bool IsEmpty { get; }    // Indicates if result has no messages
    bool Error { get; }      // Alias for IsFailure (!IsSuccess)
}
```

**Inherited from IGenericResult**:
- `bool IsSuccess` - True if operation succeeded
- `bool IsFailure` - True if operation failed (!IsSuccess)
- `string? CurrentMessage` - Gets the most recent message (LIFO - Last In, First Out)
- `IReadOnlyList<IGenericMessage> Messages` - Gets the collection of all messages

### IGenericResult&lt;T&gt; Generic Interface

Generic result interface that combines value and message handling:

```csharp
public interface IGenericResult<out T> : IGenericResult
{
    // Inherits all base functionality plus:
    T? Value { get; }  // Throws InvalidOperationException if IsSuccess is false
}
```

### GenericResult Implementation

**Message Handling**:
- Stores messages internally using collection expression syntax `[]`
- Supports construction with string, IGenericMessage, or collections of IGenericMessage
- `IsEmpty` returns true when message collection is empty (not when IsSuccess is true)
- `CurrentMessage` property returns the most recent message (LIFO - Last In, First Out) or null
- `Messages` property provides read-only access to all messages

**Factory Methods**:
```csharp
// Success results
GenericResult.Success()                                    // No message
GenericResult.Success(string message)                      // With string message  
GenericResult.Success(IGenericMessage message)                 // With IGenericMessage
GenericResult.Success<TMessage>(TMessage message)          // With typed IGenericMessage
GenericResult.Success(IEnumerable<IGenericMessage> messages)   // With message collection
GenericResult.Success(params IGenericMessage[] messages)       // With message array

// Failure results
GenericResult.Failure(string message)                      // With string message
GenericResult.Failure(IGenericMessage message)                 // With IGenericMessage
GenericResult.Failure<TMessage>(TMessage message)          // With typed IGenericMessage  
GenericResult.Failure(IEnumerable<IGenericMessage> messages)   // With message collection
GenericResult.Failure(params IGenericMessage[] messages)       // With message array
```

### GenericResult&lt;T&gt; Generic Implementation

**Value Handling**:
- `_hasValue` field tracks whether value is accessible (set to IsSuccess in constructors)
- `IsEmpty` returns true when `!_hasValue` (overrides base implementation)
- `Value` property throws `InvalidOperationException` when `!_hasValue`
- Failed results store `default!` as the value

**Factory Methods**:
```csharp
// Success results with values
GenericResult<T>.Success(T value)                                    // Value only
GenericResult<T>.Success(T value, string message)                   // Value + message
GenericResult<T>.Success(T value, IGenericMessage message)              // Value + IGenericMessage
GenericResult<T>.Success<TMessage>(T value, TMessage message)       // Value + typed message
GenericResult<T>.Success(T value, IEnumerable<IGenericMessage> messages)// Value + collection
GenericResult<T>.Success(T value, params IGenericMessage[] messages)    // Value + array

// Failure results (static methods with different signatures)
GenericResult<T>.Failure<T>(string message)           // Generic static method
GenericResult<T>.Failure(string message)              // Instance-type static method
GenericResult<T>.Failure(IGenericMessage message)         // With IGenericMessage
GenericResult<T>.Failure<TMessage>(TMessage message)  // With typed IGenericMessage
GenericResult<T>.Failure(IEnumerable<IGenericMessage> messages) // With collection
GenericResult<T>.Failure(params IGenericMessage[] messages)     // With array
```

**Functional Methods** (GenericResult&lt;T&gt; implementation-specific):
```csharp
// Map transforms successful results, preserves failures
IGenericResult<TNew> Map<TNew>(Func<T, TNew> mapper)
{
    return IsSuccess
        ? GenericResult<TNew>.Success(mapper(Value))
        : GenericResult<TNew>.Failure(CurrentMessage ?? "Operation failed");
}

// Match executes appropriate function based on result state
T Match<T>(Func<TResult, T> success, Func<string, T> failure)
{
    return IsSuccess ? success(Value) : failure(CurrentMessage ?? string.Empty);
}
```

### NonResult Unit Type

A struct that represents the absence of a value, similar to `void` but usable as a generic type parameter:

```csharp
public struct NonResult : IEquatable<NonResult>
{
    public static readonly NonResult Value;  // Default instance
    
    // All instances are equal (structural equality)
    public bool Equals(NonResult other) => true;
    public override bool Equals(object? obj) => obj is NonResult;  
    public override int GetHashCode() => 0;
    public override string ToString() => nameof(NonResult);
    
    public static bool operator ==(NonResult left, NonResult right) => true;
    public static bool operator !=(NonResult left, NonResult right) => false;
}
```

## Usage Patterns

### Basic Result Creation and Checking

```csharp
using System;
using FractalDataWorks.Results;

// Simple success/failure
IGenericResult result = GenericResult.Success("Operation completed");
if (result.IsSuccess)
{
    Console.WriteLine($"Success: {result.CurrentMessage}");
}

// Result with value
IGenericResult<int> numberResult = GenericResult<int>.Success(42, "Number processed");
if (numberResult.IsSuccess)
{
    Console.WriteLine($"Value: {numberResult.Value}"); // 42
}

// Failure handling
IGenericResult<string> failedResult = GenericResult<string>.Failure("Something went wrong");
Console.WriteLine($"Failed: {failedResult.CurrentMessage}");
// Console.WriteLine(failedResult.Value); // Would throw InvalidOperationException
```

### Working with Messages

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Results;

// Multiple messages using collection expression
IGenericMessage[] messages =
[
    new GenericMessage("First step completed"),
    new GenericMessage("Second step completed")
];
IGenericResult result = GenericResult.Success(messages);

// Access all messages
foreach (var msg in result.Messages)
{
    Console.WriteLine(msg.Message);
}

// CurrentMessage property returns most recent message (LIFO - Last In, First Out)
Console.WriteLine(result.CurrentMessage); // "Second step completed"
```

### Functional Composition

**Note:** The `GenericResult<T>` implementation provides `Map` and `Match` methods for functional composition (these are not part of the interface).

```csharp
using FractalDataWorks.Results;

IGenericResult<int> GetNumber() => GenericResult<int>.Success(10);
IGenericResult<string> ProcessNumber(int num) => GenericResult<string>.Success($"Processed: {num}");

// Using Map to transform successful results (available on GenericResult<T>)
GenericResult<int> numberResult = (GenericResult<int>)GetNumber();
IGenericResult<string> stringResult = numberResult.Map(x => $"Number is {x}");

// Using Match for branching logic (available on GenericResult<T>)
string output = numberResult.Match(
    success: value => $"Got value: {value}",
    failure: error => $"Error: {error}"
);
```

### Working with NonResult

```csharp
using System;
using FractalDataWorks.Results;

// For operations that don't return values but need result tracking
IGenericResult<NonResult> LogOperation(string message)
{
    try
    {
        Console.WriteLine(message);
        return GenericResult<NonResult>.Success(NonResult.Value, "Logged successfully");
    }
    catch (Exception ex)
    {
        return GenericResult<NonResult>.Failure(ex.Message);
    }
}
```

## Important Implementation Details

### CurrentMessage vs Messages Property
- `CurrentMessage` returns the **most recent** message (LIFO - Last In, First Out) or null if no messages
- `Messages` provides access to the **full collection** of IGenericMessage objects
- Only the current message (last added) is used in Map/Match operations

### IsEmpty Behavior Differences
- **GenericResult**: `IsEmpty` returns `true` when message collection is empty (regardless of success state)
- **GenericResult&lt;T&gt;**: `IsEmpty` returns `true` when `!_hasValue` (i.e., when failed - overrides base behavior)

### Exception Throwing Behavior
- `Value` property throws `InvalidOperationException` when accessed on failed results (when `!_hasValue`)

### Generic Factory Method Overloads
- `GenericResult<T>.Failure<T>(string message)` - Generic static method for any type
- `GenericResult<T>.Failure(string message)` - Instance-type specific static method

## Code Coverage Exclusions

The following code should be excluded from coverage testing:

### NonResult.cs (Complete Exclusion)
- **Reason**: Simple unit type with no business logic
- **Evidence**: Contains `[ExcludeFromCodeCoverage]` attribute and XML comment `<ExcludeFromTest>`
- **Justification**: All methods are trivial equality operations and constants

## Breaking Changes and Refactor Notes

This library appears to be part of a recent refactor. Key observations:

1. **Message Integration**: Heavy integration with FractalDataWorks.Messages framework for structured message handling
2. **Enhanced Enum Dependency**: IGenericMessage extends IEnumOption, indicating integration with enhanced enum pattern  
3. **Dual Interface Support**: Supports both IGenericResult (standard) and IGenericResult (enhanced) interfaces
4. **Factory Pattern**: Extensive use of static factory methods rather than public constructors

## Dependencies

This package has the following dependencies:

- **FractalDataWorks.Messages** - Required for IGenericMessage, GenericMessage, and message handling
- **FractalDataWorks.Results.Abstractions** - Contains IGenericResult interface definitions

Indirect dependencies (via Messages):
- **FractalDataWorks.EnhancedEnums** - Required for IEnumOption interface

## Target Frameworks

- .NET Standard 2.0