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
- `string Message` - First message or empty string; throws InvalidOperationException on successful results when accessed

### IGenericResult&lt;T&gt; Generic Interface

Generic result interface that combines value and message handling:

```csharp
public interface IGenericResult<T> : IGenericResult, IGenericResult<T>
{
    // Inherits all base functionality plus:
    T Value { get; }  // Throws InvalidOperationException if IsSuccess is false
    IGenericResult<TNew> Map<TNew>(Func<T, TNew> mapper);
    TResult Match<TResult>(Func<T, TResult> success, Func<string, TResult> failure);
}
```

### GenericResult Implementation

**Message Handling**:
- Stores messages as `List<IFractalMessage>` internally
- Supports construction with string, IFractalMessage, or collections of IFractalMessage
- `IsEmpty` returns true when message collection is empty (not when IsSuccess is true)
- `Message` property returns first message or empty string
- `Messages` property provides read-only access to all messages

**Factory Methods**:
```csharp
// Success results
GenericResult.Success()                                    // No message
GenericResult.Success(string message)                      // With string message  
GenericResult.Success(IFractalMessage message)                 // With IFractalMessage
GenericResult.Success<TMessage>(TMessage message)          // With typed IFractalMessage
GenericResult.Success(IEnumerable<IFractalMessage> messages)   // With message collection
GenericResult.Success(params IFractalMessage[] messages)       // With message array

// Failure results
GenericResult.Failure(string message)                      // With string message
GenericResult.Failure(IFractalMessage message)                 // With IFractalMessage
GenericResult.Failure<TMessage>(TMessage message)          // With typed IFractalMessage  
GenericResult.Failure(IEnumerable<IFractalMessage> messages)   // With message collection
GenericResult.Failure(params IFractalMessage[] messages)       // With message array
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
GenericResult<T>.Success(T value, IFractalMessage message)              // Value + IFractalMessage
GenericResult<T>.Success<TMessage>(T value, TMessage message)       // Value + typed message
GenericResult<T>.Success(T value, IEnumerable<IFractalMessage> messages)// Value + collection
GenericResult<T>.Success(T value, params IFractalMessage[] messages)    // Value + array

// Failure results (static methods with different signatures)
GenericResult<T>.Failure<T>(string message)           // Generic static method
GenericResult<T>.Failure(string message)              // Instance-type static method
GenericResult<T>.Failure(IFractalMessage message)         // With IFractalMessage
GenericResult<T>.Failure<TMessage>(TMessage message)  // With typed IFractalMessage
GenericResult<T>.Failure(IEnumerable<IFractalMessage> messages) // With collection
GenericResult<T>.Failure(params IFractalMessage[] messages)     // With array
```

**Functional Methods**:
```csharp
// Map transforms successful results, preserves failures
IGenericResult<TNew> Map<TNew>(Func<T, TNew> mapper)
{
    return IsSuccess 
        ? GenericResult<TNew>.Success(mapper(Value))
        : GenericResult<TNew>.Failure(Message);  // Only uses first message
}

// Match executes appropriate function based on result state  
TResult Match<TResult>(Func<T, TResult> success, Func<string, TResult> failure)
{
    return IsSuccess ? success(Value) : failure(Message);  // Only uses first message
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
// Simple success/failure
IGenericResult result = GenericResult.Success("Operation completed");
if (result.IsSuccess)
{
    Console.WriteLine($"Success: {result.Message}");
}

// Result with value
IGenericResult<int> numberResult = GenericResult<int>.Success(42, "Number processed");
if (numberResult.IsSuccess)
{
    Console.WriteLine($"Value: {numberResult.Value}"); // 42
}

// Failure handling
IGenericResult<string> failedResult = GenericResult<string>.Failure("Something went wrong");
Console.WriteLine($"Failed: {failedResult.Message}");
// Console.WriteLine(failedResult.Value); // Would throw InvalidOperationException
```

### Working with Messages

```csharp
// Multiple messages
var messages = new List<IFractalMessage>
{
    new FractalMessage("First step completed"),  
    new FractalMessage("Second step completed")
};
IGenericResult result = GenericResult.Success(messages);

// Access all messages
foreach (var msg in result.Messages)
{
    Console.WriteLine(msg.Message);
}

// Message property returns first message only
Console.WriteLine(result.Message); // "First step completed"
```

### Functional Composition

```csharp
IGenericResult<int> GetNumber() => GenericResult<int>.Success(10);
IGenericResult<string> ProcessNumber(int num) => GenericResult<string>.Success($"Processed: {num}");

// Using Map to transform successful results
IGenericResult<int> numberResult = GetNumber();
IGenericResult<string> stringResult = numberResult.Map(x => $"Number is {x}");

// Using Match for branching logic  
string output = numberResult.Match(
    success: value => $"Got value: {value}",
    failure: error => $"Error: {error}"
);
```

### Working with NonResult

```csharp
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

### Message vs Messages Property
- `Message` returns the **first** message from the collection or empty string if no messages
- `Messages` provides access to the **full collection** of IFractalMessage objects
- Only the first message is used in Map/Match operations

### IsEmpty Behavior Differences
- **GenericResult**: `IsEmpty` returns `true` when message collection is empty (regardless of success state)
- **GenericResult&lt;T&gt;**: `IsEmpty` returns `true` when `!_hasValue` (i.e., when failed - overrides base behavior)

### Exception Throwing Behavior
- `Value` property throws `InvalidOperationException` when accessed on failed results
- `Message` property on `IGenericResult` throws `InvalidOperationException` when accessed on successful results (though GenericResult implementation returns empty string)

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
2. **Enhanced Enum Dependency**: IFractalMessage extends IEnumOption, indicating integration with enhanced enum pattern  
3. **Dual Interface Support**: Supports both IGenericResult (standard) and IGenericResult (enhanced) interfaces
4. **Factory Pattern**: Extensive use of static factory methods rather than public constructors

## Dependencies

- **FractalDataWorks.Messages**: Required for IFractalMessage, FractalMessage, and MessageSeverity types
- **FractalDataWorks.EnhancedEnums** (indirect): Required through Messages dependency for IEnumOption interface