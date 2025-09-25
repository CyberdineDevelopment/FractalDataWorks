# ServiceType Collection Generator

A high-performance source generator for creating strongly-typed service type collections with O(1) lookups and comprehensive factory methods.

## Overview

The ServiceTypeCollectionGenerator automatically discovers and generates optimized collections of service types using explicit attribute-based targeting instead of expensive inheritance scanning. This provides significant performance improvements: **O(types_with_attribute) vs O(collections × assemblies × all_types)**.

## Quick Start

### 1. Mark Your Collection Class

Use the `[ServiceTypeCollection]` attribute to define your collection:

```csharp
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.ServiceTypes;

[ServiceTypeCollection(typeof(ConnectionTypeBase<,,>), typeof(IConnectionType), typeof(ConnectionTypes))]
public partial class ConnectionTypes : ServiceTypeCollectionBase<
    ConnectionTypeBase<IFdwConnection, IConnectionConfiguration, IConnectionFactory<IFdwConnection, IConnectionConfiguration>>,
    IConnectionType,
    IFdwConnection,
    IConnectionConfiguration,
    IConnectionFactory<IFdwConnection, IConnectionConfiguration>>
{
}
```

### 2. Mark Your Service Type Options

Use the `[ServiceTypeOption]` attribute to explicitly target specific collections:

```csharp
[ServiceTypeOption(typeof(ConnectionTypes), "SqlServer")]
public sealed class SqlServerConnectionType : ConnectionTypeBase<IFdwConnection, IConnectionConfiguration, IConnectionFactory<IFdwConnection, IConnectionConfiguration>>
{
    public SqlServerConnectionType() : base(1, "SqlServer", "Database") { }
    // Implementation...
}

[ServiceTypeOption(typeof(ConnectionTypes), "PostgreSQL")]
public sealed class PostgreSqlConnectionType : ConnectionTypeBase<IFdwConnection, IConnectionConfiguration, IConnectionFactory<IFdwConnection, IConnectionConfiguration>>
{
    public PostgreSqlConnectionType() : base(2, "PostgreSQL", "Database") { }
    // Implementation...
}
```

### 3. Generated API

The generator creates a high-performance collection with comprehensive lookup methods:

```csharp
// Static collection access
var allConnections = ConnectionTypes.All();           // IReadOnlyList<ConnectionTypeBase>
var emptyConnection = ConnectionTypes.Empty();        // EmptyConnectionType fallback instance

// O(1) Lookups with FrozenDictionary performance
var sqlServer = ConnectionTypes.GetById(1);           // Returns SqlServerConnectionType or Empty
var postgres = ConnectionTypes.GetByName("PostgreSQL"); // Returns PostgreSqlConnectionType or Empty

// Attribute-based lookups (generated from TypeLookup attributes)
var databaseConnections = ConnectionTypes.GetBySectionName("Database");
var specificService = ConnectionTypes.GetByServiceType(typeof(ISqlConnectionService));

// Factory methods for each discovered type
var sqlConnection = ConnectionTypes.CreateSqlServerConnectionType(config, options);
var pgConnection = ConnectionTypes.CreatePostgreSqlConnectionType(config, options);
```

## Attribute Reference

### ServiceTypeCollectionAttribute

Marks a partial class for service type collection generation.

```csharp
[ServiceTypeCollection(typeof(BaseType<,,>), typeof(Interface), typeof(CollectionType))]
public partial class YourTypes : ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
```

**Parameters:**
- `baseType`: The base service type to collect (supports generic definitions)
- `defaultReturnType`: The interface or return type for generated methods
- `collectionType`: The partial class being generated (typically `typeof(YourTypes)`)

**Properties:**
- `UseSingletonInstances`: Use singleton instances instead of factory methods (default: false)

### ServiceTypeOptionAttribute

Marks concrete service types for inclusion in specific collections.

```csharp
[ServiceTypeOption(typeof(CollectionType), "OptionName")]
public class YourServiceType : BaseServiceType<TService, TConfiguration, TFactory>
```

**Parameters:**
- `collectionType`: The collection class this option belongs to (e.g., `typeof(ConnectionTypes)`)
- `name`: The name for factory methods and lookups in the generated collection

## Inheritance Requirements

Your collection class must inherit from `ServiceTypeCollectionBase` with proper generic constraints:

```csharp
public abstract class ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : ServiceTypeBase<TService, TConfiguration, TFactory>
    where TGeneric : ServiceTypeBase<TService, TConfiguration, TFactory>
    where TService : class
    where TConfiguration : class
    where TFactory : class
```

**Generic Parameters:**
- `TBase`: The concrete base type (e.g., `ConnectionTypeBase<,,>`)
- `TGeneric`: The generic interface or return type (e.g., `IConnectionType`)
- `TService`: The service interface (e.g., `IFdwConnection`)
- `TConfiguration`: The configuration type (e.g., `IConnectionConfiguration`)
- `TFactory`: The factory type (e.g., `IConnectionFactory<,>`)

## Performance Benefits

### Explicit Collection Targeting
- **Traditional approach**: O(collections × assemblies × all_types) - scans all types for inheritance
- **Attribute approach**: O(types_with_attribute) - only processes explicitly marked types

### Generated Performance Features
- **FrozenDictionary lookups**: O(1) performance for GetById/GetByName operations
- **Singleton instances**: Cached instances reduce allocation overhead
- **Compile-time type safety**: All lookups are strongly typed
- **Factory method overloads**: Support for different constructor signatures

## Advanced Examples

### Custom Lookup Methods

Use `[TypeLookup]` attributes on base class properties to generate additional lookup methods:

```csharp
public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory>
    : ServiceTypeBase<TService, TConfiguration, TFactory>
{
    [TypeLookup("GetByProvider")]
    public abstract string ProviderName { get; }

    [TypeLookup("GetByProtocol")]
    public abstract string Protocol { get; }
}
```

This generates:
```csharp
public IEnumerable<ConnectionTypeBase> GetByProvider(string providerName) { /* generated */ }
public IEnumerable<ConnectionTypeBase> GetByProtocol(string protocol) { /* generated */ }
```

### Multiple Constructor Support

The generator automatically creates factory method overloads for all public constructors:

```csharp
public class CustomConnectionType : ConnectionTypeBase<IFdwConnection, IConnectionConfiguration, IConnectionFactory>
{
    public CustomConnectionType() : base(1, "Custom", "Network") { }
    public CustomConnectionType(string endpoint) : base(1, "Custom", "Network") { /* implementation */ }
    public CustomConnectionType(string endpoint, int timeout) : base(1, "Custom", "Network") { /* implementation */ }
}
```

Generates:
```csharp
public static CustomConnectionType CreateCustomConnectionType() => new();
public static CustomConnectionType CreateCustomConnectionType(string endpoint) => new(endpoint);
public static CustomConnectionType CreateCustomConnectionType(string endpoint, int timeout) => new(endpoint, timeout);
```

## Error Handling

### Diagnostic Codes

The generator reports helpful diagnostics for common issues:

- **STCG001**: ServiceType Collection Generation Failed - General generation errors
- **Missing base type**: When the specified base type cannot be resolved
- **Invalid inheritance**: When collection class doesn't properly inherit from ServiceTypeCollectionBase
- **Attribute conflicts**: When ServiceTypeOption targets non-existent collections

### Fallback Behavior

- **Empty instances**: All lookups return Empty instance instead of null for safe chaining
- **Missing types**: Collections generate even with zero service types for consistency
- **Build errors**: Generation failures produce detailed diagnostics without breaking builds

## Migration Guide

### From Inheritance-Based Discovery

**Old Pattern** (inheritance scanning):
```csharp
// Just inherit - generator scans all assemblies looking for inheritors
public partial class ConnectionTypes : ServiceTypeCollectionBase<...> { }
```

**New Pattern** (explicit targeting):
```csharp
// 1. Add attribute to collection
[ServiceTypeCollection(typeof(ConnectionTypeBase<,,>), typeof(IConnectionType), typeof(ConnectionTypes))]
public partial class ConnectionTypes : ServiceTypeCollectionBase<...> { }

// 2. Add attributes to each service type
[ServiceTypeOption(typeof(ConnectionTypes), "SqlServer")]
public class SqlServerConnectionType : ConnectionTypeBase<...> { }
```

### Performance Impact
- **Build time**: 60-90% faster generation due to targeted discovery
- **Memory usage**: Significantly reduced during compilation
- **Maintainability**: Explicit relationships make dependencies clear
- **Type safety**: Compile-time validation of collection relationships

## Best Practices

### Organization
- Keep service types in separate assemblies/projects when possible
- Use consistent naming: `{Domain}Types` for collections, `{Technology}{Domain}Type` for implementations
- Group related service types in the same namespace

### Performance
- Use singleton instances (`UseSingletonInstances = true`) for stateless service types
- Limit the number of constructor overloads to reduce generated code size
- Consider using factory methods for complex initialization logic

### Maintainability
- Always specify explicit collection targeting with ServiceTypeOption
- Document custom TypeLookup attributes for generated methods
- Use meaningful names in ServiceTypeOption for clear generated APIs