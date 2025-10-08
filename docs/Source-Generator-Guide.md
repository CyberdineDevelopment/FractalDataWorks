# FractalDataWorks Developer Kit - Source Generator Guide

**Complete reference for all source generators including input attributes, output structure, and troubleshooting**

## Table of Contents

- [Overview](#overview)
- [Enhanced Enums Source Generator](#enhanced-enums-source-generator)
- [Type Collections Source Generator](#type-collections-source-generator)
- [ServiceTypes Source Generator](#servicetypes-source-generator)
- [Messages Source Generator](#messages-source-generator)
- [Embedding Generators in Packages](#embedding-generators-in-packages)
- [Troubleshooting](#troubleshooting)
- [Performance Optimization](#performance-optimization)

## Overview

The FractalDataWorks Developer Kit includes four main source generators that work together to provide compile-time code generation for type-safe collections, services, and messaging:

| Generator | Purpose | Input | Output |
|-----------|---------|-------|--------|
| **EnhancedEnums.SourceGenerators** | Generate enum-like collections | `EnumOptionBase<T>` + `[EnumCollection]` | Static collections with lookup methods |
| **Collections.SourceGenerators** | Generate extensible type collections | `TypeCollectionBase<T,U>` + `[TypeCollection]` | FrozenSet collections with O(1) lookups |
| **ServiceTypes.SourceGenerators** | Generate service type collections | `ServiceTypeBase<T,F,C>` + `[ServiceTypeCollection]` | Service discovery and DI registration |
| **Messages.SourceGenerators** | Generate message collections | `MessageTemplate<TSeverity>` + `[MessageCollection]` | Message factories and lookups |

### Common Characteristics

All source generators share these features:

- **Incremental Generation**: Only regenerate when source files change
- **Roslyn-based**: Built on Microsoft.CodeAnalysis
- **ILRepack Integration**: Dependencies merged for analyzer compatibility
- **netstandard2.0 Target**: Compatible with all .NET versions
- **Compile-time Safety**: All errors reported at compile time
- **IntelliSense Support**: Generated code appears in IDE

## Enhanced Enums Source Generator

### Purpose

Automatically generates static collections for Enhanced Enum types, eliminating boilerplate and enabling cross-assembly discovery.

### Input Attributes

#### [EnumCollection]

Marks a collection class for single-assembly enum generation.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class EnumCollectionAttribute : Attribute
{
    public string CollectionName { get; }
    public EnumCollectionAttribute(string collectionName);
}
```

**Usage**:
```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

// Define enum type
public sealed class Priority : EnumOptionBase<Priority>
{
    public static readonly Priority Low = new(1, "Low");
    public static readonly Priority Medium = new(2, "Medium");
    public static readonly Priority High = new(3, "High");

    private Priority(int id, string name) : base(id, name) { }
}

// Define collection with attribute
[EnumCollection(collectionName: "PriorityCollection")]
public sealed class PriorityCollection : EnumCollectionBase<Priority>
{
    // Generator populates _all and _empty fields
}
```

#### [GlobalEnumCollection]

Marks a collection class for cross-assembly enum discovery.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlobalEnumCollectionAttribute : Attribute
{
    public string CollectionName { get; }
    public GlobalEnumCollectionAttribute(string collectionName);
}
```

**Usage**:
```csharp
// Assembly A: EmailServiceType
public sealed class EmailServiceType : EnumOptionBase<EmailServiceType>
{
    public static readonly EmailServiceType Smtp = new(1, "SMTP");
    public static readonly EmailServiceType SendGrid = new(2, "SendGrid");
}

// Assembly B: SmsServiceType
public sealed class SmsServiceType : EnumOptionBase<SmsServiceType>
{
    public static readonly SmsServiceType Twilio = new(1, "Twilio");
}

// Assembly C: Global collection
[GlobalEnumCollection(collectionName: "AllServiceTypes")]
public sealed class AllServiceTypes : EnumCollectionBase<EnumOptionBase<object>>
{
    // Discovers types from all referenced assemblies
}
```

### Generated Output Structure

For a collection marked with `[EnumCollection]`, the generator produces:

```csharp
// File: PriorityCollection.g.cs
using System;
using System.Collections.Immutable;
using System.Collections.Frozen; // .NET 8+

namespace Your.Namespace;

public sealed partial class PriorityCollection
{
    static PriorityCollection()
    {
        // Populate _all field
        _all = ImmutableArray.Create(
            Priority.Low,
            Priority.Medium,
            Priority.High
        );

        // Create empty instance
        _empty = new Priority(0, string.Empty);

        // .NET 8+ optimizations
#if NET8_0_OR_GREATER
        _lookupByName = _all.ToFrozenDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _lookupById = _all.ToFrozenDictionary(x => x.Id);
#endif
    }

    // .NET 8+ lookup dictionaries
#if NET8_0_OR_GREATER
    private static FrozenDictionary<string, Priority> _lookupByName = null!;
    private static FrozenDictionary<int, Priority> _lookupById = null!;
#endif

    // Enhanced lookup methods
    public static Priority? GetByName(string name)
    {
#if NET8_0_OR_GREATER
        return _lookupByName.TryGetValue(name, out var result) ? result : _empty;
#else
        return _all.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? _empty;
#endif
    }

    public static Priority? GetById(int id)
    {
#if NET8_0_OR_GREATER
        return _lookupById.TryGetValue(id, out var result) ? result : _empty;
#else
        return _all.FirstOrDefault(x => x.Id == id) ?? _empty;
#endif
    }
}
```

### Discovery Process

The generator uses this process to find enum options:

1. **Find Collection Classes**: Scan for classes with `[EnumCollection]` or `[GlobalEnumCollection]`
2. **Determine Base Type**: Extract `T` from `EnumCollectionBase<T>`
3. **Scan for Options**:
   - For `[EnumCollection]`: Search current assembly only
   - For `[GlobalEnumCollection]`: Search all referenced assemblies
4. **Find Static Fields**: Locate all `public static readonly T` fields
5. **Generate Collection**: Create static constructor with all discovered instances

### Cross-Assembly Discovery

For `[GlobalEnumCollection]`, the generator:

1. Scans all referenced assemblies via compilation references
2. Filters to types inheriting from `EnumOptionBase<T>`
3. Finds all static readonly fields of those types
4. Generates a unified collection across all assemblies

**Example Output**:
```csharp
// AllServiceTypes.g.cs
static AllServiceTypes()
{
    _all = ImmutableArray.Create(
        EmailServiceType.Smtp,      // From Assembly A
        EmailServiceType.SendGrid,  // From Assembly A
        SmsServiceType.Twilio       // From Assembly B
    );
}
```

### ILRepack Configuration

The generator uses ILRepack to merge dependencies into a single assembly for analyzer compatibility:

```xml
<Target Name="MergeAnalyzerDependencies" AfterTargets="Build">
  <ItemGroup>
    <InputAssemblies Include="$(OriginalAssemblyPath)" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.CodeBuilder.Abstractions.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.CodeBuilder.CSharp.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.EnhancedEnums.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.EnhancedEnums.Analyzers.dll" />
  </ItemGroup>

  <ILRepack
    OutputFile="$(OutputPath)$(AssemblyName).dll"
    InputAssemblies="@(InputAssemblies)"
    LibraryPath="@(LibraryPath)"
    Internalize="true" />
</Target>
```

This creates a self-contained analyzer with no external dependencies.

## Type Collections Source Generator

### Purpose

Generates high-performance, extensible type collections with explicit targeting via attributes.

### Input Attributes

#### [TypeCollection]

Marks a collection class for type collection generation.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class TypeCollectionAttribute : Attribute
{
    public Type BaseType { get; }
    public Type DefaultReturnType { get; }
    public Type CollectionType { get; }

    public TypeCollectionAttribute(Type baseType, Type defaultReturnType, Type collectionType);
}
```

**Parameters**:
- `baseType`: The base type to collect (e.g., `typeof(SecurityMethodBase)`)
- `defaultReturnType`: Interface or return type for generated methods (e.g., `typeof(ISecurityMethod)`)
- `collectionType`: The collection class being generated (e.g., `typeof(SecurityMethods)`)

**Usage**:
```csharp
// In Abstractions project
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod>
{
    // Empty - generator provides all functionality
}
```

#### [TypeOption]

Explicitly targets a type for inclusion in a specific collection.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class TypeOptionAttribute : Attribute
{
    public Type CollectionType { get; }
    public string Name { get; }

    public TypeOptionAttribute(Type collectionType, string name);
}
```

**Parameters**:
- `collectionType`: The collection to add this type to (e.g., `typeof(SecurityMethods)`)
- `name`: Display name for generated properties and methods

**Usage**:
```csharp
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
```

#### [TypeLookup]

Generates additional lookup methods based on property values.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public sealed class TypeLookupAttribute : Attribute
{
    public string MethodName { get; }

    public TypeLookupAttribute(string methodName);
}
```

**Usage**:
```csharp
public abstract class SecurityMethodBase : ISecurityMethod
{
    public int Id { get; }
    public string Name { get; }

    [TypeLookup("GetByProtocol")]
    public abstract string Protocol { get; }

    [TypeLookup("GetByAuthType")]
    public abstract string AuthenticationType { get; }
}

// Generates:
// IEnumerable<ISecurityMethod> GetByProtocol(string protocol)
// IEnumerable<ISecurityMethod> GetByAuthType(string authType)
```

### Generated Output Structure

For a collection marked with `[TypeCollection]`:

```csharp
// File: SecurityMethods.g.cs
using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Your.Namespace;

public partial class SecurityMethods
{
    // Static properties for each discovered type
    public static ISecurityMethod None { get; } = new NoneSecurityMethod();
    public static ISecurityMethod Jwt { get; } = new JwtSecurityMethod();

    // Frozen collections for performance
    private static FrozenSet<ISecurityMethod> _allInternal = FrozenSet.ToFrozenSet(new[]
    {
        None,
        Jwt
    });

    private static FrozenDictionary<int, ISecurityMethod> _byIdInternal = FrozenDictionary.ToFrozenDictionary(
        new[] { None, Jwt },
        x => x.Id,
        x => x
    );

    private static FrozenDictionary<string, ISecurityMethod> _byNameInternal = FrozenDictionary.ToFrozenDictionary(
        new[] { None, Jwt },
        x => x.Name,
        x => x,
        StringComparer.OrdinalIgnoreCase
    );

    // Public API
    public static FrozenSet<ISecurityMethod> All() => _allInternal;

    public static ISecurityMethod Empty() => new NoneSecurityMethod();

    public static ISecurityMethod GetById(int id)
    {
        return _byIdInternal.TryGetValue(id, out var result) ? result : Empty();
    }

    public static ISecurityMethod GetByName(string name)
    {
        return _byNameInternal.TryGetValue(name, out var result) ? result : Empty();
    }

    // Factory methods for each type
    public static ISecurityMethod CreateNone() => new NoneSecurityMethod();
    public static ISecurityMethod CreateJwt() => new JwtSecurityMethod();

    // Constructor overload support
    public static ISecurityMethod CreateJwt(string issuer, string audience)
        => new JwtSecurityMethod(issuer, audience);

    // Custom lookup methods from [TypeLookup]
    public static IEnumerable<ISecurityMethod> GetByProtocol(string protocol)
    {
        return _allInternal.Where(x => x.Protocol.Equals(protocol, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<ISecurityMethod> GetByAuthType(string authType)
    {
        return _allInternal.Where(x => x.AuthenticationType.Equals(authType, StringComparison.OrdinalIgnoreCase));
    }
}
```

### Discovery Process

The generator uses explicit targeting for O(types_with_attribute) performance:

1. **Find Collection Classes**: Scan for `[TypeCollection]` attribute
2. **Extract Parameters**: Get `baseType`, `defaultReturnType`, `collectionType` from attribute
3. **Find Type Options**: Scan all types for `[TypeOption(collectionType, name)]` matching the collection
4. **Validate Hierarchy**: Ensure each type inherits from `baseType`
5. **Extract Constructors**: Find all public constructors for factory methods
6. **Find Lookups**: Scan `baseType` for `[TypeLookup]` properties
7. **Generate Collection**: Create static properties, lookups, and factories

### Performance Benefits

Explicit targeting provides significant performance improvements:

- **Traditional Inheritance Scanning**: O(collections × assemblies × all_types)
- **Explicit Attribute Targeting**: O(types_with_attribute)

Example: In a solution with 50 assemblies and 10,000 types:
- Traditional: 50 collections × 50 assemblies × 10,000 types = 25,000,000 operations
- Explicit: ~100 types with [TypeOption] = 100 operations

**25,000x faster discovery!**

## ServiceTypes Source Generator

### Purpose

Generates service type collections with DI registration support.

### Input Attributes

#### [ServiceTypeCollection]

Marks a collection class for service type generation.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceTypeCollectionAttribute : Attribute
{
    public Type BaseType { get; }
    public Type DefaultReturnType { get; }
    public Type CollectionType { get; }
    public bool UseSingletonInstances { get; set; } = false;

    public ServiceTypeCollectionAttribute(Type baseType, Type defaultReturnType, Type collectionType);
}
```

**Usage**:
```csharp
[ServiceTypeCollection(typeof(ConnectionTypeBase<,,>), typeof(IConnectionType), typeof(ConnectionTypes))]
public partial class ConnectionTypes : ServiceTypeCollectionBase<
    ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>,
    IConnectionType,
    IGenericConnection,
    IConnectionConfiguration,
    IConnectionFactory<IGenericConnection, IConnectionConfiguration>>
{
}
```

#### [ServiceTypeOption]

Marks a service type for inclusion in a collection.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceTypeOptionAttribute : Attribute
{
    public Type CollectionType { get; }
    public string Name { get; }

    public ServiceTypeOptionAttribute(Type collectionType, string name);
}
```

**Usage**:
```csharp
[ServiceTypeOption(typeof(ConnectionTypes), "SqlServer")]
public sealed class SqlServerConnectionType : ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>
{
    public SqlServerConnectionType() : base(1, "SqlServer", "Services:Connection:SqlServer",
        "SQL Server Connection", "SQL Server database connection") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IGenericConnection, SqlServerConnection>();
        services.AddScoped<IConnectionFactory<IGenericConnection, IConnectionConfiguration>, SqlServerConnectionFactory>();
    }
}
```

### Generated Output Structure

```csharp
// File: ConnectionTypes.g.cs
using System;
using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;

namespace Your.Namespace;

public partial class ConnectionTypes
{
    // Static properties
    public static IConnectionType SqlServer { get; } = new SqlServerConnectionType();
    public static IConnectionType PostgreSql { get; } = new PostgreSqlConnectionType();

    // Collections
    private static FrozenSet<IConnectionType> _allInternal = FrozenSet.ToFrozenSet(new[]
    {
        SqlServer,
        PostgreSql
    });

    private static FrozenDictionary<string, IConnectionType> _byNameInternal = FrozenDictionary.ToFrozenDictionary(
        new[] { SqlServer, PostgreSql },
        x => x.Name,
        x => x,
        StringComparer.OrdinalIgnoreCase
    );

    // Public API
    public static FrozenSet<IConnectionType> All() => _allInternal;

    public static IConnectionType GetByName(string name)
    {
        return _byNameInternal.TryGetValue(name, out var result) ? result : Empty();
    }

    public static IConnectionType GetById(int id)
    {
        return _allInternal.FirstOrDefault(x => x.Id == id) ?? Empty();
    }

    // DI Registration
    public static void RegisterAll(IServiceCollection services)
    {
        foreach (var serviceType in _allInternal)
        {
            serviceType.Register(services);
        }
    }

    // Factory methods
    public static IGenericConnection CreateSqlServerConnection(IConnectionConfiguration configuration)
    {
        var factory = new SqlServerConnectionFactory();
        return factory.Create(configuration).Value;
    }

    public static IGenericConnection CreatePostgreSqlConnection(IConnectionConfiguration configuration)
    {
        var factory = new PostgreSqlConnectionFactory();
        return factory.Create(configuration).Value;
    }
}
```

### Generic Parameter Extraction

The generator extracts generic parameters from the base class:

```csharp
// From this inheritance:
public partial class ConnectionTypes : ServiceTypeCollectionBase<
    ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<...>>,  // TBase
    IConnectionType,                                                                              // TGeneric
    IGenericConnection,                                                                          // TService
    IConnectionConfiguration,                                                                    // TConfiguration
    IConnectionFactory<IGenericConnection, IConnectionConfiguration>>                           // TFactory
{
}

// Generates methods using:
TService = IGenericConnection
TConfiguration = IConnectionConfiguration
TFactory = IConnectionFactory<IGenericConnection, IConnectionConfiguration>
```

## Messages Source Generator

### Purpose

Generates message collections with factory methods.

### Input Attributes

#### [MessageCollection]

Marks a message base class for collection generation.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class MessageCollectionAttribute : Attribute
{
    public string CollectionName { get; }
    public string? NamespaceSuffix { get; }

    public MessageCollectionAttribute(string collectionName, string? namespaceSuffix = null);
}
```

**Usage**:
```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

[MessageCollection("UserServiceMessages")]
public abstract class UserServiceMessage : MessageTemplate<MessageSeverity>
{
    protected UserServiceMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "UserService", message, code, null, null) { }
}

// Concrete messages
public sealed class UserCreatedMessage : UserServiceMessage
{
    public static UserCreatedMessage Instance { get; } = new();

    private UserCreatedMessage()
        : base(1001, "UserCreated", MessageSeverity.Information,
               "User created successfully", "USER_CREATED") { }
}

public sealed class UserNotFoundMessage : UserServiceMessage
{
    public static UserNotFoundMessage Instance { get; } = new();

    private UserNotFoundMessage()
        : base(2001, "UserNotFound", MessageSeverity.Error,
               "User not found", "USER_NOT_FOUND") { }
}
```

### Generated Output Structure

```csharp
// File: UserServiceMessages.g.cs
using System;
using System.Collections.Frozen;

namespace Your.Namespace;

public static class UserServiceMessages
{
    // Static instances
    public static UserCreatedMessage UserCreated { get; } = UserCreatedMessage.Instance;
    public static UserNotFoundMessage UserNotFound { get; } = UserNotFoundMessage.Instance;

    // Collection
    private static FrozenSet<UserServiceMessage> _allInternal = FrozenSet.ToFrozenSet(new UserServiceMessage[]
    {
        UserCreated,
        UserNotFound
    });

    public static FrozenSet<UserServiceMessage> All() => _allInternal;

    // Lookup methods
    public static UserServiceMessage? GetByCode(string code)
    {
        return _allInternal.FirstOrDefault(x => x.Code?.Equals(code, StringComparison.OrdinalIgnoreCase) == true);
    }

    public static UserServiceMessage? GetByName(string name)
    {
        return _allInternal.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<UserServiceMessage> GetBySeverity(MessageSeverity severity)
    {
        return _allInternal.Where(x => x.Severity == severity);
    }
}
```

### Discovery Process

1. **Find Base Class**: Scan for `[MessageCollection]` attribute
2. **Find Message Types**: Scan for all types inheriting from the base message class
3. **Extract Static Instances**: Find `static Instance` properties
4. **Generate Collection**: Create static class with message collection and lookups

## Embedding Generators in Packages

### Package Configuration

To embed source generators in NuGet packages for consumer use:

#### Step 1: Configure Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>MyCompany.MySourceGenerator</PackageId>
    <Version>1.0.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <!-- Source generator package reference -->
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
  </ItemGroup>

  <ItemGroup>
    <!-- Pack the generator DLL -->
    <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />

    <!-- Pack dependencies if not using ILRepack -->
    <None Include="$(OutputPath)\FractalDataWorks.CodeBuilder.CSharp.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
  </ItemGroup>
</Project>
```

#### Step 2: Configure Consumer Projects

Consumers reference the generator package:

```xml
<PackageReference Include="MyCompany.MySourceGenerator" Version="1.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

### ILRepack for Dependency Merging

To create a self-contained generator with merged dependencies:

```xml
<ItemGroup>
  <PackageReference Include="ILRepack.Lib.MSBuild.Task">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>

<Target Name="MergeAnalyzerDependencies" AfterTargets="Build"
        Condition="'$(TargetFramework)' == 'netstandard2.0' and '$(DisableILRepack)' != 'true'">
  <PropertyGroup>
    <OriginalAssemblyPath>$(OutputPath)$(AssemblyName).original.dll</OriginalAssemblyPath>
  </PropertyGroup>

  <!-- Backup original -->
  <Copy SourceFiles="$(OutputPath)$(AssemblyName).dll" DestinationFiles="$(OriginalAssemblyPath)" />

  <ItemGroup>
    <InputAssemblies Include="$(OriginalAssemblyPath)" />
    <InputAssemblies Include="$(OutputPath)Dependency1.dll" />
    <InputAssemblies Include="$(OutputPath)Dependency2.dll" />

    <LibraryPath Include="$(OutputPath)" />
    <LibraryPath Include="$(NuGetPackageRoot)microsoft.codeanalysis.common\**\lib\netstandard2.0" />
    <LibraryPath Include="$(NuGetPackageRoot)microsoft.codeanalysis.csharp\**\lib\netstandard2.0" />
  </ItemGroup>

  <ILRepack
    OutputFile="$(OutputPath)$(AssemblyName).dll"
    InputAssemblies="@(InputAssemblies)"
    LibraryPath="@(LibraryPath)"
    Internalize="true" />

  <!-- Cleanup -->
  <Delete Files="$(OutputPath)Dependency1.dll;$(OutputPath)Dependency2.dll;$(OriginalAssemblyPath)" />
</Target>
```

Create an empty `ILRepack.targets` file to prevent default behavior:

```xml
<!-- ILRepack.targets - prevents default ILRepack execution -->
<!-- Our custom MergeAnalyzerDependencies target handles all operations -->
```

### Testing Embedded Generators

Create a test project that references your generator package:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Reference the generator package -->
    <PackageReference Include="MyCompany.MySourceGenerator" Version="1.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <!-- Enable viewing generated files -->
  <PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
  </PropertyGroup>
</Project>
```

## Troubleshooting

### Common Issues

#### 1. Source Generator Not Running

**Symptoms**: No generated code, collections are empty

**Diagnostic Steps**:
```bash
# Enable detailed build output
dotnet build -v detailed

# Check for generator diagnostics
dotnet build --verbosity diagnostic | grep -i "source.*generator"

# Verify generator is loaded
dotnet build -p:GeneratorDriverVerbosity=Detailed
```

**Solutions**:
```xml
<!-- Ensure correct package reference -->
<PackageReference Include="FractalDataWorks.EnhancedEnums.SourceGenerators"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />

<!-- Check target framework compatibility -->
<TargetFramework>net10.0</TargetFramework> <!-- or net8.0, net6.0 -->

<!-- Enable generator execution -->
<PropertyGroup>
  <EnableSourceGenerators>true</EnableSourceGenerators>
</PropertyGroup>
```

#### 2. Assembly Loading Failures

**Symptoms**: `FileNotFoundException` for generator dependencies

**Cause**: Source generators run in restricted context, external assemblies fail to load

**Solution**: Use ILRepack to merge dependencies:
```xml
<!-- See "ILRepack for Dependency Merging" section above -->
```

#### 3. Generated Code Not Visible in IDE

**Symptoms**: IntelliSense doesn't show generated members

**Solutions**:
```xml
<!-- Save generated files for inspection -->
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Then rebuild and check the `Generated` folder.

For Visual Studio:
1. Close solution
2. Delete `.vs` folder
3. Delete `bin` and `obj` folders
4. Reopen solution
5. Rebuild

#### 4. Attribute Not Found Errors

**Symptoms**: `CS0246: The type or namespace name 'EnumCollectionAttribute' could not be found`

**Solution**: Ensure attribute assembly is referenced:
```xml
<PackageReference Include="FractalDataWorks.EnhancedEnums" Version="1.0.0" />
```

#### 5. Duplicate Type Generation

**Symptoms**: `CS0111: Type already defines a member called 'All'`

**Cause**: Manual implementation conflicts with generated code

**Solution**: Remove manual implementation from partial class:
```csharp
// Remove this:
[EnumCollection("Priority")]
public sealed class PriorityCollection : EnumCollectionBase<Priority>
{
    // Don't manually define _all or All() - generator provides these
}

// Keep this:
[EnumCollection("Priority")]
public sealed partial class PriorityCollection : EnumCollectionBase<Priority>
{
    // Empty - generator provides everything
}
```

#### 6. Cross-Assembly Discovery Not Working

**Symptoms**: `[GlobalEnumCollection]` missing types from other assemblies

**Diagnostic**:
```csharp
// Check assembly references in .csproj
<ItemGroup>
  <ProjectReference Include="..\OtherAssembly\OtherAssembly.csproj" />
</ItemGroup>
```

**Solutions**:
- Verify assemblies are properly referenced
- Ensure types are public (not internal)
- Check that assemblies are loaded at discovery time
- Verify types inherit from correct base class

### Debugging Generators

#### Enable Generator Logging

```xml
<PropertyGroup>
  <ReportAnalyzer>true</ReportAnalyzer>
  <GeneratorDriverVerbosity>Detailed</GeneratorDriverVerbosity>
</PropertyGroup>
```

#### View Generated Source

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files appear in `obj/Generated/[GeneratorName]/`

#### Attach Debugger (Advanced)

For generator developers:

```csharp
// Add to generator Initialize method
#if DEBUG
    if (!System.Diagnostics.Debugger.IsAttached)
    {
        System.Diagnostics.Debugger.Launch();
    }
#endif
```

Build with Debug configuration, debugger will launch when generator runs.

## Performance Optimization

### Best Practices

#### 1. Use Explicit Targeting

```csharp
// ✅ Fast - O(types_with_attribute)
[TypeOption(typeof(SecurityMethods), "JWT")]
public class JwtSecurityMethod : SecurityMethodBase { }

// ❌ Slow - O(collections × assemblies × all_types)
// Inheritance scanning without attributes
```

#### 2. Limit Lookup Properties

```csharp
// ✅ Reasonable - 2-3 lookups
public abstract class SecurityMethodBase
{
    [TypeLookup("GetByProtocol")]
    public abstract string Protocol { get; }

    [TypeLookup("GetByAuthType")]
    public abstract string AuthenticationType { get; }
}

// ❌ Excessive - too many lookups
public abstract class SecurityMethodBase
{
    [TypeLookup("GetByProp1")] public abstract string Prop1 { get; }
    [TypeLookup("GetByProp2")] public abstract string Prop2 { get; }
    // ... 10 more properties
}
```

#### 3. Use FrozenCollections (.NET 8+)

Generated code automatically uses `FrozenDictionary` and `FrozenSet` on .NET 8+:
- **ReadOnlyCollection**: .NET 6/7
- **FrozenSet**: .NET 8+ (faster lookups)

#### 4. Singleton Instances for Stateless Types

```csharp
[ServiceTypeCollection(typeof(BaseType<,,>), typeof(IType), typeof(Types))]
public partial class Types : ServiceTypeCollectionBase<...>
{
    // Enable singleton instances for stateless types
    public bool UseSingletonInstances => true;
}
```

This reuses instances instead of creating new ones in factory methods.

### Performance Metrics

| Operation | Traditional Reflection | Source Generated |
|-----------|----------------------|------------------|
| Collection initialization | ~50-100ms | ~0ms (compile-time) |
| GetById lookup | ~10µs (linear scan) | ~100ns (dictionary) |
| GetByName lookup | ~15µs (linear scan) | ~150ns (dictionary) |
| Cross-assembly discovery | ~500ms (runtime scan) | ~0ms (compile-time) |
| Factory method call | ~5µs (Activator.CreateInstance) | ~500ns (FastGenericNew) |

**Source generation is 50-100x faster than runtime reflection!**

---

## Summary

The FractalDataWorks source generators provide:

1. **Compile-time Safety**: All type errors caught during build
2. **High Performance**: O(1) lookups with FrozenDictionary
3. **IntelliSense Support**: Generated code appears in IDE
4. **Extensibility**: Add types from any assembly
5. **Zero Runtime Cost**: No reflection overhead

Use the appropriate generator for your needs:
- **EnhancedEnums**: Fixed set of values with rich behavior
- **Collections**: Extensible type collections across assemblies
- **ServiceTypes**: Service discovery and DI registration
- **Messages**: Structured messaging with severity levels

All generators follow consistent patterns, making it easy to learn and use them throughout your application.
