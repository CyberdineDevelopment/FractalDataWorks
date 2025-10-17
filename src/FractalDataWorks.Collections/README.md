# FractalDataWorks.Collections

High-performance, source-generated type collections for cross-project extensible type discovery in the FractalDataWorks ecosystem. Provides compile-time type safety and O(1) lookup performance for type collections where downstream developers can add their own options.

## Overview

The Collections system provides cross-project type discovery for extensible type systems where:
- **Collections** are placed in abstractions projects for discoverability
- **Base Types** are in concrete projects
- **Type Options** can be added by downstream developers in concrete or implementation projects

## Architecture

### TypeCollection Pattern
```
FractalDataWorks.Web.Http.Abstractions/
├── Security/
│   ├── SecurityMethods.cs              <- Collection (abstractions for discoverability)
│   └── ISecurityMethod.cs              <- Interface
FractalDataWorks.Web.Http/
├── Security/
│   └── SecurityMethodBase.cs           <- Base Type (concrete project)
Any.Implementation.Project/
├── CustomSecurityMethod.cs             <- Options (can be added anywhere)
```

## Core Components

1. **Collection Classes**: Placed in abstractions projects for cross-project discoverability
2. **Base Types**: Abstract classes in concrete projects that options inherit from
3. **Type Options**: Concrete implementations that can be added by any downstream project
4. **Source Generator**: Generates high-performance collection implementations with O(1) lookups

## Collection Base Classes

### Single Generic Collection
Use when base type and return type are the same:

```csharp
public abstract class TypeCollectionBase<TBase> where TBase : class
{
    // Generator populates with ReadOnlyCollection<TBase> for All() method
    // Optimized for single type scenarios
}
```

### Dual Generic Collection
Use when you want collections to return interface types for polymorphism:

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

**Use Case**: Collection works with `CsvDataContainer` (TBase) but returns `IDataContainer` (TGeneric) for polymorphism.

## Attributes

### TypeCollectionAttribute
Marks a partial class in an abstractions project for collection generation:

```csharp
// In Abstractions Project - for discoverability
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod>
{
    // Empty - generator populates all functionality
}
```

**Parameters:**
- `baseType`: Type to discover inheritors from (in concrete project)
- `defaultReturnType`: Return type for generated methods (interface type)
- `collectionType`: The collection class being generated (this class)

### TypeOptionAttribute
Marks individual types for explicit collection targeting:

```csharp
// In Any Project - downstream extensibility
[TypeOption(typeof(SecurityMethods), "CustomAuth")]
public class CustomAuthMethod : SecurityMethodBase
{
    // Implementation
}
```

**Parameters:**
- `collectionType`: The collection type this option belongs to
- `name`: Display name for the method/property in the generated collection

**Performance Benefits:**
- O(types_with_attribute) vs O(collections × assemblies × all_types) discovery
- Eliminates expensive inheritance scanning across all assemblies
- Direct collection type targeting for faster source generation

## Complete Example: Security Methods

### 1. Abstractions Project (FractalDataWorks.Web.Http.Abstractions)

```csharp
// Security/ISecurityMethod.cs - Interface
namespace FractalDataWorks.Web.Http.Abstractions.Security;

public interface ISecurityMethod
{
    int Id { get; }
    string Name { get; }
    bool RequiresAuthentication { get; }
}

// Security/SecurityMethods.cs - Collection (for discoverability)
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod>
{
    // Empty - generator populates all functionality
}
```

### 2. Concrete Project (FractalDataWorks.Web.Http)

```csharp
// Security/SecurityMethodBase.cs - Base Type
using FractalDataWorks.Web.Http.Abstractions.Security;

namespace FractalDataWorks.Web.Http.Security;

public abstract class SecurityMethodBase : ISecurityMethod
{
    protected SecurityMethodBase(int id, string name, bool requiresAuthentication)
    {
        Id = id;
        Name = name;
        RequiresAuthentication = requiresAuthentication;
    }

    public int Id { get; }
    public string Name { get; }
    public bool RequiresAuthentication { get; }
}
```

### 3. Implementation Projects (Any Project)

```csharp
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Web.Http.Security;
using FractalDataWorks.Web.Http.Abstractions.Security;

namespace FractalDataWorks.Web.Http.Security.Methods;

// Built-in options in framework
[TypeOption(typeof(SecurityMethods), "None")]
public sealed class NoneSecurityMethod : SecurityMethodBase
{
    public NoneSecurityMethod() : base(1, "None", false) { }
}

[TypeOption(typeof(SecurityMethods), "JWT")]
public sealed class JwtSecurityMethod : SecurityMethodBase
{
    public JwtSecurityMethod() : base(2, "JWT", true) { }
}

// Custom option in downstream project
[TypeOption(typeof(SecurityMethods), "CustomAuth")]
public sealed class CustomAuthMethod : SecurityMethodBase
{
    public CustomAuthMethod() : base(100, "CustomAuth", true) { }
}
```

### 4. Usage

```csharp
using System.Collections.Frozen;
using FractalDataWorks.Web.Http.Abstractions.Security;

// Generated API - all options discovered automatically
ISecurityMethod none = SecurityMethods.None;              // Built-in
ISecurityMethod jwt = SecurityMethods.Jwt;                // Built-in
ISecurityMethod custom = SecurityMethods.CustomAuth;      // Downstream

// High-performance collections
FrozenSet<ISecurityMethod> all = SecurityMethods.All();   // All discovered options
ISecurityMethod byName = SecurityMethods.GetByName("JWT");
ISecurityMethod byId = SecurityMethods.GetById(2);

// Static properties for each discovered type
var hasCustom = SecurityMethods.CustomAuth != null;       // True if downstream added it
```

## Generated API

All collections automatically generate:

### Static Properties
```csharp
public static ISecurityMethod None { get; }           // For each discovered type
public static ISecurityMethod Jwt { get; }            // CamelCase property name
public static ISecurityMethod CustomAuth { get; }     // Even downstream types
```

### Collection Methods
```csharp
public static FrozenSet<ISecurityMethod> All();                    // All discovered options
public static ISecurityMethod Empty();                             // Default/fallback instance
public static ISecurityMethod GetById(int id);                     // O(1) lookup
public static ISecurityMethod GetByName(string name);              // O(1) lookup
```

### Factory Methods
```csharp
public static ISecurityMethod CreateNone();                        // For each constructor overload
public static ISecurityMethod CreateJwt();
public static ISecurityMethod CreateCustomAuth();
```

## Project Structure and References

### Abstractions Project
```xml
<PackageReference Include="FractalDataWorks.Collections" />
<PackageReference Include="FractalDataWorks.Collections.SourceGenerators">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### Concrete Project
```xml
<ProjectReference Include="..\Project.Abstractions\Project.Abstractions.csproj" />
<PackageReference Include="FractalDataWorks.Collections" />
```

### Implementation Projects
```xml
<ProjectReference Include="..\Project.Abstractions\Project.Abstractions.csproj" />
<!-- Collections reference comes transitively through Abstractions -->
```

## Performance Features

- **O(1) Lookups**: Uses FrozenDictionary for ID/name lookups
- **Optimized Collections**: ReadOnlyCollection for single generic, FrozenSet for dual generic
- **Singleton Instances**: Reuses instances for better memory efficiency
- **Compile-time Discovery**: All types validated and discovered at compile time
- **Thread Safety**: All generated collections are thread-safe for reads
- **Cross-Assembly Discovery**: Finds options across all referenced assemblies

## Validation Rules

1. **TypeOption Required**: Types inheriting from base types must have `[TypeOption(typeof(CollectionType), "Name")]`
2. **Explicit Collection Targeting**: TypeOption must specify the exact collection type
3. **Generic Consistency**: `TGeneric` in base class must match `defaultReturnType` in attribute
4. **Base Type Consistency**: `TBase` in base class must match `baseType` in attribute
5. **Concrete Types Only**: Only concrete (non-abstract) types are discovered
6. **Unique Names**: Each option must have a unique name within its collection

## Error Codes

- **TC001**: Type inherits from collection base but missing `[TypeOption]` attribute
- **TC002**: TGeneric in base class doesn't match defaultReturnType in attribute
- **TC003**: TBase in base class doesn't match baseType in attribute
- **TC004**: Duplicate option names in collection
- **TC005**: Collection not found for TypeOption attribute

## Best Practices

1. **Abstractions Placement**: Always place collections in abstractions projects for maximum discoverability
2. **Interface Return Types**: Use `TypeCollectionBase<TBase, TInterface>` for polymorphism
3. **Descriptive Names**: Use clear, meaningful names in `[TypeOption]` attributes
4. **Unique IDs**: Assign unique integer IDs to avoid conflicts across implementations
5. **Documentation**: Document your base types well since downstream developers will inherit from them

## Cross-Project Extensibility

The key benefit of TypeCollections is that **any project** can add new options:

```csharp
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Web.Http.Security;
using FractalDataWorks.Web.Http.Abstractions.Security;

namespace MyCustom.SecurityExtensions;

// In MyCustom.SecurityExtensions project
[TypeOption(typeof(SecurityMethods), "SAML")]
public class SamlSecurityMethod : SecurityMethodBase
{
    public SamlSecurityMethod() : base(200, "SAML", true) { }
}

// Automatically discovered and available
var saml = SecurityMethods.Saml;  // Works immediately
```

This enables plugin architectures and extensible frameworks where the core defines the contracts (collections + base types) and implementations can be provided by any assembly.

## Target Frameworks

- .NET Standard 2.0
- .NET 10.0

## Dependencies

This package has no external dependencies (framework only).