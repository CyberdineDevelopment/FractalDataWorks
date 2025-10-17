# FractalDataWorks Enhanced Enums Source Generators

A comprehensive source generator package that automatically generates static collections for Enhanced Enum types, enabling compile-time safe, discoverable enum-like patterns with cross-assembly support.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [Usage Patterns](#usage-patterns)
  - [Basic EnumCollection](#basic-enumcollection)
  - [GlobalEnumCollection (Cross-Assembly)](#globalenumcollection-cross-assembly)
  - [Custom Collection Names](#custom-collection-names)
  - [Multiple Collections](#multiple-collections)
- [Generated Code Structure](#generated-code-structure)
- [Advanced Features](#advanced-features)
  - [Cross-Assembly Discovery](#cross-assembly-discovery)
  - [Custom Attributes](#custom-attributes)
  - [Generic Type Support](#generic-type-support)
- [Performance Considerations](#performance-considerations)
- [Troubleshooting](#troubleshooting)
- [ILRepack Configuration](#ilrepack-configuration)
- [Contributing](#contributing)
- [License](#license)

## Overview

The Enhanced Enums Source Generators provide automatic collection generation for Enhanced Enum types, eliminating boilerplate code and enabling powerful features like:

- **Automatic collection population** - No more manual `_all` field maintenance
- **Cross-assembly discovery** - Find enum options across multiple assemblies
- **Compile-time safety** - All collections generated at compile time
- **High performance** - Uses frozen collections (.NET 8+) and immutable arrays
- **IntelliSense support** - Full IDE support for generated collections

## Installation

Add the source generator package to your project:

```xml
<PackageReference Include="FractalDataWorks.EnhancedEnums" />
<PackageReference Include="FractalDataWorks.EnhancedEnums.SourceGenerators" 
                  OutputItemType="Analyzer" 
                  ReferenceOutputAssembly="false" />
```

The source generator will automatically process your Enhanced Enum types and generate collections.

## Quick Start

### 1. Define an Enhanced Enum Type

```csharp
using FractalDataWorks.EnhancedEnums;

public sealed class Priority : EnumOptionBase<Priority>
{
    public static readonly Priority Low = new(1, "Low");
    public static readonly Priority Medium = new(2, "Medium");
    public static readonly Priority High = new(3, "High");
    public static readonly Priority Critical = new(4, "Critical");

    private Priority(int id, string name) : base(id, name) { }
}
```

### 2. Define a Collection with Source Generator

```csharp
using FractalDataWorks.EnhancedEnums.Attributes;

[EnumCollection(collectionName: "PriorityCollection")]
public sealed class PriorityCollection : EnumCollectionBase<Priority>
{
    // The source generator automatically populates:
    // - _all field with all Priority instances
    // - _empty field with a default empty instance
}
```

### 3. Use the Generated Collection

```csharp
// Access all priorities
var allPriorities = PriorityCollection.All();
Console.WriteLine($"Total priorities: {allPriorities.Length}");

// Get by ID
var medium = PriorityCollection.GetById(2);
Console.WriteLine($"Priority: {medium?.Name}"); // Output: Priority: Medium

// Get by name
var high = PriorityCollection.GetByName("High");
Console.WriteLine($"Priority ID: {high?.Id}"); // Output: Priority ID: 3

// Check if exists
bool exists = PriorityCollection.TryGetByName("Critical", out var critical);
Console.WriteLine($"Critical exists: {exists}"); // Output: Critical exists: True
```

## Core Concepts

### Enhanced Enum Base Types

Enhanced Enums are built on these base classes:

```csharp
// Base class for enum options
public abstract class EnumOptionBase<T> where T : EnumOptionBase<T>
{
    public int Id { get; }
    public string Name { get; }
    
    protected EnumOptionBase(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

// Base class for collections
public abstract class EnumCollectionBase<T> where T : EnumOptionBase<T>
{
    protected static ImmutableArray<T> _all = []; // Populated by source generator
    protected static T _empty = default!;          // Populated by source generator
    
    public static ImmutableArray<T> All() => _all;
    public static T Empty() => _empty;
    public static T? GetById(int id) { /* ... */ }
    public static T? GetByName(string name) { /* ... */ }
    // ... other utility methods
}
```

### Source Generator Attributes

The source generator recognizes these attributes:

```csharp
using System;

// For single-assembly collections
[AttributeUsage(AttributeTargets.Class)]
public sealed class EnumCollectionAttribute : Attribute
{
    public string CollectionName { get; }
    public EnumCollectionAttribute(string collectionName) { /* ... */ }
}

// For cross-assembly collections
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlobalEnumCollectionAttribute : Attribute
{
    public string CollectionName { get; }
    public GlobalEnumCollectionAttribute(string collectionName) { /* ... */ }
}
```

## Usage Patterns

### Basic EnumCollection

The most common pattern for single-assembly enum collections:

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

// Define the enum type
public sealed class Status : EnumOptionBase<Status>
{
    public static readonly Status Pending = new(1, "Pending");
    public static readonly Status InProgress = new(2, "InProgress");
    public static readonly Status Completed = new(3, "Completed");
    public static readonly Status Cancelled = new(4, "Cancelled");

    private Status(int id, string name) : base(id, name) { }
}

// Define the collection
[EnumCollection(collectionName: "StatusCollection")]
public sealed class StatusCollection : EnumCollectionBase<Status>
{
    // Source generator populates _all and _empty
}

// Usage
var allStatuses = StatusCollection.All(); // [Pending, InProgress, Completed, Cancelled]
var pending = StatusCollection.GetById(1); // Status.Pending
var inProgress = StatusCollection.GetByName("InProgress"); // Status.InProgress
```

### GlobalEnumCollection (Cross-Assembly)

For enum types distributed across multiple assemblies:

**Assembly A: Core.dll**
```csharp
using FractalDataWorks.EnhancedEnums;

public sealed class EmailServiceType : EnumOptionBase<EmailServiceType>
{
    public static readonly EmailServiceType Smtp = new(1, "SMTP");
    public static readonly EmailServiceType SendGrid = new(2, "SendGrid");

    private EmailServiceType(int id, string name) : base(id, name) { }
}
```

**Assembly B: Services.dll**
```csharp
using FractalDataWorks.EnhancedEnums;

public sealed class SmsServiceType : EnumOptionBase<SmsServiceType>
{
    public static readonly SmsServiceType Twilio = new(1, "Twilio");
    public static readonly SmsServiceType AzureSms = new(2, "AzureSms");

    private SmsServiceType(int id, string name) : base(id, name) { }
}
```

**Assembly C: Consumer.dll**
```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

// This collection will include types from all referenced assemblies
[GlobalEnumCollection(collectionName: "AllServiceTypes")]
public sealed class AllServiceTypes : EnumCollectionBase<EnumOptionBase<object>>
{
    // Source generator discovers:
    // - EmailServiceType.Smtp, EmailServiceType.SendGrid (from Core.dll)
    // - SmsServiceType.Twilio, SmsServiceType.AzureSms (from Services.dll)
}

// Usage
var allServices = AllServiceTypes.All(); 
// Contains: [EmailServiceType.Smtp, EmailServiceType.SendGrid, SmsServiceType.Twilio, SmsServiceType.AzureSms]

var emailService = AllServiceTypes.GetByName("SMTP"); // EmailServiceType.Smtp
var smsService = AllServiceTypes.GetByName("Twilio"); // SmsServiceType.Twilio
```

### Custom Collection Names

You can customize the collection class name:

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

[EnumCollection(collectionName: "LogLevels")]
public sealed class LogLevels : EnumCollectionBase<LogLevel>
{
    // Collection will be accessible as LogLevels.All(), LogLevels.GetById(), etc.
}

// Usage matches the class name
var levels = LogLevels.All();
var error = LogLevels.GetByName("Error");
```

### Multiple Collections

You can create multiple collections for the same enum type:

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

public sealed class Color : EnumOptionBase<Color>
{
    public static readonly Color Red = new(1, "Red");
    public static readonly Color Green = new(2, "Green");
    public static readonly Color Blue = new(3, "Blue");
    public static readonly Color Yellow = new(4, "Yellow");

    private Color(int id, string name) : base(id, name) { }
}

// All colors
[EnumCollection(collectionName: "AllColors")]
public sealed class AllColors : EnumCollectionBase<Color> { }

// Primary colors only (would require custom filtering - see Advanced Features)
[EnumCollection(collectionName: "PrimaryColors")]
public sealed class PrimaryColors : EnumCollectionBase<Color> { }
```

## Generated Code Structure

The source generator produces code similar to this:

```csharp
// Generated code for StatusCollection
static StatusCollection()
{
    // Populate _all with all Status instances
    _all = ImmutableArray.Create(
        Status.Pending,
        Status.InProgress,
        Status.Completed,
        Status.Cancelled
    );
    
    // Create empty instance
    _empty = new Status(0, string.Empty);
}
```

### .NET 8+ Optimizations

On .NET 8 and later, the generated code uses `FrozenDictionary` for optimal lookup performance:

```csharp
// .NET 8+ generated lookup optimization
private static FrozenDictionary<string, Status>? _lookupByName;
private static FrozenDictionary<int, Status>? _lookupById;

private static void EnsureLookupsInitialized()
{
    if (_lookupByName != null) return;
    
    _lookupByName = _all.ToFrozenDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    _lookupById = _all.ToFrozenDictionary(x => x.Id);
}
```

## Advanced Features

### Cross-Assembly Discovery

The source generator can discover Enhanced Enum types across multiple assemblies:

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

// This will find all Enhanced Enum types in referenced assemblies
[GlobalEnumCollection(collectionName: "AllEnumTypes")]
public sealed class AllEnumTypes : EnumCollectionBase<EnumOptionBase<object>>
{
    // Automatically populated with types from all assemblies
}

// Usage
var allTypes = AllEnumTypes.All();
Console.WriteLine($"Found {allTypes.Length} enum types across all assemblies");

// Search across all types
var foundType = AllEnumTypes.GetByName("SMTP");
Console.WriteLine($"Found type: {foundType?.GetType().Name}");
```

### Custom Attributes

You can add metadata to your enum types that will be preserved:

```csharp
using System.ComponentModel;
using FractalDataWorks.EnhancedEnums;

[Description("Represents task priorities")]
public sealed class Priority : EnumOptionBase<Priority>
{
    [Description("Low priority task")]
    public static readonly Priority Low = new(1, "Low");

    [Description("High priority task")]
    public static readonly Priority High = new(2, "High");

    private Priority(int id, string name) : base(id, name) { }
}

// The attributes are preserved and accessible via reflection
var lowPriority = PriorityCollection.GetByName("Low");
var description = lowPriority?.GetType()
    .GetField(nameof(Priority.Low))?
    .GetCustomAttribute<DescriptionAttribute>()?
    .Description;
Console.WriteLine($"Description: {description}"); // Output: Description: Low priority task
```

### Generic Type Support

Enhanced Enums can work with generic types:

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

public sealed class DataType<T> : EnumOptionBase<DataType<T>>
{
    public static readonly DataType<T> String = new(1, "String");
    public static readonly DataType<T> Number = new(2, "Number");
    public static readonly DataType<T> Boolean = new(3, "Boolean");

    private DataType(int id, string name) : base(id, name) { }
}

[EnumCollection(collectionName: "StringDataTypes")]
public sealed class StringDataTypes : EnumCollectionBase<DataType<string>>
{
    // Specialized for string data types
}
```

## Performance Considerations

### Memory Usage

- **ImmutableArray**: Collections use `ImmutableArray<T>` for memory efficiency
- **Lazy Loading**: Lookup dictionaries are created only when first accessed
- **Frozen Collections**: On .NET 8+, uses `FrozenDictionary` for optimal read performance

### Lookup Performance

| Operation | Time Complexity | Notes |
|-----------|----------------|-------|
| `All()` | O(1) | Direct array access |
| `GetById(id)` | O(1) | Dictionary lookup after initialization |
| `GetByName(name)` | O(1) | Dictionary lookup after initialization |
| `Count` | O(1) | Array length |

### Build Performance

The source generator is designed for minimal build impact:

- **Incremental Generation**: Only regenerates when source files change
- **Efficient Discovery**: Uses Roslyn's semantic model for fast type discovery
- **Cached Results**: Results are cached between builds

## Troubleshooting

### Common Issues

#### 1. Source Generator Not Running

**Symptoms**: No generated collections, manual `_all` field required

**Solutions**:
```xml
<!-- Ensure correct package reference -->
<PackageReference Include="FractalDataWorks.EnhancedEnums.SourceGenerators" 
                  OutputItemType="Analyzer" 
                  ReferenceOutputAssembly="false" />

<!-- Check that target framework is supported -->
<TargetFramework>net8.0</TargetFramework> <!-- or net6.0, net7.0, etc. -->
```

#### 2. Collections Are Empty

**Symptoms**: `Collection.All()` returns empty array

**Diagnostic Steps**:
```csharp
// Check if your enum type follows the correct pattern
public sealed class MyEnum : EnumOptionBase<MyEnum> // ✅ Correct
{
    public static readonly MyEnum Value1 = new(1, "Value1"); // ✅ Static readonly
    private MyEnum(int id, string name) : base(id, name) { } // ✅ Private constructor
}

// Check collection attribute
[EnumCollection(collectionName: "MyEnumCollection")] // ✅ Correct attribute
public sealed class MyEnumCollection : EnumCollectionBase<MyEnum> { }
```

#### 3. Cross-Assembly Discovery Not Working

**Symptoms**: `GlobalEnumCollection` missing types from other assemblies

**Solutions**:
- Ensure assemblies are properly referenced
- Verify enum types are public
- Check that assemblies are loaded at discovery time

#### 4. Build Errors

**Symptoms**: Compilation errors related to source generation

**Diagnostic Commands**:
```bash
# Enable detailed MSBuild logging
dotnet build -v detailed

# Check for source generator diagnostics
dotnet build --verbosity diagnostic | grep -i "source.*generator"
```

### Debugging Generated Code

You can inspect the generated code:

```xml
<!-- Add to your project file to save generated files -->
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files will be saved to the `Generated` folder for inspection.

### Logging and Diagnostics

Enable source generator diagnostics:

```xml
<PropertyGroup>
    <ReportAnalyzer>true</ReportAnalyzer>
</PropertyGroup>
```

## ILRepack Configuration

The Enhanced Enums source generator uses ILRepack to merge all dependencies into a single self-contained assembly, resolving runtime dependency loading issues that occur when source generators try to load external dependencies.

### Why ILRepack is Needed

Source generators run in a restricted execution context where external assembly dependencies often fail to load. The Enhanced Enums source generator depends on several FractalDataWorks assemblies:

- `FractalDataWorks.CodeBuilder.Abstractions.dll`
- `FractalDataWorks.CodeBuilder.CSharp.dll`  
- `FractalDataWorks.EnhancedEnums.dll`
- `FractalDataWorks.EnhancedEnums.Analyzers.dll`

Without ILRepack, these dependencies would cause runtime errors like:
```
Could not load file or assembly 'FractalDataWorks.CodeBuilder.Abstractions, Version=0.3.0.0'
FileNotFoundException: Could not locate the assembly
```

### ILRepack Implementation

Our implementation uses a custom MSBuild target to merge all dependencies during the build process, creating a single DLL that can execute in the source generator context without any external dependencies.

#### Package Configuration

```xml
<PackageReference Include="ILRepack.Lib.MSBuild.Task">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

#### Custom MSBuild Target

```xml
<Target Name="MergeAnalyzerDependencies" AfterTargets="Build" 
        Condition="'$(TargetFramework)' == 'netstandard2.0' and '$(DisableILRepack)' != 'true'">
  <PropertyGroup>
    <OriginalAssemblyPath>$(OutputPath)$(AssemblyName).original.dll</OriginalAssemblyPath>
  </PropertyGroup>
  
  <!-- Backup original assembly -->
  <Copy SourceFiles="$(OutputPath)$(AssemblyName).dll" DestinationFiles="$(OriginalAssemblyPath)" />
  
  <ItemGroup>
    <!-- Main assembly first -->
    <InputAssemblies Include="$(OriginalAssemblyPath)" />
    
    <!-- Custom dependencies to internalize -->
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.CodeBuilder.Abstractions.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.CodeBuilder.CSharp.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.EnhancedEnums.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.EnhancedEnums.Analyzers.dll" />
    
    <!-- Comprehensive library search paths -->
    <LibraryPath Include="$(OutputPath)" />
    <LibraryPath Include="$(NuGetPackageRoot)microsoft.codeanalysis.common\**\lib\netstandard2.0" />
    <LibraryPath Include="$(NuGetPackageRoot)microsoft.codeanalysis.csharp\**\lib\netstandard2.0" />
    <LibraryPath Include="$(NuGetPackageRoot)system.collections.immutable\**\lib\netstandard2.0" />
    <LibraryPath Include="$(NuGetPackageRoot)system.memory\**\lib\netstandard2.0" />
  </ItemGroup>
  
  <ILRepack 
    OutputFile="$(OutputPath)$(AssemblyName).dll"
    InputAssemblies="@(InputAssemblies)"
    LibraryPath="@(LibraryPath)"
    Internalize="true" />
    
  <!-- Cleanup internalized dependency DLLs and backup -->
  <Delete Files="$(OutputPath)FractalDataWorks.CodeBuilder.Abstractions.dll;$(OutputPath)FractalDataWorks.CodeBuilder.CSharp.dll;$(OutputPath)FractalDataWorks.EnhancedEnums.dll;$(OutputPath)FractalDataWorks.EnhancedEnums.Analyzers.dll;$(OriginalAssemblyPath)" />
</Target>
```

#### Disabling Default ILRepack Behavior

The project includes an empty `ILRepack.targets` file to prevent the `ILRepack.Lib.MSBuild.Task` package from running its default target, which would conflict with our custom implementation:

**ILRepack.targets** (empty file):
```xml
<!-- This file prevents ILRepack.Lib.MSBuild.Task from running its default target -->
<!-- Our custom MergeAnalyzerDependencies target handles all ILRepack operations -->
```

This approach ensures that only our custom `MergeAnalyzerDependencies` target runs, avoiding conflicts between different ILRepack executions.

### Build Results

After ILRepack merging:
- **Self-contained assembly**: All dependencies internalized
- **No external dependencies**: Source generator can execute independently
- **Optimized size**: Only necessary dependencies included
- **Clean output**: Dependency DLLs removed from output directory

### Verification

Verify ILRepack is working correctly:

```bash
# Build and check for ILRepack messages
dotnet build -c Debug

# Expected output:
# ILRepack: Merging assemblies directly to bin\Debug\netstandard2.0\FractalDataWorks.EnhancedEnums.SourceGenerators.dll
# Added assembly 'bin\Debug\netstandard2.0\FractalDataWorks.EnhancedEnums.SourceGenerators.original.dll'
# Added assembly 'bin\Debug\netstandard2.0\FractalDataWorks.CodeBuilder.Abstractions.dll'
# Added assembly 'bin\Debug\netstandard2.0\FractalDataWorks.CodeBuilder.CSharp.dll'
# Added assembly 'bin\Debug\netstandard2.0\FractalDataWorks.EnhancedEnums.dll'
# Added assembly 'bin\Debug\netstandard2.0\FractalDataWorks.EnhancedEnums.Analyzers.dll'
# Merging 5 assemblies to 'bin\Debug\netstandard2.0\FractalDataWorks.EnhancedEnums.SourceGenerators.dll'
# Merge succeeded in X.X s
# ILRepack: Successfully merged source generator dependencies

# Test Release build as well
dotnet build -c Release
```

### Debug vs Release Configuration

The ILRepack configuration works identically in both Debug and Release modes:

- **Debug build**: Merges dependencies for development and testing
- **Release build**: Merges dependencies for production NuGet packages
- **Both configurations**: Produce self-contained, dependency-free assemblies

### Troubleshooting ILRepack

Common issues and solutions:

#### 1. "Unable to resolve assembly" Errors

```bash
# Error: Unable to resolve assembly 'AssemblyName'
# Cause: Missing LibraryPath entries or incorrect assembly references

# Solution: Add missing library paths to the target
<LibraryPath Include="$(NuGetPackageRoot)missing-package\**\lib\netstandard2.0" />
```

#### 2. Conflicting ILRepack Targets

```bash
# Error: ILRepack runs twice, second execution fails
# Cause: Default ILRepack.Lib.MSBuild.Task target conflicts with custom target

# Solution: Ensure ILRepack.targets file exists (already implemented)
```

#### 3. Assembly Resolution in Different Configurations

```bash
# Error: Works in Debug but fails in Release (or vice versa)
# Cause: Path differences between configurations

# Solution: Use MSBuild properties for dynamic path resolution:
<LibraryPath Include="$(OutputPath)" />
<LibraryPath Include="$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)" />
```

#### 4. Source Generator Loading Errors

```bash
# Error: Source generator fails to load even after ILRepack
# Diagnosis: Check if merge actually succeeded

# Solution: 
# 1. Verify "Merge succeeded" message appears in build output
# 2. Check that dependency DLLs are removed from output directory
# 3. Inspect merged assembly size (should be larger than original)
```

### Disabling ILRepack

If you need to disable ILRepack for debugging:

```xml
<!-- Add to PropertyGroup -->
<DisableILRepack>true</DisableILRepack>
```

This will skip the merge process and leave all assemblies separate, which can be useful for debugging dependency loading issues.

## Contributing

### Development Setup

1. Clone the repository
2. Install .NET 8.0+ SDK
3. Build the solution: `dotnet build`
4. Run tests: `dotnet test`

### Adding New Features

1. Implement feature in appropriate service class
2. Add corresponding tests
3. Update documentation
4. Test with sample projects

### Debugging Source Generators

```xml
<!-- Enable source generator debugging -->
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../../LICENSE) file for details.