# FractalDataWorks.Collections.SourceGenerators

Source generator for FractalDataWorks.Collections that creates ultra-high-performance type collections with revolutionary compile-time discovery and O(1) lookup performance using .NET 8+ FrozenDictionary with alternate key lookup.

## Overview

This source generator analyzes your code at compile time to:
1. **Ultra-Fast Discovery**: TypeOption-first discovery with O(types_with_typeoption) complexity
2. **Universal Type Support**: Include concrete, abstract, static, and interface types in collections
3. **Dynamic Lookup Generation**: Create lookup methods based on `[TypeLookup]` attributes automatically
4. **Smart Instantiation**: Only instantiate concrete types, safely handle abstract/static types
5. **Alternate Key Lookup**: O(1) property-based lookups using `FrozenDictionary.GetAlternateLookup<T>()`

## Installation

```bash
dotnet add package FractalDataWorks.Collections.SourceGenerators
```

For `.Abstractions` projects, use analyzer-only packaging:
```xml
<PackageReference Include="FractalDataWorks.Collections.SourceGenerators" Version="*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

## How It Works

### Discovery Process (ULTRA-OPTIMIZED)

1. **TypeOption-First Discovery (Revolutionary Performance)**
   ```csharp
   [TypeOption("CSV")]
   public class CsvDataContainerType : DataContainerType { }

   [TypeOption] // Uses class name if no parameter provided
   public class JsonDataContainerType : DataContainerType { }

   [TypeOption("BaseContainer")] // Abstract types included in collection
   public abstract class BaseDataContainerType : DataContainerType { }

   [TypeOption("UtilityContainer")] // Static types included in collection
   public static class UtilityDataContainerType : DataContainerType { }
   ```
   - **STEP 1**: Single pass through all assemblies to find `[TypeOption]` attributes
   - **STEP 2**: Group discovered types by their base type (inheritance check only on TypeOption types)
   - **Includes ALL types**: concrete, abstract, static, and interface types with `[TypeOption]`
   - **Smart Instantiation**: Only instantiates concrete types that can be created with `new`
   - **Complexity**: O(types_with_typeoption) instead of O(collections × assemblies × all_types)
   - **Performance**: Dramatically faster on large codebases (exponential improvement)

2. **Collection Discovery (Attribute-Based - O(k) Performance)**
   ```csharp
   [TypeCollection(typeof(DataContainerType), typeof(IDataContainer), typeof(DataContainerTypes))]
   public partial class DataContainerTypes : TypeCollectionBase<DataContainerType, IDataContainer> { }
   ```
   - **STEP 3**: Find `[TypeCollection]` attributes and extract base type
   - **STEP 4**: O(1) lookup of pre-discovered TypeOption types by base type
   - **No more scanning**: All type discovery already completed in step 1
   - **Result**: Near-instant collection generation regardless of codebase size

3. **Code Generation with .NET 8+ Optimizations**
   - Creates partial class implementations with FrozenDictionary&lt;int, T&gt; primary storage
   - **Dynamic TypeLookup**: Generates lookup methods based on `[TypeLookup]` attributes in base class
   - **Alternate Key Lookup**: Uses `FrozenDictionary.GetAlternateLookup<T>()` for O(1) property-based lookups
   - **Clean Method Names**: `Id(5)` and `Category("file")` instead of `GetById()` and `GetByCategory()`
   - Generates Empty() implementations with intelligent defaults

### Generated Code Structure

For a base type with TypeLookup attributes:
```csharp
public abstract class DataContainerType : TypeOptionBase
{
    [TypeLookup] // Generates Id(int id) method
    public int Id { get; }

    [TypeLookup] // Generates Name(string name) method
    public string Name { get; }

    [TypeLookup] // Generates Category(string category) method
    public string? Category { get; }
}

[TypeCollection(typeof(DataContainerType), typeof(IDataContainer), typeof(DataContainerTypes))]
public partial class DataContainerTypes : TypeCollectionBase<DataContainerType, IDataContainer>
{
}
```

The generator creates:

```csharp
public static partial class DataContainerTypes
{
    // Primary FrozenDictionary storage with ID-based primary key
    private static readonly FrozenDictionary<int, IDataContainer> _all = /* initialized in static constructor */;

    // Static properties for ALL discovered types (concrete, abstract, static)
    public static IDataContainer Csv => _all.TryGetValue(1, out var result) ? result : _empty;      // Concrete
    public static IDataContainer Json => _all.TryGetValue(2, out var result) ? result : _empty;     // Concrete
    public static IDataContainer Parquet => _all.TryGetValue(3, out var result) ? result : _empty;  // Concrete
    public static IDataContainer BaseContainer => _empty;     // Abstract - returns empty instance
    public static IDataContainer UtilityContainer => _empty;  // Static - returns empty instance

    // High-performance collection access
    public static IReadOnlyCollection<IDataContainer> All() => _all.Values;

    // DYNAMIC LOOKUP METHODS - Generated based on [TypeLookup] attributes

    // Primary key lookup (uses dictionary directly)
    public static IDataContainer Id(int id) =>
        _all.TryGetValue(id, out var result) ? result : _empty;

    // Alternate key lookups (uses GetAlternateLookup for O(1) performance)
    public static IDataContainer Name(string name)
    {
        var alternateLookup = _all.GetAlternateLookup<string>();
        return alternateLookup.TryGetValue(name, out var result) ? result : _empty;
    }

    public static IDataContainer Category(string category)
    {
        var alternateLookup = _all.GetAlternateLookup<string>();
        return alternateLookup.TryGetValue(category, out var result) ? result : _empty;
    }

    // Empty instance with intelligent defaults
    public static IDataContainer Empty() => _empty;
    private static readonly IDataContainer _empty = new EmptyDataContainer();

    // Nested Empty class with default property values
    internal sealed class EmptyDataContainer : DataContainerType
    {
        internal EmptyDataContainer() : base(0, string.Empty, null) { }

        // Override abstract properties with appropriate defaults
        public override string FileExtension => string.Empty;
        public override bool SupportsStreaming => false;
        // ... other property overrides
    }

    static DataContainerTypes()
    {
        var dictionary = new Dictionary<int, IDataContainer>();

        // Only instantiate concrete types - abstract/static types are included but not instantiated
        var csv = new CsvDataContainerType();
        dictionary.Add(csv.Id, csv);

        var json = new JsonDataContainerType();
        dictionary.Add(json.Id, json);

        var parquet = new ParquetDataContainerType();
        dictionary.Add(parquet.Id, parquet);

        // BaseContainer is abstract - included in collection but not instantiated
        // UtilityContainer is static - included in collection but not instantiated

        _all = dictionary.ToFrozenDictionary();
    }
}
```

## Performance Optimizations

### Runtime Performance
- **FrozenDictionary&lt;int, T&gt;**: O(1) ID-based primary storage using hash-optimized .NET 8+ collections
- **Alternate Key Lookup**: O(1) property-based lookups using `GetAlternateLookup<T>()` - no separate dictionaries needed
- **Static Property Access**: O(1) property access via direct dictionary lookups using constructor-extracted IDs
- **Smart Instantiation**: Only concrete types instantiated, abstract/static types return safe empty instances
- **Empty Pattern**: Returns cached empty instance instead of null, eliminating null checks
- **Zero Allocations**: All lookups use pre-computed frozen collections with no runtime allocations

### Collection Access Patterns
- **TypeCollectionBase&lt;TBase&gt;**: Single generic returns `ImmutableArray<TBase>` for simple scenarios
- **TypeCollectionBase&lt;TBase, TGeneric&gt;**: Dual generic returns `IReadOnlyCollection<TGeneric>` from FrozenDictionary.Values
- **All() Method**: Direct access to FrozenDictionary.Values for optimal enumeration performance

### Compilation Performance (ULTRA-OPTIMIZED)
- **TypeOption-First Discovery**: O(types_with_typeoption) vs O(collections × assemblies × all_types) - revolutionary performance improvement
- **Single-Pass Assembly Scanning**: One scan to find all [TypeOption] attributes, then O(1) lookup by base type
- **Attribute-Based Collection Discovery**: O(k) vs O(n×m) inheritance scanning for collection classes
- **Incremental Generation**: Only regenerates when source changes
- **Efficient Symbol Resolution**: Caches type symbols and avoids repeated inheritance checks

## Validation and Diagnostics

### Generated Analyzers
The source generator package includes analyzers that warn about:

#### TC001: Missing TypeOption Attribute
```csharp
// Warning: Type inherits from collection base but missing [TypeOption]
public class SqlDataContainer : DataContainerType // Missing [TypeOption]
{
}
```

#### TC002: Generic Type Mismatch
```csharp
// Error: TGeneric doesn't match defaultReturnType
[TypeCollection(typeof(DataContainerType), typeof(IDataContainer), typeof(DataContainerTypes))]
public partial class DataContainerTypes : TypeCollectionBase<DataContainerType, IWrongInterface> // Mismatch
{
}
```

#### TC003: Base Type Mismatch
```csharp
// Error: TBase doesn't match baseType
[TypeCollection(typeof(DataContainerType), typeof(IDataContainer), typeof(DataContainerTypes))]
public partial class DataContainerTypes : TypeCollectionBase<WrongBaseType, IDataContainer> // Mismatch
{
}
```

## Configuration Options

### TypeCollectionAttribute Constructor
```csharp
// Required parameters only - no additional configuration options
[TypeCollection(typeof(BaseType), typeof(ReturnType), typeof(CollectionType))]
public partial class MyTypes : TypeCollectionBase<BaseType, ReturnType>
{
}
```

### TypeLookupAttribute for Dynamic Lookup Methods
```csharp
public abstract class MyBaseType : TypeOptionBase
{
    [TypeLookup] // Generates Id(int id) method
    public abstract int Id { get; }

    [TypeLookup] // Generates Name(string name) method
    public abstract string Name { get; }

    [TypeLookup] // Generates Category(string category) method
    public abstract string Category { get; }
}
```

This generates clean lookup methods using FrozenDictionary alternate key lookup:
```csharp
// Primary key lookup (uses dictionary directly)
public static MyBaseType Id(int id) =>
    _all.TryGetValue(id, out var result) ? result : _empty;

// Alternate key lookups (uses GetAlternateLookup for O(1) performance)
public static MyBaseType Name(string name)
{
    var alternateLookup = _all.GetAlternateLookup<string>();
    return alternateLookup.TryGetValue(name, out var result) ? result : _empty;
}

public static MyBaseType Category(string category)
{
    var alternateLookup = _all.GetAlternateLookup<string>();
    return alternateLookup.TryGetValue(category, out var result) ? result : _empty;
}
```

### Build Configuration
```xml
<!-- Disable source generator in specific configurations -->
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DisableSourceGenerators>true</DisableSourceGenerators>
</PropertyGroup>
```

## Troubleshooting

### Common Issues

1. **Types Not Discovered**
   - Ensure types have `[TypeOption]` attribute
   - Check that types inherit from the correct base type
   - Verify base type name matches `[TypeCollection]` parameter

2. **Compilation Errors**
   - Check generic type constraints match attribute parameters
   - Ensure all referenced types are available at compile time
   - Verify attribute parameters use `typeof()` correctly

3. **Performance Issues**
   - Use dual generic collections for interface return types
   - Prefer `FrozenSet<T>` over `ReadOnlyCollection<T>` when possible
   - Consider caching frequently accessed properties

### Debug Output
In DEBUG builds, the generator produces debug files:
- `TypeCollectionGenerator.Debug.g.cs`: Shows discovery results
- `TypeCollectionGenerator.Init.g.cs`: Confirms generator loading

## Integration with MSBuild

The source generator integrates seamlessly with MSBuild:
- Runs during compilation
- Respects incremental builds
- Works with IDEs and command line
- Supports design-time builds for IntelliSense

### Package References
For runtime libraries:
```xml
<PackageReference Include="FractalDataWorks.Collections.SourceGenerators" Version="*" />
```

For analyzer-only (abstractions):
```xml
<PackageReference Include="FractalDataWorks.Collections.SourceGenerators" Version="*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

## Advanced Scenarios

### Custom Empty Implementations
```csharp
// The generator creates intelligent empty implementations
// based on the base type's constructor requirements
```

### Cross-Assembly Discovery
```csharp
// Generator discovers types across all referenced assemblies
// enabling modular type option definitions
```

### Factory Method Generation
```csharp
// All public constructors automatically get factory methods
// with proper parameter handling and default values
```

## Version Compatibility

- **Target Framework**: .NET Standard 2.0+
- **C# Language Version**: 9.0+ (for records and init properties)
- **Roslyn Version**: 4.0+ (for incremental generators)