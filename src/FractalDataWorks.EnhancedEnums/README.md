# FractalDataWorks.EnhancedEnums

**VALIDATED CODE STATUS: PRODUCTION READY**

Core library providing base classes, interfaces, and attributes for Enhanced Enums - a pattern for creating type-safe, extensible, object-oriented alternatives to traditional C# enums. This library is designed to work with source generators and provides the foundational types for enhanced enum implementations.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Types](#core-types)
- [Implementation Patterns](#implementation-patterns)
- [Extended Enums](#extended-enums)
- [Attribute Reference](#attribute-reference)
- [Performance Characteristics](#performance-characteristics)
- [Best Practices](#best-practices)
- [Code Coverage Notes](#code-coverage-notes)

## Overview

FractalDataWorks.EnhancedEnums provides the foundational infrastructure for enhanced enum patterns in C#. It includes base classes, interfaces, and attributes that enable:

### Key Benefits

- **Type Safety**: Strong typing with compile-time validation
- **Rich Behavior**: Object-oriented enum instances with properties and methods
- **Efficient Lookups**: O(1) dictionary-based lookups using FrozenDictionary (.NET 8+) or ReadOnlyDictionary
- **Extensibility**: Easy to add new enum values and behavior
- **Cross-Assembly Discovery**: Support for discovering enum options across multiple assemblies
- **Multiple Patterns**: Support for various implementation approaches including extended enums

## Installation

This is the core library used by Enhanced Enums source generators. Typically you would reference the source generator packages which include this automatically:

```bash
# For source generation scenarios (recommended)
dotnet add package FractalDataWorks.EnhancedEnums.SourceGenerators

# For manual usage of base classes only
dotnet add package FractalDataWorks.EnhancedEnums
```

## Core Types

### IEnumOption Interface

The foundational interface for all enhanced enum types:

```csharp
public interface IEnumOption
{
    int Id { get; }      // Unique identifier
    string Name { get; } // Display name
}

public interface IEnumOption<T> : IEnumOption where T : IEnumOption<T>
{
    // Self-referencing generic interface for strongly-typed enhanced enums
}
```

### EnumOptionBase<T> Abstract Class

Base class for creating enhanced enum options with built-in Id and Name properties:

```csharp
public abstract class EnumOptionBase<T> : IEnumOption<T> where T : IEnumOption<T>
{
    [EnumLookup("GetById")]
    public int Id { get; }

    [EnumLookup("GetByName")]  
    public string Name { get; }

    protected EnumOptionBase(int id, string name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
```

### EnumCollectionBase<T> Abstract Class

Base class providing standard collection methods for enhanced enums:

```csharp
public abstract class EnumCollectionBase<T> where T : IEnumOption<T>
{
    protected static ImmutableArray<T> _all = [];
    protected static T _empty = default!;

    // Core methods
    public static ImmutableArray<T> All() => _all;
    public static T Empty() => _empty;
    public static T GetByName(string? name) => /* implementation */;
    public static T GetById(int id) => /* implementation */;
    public static bool TryGetByName(string? name, out T? value) => /* implementation */;
    public static bool TryGetById(int id, out T? value) => /* implementation */;
    public static IEnumerable<T> AsEnumerable() => _all;
    public static int Count => _all.Length;
    public static bool Any() => _all.Length > 0;
    public static T GetByIndex(int index) => _all[index];
}
```

## Implementation Patterns

### Pattern 1: Basic Enhanced Enum

Create a basic enhanced enum using the base class:

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace MyApp.Enums;

public abstract class Priority : EnumOptionBase<Priority>
{
    protected Priority(int id, string name) : base(id, name) { }
    public abstract int Level { get; }
}

[EnumOption("High")]
public sealed class HighPriority : Priority
{
    public HighPriority() : base(1, "High") { }
    public override int Level => 100;
}

[EnumOption("Medium")]
public sealed class MediumPriority : Priority
{
    public MediumPriority() : base(2, "Medium") { }
    public override int Level => 50;
}

// Collection generation (handled by source generators)
[EnumCollection(CollectionName = "Priorities")]
public abstract class PriorityCollectionBase : EnumCollectionBase<Priority>
{
    // Source generator populates this automatically
}
```

### Pattern 2: Static Readonly Pattern

Alternative pattern for self-contained enums:

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace MyApp.Enums;

public sealed class Status : EnumOptionBase<Status>
{
    public static readonly Status Active = new(1, "Active", true);
    public static readonly Status Inactive = new(2, "Inactive", false);
    public static readonly Status Pending = new(3, "Pending", true);

    public bool IsActionable { get; }

    private Status(int id, string name, bool isActionable) : base(id, name)
    {
        IsActionable = isActionable;
    }
}

// Use with StaticEnumCollectionAttribute for source generation
[StaticEnumCollection("Statuses")]
public abstract class StatusCollection : EnumCollectionBase<Status>
{
}
```

### Pattern 3: Cross-Assembly Discovery

Enable discovery across multiple assemblies:

```csharp
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace MyApp.Plugins;

// In shared library
public abstract class PluginCommand : EnumOptionBase<PluginCommand>
{
    protected PluginCommand(int id, string name) : base(id, name) { }
    public abstract Task Execute();
}

// In main application
[GlobalEnumCollection(CollectionName = "AllPluginCommands")]
public abstract class PluginCommandRegistry : EnumCollectionBase<PluginCommand>
{
    // Automatically discovers commands from all referenced assemblies
}

// In plugin assembly
[EnumOption("SaveFile")]
public sealed class SaveFileCommand : PluginCommand
{
    public SaveFileCommand() : base(1, "SaveFile") { }
    public override Task Execute() => Task.CompletedTask; // implementation
}
```

## Extended Enums

Enhanced Enums also supports extending existing C# enums with rich behavior:

### ExtendedEnumOptionBase<T, TEnum>

Base class for wrapping standard C# enums:

```csharp
public abstract class ExtendedEnumOptionBase<T, TEnum> : IEnumOption
    where T : ExtendedEnumOptionBase<T, TEnum>  
    where TEnum : struct, Enum
{
    public int Id => (int)(object)EnumValue;
    public string Name => EnumValue.ToString();
    public TEnum EnumValue { get; }

    protected ExtendedEnumOptionBase(TEnum enumValue)
    {
        EnumValue = enumValue;
    }

    // Implicit conversion operators and equality members
    public static implicit operator TEnum(ExtendedEnumOptionBase<T, TEnum> extendedEnum)
        => extendedEnum.EnumValue;
}
```

### Extended Enum Usage

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace MyApp.Http;

// Standard C# enum
public enum HttpStatusCodeEnum
{
    OK = 200,
    NotFound = 404,
    InternalServerError = 500
}

// Extended enum with rich behavior
[ExtendEnum(typeof(HttpStatusCodeEnum))]
public sealed class HttpStatusCode : ExtendedEnumOptionBase<HttpStatusCode, HttpStatusCodeEnum>
{
    public HttpStatusCode(HttpStatusCodeEnum value) : base(value) { }

    public bool IsSuccess => (int)EnumValue >= 200 && (int)EnumValue < 300;
    public bool IsClientError => (int)EnumValue >= 400 && (int)EnumValue < 500;
    public bool IsServerError => (int)EnumValue >= 500;
}
```

## Attribute Reference

### Core Attributes

#### EnumCollectionAttribute
Marks a class for local enhanced enum collection generation.
```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EnumCollectionAttribute : Attribute
{
    public string? CollectionName { get; set; }
    public Type? DefaultGenericReturnType { get; set; }
    public bool UseSingletonInstances { get; set; }
}
```

#### GlobalEnumCollectionAttribute  
Marks a class for cross-assembly enhanced enum collection generation.
```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class GlobalEnumCollectionAttribute : Attribute
{
    public string? CollectionName { get; set; }
    public Type? DefaultGenericReturnType { get; set; }
    public bool UseSingletonInstances { get; set; }
}
```

#### EnumOptionAttribute
Marks a class as an enhanced enum option.
```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EnumOptionAttribute : Attribute
{
    public string? Name { get; }
    
    public EnumOptionAttribute() { } // Uses class name
    public EnumOptionAttribute(string name) { Name = name; }
}
```

#### EnumLookupAttribute
Marks a property for lookup method generation.
```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class EnumLookupAttribute : Attribute
{
    public string MethodName { get; }
    public bool AllowMultiple { get; }
    public Type? ReturnType { get; }

    public EnumLookupAttribute(string methodName = "", bool allowMultiple = false, Type? returnType = null)
    {
        MethodName = methodName;
        AllowMultiple = allowMultiple;
        ReturnType = returnType;
    }
}
```

### Static Collection Attributes

#### StaticEnumCollectionAttribute
For static collections with local discovery.
```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class StaticEnumCollectionAttribute : Attribute
{
    public string CollectionName { get; set; } = string.Empty;
    public Type? DefaultGenericReturnType { get; set; }
    public bool UseSingletonInstances { get; set; }
}
```

#### GlobalStaticEnumCollectionAttribute  
For static collections with cross-assembly discovery.
```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GlobalStaticEnumCollectionAttribute : Attribute
{
    public string CollectionName { get; set; } = string.Empty;
    public Type? DefaultGenericReturnType { get; set; }
    public bool UseSingletonInstances { get; set; }
}
```

### Extended Enum Attributes

#### ExtendEnumAttribute
Extends an existing C# enum with rich behavior.
```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ExtendEnumAttribute : Attribute
{
    public Type EnumType { get; }
    public string CollectionName { get; set; }
    public StringComparison NameComparison { get; set; }
    public bool GenerateFactoryMethods { get; set; } 
    public bool GenerateStaticCollection { get; set; }
    public bool UseSingletonInstances { get; set; }
    public bool IncludeReferencedAssemblies { get; set; }
    public bool Generic { get; set; }
    public Type? DefaultReturnType { get; set; }
}
```

## Performance Characteristics

### Lookup Performance
- **All() Method**: O(1) - Returns cached `ImmutableArray<T>`
- **GetByName()**: O(1) - Uses `FrozenDictionary<string, T>` (.NET 8+) or `ReadOnlyDictionary<string, T>`
- **GetById()**: O(1) - Uses `FrozenDictionary<int, T>` (.NET 8+) or `ReadOnlyDictionary<int, T>`
- **Custom Lookups**: O(1) - Generated lookup methods use dedicated dictionaries

### Memory Efficiency
- **Singleton Pattern**: Single instance per enum value when `UseSingletonInstances = true`
- **Lazy Initialization**: Lookup dictionaries created on first access
- **Immutable Collections**: Uses `ImmutableArray<T>` for thread safety

### .NET Version Optimizations
The library includes conditional compilation for optimal performance:

```csharp
#if NET8_0_OR_GREATER
private static FrozenDictionary<string, T>? _lookupByName;
private static FrozenDictionary<int, T>? _lookupById;
#else
private static ReadOnlyDictionary<string, T>? _lookupByName;
private static ReadOnlyDictionary<int, T>? _lookupById;
#endif
```

## Best Practices

### 1. Choosing the Right Pattern
- **EnumOptionBase<T>**: Use for new enhanced enums with rich behavior
- **ExtendedEnumOptionBase<T, TEnum>**: Use to add behavior to existing C# enums
- **EnumCollectionBase<T>**: Use when you need standard collection methods
- **Static readonly**: Use for self-contained enums with business logic

### 2. Performance Considerations
- Enable singleton instances for immutable enum options: `UseSingletonInstances = true`
- Use appropriate string comparison: `StringComparison.Ordinal` for performance
- Consider lazy initialization patterns for large enum collections

### 3. Cross-Assembly Scenarios
- Use `GlobalEnumCollectionAttribute` for plugin architectures
- Ensure referenced assemblies are properly configured
- Use package references rather than project references for global discovery

### 4. Type Safety
- Always use generic constraints where possible
- Implement abstract properties/methods to enforce behavior contracts
- Use `[EnumLookup]` attributes for type-safe property-based lookups

## Code Coverage Notes

The following code should be excluded from coverage analysis as they are infrastructure classes with minimal testable behavior:

### Attributes (Configuration Only)
- `EnumCollectionAttribute` - Configuration attribute, no business logic
- `GlobalEnumCollectionAttribute` - Configuration attribute, no business logic  
- `EnumOptionAttribute` - Configuration attribute, no business logic
- `EnumLookupAttribute` - Configuration attribute, no business logic
- `StaticEnumCollectionAttribute` - Configuration attribute, no business logic
- `GlobalStaticEnumCollectionAttribute` - Configuration attribute, no business logic
- `ExtendEnumAttribute` - Configuration attribute, no business logic
- `ExtendedEnumOptionAttribute` - Configuration attribute, no business logic
- `GlobalExtendedEnumCollectionAttribute` - Configuration attribute with `[ExcludeFromCodeCoverage]`

### Infrastructure Classes
- `IEnumOption` interface - No implementation to test
- `IEnumOption<T>` interface - No implementation to test

### Partial Coverage Expected
- `EnumOptionBase<T>` - Constructor and property initialization (high coverage expected)
- `ExtendedEnumOptionBase<T, TEnum>` - Core functionality should be tested
- `EnumCollectionBase<T>` - Collection methods should be tested but some edge cases may be difficult to reach

The actual enhanced enum implementations and generated collections should have comprehensive test coverage as they contain the core business logic.

## Target Frameworks

- .NET Standard 2.0
- .NET 10.0

## Dependencies

This package has the following dependencies:

**NuGet Packages:**
- Microsoft.CodeAnalysis.CSharp - For source generation support
- Microsoft.Bcl.HashCode - For HashCode compatibility across frameworks

**Project References:**
- FractalDataWorks.Abstractions - Core abstractions and interfaces

**Framework Types** (no explicit package references required):
- System.Collections.Immutable - For efficient collection storage
- System.Collections.Frozen (NET8_0_OR_GREATER) - For optimized lookups on .NET 8+

## License

This library is part of the FractalDataWorks toolkit and is licensed under the MIT License.