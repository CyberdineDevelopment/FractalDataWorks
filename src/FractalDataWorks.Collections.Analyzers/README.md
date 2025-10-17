# FractalDataWorks.Collections.Analyzers

[![NuGet](https://img.shields.io/nuget/v/FractalDataWorks.Collections.Analyzers.svg)](https://www.nuget.org/packages/FractalDataWorks.Collections.Analyzers/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Roslyn analyzers for TypeCollections that provide compile-time validation and code quality checks for the plugin architecture pattern.

## Table of Contents

- [Project Overview](#project-overview)
- [Analyzer Rules](#analyzer-rules)
  - [TypeCollection-Specific Rules](#typecollection-specific-rules)
  - [Shared Pattern Validation Rules](#shared-pattern-validation-rules)
- [Usage Instructions](#usage-instructions)
- [Integration](#integration)
- [Rule Details](#rule-details)
- [Development](#development)
- [Testing](#testing)

## Project Overview

The FractalDataWorks.Collections.Analyzers project provides compile-time code analysis for TypeCollections to ensure:

- **Required Attributes**: Validates that types inheriting from collection base types have the required `[TypeOption]` attribute
- **Generic Type Safety**: Ensures `TGeneric` in base class matches `defaultReturnType` in `[TypeCollection]` attribute
- **Base Type Consistency**: Validates that `TBase` in base class matches `baseType` in `[TypeCollection]` attribute
- **Plugin Discovery**: Catches missing attributes that would prevent type discovery by TypeCollectionGenerator
- **Best Practices**: Enforces recommended patterns for TypeCollection implementations

### Why Analyzers Matter

TypeCollections enable plugin architectures where downstream developers can extend functionality by adding new type options. These analyzers catch configuration errors at compile time that would otherwise result in types being silently ignored during code generation, ensuring all type options are properly discovered and included.

## Analyzer Rules

### TypeCollection-Specific Rules

| Rule ID | Category | Severity | Description |
|---------|----------|----------|-------------|
| [TC001](#tc001-missing-typeoption-attribute) | Usage | Warning | Type inherits from collection base but missing required `[TypeOption]` attribute |
| [TC002](#tc002-tgeneric-mismatch-error) | Usage | Error | TGeneric in base class doesn't match defaultReturnType in TypeCollection attribute |
| [TC003](#tc003-tbase-mismatch-error) | Usage | Error | TBase in base class doesn't match baseType in TypeCollection attribute |

### Shared Pattern Validation Rules

These rules are shared with other collection-based patterns in the FractalDataWorks ecosystem:

| Rule ID | Category | Severity | Description |
|---------|----------|----------|-------------|
| ENH1001 | Usage | Warning | Enhanced enum base class should implement IEnumOption |
| ENH1002 | Design | Warning | Enhanced enum constructor validation |

The ENH* rules provide foundational validation for collection patterns and are inherited for consistency across the ecosystem.

## Usage Instructions

### Installation

Install the analyzer package via NuGet:

```bash
# .NET CLI
dotnet add package FractalDataWorks.Collections.Analyzers

# Package Manager Console
Install-Package FractalDataWorks.Collections.Analyzers
```

### Package Reference

```xml
<PackageReference Include="FractalDataWorks.Collections.Analyzers" Version="*.*.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### Configuration

The analyzers run automatically when the package is installed. You can configure specific rules in your `.editorconfig` file:

```ini
# Disable specific rules
dotnet_diagnostic.TC001.severity = none

# Treat TC001 as error instead of warning
dotnet_diagnostic.TC001.severity = error

# Configure category severity
dotnet_analyzer_diagnostic.category-usage.severity = warning
```

## Integration

### With TypeCollections Packages

The analyzers are designed to work seamlessly with the TypeCollections ecosystem:

```xml
<!-- Core TypeCollections functionality -->
<PackageReference Include="FractalDataWorks.Collections" Version="*.*.*" />

<!-- Source generators for automatic collection generation -->
<PackageReference Include="FractalDataWorks.Collections.SourceGenerators" Version="*.*.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>

<!-- Analyzers for compile-time validation -->
<PackageReference Include="FractalDataWorks.Collections.Analyzers" Version="*.*.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### Build Integration

The analyzers integrate with MSBuild and provide diagnostics during compilation:

```bash
# Build with analyzer output
dotnet build

# Treat analyzer warnings as errors
dotnet build --warnaserror
```

## Rule Details

### TC001: Missing TypeOption Attribute

**Category:** Usage | **Severity:** Warning

Types that inherit from a base type specified in a `[TypeCollection]` attribute must have the `[TypeOption]` attribute to be discovered by TypeCollectionGenerator. Without this attribute, the type will be silently ignored during collection generation, resulting in missing functionality at runtime.

**Problem:**
```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Web.Http.Abstractions.Security;
using FractalDataWorks.Web.Http.Security;

namespace MyCustom.SecurityExtensions;

// Missing [TypeOption] attribute - will NOT be discovered!
public class CustomAuthMethod : SecurityMethodBase
{
    public CustomAuthMethod() : base(100, "CustomAuth", true) { }
}

// This will fail at runtime:
// var custom = SecurityMethods.CustomAuth; // Property doesn't exist!
```

**Solution:**
```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Web.Http.Abstractions.Security;
using FractalDataWorks.Web.Http.Security;

namespace MyCustom.SecurityExtensions;

// Added [TypeOption] - now properly discovered
[TypeOption(typeof(SecurityMethods), "CustomAuth")]
public class CustomAuthMethod : SecurityMethodBase
{
    public CustomAuthMethod() : base(100, "CustomAuth", true) { }
}

// Now works correctly:
// var custom = SecurityMethods.CustomAuth; // Property exists!
```

**Why This Matters:**
- Without `[TypeOption]`, types are silently ignored during code generation
- No runtime error occurs - the property/method simply doesn't exist
- Plugin developers may not realize their extension isn't being discovered
- This analyzer catches the issue at compile time with a clear warning

### TC002: TGeneric Mismatch Error

**Category:** Usage | **Severity:** Error

When using `TypeCollectionBase<TBase, TGeneric>`, the `TGeneric` type parameter must exactly match the `defaultReturnType` parameter in the `[TypeCollection]` attribute. This ensures type safety and consistent return types across all generated methods.

**Problem:**
```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

// ERROR: TGeneric (IAuthMethod) doesn't match defaultReturnType (ISecurityMethod)
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, IAuthMethod>
{
    // Compilation error - type mismatch!
}
```

**Solution:**
```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

// Correct: TGeneric matches defaultReturnType
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod>
{
    // All generated methods return ISecurityMethod
}
```

**Generated API Impact:**
```csharp
using FractalDataWorks.Web.Http.Abstractions.Security;

// All methods use the correct return type
ISecurityMethod method = SecurityMethods.None;           // Static property
ISecurityMethod byName = SecurityMethods.GetByName("None"); // Lookup method
```

**Why This Matters:**
- Ensures consistent return types across all generated methods
- Prevents type safety issues in generated code
- Catches configuration errors before code generation occurs
- Provides clear error message with exact types that don't match

### TC003: TBase Mismatch Error

**Category:** Usage | **Severity:** Error

The `TBase` type parameter in `TypeCollectionBase<TBase, TGeneric>` must exactly match the `baseType` parameter in the `[TypeCollection]` attribute. This ensures the generator scans for the correct base type when discovering type options.

**Problem:**
```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

// ERROR: TBase (AuthMethodBase) doesn't match baseType (SecurityMethodBase)
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<AuthMethodBase, ISecurityMethod>
{
    // Compilation error - base type mismatch!
}
```

**Solution:**
```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

// Correct: TBase matches baseType
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod>
{
    // Generator will discover all types inheriting from SecurityMethodBase
}
```

**Discovery Impact:**
```csharp
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Web.Http.Security;
using FractalDataWorks.Web.Http.Abstractions.Security;

namespace FractalDataWorks.Web.Http.Security.Methods;

// This type will be discovered because it inherits from SecurityMethodBase
[TypeOption(typeof(SecurityMethods), "JWT")]
public class JwtSecurityMethod : SecurityMethodBase
{
    public JwtSecurityMethod() : base(1, "JWT", true) { }
}
```

**Why This Matters:**
- Ensures the generator scans for the correct base type
- Prevents missing type options due to incorrect base type configuration
- Catches configuration errors that would result in empty collections
- Provides clear error message showing the type mismatch

## Development

### Project Structure

```
FractalDataWorks.Collections.Analyzers/
├── Analyzers/
│   └── MissingTypeOptionAnalyzer.cs    # TC001, TC002, TC003
├── AnalyzerReleases.Shipped.md
├── AnalyzerReleases.Unshipped.md
└── FractalDataWorks.Collections.Analyzers.csproj
```

### Analyzer Implementation

The `MissingTypeOptionAnalyzer` performs three key validations:

1. **TC001 - Missing TypeOption Detection:**
   - Scans all types with `[TypeCollection]` attributes to find base types
   - Searches the compilation for types inheriting from those base types
   - Reports warning for concrete types missing `[TypeOption]` attribute
   - Only reports for types in current compilation (not referenced assemblies)

2. **TC002 - TGeneric Validation:**
   - Validates `TypeCollectionBase<TBase, TGeneric>` declarations
   - Compares `TGeneric` against `defaultReturnType` in `[TypeCollection]`
   - Reports error if types don't match exactly

3. **TC003 - TBase Validation:**
   - Validates `TypeCollectionBase<TBase, TGeneric>` declarations
   - Compares `TBase` against `baseType` in `[TypeCollection]`
   - Reports error if types don't match exactly

### Adding New Analyzers

1. **Create Analyzer Class:**
```csharp
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FractalDataWorks.Collections.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MyCustomAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TC004";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Rule title",
        "Message format",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Detailed description");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        // Analysis logic here
    }
}
```

2. **Update AnalyzerReleases.Unshipped.md:**
```markdown
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
TC004 | Usage | Warning | MyCustomAnalyzer - Description
```

3. **Add Tests:**
```csharp
using System.Threading.Tasks;
using Xunit;

namespace FractalDataWorks.Collections.Analyzers.Tests;

public class MyCustomAnalyzerTests
{
    [Fact]
    public async Task ShouldDetectViolation()
    {
        var source = @"
using FractalDataWorks.Collections.Attributes;

namespace TestNamespace;

// Test code that should trigger diagnostic
";

        var expected = new DiagnosticResult(MyCustomAnalyzer.DiagnosticId)
            .WithLocation(line, column)
            .WithArguments("argument");

        await VerifyAnalyzerAsync(source, expected);
    }
}
```

### Building and Packaging

The analyzer project uses the standard analyzer packaging configuration:

```xml
<!-- Analyzer package settings -->
<IncludeBuildOutput>false</IncludeBuildOutput>
<DevelopmentDependency>true</DevelopmentDependency>

<!-- Package analyzers in the correct location -->
<ItemGroup>
  <None Include="$(OutputPath)\$(AssemblyName).dll"
        Pack="true"
        PackagePath="analyzers/dotnet/cs"
        Visible="false" />
</ItemGroup>
```

### Release Process

1. **Move rules from Unshipped to Shipped:**
   - When releasing, move rules from `AnalyzerReleases.Unshipped.md` to `AnalyzerReleases.Shipped.md`
   - Add release version header in shipped file
   - Clear unshipped file (keeping header structure)

2. **Version and publish:**
   - Update version in project file
   - Build and pack: `dotnet pack`
   - Publish to NuGet

## Testing

### Unit Testing

Analyzers should be thoroughly tested with various scenarios:

```csharp
using System.Threading.Tasks;
using Xunit;

namespace FractalDataWorks.Collections.Analyzers.Tests;

public class MissingTypeOptionAnalyzerTests
{
    [Fact]
    public async Task ShouldDetectMissingTypeOption()
    {
        var source = @"
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace TestNamespace;

[TypeCollection(typeof(TestBase), typeof(ITest), typeof(TestCollection))]
public partial class TestCollection : TypeCollectionBase<TestBase, ITest>
{
}

public interface ITest
{
    int Id { get; }
}

public abstract class TestBase : ITest
{
    public int Id { get; }
    protected TestBase(int id) { Id = id; }
}

// Missing [TypeOption] - should trigger TC001
public class ConcreteTest : TestBase
{
    public ConcreteTest() : base(1) { }
}";

        var expected = DiagnosticResult
            .CompilerWarning(MissingTypeOptionAnalyzer.MissingTypeOptionDiagnosticId)
            .WithSpan(22, 14, 22, 26)
            .WithArguments("ConcreteTest", "TestBase");

        await VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ShouldNotWarnWhenTypeOptionPresent()
    {
        var source = @"
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace TestNamespace;

[TypeCollection(typeof(TestBase), typeof(ITest), typeof(TestCollection))]
public partial class TestCollection : TypeCollectionBase<TestBase, ITest>
{
}

public interface ITest
{
    int Id { get; }
}

public abstract class TestBase : ITest
{
    public int Id { get; }
    protected TestBase(int id) { Id = id; }
}

// Has [TypeOption] - should not trigger warning
[TypeOption(typeof(TestCollection), ""ConcreteTest"")]
public class ConcreteTest : TestBase
{
    public ConcreteTest() : base(1) { }
}";

        // Should produce no diagnostics
        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ShouldDetectTGenericMismatch()
    {
        var source = @"
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace TestNamespace;

public interface ITest { }
public interface IOther { }
public abstract class TestBase : ITest { }

// TGeneric (IOther) doesn't match defaultReturnType (ITest)
[TypeCollection(typeof(TestBase), typeof(ITest), typeof(TestCollection))]
public partial class TestCollection : TypeCollectionBase<TestBase, IOther>
{
}";

        var expected = DiagnosticResult
            .CompilerError(MissingTypeOptionAnalyzer.GenericTypeMismatchDiagnosticId)
            .WithSpan(12, 14, 12, 28)
            .WithArguments("TestCollection", "IOther", "ITest");

        await VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ShouldDetectTBaseMismatch()
    {
        var source = @"
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace TestNamespace;

public interface ITest { }
public abstract class TestBase : ITest { }
public abstract class OtherBase : ITest { }

// TBase (OtherBase) doesn't match baseType (TestBase)
[TypeCollection(typeof(TestBase), typeof(ITest), typeof(TestCollection))]
public partial class TestCollection : TypeCollectionBase<OtherBase, ITest>
{
}";

        var expected = DiagnosticResult
            .CompilerError(MissingTypeOptionAnalyzer.BaseTypeMismatchDiagnosticId)
            .WithSpan(12, 14, 12, 28)
            .WithArguments("TestCollection", "OtherBase", "TestBase");

        await VerifyAnalyzerAsync(source, expected);
    }
}
```

### Integration Testing

Test analyzers with realistic TypeCollection scenarios:

```csharp
using System.Threading.Tasks;
using Xunit;

namespace FractalDataWorks.Collections.Analyzers.Tests;

public class TypeCollectionIntegrationTests
{
    [Fact]
    public async Task ShouldWorkWithCompleteTypeCollection()
    {
        var source = @"
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace TestNamespace;

public interface ISecurityMethod
{
    int Id { get; }
    string Name { get; }
}

public abstract class SecurityMethodBase : ISecurityMethod
{
    protected SecurityMethodBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }
    public string Name { get; }
}

[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod>
{
}

[TypeOption(typeof(SecurityMethods), ""None"")]
public sealed class NoneSecurityMethod : SecurityMethodBase
{
    public NoneSecurityMethod() : base(1, ""None"") { }
}

[TypeOption(typeof(SecurityMethods), ""JWT"")]
public sealed class JwtSecurityMethod : SecurityMethodBase
{
    public JwtSecurityMethod() : base(2, ""JWT"") { }
}";

        // Should produce no diagnostics - correctly configured
        await VerifyAnalyzerAsync(source);
    }
}
```

### Performance Testing

Ensure analyzers perform well on large codebases:

```csharp
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace FractalDataWorks.Collections.Analyzers.Tests;

public class PerformanceTests
{
    [Fact]
    public async Task ShouldPerformWellOnLargeCodebase()
    {
        // Generate large codebase with multiple collections and options
        var sources = GenerateLargeCodebase(
            collectionCount: 50,
            optionsPerCollection: 100);

        var stopwatch = Stopwatch.StartNew();
        await VerifyAnalyzerAsync(sources);
        stopwatch.Stop();

        // Should complete analysis in reasonable time
        Assert.True(stopwatch.ElapsedMilliseconds < 10000); // Under 10 seconds
    }

    private static string[] GenerateLargeCodebase(int collectionCount, int optionsPerCollection)
    {
        // Generate test code with specified number of collections and options
        // Implementation details...
    }
}
```

---

For more information about TypeCollections, visit the [Collections documentation](../FractalDataWorks.Collections/README.md) or the [GitHub repository](https://github.com/FractalDataWorks/FractalDataWorks).
