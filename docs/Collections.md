# FractalDataWorks.Collections Documentation

## Overview

The FractalDataWorks.Collections library provides the foundation for creating type-safe, source-generated collections of type options. It enables the implementation of Enhanced Enum-like patterns where collections of types are automatically discovered and generated at compile time, providing intellisense support and type safety without runtime reflection.

## Core Components

### TypeCollectionBase<TBase>

The primary base class for type collections:

```csharp
public abstract class TypeCollectionBase<TBase> where TBase : class
{
    // TypeCollectionGenerator will generate all functionality in the partial class
}
```

**Generated Members (by Source Generator):**
- `All`: Returns all type options in the collection
- `GetById(int id)`: Lookup by ID
- `GetByName(string name)`: Lookup by name
- `GetByCategory(string category)`: Get all items in a category
- `Count`: Total number of items
- `Categories`: Distinct categories
- Custom lookup methods based on `[TypeLookup]` attributes

### TypeCollectionBase<TBase, TGeneric>

Variant with different return type:

```csharp
public abstract class TypeCollectionBase<TBase, TGeneric>
    where TBase : class, TGeneric
    where TGeneric : class
{
    // TypeCollectionGenerator will generate all functionality in the partial class
}
```

**Use Case:** When you want collection items to be concrete types but return a common interface.

Example:
```csharp
// Collection of SqlConnectionType, RedisConnectionType, etc.
// But all methods return IConnectionType
public partial class ConnectionTypes : TypeCollectionBase<ConnectionTypeBase, IConnectionType>
```

### TypeOptionBase<T>

Base class for type option implementations:

```csharp
public abstract class TypeOptionBase<T> : ITypeOption<T> where T : ITypeOption<T>
{
    [TypeLookup("GetById")]
    public int Id { get; }

    [TypeLookup("GetByName")]
    public string Name { get; }

    [TypeLookup("GetByCategory")]
    public string Category { get; }

    protected TypeOptionBase(int id, string name, string? category = null)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = string.IsNullOrEmpty(category) ? "NotCategorized" : category;
    }
}
```

**Key Features:**
- Enforces Id, Name, and Category properties
- Constructor-based initialization for immutability
- Automatic "NotCategorized" default
- TypeLookup attributes for source generator

### ITypeOption

Core interface for type options:

```csharp
public interface ITypeOption
{
    int Id { get; }
    string Name { get; }
    string Category { get; }
}

public interface ITypeOption<T> : ITypeOption where T : ITypeOption<T>
{
    // Self-referencing generic for type safety
}
```

## Attributes

### TypeCollectionAttribute

Marks a class for collection generation:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TypeCollectionAttribute : Attribute
{
    public string BaseTypeName { get; }
    public string CollectionName { get; }
    public bool GenerateExtensions { get; }

    public TypeCollectionAttribute(string baseTypeName, string collectionName, bool generateExtensions = true);
}
```

**Usage:**
```csharp
[TypeCollection("DatabaseType", "DatabaseTypes")]
public static partial class DatabaseTypes : TypeCollectionBase<DatabaseType>
{
    // Source generator fills in the implementation
}
```

### GlobalTypeCollectionAttribute

For cross-assembly type discovery:

```csharp
[AttributeUsage(AttributeTargets.Assembly)]
public class GlobalTypeCollectionAttribute : Attribute
{
    public string CollectionName { get; }
    public string TargetNamespace { get; }

    public GlobalTypeCollectionAttribute(string collectionName, string targetNamespace);
}
```

**Usage:**
```csharp
[assembly: GlobalTypeCollection("ConnectionTypes", "FractalDataWorks.Connections")]
```

### TypeLookupAttribute

Generates custom lookup methods:

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TypeLookupAttribute : Attribute
{
    public string MethodName { get; }

    public TypeLookupAttribute(string methodName);
}
```

**Usage:**
```csharp
public abstract class ServerType : TypeOptionBase<ServerType>
{
    [TypeLookup("GetByPort")]
    public int Port { get; }

    [TypeLookup("GetByProtocol")]
    public string Protocol { get; }
}
```

### TypeOptionAttribute

Marks types for inclusion in collections:

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class TypeOptionAttribute : Attribute
{
    public string CollectionName { get; }

    public TypeOptionAttribute(string collectionName);
}
```

**Usage:**
```csharp
[TypeOption("DatabaseTypes")]
public class PostgreSqlType : DatabaseType
{
    // Implementation
}
```

## Source Generation Process

### Discovery Mechanisms

The source generator discovers types through:

1. **Inheritance**: All classes inheriting from the specified base type
2. **Attributes**: Classes marked with `[TypeOption]`
3. **Assembly Scanning**: Cross-assembly discovery via `[GlobalTypeCollection]`
4. **Nested Classes**: Inner type definitions within collection classes

### Generated Code Example

For a collection definition:

```csharp
[TypeCollection("ConnectionType", "ConnectionTypes")]
public static partial class ConnectionTypes : TypeCollectionBase<ConnectionType>
{
}
```

The generator produces:

```csharp
public static partial class ConnectionTypes
{
    private static readonly Dictionary<int, ConnectionType> _byId;
    private static readonly Dictionary<string, ConnectionType> _byName;
    private static readonly IReadOnlyList<ConnectionType> _all;

    static ConnectionTypes()
    {
        var items = new[]
        {
            SqlServerType.Instance,
            PostgreSqlType.Instance,
            MongoDbType.Instance
        };

        _all = items;
        _byId = items.ToDictionary(x => x.Id);
        _byName = items.ToDictionary(x => x.Name);
    }

    public static IReadOnlyList<ConnectionType> All => _all;
    public static int Count => _all.Count;

    public static ConnectionType GetById(int id)
        => _byId.TryGetValue(id, out var result) ? result : ConnectionType.Empty;

    public static ConnectionType GetByName(string name)
        => _byName.TryGetValue(name, out var result) ? result : ConnectionType.Empty;

    public static IEnumerable<ConnectionType> GetByCategory(string category)
        => _all.Where(x => x.Category == category);
}
```

## Advanced Features

### Custom Lookup Methods

Properties marked with `[TypeLookup]` generate specialized lookups:

```csharp
public class ServiceType : TypeOptionBase<ServiceType>
{
    [TypeLookup("GetByEndpoint")]
    public string Endpoint { get; }

    [TypeLookup("GetByPriority")]
    public int Priority { get; }
}

// Generated methods:
public static ServiceType GetByEndpoint(string endpoint);
public static IEnumerable<ServiceType> GetByPriority(int priority);
```

### Empty/Unknown Pattern

Collections can define an "Empty" or "Unknown" value:

```csharp
public class StatusType : TypeOptionBase<StatusType>
{
    public static StatusType Unknown { get; } = new UnknownStatus();

    private class UnknownStatus : StatusType
    {
        public UnknownStatus() : base(0, "Unknown", "System") { }
    }
}
```

### Hierarchical Categories

Support for nested categorization:

```csharp
public class OperationType : TypeOptionBase<OperationType>
{
    public string MainCategory { get; }
    public string SubCategory { get; }

    protected OperationType(int id, string name, string mainCategory, string subCategory)
        : base(id, name, $"{mainCategory}/{subCategory}")
    {
        MainCategory = mainCategory;
        SubCategory = subCategory;
    }
}
```

## Collection Patterns

### Singleton Pattern

Most type options use singleton instances:

```csharp
public sealed class TcpProtocol : ProtocolType
{
    public static TcpProtocol Instance { get; } = new();

    private TcpProtocol() : base(1, "TCP", "Network") { }
}
```

### Factory Pattern

Collections can act as factories:

```csharp
public static partial class ServiceTypes
{
    public static IService CreateService(string typeName, IConfiguration config)
    {
        var serviceType = GetByName(typeName);
        return serviceType.CreateInstance(config);
    }
}
```

### Registry Pattern

Collections serve as type registries:

```csharp
public static partial class HandlerTypes
{
    public static void RegisterAll(IServiceCollection services)
    {
        foreach (var handlerType in All)
        {
            handlerType.Register(services);
        }
    }
}
```

## Performance Characteristics

- **Compile-Time Generation**: Zero runtime reflection
- **Dictionary Lookups**: O(1) for ID and name lookups
- **Memory Efficient**: Single static initialization
- **Thread-Safe**: Immutable after static construction
- **IntelliSense Support**: Full IDE support for all members

## Integration with Source Generators

### Collections.SourceGenerators Project

The companion source generator project provides:

1. **TypeCollectionGenerator**: Main generator for collections
2. **Cross-Assembly Discovery**: Scans referenced assemblies
3. **Incremental Generation**: Only regenerates on changes
4. **Diagnostic Reporting**: Compiler warnings for issues

### Build Integration

```xml
<ItemGroup>
  <ProjectReference Include="FractalDataWorks.Collections.SourceGenerators"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Best Practices

1. **Use Singletons**: Type options should be singleton instances
2. **Immutable Properties**: All properties should be read-only
3. **Unique IDs**: Ensure IDs are unique within a collection
4. **Meaningful Names**: Use descriptive names for lookup
5. **Category Organization**: Use categories for logical grouping
6. **Avoid Complex Logic**: Keep type options simple
7. **Document Values**: Add XML comments to type definitions

## Common Use Cases

### Configuration Types

```csharp
public abstract class ConfigSection : TypeOptionBase<ConfigSection>
{
    public abstract string SectionPath { get; }
    public abstract Type ConfigurationType { get; }
}
```

### Command Types

```csharp
public abstract class CommandType : TypeOptionBase<CommandType>
{
    public abstract Type CommandClass { get; }
    public abstract Type HandlerClass { get; }
}
```

### Status/State Types

```csharp
public abstract class ProcessState : TypeOptionBase<ProcessState>
{
    public abstract bool IsTerminal { get; }
    public abstract bool CanTransitionTo(ProcessState other);
}
```

## Migration from Enums

Traditional Enum:
```csharp
public enum ConnectionType
{
    SqlServer = 1,
    PostgreSql = 2,
    MongoDB = 3
}
```

Type Collection Pattern:
```csharp
public abstract class ConnectionType : TypeOptionBase<ConnectionType>
{
    public abstract Type FactoryType { get; }
    public abstract string ConnectionStringFormat { get; }
}

public sealed class SqlServerType : ConnectionType
{
    public static SqlServerType Instance { get; } = new();
    public override Type FactoryType => typeof(SqlConnectionFactory);
}
```

Benefits:
- Rich object model with methods and properties
- Type safety with generic constraints
- Extensibility across assemblies
- IntelliSense support
- No boxing/unboxing