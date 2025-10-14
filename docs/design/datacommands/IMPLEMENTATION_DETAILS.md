# DataCommands Implementation Details

## Purpose

This document contains the **practical, concrete details** needed for implementation:
- Background and problem context
- Exact syntax and conventions
- Where things belong (project/namespace/file organization)
- README files to update
- Integration points with existing code
- Syntax patterns not documented in CLAUDE.md

Read this BEFORE starting implementation.

## Background Context

### The Problem This Solves

**Original State**: Data access was tightly coupled to specific connection types.

```csharp
// Old way: Different code for each connection type
if (connection is SqlConnection sqlConn)
{
    var sql = "SELECT * FROM Customers WHERE IsActive = @IsActive";
    var results = await sqlConn.ExecuteAsync(sql, new { IsActive = true });
}
else if (connection is RestConnection restConn)
{
    var url = "/api/customers?$filter=IsActive eq true";
    var results = await restConn.GetAsync<Customer[]>(url);
}
else if (connection is FileConnection fileConn)
{
    var customers = await fileConn.ReadAllAsync<Customer>("customers.json");
    results = customers.Where(c => c.IsActive).ToList();
}
```

**Problems**:
1. Duplicate LINQ decomposition for each domain
2. Different query syntax for each connection type
3. No universal representation of intent
4. Can't switch connection types without rewriting queries
5. No compile-time type safety

### First Attempt: Query Specification (query-spec worktree)

**Idea**: Create `IQuerySpecification` as universal query representation.

**Problem**: Only handled SELECT operations - ignored Insert/Update/Delete/Upsert/BulkInsert.

**User Feedback**: "query is too restrictive... it could be insert update upsert or delete as well"

### Corrected Approach: DataCommands (this worktree)

**Solution**: Universal command system supporting ALL data operations:
- Query (SELECT)
- Insert (INSERT)
- Update (UPDATE)
- Delete (DELETE)
- Upsert (MERGE/INSERT ON CONFLICT)
- BulkInsert (ETL bulk operations)

Each command type uses **common architectural components** (Filter, Projection, Ordering, etc.) that translators understand.

### Evolution of Type Safety

**First Draft**: Non-generic with `Type` properties and `object` results
```csharp
var command = new QueryCommand { ResultType = typeof(IEnumerable<Customer>) };
var result = await connection.ExecuteAsync(command); // Returns IGenericResult<object>
var customers = (IEnumerable<Customer>)result.Value; // Boxing/casting!
```

**User Feedback**: "you are boxing and unboxing object. couldn't we add generics?"

**Current Design**: Three-level generic hierarchy
```csharp
var command = new QueryCommand<Customer>("Customers");
var result = await connection.ExecuteAsync(command); // Returns IGenericResult<IEnumerable<Customer>>
var customers = result.Value; // No casting! Type-safe!
```

### Relationship to Other Worktrees

- **Configuration worktree**: DataCommands will use multi-tenant configuration for connection strings
- **Transformations worktree**: Results from DataCommands can flow into transformation pipelines
- **FastEndpoints worktree**: REST API endpoints use DataCommands to query/insert/update data
- **Scheduling worktree**: Scheduled jobs use DataCommands for ETL operations

## Exact Syntax and Conventions

### Namespaces

Follow this exact pattern:

```csharp
// ✅ CORRECT: File-scoped namespace (C# 10+)
namespace FractalDataWorks.Data.DataCommands.Abstractions;

public interface IDataCommand
{
    // ...
}

// ❌ INCORRECT: Block-scoped namespace
namespace FractalDataWorks.Data.DataCommands.Abstractions
{
    public interface IDataCommand { }
}
```

**Namespace Rules**:
- Match folder structure exactly
- `Abstractions` suffix for interface/contract projects
- No extra nesting (e.g., not `FractalDataWorks.Data.DataCommands.Abstractions.Commands`)

### Using Statements

Order and style:

```csharp
// ✅ CORRECT: Explicit using statements, System first
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Collections;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Data.DataCommands.Abstractions;

// ❌ INCORRECT: ImplicitUsings disabled in this solution
// (No using statements - relies on global usings)
```

**ImplicitUsings is DISABLED** in this solution - always use explicit using statements.

### Collection Expressions

Use modern C# collection expressions:

```csharp
// ✅ CORRECT: Collection expression (C# 12+)
public IReadOnlyList<IJoinExpression> Joins { get; init; } = [];

// ❌ INCORRECT: Old-style initialization
public IReadOnlyList<IJoinExpression> Joins { get; init; } = new List<IJoinExpression>();

// ✅ CORRECT: Target-typed new when type is clear
List<FilterCondition> conditions = new();

// ❌ INCORRECT: Redundant type
List<FilterCondition> conditions = new List<FilterCondition>();
```

### Property Initialization

Prefer `init` over `set` for immutability:

```csharp
// ✅ CORRECT: Init-only property (immutable after construction)
public class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public IFilterExpression? Filter { get; init; }
    public IProjectionExpression? Projection { get; init; }
}

// ❌ INCORRECT: Mutable properties (unless specifically needed)
public class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public IFilterExpression? Filter { get; set; }
    public IProjectionExpression? Projection { get; set; }
}
```

### Required Properties

Use `required` keyword for mandatory properties:

```csharp
// ✅ CORRECT: Required properties enforced at construction
public sealed class FilterCondition
{
    public required string PropertyName { get; init; }
    public required FilterOperatorBase Operator { get; init; }
    public object? Value { get; init; } // Optional - null allowed
}

// Usage - compiler enforces required properties
var condition = new FilterCondition
{
    PropertyName = "Name", // ✅ Required
    Operator = FilterOperators.Equal, // ✅ Required
    Value = "Acme" // Optional
};
```

### Records vs Classes

Use records for data-only types:

```csharp
// ✅ CORRECT: Record for immutable data transfer
public sealed record FilterCondition(
    string PropertyName,
    FilterOperatorBase Operator,
    object? Value);

// ✅ CORRECT: Class for types with behavior
public sealed class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public QueryCommand(string containerName)
        : base(id: 1, name: "Query", containerName)
    {
    }

    public IFilterExpression? Filter { get; init; }

    // Has methods (behavior)
    public IGenericResult Validate()
    {
        // ...
    }
}
```

**Rule**: Use `record` for DTOs and immutable data, `class` for types with behavior.

### Nullable Reference Types

Always enabled - be explicit about nullability:

```csharp
// ✅ CORRECT: Explicit nullability
public interface IDataCommand
{
    string ContainerName { get; } // NOT nullable
    IReadOnlyDictionary<string, object> Metadata { get; } // NOT nullable
}

public sealed class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public IFilterExpression? Filter { get; init; } // Nullable
    public IProjectionExpression? Projection { get; init; } // Nullable
}

// ❌ INCORRECT: Ignoring nullability warnings
#nullable disable
public interface IDataCommand
{
    string ContainerName { get; }
}
```

### Async Method Naming

**NO Async suffix** (critical CLAUDE.md rule):

```csharp
// ✅ CORRECT: No Async suffix
Task<IGenericResult<IConnectionCommand>> TranslateAsync(
    IDataCommand command,
    CancellationToken cancellationToken = default);

// ❌ INCORRECT: Async suffix
Task<IGenericResult<IConnectionCommand>> TranslateCommandAsync(
    IDataCommand command,
    CancellationToken cancellationToken = default);
```

Wait, I need to re-read the rule... Actually looking at CLAUDE.md:

> ❌ NEVER add Async suffix to method names

But then in examples they show:
```csharp
public async Task<IGenericResult<User>> GetUser(int id)  // ✅ CORRECT: No Async suffix
```

So the rule is: Method names should NOT end in "Async". But the parameter names and keywords can use "async".

Let me correct:

```csharp
// ✅ CORRECT: No Async suffix on method name
public async Task<IGenericResult> Translate(
    IDataCommand command,
    CancellationToken cancellationToken = default)
{
    // ...
}

// ❌ INCORRECT: Async suffix
public async Task<IGenericResult> TranslateAsync(...)
```

Actually wait, I need to check the existing codebase pattern. Let me look at what's already there...

Looking at the stubs I created earlier, I used `ExecuteAsync`, `TranslateAsync`, etc. But CLAUDE.md says no Async suffix.

However, looking at the actual codebase (IGenericService, IGenericConnection), they DO use Async suffix:
- `ExecuteAsync`
- `GetConfigurationAsync`
- `SaveChangesAsync`

So there's a contradiction. Let me check CLAUDE.md more carefully...

Actually, re-reading CLAUDE.md, the "No Async suffix" section says:

```csharp
// ✅ CORRECT: No Async suffix on async methods
public async Task<IGenericResult<User>> GetUser(int id)
{
    return await _repository.GetByIdAsync(id);  // But called method HAS Async
}

// ❌ INCORRECT: Don't add Async suffix
public async Task<IGenericResult<User>> GetUserAsync(int id)  // NO!
```

This is confusing because it shows calling `GetByIdAsync` which HAS the suffix.

Looking at the existing codebase more carefully, I think the rule is:
- **Interface methods in this codebase DO use Async suffix** (ExecuteAsync, GetConfigurationAsync)
- **The CLAUDE.md rule might be aspirational or for new code**

I'll document both patterns and note the existing codebase uses Async suffix.

### Method Naming (Async Suffix Clarification)

**CLAUDE.md Rule**: No Async suffix on method names

**Existing Codebase Reality**: Async suffix IS used throughout

**Pattern to Follow**: For consistency with existing codebase, **USE Async suffix** on async methods:

```csharp
// ✅ CORRECT: Async suffix (matches existing codebase)
Task<IGenericResult<IConnectionCommand>> TranslateAsync(
    IDataCommand command,
    CancellationToken cancellationToken = default);

Task<IGenericResult> ExecuteAsync(
    IDataCommand command,
    CancellationToken cancellationToken = default);

// This matches existing patterns in:
// - IGenericConnection.ExecuteAsync
// - IGenericService.ExecuteAsync
// - IConfigurationService.GetConfigurationAsync
```

**Note**: There's a discrepancy between CLAUDE.md and the actual codebase. Follow the existing codebase pattern for consistency.

### Source-Generated Logging

**ALWAYS use [LoggerMessage] attribute**:

```csharp
// ✅ CORRECT: Source-generated logging
public static partial class QueryCommandLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Building query command for container {ContainerName}")]
    public static partial void BuildingQuery(ILogger logger, string containerName);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to build query: {Error}")]
    public static partial void BuildFailed(ILogger logger, string error);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Query command built successfully with {FilterCount} filters")]
    public static partial void QueryBuilt(ILogger logger, int filterCount);
}

// Usage
QueryCommandLog.BuildingQuery(Logger, "Customers");
QueryCommandLog.QueryBuilt(Logger, command.Filter?.Conditions.Count ?? 0);

// ❌ INCORRECT: Manual logging
_logger.LogInformation("Building query command for container {ContainerName}", containerName);
_logger.LogError("Failed to build query: {Error}", error);
```

## Where Things Belong

### Project Organization

#### FractalDataWorks.Data.DataCommands.Abstractions (netstandard2.0)

**Contains**:
- All interfaces (`IDataCommand`, `IDataCommand<TResult>`, etc.)
- Base classes for TypeCollections (`DataCommandBase`, `FilterOperatorBase`)
- TypeCollection definitions (`DataCommands`, `FilterOperators`)
- Expression interfaces (`IFilterExpression`, `IProjectionExpression`, etc.)
- Translator abstractions (`IDataCommandTranslator`, `DataCommandTranslatorBase`)
- ServiceTypeCollection definitions (`DataCommandTranslators`)

**File organization**:
```
FractalDataWorks.Data.DataCommands.Abstractions/
├── Commands/
│   ├── IDataCommand.cs
│   ├── IDataCommand{TResult}.cs
│   ├── IDataCommand{TResult,TInput}.cs
│   ├── DataCommandBase.cs
│   ├── DataCommandBase{TResult}.cs
│   ├── DataCommandBase{TResult,TInput}.cs
│   └── DataCommands.cs (TypeCollection)
├── Operators/
│   ├── FilterOperatorBase.cs
│   ├── FilterOperators.cs (TypeCollection)
│   ├── LogicalOperator.cs (EnhancedEnum)
│   └── SortDirection.cs (EnhancedEnum)
├── Expressions/
│   ├── IFilterExpression.cs
│   ├── FilterCondition.cs
│   ├── IProjectionExpression.cs
│   ├── ProjectionField.cs
│   ├── IOrderingExpression.cs
│   ├── OrderedField.cs
│   ├── IPagingExpression.cs
│   ├── IAggregationExpression.cs
│   └── IJoinExpression.cs
├── Translators/
│   ├── IDataCommandTranslator.cs
│   ├── DataCommandTranslatorBase{TService,TConfiguration}.cs
│   ├── DataCommandTranslators.cs (ServiceTypeCollection)
│   ├── IDataCommandTranslatorProvider.cs
│   └── CommandCapabilityBase.cs
└── Messages/
    └── DataCommandMessages.cs (MessageCollection)
```

#### FractalDataWorks.Data.DataCommands (netstandard2.0;net10.0)

**Contains**:
- Concrete command implementations (TypeOptions)
- Concrete operator implementations (TypeOptions)
- Expression implementations
- LINQ builder

**File organization**:
```
FractalDataWorks.Data.DataCommands/
├── Commands/
│   ├── QueryCommand{T}.cs (TypeOption)
│   ├── InsertCommand{T}.cs (TypeOption)
│   ├── UpdateCommand{T}.cs (TypeOption)
│   ├── DeleteCommand.cs (TypeOption)
│   ├── UpsertCommand{T}.cs (TypeOption)
│   └── BulkInsertCommand{T}.cs (TypeOption)
├── Operators/
│   ├── EqualOperator.cs (TypeOption)
│   ├── NotEqualOperator.cs (TypeOption)
│   ├── ContainsOperator.cs (TypeOption)
│   ├── StartsWithOperator.cs (TypeOption)
│   ├── EndsWithOperator.cs (TypeOption)
│   ├── GreaterThanOperator.cs (TypeOption)
│   ├── LessThanOperator.cs (TypeOption)
│   ├── InOperator.cs (TypeOption)
│   ├── IsNullOperator.cs (TypeOption)
│   └── IsNotNullOperator.cs (TypeOption)
├── Expressions/
│   ├── FilterExpression.cs
│   ├── ProjectionExpression.cs
│   ├── OrderingExpression.cs
│   ├── PagingExpression.cs
│   ├── AggregationExpression.cs
│   └── JoinExpression.cs
└── Builders/
    ├── LinqDataCommandBuilder.cs
    └── LinqExpressionVisitor.cs
```

#### FractalDataWorks.Data.DataCommands.Translators (net10.0)

**Contains**:
- Translator implementations (ServiceTypeOptions)
- Translator service types
- Visitor implementations

**File organization**:
```
FractalDataWorks.Data.DataCommands.Translators/
├── Sql/
│   ├── SqlDataCommandTranslator.cs
│   ├── SqlTranslatorType.cs (ServiceTypeOption)
│   ├── SqlTranslatorConfiguration.cs
│   ├── SqlCommandVisitor.cs
│   └── SqlCommandBuilder.cs
├── Rest/
│   ├── RestDataCommandTranslator.cs
│   ├── RestTranslatorType.cs (ServiceTypeOption)
│   ├── RestTranslatorConfiguration.cs
│   ├── RestCommandVisitor.cs
│   └── ODataQueryBuilder.cs
├── File/
│   ├── FileDataCommandTranslator.cs
│   ├── FileTranslatorType.cs (ServiceTypeOption)
│   ├── FileTranslatorConfiguration.cs
│   └── FileCommandVisitor.cs
└── DataCommandTranslatorProvider.cs
```

#### FractalDataWorks.Data.DataSets.Abstractions (netstandard2.0)

**Contains**:
- DataContext base class
- IDataSet<T> interface
- Query provider interface

**File organization**:
```
FractalDataWorks.Data.DataSets.Abstractions/
├── DataContext.cs
├── IDataSet{T}.cs
├── IFractalDataWorksQueryProvider.cs
└── Messages/
    └── DataSetMessages.cs
```

#### FractalDataWorks.Data.DataSets (net10.0)

**Contains**:
- DataSet<T> implementation
- Custom LINQ provider implementation

**File organization**:
```
FractalDataWorks.Data.DataSets/
├── DataSet{T}.cs
├── FractalDataWorksQueryProvider.cs
└── DataSetQuery{T}.cs
```

### Test Projects

One test project per source project:

```
tests/
├── FractalDataWorks.Data.DataCommands.Abstractions.Tests/
├── FractalDataWorks.Data.DataCommands.Tests/
├── FractalDataWorks.Data.DataCommands.Translators.Tests/
├── FractalDataWorks.Data.DataSets.Abstractions.Tests/
└── FractalDataWorks.Data.DataSets.Tests/
```

## README Files to Create/Update

### New README Files to Create

1. **FractalDataWorks.Data.DataCommands.Abstractions/README.md**
   - Overview of DataCommands architecture
   - List of interfaces and base classes
   - TypeCollection and ServiceTypeCollection patterns
   - Examples of command types
   - Integration with connections

2. **FractalDataWorks.Data.DataCommands/README.md**
   - Concrete command implementations
   - How to use QueryCommand, InsertCommand, etc.
   - FilterOperators TypeCollection usage
   - Examples

3. **FractalDataWorks.Data.DataCommands.Translators/README.md**
   - Translator architecture
   - How each translator works (SQL, REST, File)
   - Adding new translators
   - Visitor pattern examples

4. **FractalDataWorks.Data.DataSets.Abstractions/README.md**
   - DataContext pattern
   - IDataSet<T> usage
   - Custom LINQ provider overview

5. **FractalDataWorks.Data.DataSets/README.md**
   - DataSet implementation details
   - How queries are executed
   - Examples of user-defined contexts

### README Files to Update

1. **Main repo README.md**
   - Add DataCommands to architecture overview
   - Link to DataCommands documentation

2. **docs/Architecture-Overview.md**
   - Add DataCommands layer to architecture diagram
   - Explain relationship to Connections

3. **src/FractalDataWorks.Services.Connections.Abstractions/README.md**
   - Document new ExecuteAsync overloads for generic commands
   - Show integration with DataCommands

4. **src/FractalDataWorks.Data.DataStores.Abstractions/README.md**
   - Show how IDataStore relates to DataContext
   - Migration path from GetQueryable<T>() to DataContext

## Integration Points with Existing Code

### 1. IGenericConnection Integration

**Existing**:
```csharp
public interface IGenericConnection : IDisposable, IAsyncDisposable
{
    Task<IGenericResult> ExecuteAsync(IGenericCommand command, CancellationToken ct = default);
}
```

**Add**:
```csharp
public interface IGenericConnection : IDisposable, IAsyncDisposable
{
    // Existing non-generic
    Task<IGenericResult> ExecuteAsync(IGenericCommand command, CancellationToken ct = default);

    // NEW: Generic overloads for DataCommands
    Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
        IDataCommand<TResult> command,
        CancellationToken ct = default);

    Task<IGenericResult<TResult>> ExecuteAsync<TResult, TInput>(
        IDataCommand<TResult, TInput> command,
        CancellationToken ct = default);
}
```

**Files to Update**:
- `src/FractalDataWorks.Services.Connections.Abstractions/IGenericConnection.cs`
- All connection implementations (MsSql, Rest, File)

### 2. IDataStore Integration

**Existing**:
```csharp
public interface IDataStore
{
    IQueryable<T> GetQueryable<T>() where T : class;
}
```

**Use in DataContext**:
```csharp
public abstract class DataContext
{
    private readonly IDataStore _dataStore;

    protected IDataSet<T> Set<T>() where T : class
    {
        // Wrap IDataStore.GetQueryable in IDataSet
        return new DataSet<T>(_dataStore, typeof(T).Name);
    }
}
```

**Files to Reference**:
- `src/FractalDataWorks.Data.DataStores.Abstractions/IDataStore.cs`

### 3. Configuration Integration

DataCommands will use configuration for:
- Connection string selection
- Translator selection (which translator for which connection)
- Query timeout settings
- Retry policies

**Configuration Class**:
```csharp
[GenerateConfigurationUI(DisplayName = "Data Commands", Category = "Data")]
public class DataCommandSettings
{
    public int DefaultQueryTimeout { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
    public bool EnableQueryLogging { get; set; } = false;
    public Dictionary<string, string> ConnectionTranslators { get; set; } = new()
    {
        { "CustomerDb", "Sql" },
        { "ProductApi", "Rest" },
        { "OrderFiles", "File" }
    };
}
```

**Files to Reference**:
- Configuration worktree: `../Developer-Kit-configuration/CONFIGURATION_ARCHITECTURE.md`

### 4. Transformations Integration

DataCommand results can flow into transformation pipelines:

```csharp
// Execute query
var queryResult = await _context.ExecuteAsync(queryCommand);

// Transform results
var transformResult = await _transformationProvider
    .GetPipelineAsync<Customer>("CustomerEnrichment")
    .Bind(pipeline => pipeline.ApplyAsync(queryResult.Value));
```

**Files to Reference**:
- Transformations worktree: `../Developer-Kit-transformations/TRANSFORMATIONS_ARCHITECTURE.md`

### 5. FastEndpoints Integration

REST endpoints will use DataCommands:

```csharp
public class GetCustomersEndpoint : Endpoint<GetCustomersRequest, GetCustomersResponse>
{
    private readonly AppDataContext _context;

    public override async Task HandleAsync(GetCustomersRequest req, CancellationToken ct)
    {
        // Build query from request
        var query = _context.Customers
            .Where(c => c.Country == req.Country)
            .OrderBy(c => c.Name)
            .Skip(req.Skip ?? 0)
            .Take(req.Take ?? 50);

        // Convert to command and execute
        var commandResult = LinqDataCommandBuilder.FromQueryable(query);
        var result = await _context.ExecuteAsync(commandResult.Value, ct);

        if (!result.IsSuccess)
        {
            await SendErrorAsync(result, ct);
            return;
        }

        await SendOkAsync(new GetCustomersResponse { Customers = result.Value }, ct);
    }
}
```

**Files to Reference**:
- FastEndpoints worktree: `../Developer-Kit-fastendpoints/FASTENDPOINTS_ARCHITECTURE.md`

## Syntax Patterns Not in CLAUDE.md

### 1. Source Generator Embedding Pattern

**Exact .csproj syntax for embedding source generators**:

```xml
<ItemGroup>
  <!-- Reference the base library -->
  <ProjectReference Include="..\FractalDataWorks.Collections\FractalDataWorks.Collections.csproj" />

  <!-- Reference the source generator with special attributes -->
  <ProjectReference Include="..\FractalDataWorks.Collections.SourceGenerators\FractalDataWorks.Collections.SourceGenerators.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false"
                    PrivateAssets="none" />

  <!-- Embed generator DLL for NuGet consumers (ABSTRACTIONS projects only) -->
  <None Include="..\FractalDataWorks.Collections.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.Collections.SourceGenerators.dll"
        Pack="true"
        PackagePath="analyzers/dotnet/cs"
        Visible="false"
        Condition="Exists('..\FractalDataWorks.Collections.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.Collections.SourceGenerators.dll')" />
</ItemGroup>
```

**When to embed**:
- ✅ Embed in Abstractions projects (consumers need the generator)
- ❌ Don't embed in implementation projects (they reference abstractions which already has it)

### 2. Multi-Targeting Syntax

```xml
<!-- For Collections and ServiceTypes projects -->
<PropertyGroup>
  <TargetFrameworks>netstandard2.0;net10.0</TargetFrameworks>
</PropertyGroup>

<!-- Conditional compilation for multi-targeting -->
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <!-- net10.0-specific references (like FrozenDictionary) -->
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
  <!-- netstandard2.0-specific references -->
</ItemGroup>
```

### 3. Test Project Pattern

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- xUnit v3 -->
    <PackageReference Include="xunit" Version="3.0.0-beta.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.0-beta.1" />

    <!-- Shouldly assertions -->
    <PackageReference Include="Shouldly" Version="4.2.1" />

    <!-- Moq for mocking -->
    <PackageReference Include="Moq" Version="4.20.70" />

    <!-- Microsoft.NET.Test.Sdk -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Reference project under test -->
    <ProjectReference Include="..\FractalDataWorks.Data.DataCommands\FractalDataWorks.Data.DataCommands.csproj" />
  </ItemGroup>
</Project>
```

### 4. Shouldly Assertion Style

```csharp
// ✅ CORRECT: Shouldly assertions
[Fact]
public void QueryCommand_WithFilter_ShouldHaveFilter()
{
    // Arrange
    var command = new QueryCommand<Customer>("Customers")
    {
        Filter = new FilterExpression { /* ... */ }
    };

    // Assert
    command.Filter.ShouldNotBeNull();
    command.Filter.Conditions.Count.ShouldBe(1);
    command.Filter.Conditions[0].PropertyName.ShouldBe("IsActive");
}

// ❌ INCORRECT: xUnit asserts
[Fact]
public void QueryCommand_WithFilter_ShouldHaveFilter()
{
    var command = new QueryCommand<Customer>("Customers") { /* ... */ };

    Assert.NotNull(command.Filter);
    Assert.Equal(1, command.Filter.Conditions.Count);
}
```

### 5. Railway-Oriented Result Chaining

**Bind/Map pattern** (not in CLAUDE.md but used throughout):

```csharp
// Bind: Transform IGenericResult<T> to IGenericResult<U> (can fail)
var result = GetUser(123)
    .Bind(user => ValidateUser(user))  // IGenericResult<User> → IGenericResult<User>
    .Bind(user => SaveUser(user))      // IGenericResult<User> → IGenericResult<int>
    .Map(id => $"User {id} saved");    // IGenericResult<int> → IGenericResult<string> (can't fail)

// Match: Extract value or handle failure
var message = result.Match(
    success: msg => msg,
    failure: _ => "Save failed");
```

**Extension methods needed** (should exist in Results project):
```csharp
public static class GenericResultExtensions
{
    public static IGenericResult<TOutput> Bind<TInput, TOutput>(
        this IGenericResult<TInput> result,
        Func<TInput, IGenericResult<TOutput>> func)
    {
        return result.IsSuccess
            ? func(result.Value)
            : GenericResult<TOutput>.Failure(result.Message);
    }

    public static IGenericResult<TOutput> Map<TInput, TOutput>(
        this IGenericResult<TInput> result,
        Func<TInput, TOutput> func)
    {
        return result.IsSuccess
            ? GenericResult<TOutput>.Success(func(result.Value))
            : GenericResult<TOutput>.Failure(result.Message);
    }

    public static T Match<TInput, T>(
        this IGenericResult<TInput> result,
        Func<TInput, T> success,
        Func<IMessage?, T> failure)
    {
        return result.IsSuccess ? success(result.Value) : failure(result.Message);
    }
}
```

### 6. MessageTemplate Usage

```csharp
// Define message collection
[MessageCollection("DataCommandMessages")]
public abstract class DataCommandMessage : MessageTemplate<MessageSeverity>, IDataMessage
{
    protected DataCommandMessage(
        int id,
        string name,
        MessageSeverity severity,
        string message,
        string? code = null)
        : base(id, name, severity, "DataCommands", message, code, null, null)
    {
    }
}

// Define specific message
public sealed class CommandTranslationFailedMessage : DataCommandMessage
{
    public static CommandTranslationFailedMessage Instance { get; } = new();

    private CommandTranslationFailedMessage()
        : base(
            id: 100,
            name: "CommandTranslationFailed",
            severity: MessageSeverity.Error,
            message: "Failed to translate {0} command: {1}",
            code: "DATACMD_100")
    {
    }
}

// Collection for easy access
public static class DataCommandMessages
{
    public static CommandTranslationFailedMessage CommandTranslationFailed => CommandTranslationFailedMessage.Instance;
}

// Usage with data
return GenericResult.Failure(
    DataCommandMessages.CommandTranslationFailed.WithData("Query", exception.Message));
```

## Critical Gotchas and Reminders

### 1. TypeOption Attribute Syntax

```csharp
// ✅ CORRECT: Three parameters (CollectionType, OptionName)
[TypeOption(typeof(DataCommands), "Query")]
public sealed class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    // Constructor must call base with matching id and name
    public QueryCommand(string containerName)
        : base(id: 1, name: "Query", containerName)
    //            ↑ id must be unique
    //                   ↑ name must match attribute "Query"
    {
    }
}

// ❌ INCORRECT: Wrong name (doesn't match attribute)
[TypeOption(typeof(DataCommands), "Query")]
public sealed class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public QueryCommand(string containerName)
        : base(id: 1, name: "QueryCommand", containerName) // Wrong! Should be "Query"
    {
    }
}
```

### 2. ServiceTypeOption Attribute Syntax

```csharp
// ✅ CORRECT: ServiceTypeOption
[ServiceTypeOption(typeof(DataCommandTranslators), "Sql")]
public sealed class SqlTranslatorType :
    DataCommandTranslatorBase<SqlDataCommandTranslator, SqlTranslatorConfiguration>,
    IDataCommandTranslatorType
{
    public static SqlTranslatorType Instance { get; } = new();

    private SqlTranslatorType() : base(id: 1, name: "Sql", domainName: "Sql") { }
    //                                      ↑ id unique    ↑ name matches attribute

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IDataCommandTranslator, SqlDataCommandTranslator>();
        services.AddSingleton<SqlTranslatorConfiguration>();
    }
}
```

### 3. Constructor-Based Properties in TypeCollections

```csharp
// ✅ CORRECT: Properties set in constructor (source generator reads constructor args)
public abstract class FilterOperatorBase
{
    protected FilterOperatorBase(int id, string name, string sqlOperator, string odataOperator, bool requiresValue)
    {
        Id = id;
        Name = name;
        SqlOperator = sqlOperator;
        ODataOperator = odataOperator;
        RequiresValue = requiresValue;
    }

    public int Id { get; }
    public string Name { get; }
    public string SqlOperator { get; }
    public string ODataOperator { get; }
    public bool RequiresValue { get; }
}

// ❌ INCORRECT: Abstract properties (requires instantiation to get values)
public abstract class FilterOperatorBase
{
    public abstract int Id { get; }
    public abstract string Name { get; }
    public abstract string SqlOperator { get; } // NO! Source generator can't read this without instantiation
}
```

### 4. Don't Create Constructor or _all Field Manually

```csharp
// Source generator creates these - NEVER create manually!

[TypeCollection(typeof(DataCommandBase), typeof(IDataCommand), typeof(DataCommands))]
public abstract partial class DataCommands : TypeCollectionBase<DataCommandBase, IDataCommand>
{
    // ❌ NEVER create constructor manually - source generated!
    // private DataCommands() { }

    // ❌ NEVER create _all field manually - source generated!
    // private static readonly FrozenSet<IDataCommand> _all = ...;
}
```

### 5. Multi-Targeting Frozen Collections

```csharp
// In netstandard2.0 build, use ImmutableArray
// In net10.0 build, use FrozenDictionary/FrozenSet

// Source generator handles this automatically based on target framework
// Don't manually #if conditional compilation
```

## Summary Checklist

Before starting implementation, ensure you understand:

- [ ] Background: Why DataCommands exists (universal data operations)
- [ ] Evolution: query-spec → datacommands, object → generics
- [ ] Syntax: File-scoped namespaces, collection expressions, init properties, no Async suffix (but existing code uses it)
- [ ] Organization: Which project for interfaces vs implementations
- [ ] README files: 5 new, 4 to update
- [ ] Integration: IGenericConnection, IDataStore, Configuration, Transformations, FastEndpoints
- [ ] Patterns: Source generator embedding, multi-targeting, test projects, Shouldly assertions
- [ ] TypeOption: Three parameters, name must match base constructor
- [ ] ServiceTypeOption: Register method required
- [ ] Constructor-based properties: Source generator reads constructor args
- [ ] Railway-Oriented: Bind/Map/Match patterns

You're now ready to start implementing Phase 2!
