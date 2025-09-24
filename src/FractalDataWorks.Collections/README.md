# FractalDataWorks.Collections

High-performance, source-generated type collections for the FractalDataWorks ecosystem. Provides compile-time type safety and O(1) lookup performance for type option collections.

## Overview

The Collections system provides three main components:
1. **Base Classes**: Optional convenience classes with built-in collections
2. **Attributes**: Type-safe attributes for defining collections and options
3. **Source Generator**: Generates high-performance collection implementations

## Installation

```bash
dotnet add package FractalDataWorks.Collections
```

For source generation support, also add:
```bash
dotnet add package FractalDataWorks.Collections.SourceGenerators
```

## Core Concepts

### Type Collections
Collections that discover and provide access to all types inheriting from a base type.

### Type Options
Individual types that inherit from a base type and are discovered by collections.

## Collection Base Classes

### Single Generic Collection
Use when base type and return type are the same. Provides ReadOnlyCollection for performance:

```csharp
public abstract class TypeCollectionBase<TBase> where TBase : class
{
    // Provides ReadOnlyCollection<TBase> for All() method
}
```

### Dual Generic Collection
Use when you want collections to return interface types. Uses FrozenSet primary storage with FrozenDictionary lookup caches for O(1) performance:

```csharp
public abstract class TypeCollectionBase<TBase, TGeneric>
    where TBase : class, TGeneric
    where TGeneric : class
{
    // Generator creates:
    // - FrozenSet<TGeneric> _all (primary storage)
    // - FrozenDictionary<int, TGeneric> _byId (O(1) ID lookups)
    // - FrozenDictionary<string, TGeneric> _byName (O(1) name lookups)
}
```

**Example**: Collection works with `SqlServerDataContainer` (TBase) but returns `IDataContainer` (TGeneric).

## Attributes

### TypeCollectionAttribute
Marks a partial class for collection generation:

```csharp
[TypeCollection(typeof(BaseType), typeof(ReturnType), typeof(CollectionClass))]
public partial class MyTypes : TypeCollectionBase<BaseType, ReturnType>
{
}
```

**Parameters:**
- `baseType`: Type to discover inheritors from (e.g., `typeof(DataContainerType)`)
- `defaultReturnType`: Return type for generated methods (e.g., `typeof(IDataContainer)`)
- `collectionType`: The collection class being generated (e.g., `typeof(DataContainerTypes)`)

### TypeOptionAttribute
Marks individual types for discovery:

```csharp
[TypeOption("Display Name")]
public class SqlServerDataContainer : DataContainerType
{
}
```

**Parameter:**
- `name`: Optional display name (defaults to class name if omitted)

## Usage Examples

### Example 1: Data Container Collection (FractalDataWorks.DataContainers)

```csharp
// Interface
public interface IDataContainer
{
    string Format { get; }
    Task<string> ReadAsync();
}

// Base type
public abstract class DataContainerType : IDataContainer
{
    public abstract string Format { get; }
    public abstract Task<string> ReadAsync();
}

// Collection definition (returns interface for polymorphism)
[TypeCollection(typeof(DataContainerType), typeof(IDataContainer), typeof(DataContainerTypes))]
public partial class DataContainerTypes : TypeCollectionBase<DataContainerType, IDataContainer>
{
}

// Type options
[TypeOption("CSV")]
public class CsvDataContainer : DataContainerType
{
    public override string Format => "text/csv";
    public override Task<string> ReadAsync() => /* CSV reading logic */;
}

[TypeOption("JSON")]
public class JsonDataContainer : DataContainerType
{
    public override string Format => "application/json";
    public override Task<string> ReadAsync() => /* JSON reading logic */;
}

[TypeOption("Parquet")]
public class ParquetDataContainer : DataContainerType
{
    public override string Format => "application/parquet";
    public override Task<string> ReadAsync() => /* Parquet reading logic */;
}

// Generated API - returns IDataContainer for polymorphism
IDataContainer csv = DataContainerTypes.Csv;
IDataContainer json = DataContainerTypes.Json;
FrozenSet<IDataContainer> all = DataContainerTypes.All(); // High-performance collection
IDataContainer byName = DataContainerTypes.GetByName("CSV");
```

### Example 2: Simple Collection (Same Base/Return Type)

```csharp
// Base type
public abstract class SortDirectionBase
{
    public abstract string Direction { get; }
}

// Collection definition
[TypeCollection(typeof(SortDirectionBase), typeof(SortDirectionBase), typeof(SortDirections))]
public partial class SortDirections : TypeCollectionBase<SortDirectionBase>
{
}

// Type options
[TypeOption("Ascending")]
public class AscendingSortDirection : SortDirectionBase
{
    public override string Direction => "ASC";
}

[TypeOption("Descending")]
public class DescendingSortDirection : SortDirectionBase
{
    public override string Direction => "DESC";
}

// Generated API
var ascending = SortDirections.Ascending;
ReadOnlyCollection<SortDirectionBase> all = SortDirections.All(); // ReadOnlyCollection for single generic
var byName = SortDirections.GetByName("Ascending");
```

### Example 3: No Base Class (Full Control)

```csharp
// Collection without inheritance - generator creates all methods
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods
{
    // Generator creates all methods and properties
}
```

## Generated API

All collections generate the following API:

### Static Properties
```csharp
public static TReturn TypeName { get; } // For each discovered type
```

### Collection Methods
```csharp
// Single generic returns ReadOnlyCollection<TBase>
public static ReadOnlyCollection<TBase> All()

// Dual generic returns FrozenSet<TGeneric>
public static FrozenSet<TGeneric> All()

public static TReturn Empty()
public static TReturn GetById(int id)
public static TReturn GetByName(string name)
```

### Factory Methods
```csharp
public static TReturn CreateTypeName(params...) // For each constructor overload
```

### Performance Features
- **O(1) Lookups**: Uses FrozenDictionary for ID/name lookups
- **Optimized Collections**: ReadOnlyCollection for single generic, FrozenSet for dual generic
- **Singleton Instances**: Reuses instances for better memory efficiency
- **Compile-time Safety**: All types validated at compile time

## Validation Rules

The system enforces these rules:

1. **TypeOption Required**: Types inheriting from collection base types must have `[TypeOption]` attribute
2. **Generic Consistency**: `TGeneric` in base class must match `defaultReturnType` in attribute
3. **Base Type Consistency**: `TBase` in base class must match `baseType` in attribute
4. **Concrete Types Only**: Only concrete (non-abstract) types are discovered
5. **Public Constructors**: Only public constructors generate factory methods

## Error Codes

- **TC001**: Type inherits from collection base but missing `[TypeOption]` attribute
- **TC002**: TGeneric in base class doesn't match defaultReturnType in attribute
- **TC003**: TBase in base class doesn't match baseType in attribute

## Best Practices

1. **Use Interface Return Types**: Prefer `TypeCollectionBase<TBase, TInterface>` for polymorphism and FrozenSet performance
2. **Descriptive Names**: Use meaningful names in `[TypeOption]` attributes
3. **Factory Patterns**: Leverage generated factory methods for complex construction
4. **Performance**: Collections are optimized for read-heavy scenarios
5. **Thread Safety**: All generated collections are thread-safe for reads

## Usage by Other Packages

This package provides the foundation for:
- **FractalDataWorks.DataContainers**: Uses Collections for data container type discovery
- **FractalDataWorks.DataSets**: Uses Collections for query operator collections
- **FractalDataWorks.Web.Http**: Uses Collections for security method and endpoint type collections

## Packaging for Abstractions Projects

When using in `.Abstractions` projects, add the source generator as an analyzer:

```xml
<PackageReference Include="FractalDataWorks.Collections.SourceGenerators" Version="*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

This ensures the generator runs during compilation but isn't included as a runtime dependency.