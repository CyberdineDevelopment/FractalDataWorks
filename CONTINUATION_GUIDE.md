# DataCommands Architecture - Continuation Guide

## Purpose of This Document

This document contains everything needed to continue implementing the DataCommands architecture in a new chat session. Read this first to understand the current state, key decisions, and next steps.

## Current State Summary

This worktree (`feature/datacommands-architecture` branch) contains the **complete architectural design** for DataCommands - a universal data command system that works across all connection types (SQL, REST, File, GraphQL).

**Status**: Architecture design complete, ready for implementation.

## Key Documents in This Worktree

### 1. DATACOMMANDS_ARCHITECTURE.md (Core Architecture)
**Purpose**: The main architectural proposal showing the corrected approach.

**Key Points**:
- Replaces "Query Specification" terminology (which was too restrictive)
- Supports ALL data operations: Query, Insert, Update, Delete, Upsert, BulkInsert
- Uses TypeCollection pattern with DataCommands
- Each operation is a TypeOption with common architectural components
- Translators convert universal IDataCommand to domain-specific IConnectionCommand

**Critical Correction**: This supersedes the query-spec worktree which only handled SELECT operations.

### 2. TYPECOLLECTIONS_FIX.md (Compliance Fix)
**Purpose**: Fixes violations of CLAUDE.md rules in the original stubs.

**Problems Fixed**:
- ❌ Enums → ✅ TypeCollections/EnhancedEnums
- ❌ Switch statements → ✅ Property access and visitor pattern
- ❌ Manual translator registration → ✅ ServiceTypeCollection
- ❌ `[Flags]` enum for capabilities → ✅ TypeCollection + HashSet

**Key Pattern**: `FilterOperator` enum replaced with `FilterOperators` TypeCollection where each operator knows its own SQL, OData, and parameter formatting.

### 3. GENERIC_DATACOMMANDS_ULTRATHINK.md (Type Safety)
**Purpose**: Addresses boxing/unboxing and adds compile-time type safety with generics.

**Key Innovation**: Three-level generic hierarchy:
```csharp
IDataCommand                           // Non-generic marker for TypeCollection
IDataCommand<TResult>                  // Typed result
IDataCommand<TResult, TInput>          // Typed input and result
```

**Examples**:
- `QueryCommand<Customer>` → returns `IEnumerable<Customer>` (no casting!)
- `InsertCommand<Customer>` → accepts `Customer`, returns `int` (identity)
- `BulkInsertCommand<Customer>` → accepts `IEnumerable<Customer>`, returns `BulkInsertResult`

**Execute Overloads**:
```csharp
Task<IGenericResult<object>> ExecuteAsync(IDataCommand command)
Task<IGenericResult<TResult>> ExecuteAsync<TResult>(IDataCommand<TResult> command)
Task<IGenericResult<TResult>> ExecuteAsync<TResult, TInput>(IDataCommand<TResult, TInput> command)
```

**Benefits**: Zero boxing/unboxing, compile-time type safety, full IntelliSense, type inference from LINQ.

### 4. QUERYABLE_ENTRY_POINT.md (User-Facing API)
**Purpose**: Defines how users write LINQ queries (the "DbContext" equivalent).

**Recommended Approach**: `DataContext` with `IDataSet<T>` properties:
```csharp
public class AppDataContext : DataContext
{
    public IDataSet<Customer> Customers => Set<Customer>();
    public IDataSet<Order> Orders => Set<Order>();
}

// Usage - identical to EF Core!
var query = _context.Customers
    .Where(c => c.IsActive)
    .OrderBy(c => c.Name)
    .Skip(0)
    .Take(50);

// Behind the scenes: converts to QueryCommand<Customer> and executes
```

**Key Components**:
- `DataContext` - Base class for user-defined contexts
- `IDataSet<T>` - Collection-like API implementing `IQueryable<T>`
- `FractalDataWorksQueryProvider` - Custom LINQ provider that builds DataCommands
- Works with ANY connection type (SQL, REST, File, GraphQL)

## Critical Architectural Decisions

### 1. TypeCollection for Command Types
**Decision**: Use non-generic `DataCommandBase` for TypeCollection, but actual command instances are generic.

**Rationale**: TypeCollection source generator requires non-generic base, but we want type-safe generic commands.

**Pattern**:
```csharp
// Non-generic base for TypeCollection
[TypeCollection(typeof(DataCommandBase), typeof(IDataCommand), typeof(DataCommands))]
public abstract partial class DataCommands : TypeCollectionBase<DataCommandBase, IDataCommand>
{
    // Source generator creates static properties for command types
}

// Generic instances for actual use
var command = new QueryCommand<Customer>("Customers") { ... };
```

### 2. Generics Over Object
**Decision**: Use `IDataCommand<TResult>` and `IDataCommand<TResult, TInput>` instead of `Type` properties and `object`.

**Rationale**: Eliminates boxing/unboxing, provides compile-time type safety, enables IntelliSense.

**Impact**: Execute methods have three overloads for type inference.

### 3. TypeCollections Over Enums
**Decision**: Replace ALL enums with TypeCollections or EnhancedEnums.

**Examples**:
- `FilterOperator` enum → `FilterOperators` TypeCollection
- `LogicalOperator` enum → `LogicalOperator` EnhancedEnum (only 2 values)
- `SortDirection` enum → `SortDirection` EnhancedEnum

**Rationale**: Follows CLAUDE.md rules - no enums, no switch statements, properties instead.

### 4. Visitor Pattern for Command Dispatch
**Decision**: Use visitor pattern instead of switch statements on command type.

**Pattern**:
```csharp
public interface ISqlCommandVisitor
{
    Task<IGenericResult<IConnectionCommand>> VisitQuery(QueryCommand command, CancellationToken ct);
    Task<IGenericResult<IConnectionCommand>> VisitInsert(InsertCommand command, CancellationToken ct);
    // ... etc
}

// Commands accept visitors
public interface IDataCommand
{
    Task<IGenericResult<IConnectionCommand>> AcceptAsync<TVisitor>(TVisitor visitor, CancellationToken ct);
}
```

### 5. ServiceTypeCollection for Translators
**Decision**: Use ServiceTypeCollection for automatic translator registration.

**Pattern**:
```csharp
[ServiceTypeCollection(typeof(DataCommandTranslatorBase<,>), typeof(IDataCommandTranslator), typeof(DataCommandTranslators))]
public static partial class DataCommandTranslators
{
    // Source generator creates RegisterAll(IServiceCollection) method
}

// Registration
DataCommandTranslators.RegisterAll(services);
```

### 6. DataContext for User-Facing API
**Decision**: Provide EF Core-like API with `DataContext` and `IDataSet<T>`.

**Rationale**: Familiar to most .NET developers, works across all connection types.

## Key Patterns from CLAUDE.md

### Never Do:
- ❌ Enums (use TypeCollection or EnhancedEnum)
- ❌ Switch statements (use properties or visitor pattern)
- ❌ Manual service registration (use ServiceTypeCollection)
- ❌ ArgumentNullException (check null, return Result with message)
- ❌ Async suffix on method names
- ❌ Core suffix pattern (e.g., `ValidateCore` called by `Validate`)
- ❌ String literals (use `nameof` and `typeof`)
- ❌ Manual logging (use source-generated `[LoggerMessage]`)
- ❌ Exceptions for anticipated conditions (use Results)

### Always Do:
- ✅ Railway-Oriented Programming (return `IGenericResult` or `IGenericResult<T>`)
- ✅ TypeCollections for fixed sets
- ✅ EnhancedEnums for simple fixed sets (2-5 values)
- ✅ Properties instead of switch statements
- ✅ ServiceTypeCollection for DI registration
- ✅ Source-generated logging with `[LoggerMessage]`
- ✅ Messages from MessageTemplate collections
- ✅ Constructor-based properties in TypeCollections (not abstract properties)

### Target Frameworks:
- Abstractions projects: `netstandard2.0` ONLY
- Collections/ServiceTypes: Multi-target `netstandard2.0;net10.0`
- SourceGenerators: `netstandard2.0` ONLY
- Implementation projects: `net10.0`

## Architecture Overview

### Flow Diagram

```
User Code
    ↓
DataContext.Customers.Where(...) (LINQ)
    ↓
FractalDataWorksQueryProvider (custom LINQ provider)
    ↓
LinqDataCommandBuilder.FromQueryable(query)
    ↓
QueryCommand<Customer> (generic, type-safe)
    ↓
IDataConnection.ExecuteAsync<IEnumerable<Customer>>(command)
    ↓
DataCommandTranslator (SQL/REST/File/GraphQL)
    ↓
IConnectionCommand (domain-specific: SQL string, REST URL, etc.)
    ↓
Connection.ExecuteAsync(connectionCommand)
    ↓
IGenericResult<IEnumerable<Customer>> (no casting!)
```

### Project Structure (To Be Created)

```
FractalDataWorks.Data.DataCommands.Abstractions/
├── IDataCommand.cs (non-generic)
├── IDataCommand{TResult}.cs
├── IDataCommand{TResult,TInput}.cs
├── DataCommandBase.cs (for TypeCollection)
├── DataCommandBase{TResult}.cs
├── DataCommandBase{TResult,TInput}.cs
├── DataCommands.cs (TypeCollection)
├── FilterOperatorBase.cs
├── FilterOperators.cs (TypeCollection)
├── LogicalOperator.cs (EnhancedEnum)
├── SortDirection.cs (EnhancedEnum)
├── IFilterExpression.cs
├── IProjectionExpression.cs
├── IOrderingExpression.cs
├── IPagingExpression.cs
├── IAggregationExpression.cs
├── IJoinExpression.cs
├── IDataCommandTranslator.cs
├── DataCommandTranslatorBase.cs
├── DataCommandTranslators.cs (ServiceTypeCollection)
└── ... (other abstractions)

FractalDataWorks.Data.DataCommands/
├── QueryCommand{T}.cs (TypeOption)
├── InsertCommand{T}.cs (TypeOption)
├── UpdateCommand{T}.cs (TypeOption)
├── DeleteCommand.cs (TypeOption)
├── UpsertCommand{T}.cs (TypeOption)
├── BulkInsertCommand{T}.cs (TypeOption)
├── FilterExpression.cs
├── ProjectionExpression.cs
├── ... (implementations)
└── LinqDataCommandBuilder.cs

FractalDataWorks.Data.DataCommands.Translators/
├── SqlDataCommandTranslator.cs (ServiceTypeOption)
├── SqlTranslatorType.cs
├── RestDataCommandTranslator.cs (ServiceTypeOption)
├── RestTranslatorType.cs
├── FileDataCommandTranslator.cs (ServiceTypeOption)
├── FileTranslatorType.cs
└── ... (translator implementations)

FractalDataWorks.Data.DataSets.Abstractions/
├── IDataSet{T}.cs
├── DataContext.cs
└── FractalDataWorksQueryProvider.cs

FractalDataWorks.Data.DataSets/
├── DataSet{T}.cs
└── ... (implementations)
```

## Implementation Phases

### Phase 1: Core Abstractions ✅ (Designed)
- [x] Define generic command interfaces
- [x] Define command base classes
- [x] Design TypeCollection pattern for commands
- [x] Design FilterOperators TypeCollection
- [x] Design LogicalOperator/SortDirection EnhancedEnums
- [x] Define IDataCommandTranslator interface
- [x] Design ServiceTypeCollection for translators

**Next**: Implement these in actual code.

### Phase 2: Concrete Commands (Not Started)
- [ ] Implement `QueryCommand<T>`
- [ ] Implement `InsertCommand<T>`
- [ ] Implement `UpdateCommand<T>`
- [ ] Implement `DeleteCommand`
- [ ] Implement `UpsertCommand<T>`
- [ ] Implement `BulkInsertCommand<T>`
- [ ] Add `[TypeOption]` attributes to each
- [ ] Verify source generator creates `DataCommands` collection

### Phase 3: Expression Components (Not Started)
- [ ] Implement `FilterExpression` and `FilterCondition`
- [ ] Implement `ProjectionExpression`
- [ ] Implement `OrderingExpression`
- [ ] Implement `PagingExpression`
- [ ] Implement `AggregationExpression`
- [ ] Implement `JoinExpression`
- [ ] Implement FilterOperators TypeOptions (Equal, NotEqual, Contains, etc.)

### Phase 4: LINQ Builder (Not Started)
- [ ] Implement `LinqDataCommandBuilder`
- [ ] Implement `LinqExpressionVisitor` to decompose LINQ
- [ ] Handle Where clauses → FilterExpression
- [ ] Handle Select clauses → ProjectionExpression
- [ ] Handle OrderBy clauses → OrderingExpression
- [ ] Handle Skip/Take → PagingExpression
- [ ] Handle GroupBy → AggregationExpression
- [ ] Handle Join clauses → JoinExpression
- [ ] Add type inference for `QueryCommand<T>`

### Phase 5: Translators (Not Started)
- [ ] Implement `SqlDataCommandTranslator`
- [ ] Implement SQL visitor pattern
- [ ] Implement `RestDataCommandTranslator`
- [ ] Implement REST visitor pattern
- [ ] Implement `FileDataCommandTranslator`
- [ ] Add `[ServiceTypeOption]` attributes
- [ ] Verify source generator creates `DataCommandTranslators.RegisterAll()`

### Phase 6: DataContext and DataSets (Not Started)
- [ ] Implement `DataContext` base class
- [ ] Implement `IDataSet<T>` interface
- [ ] Implement `DataSet<T>` class
- [ ] Implement `FractalDataWorksQueryProvider` (custom LINQ provider)
- [ ] Handle `IQueryable<T>` execution
- [ ] Integrate with LinqDataCommandBuilder
- [ ] Test end-to-end with user-defined context

### Phase 7: IDataConnection Integration (Not Started)
- [ ] Add generic `ExecuteAsync<TResult>` overload to IDataConnection
- [ ] Add generic `ExecuteAsync<TResult, TInput>` overload
- [ ] Update existing connections to implement new overloads
- [ ] Test with SQL connection
- [ ] Test with REST connection
- [ ] Test with File connection

### Phase 8: Testing (Not Started)
- [ ] Unit tests for each command type
- [ ] Unit tests for FilterOperators TypeCollection
- [ ] Unit tests for LINQ decomposition
- [ ] Unit tests for translators
- [ ] Integration tests with real connections
- [ ] End-to-end tests with DataContext

### Phase 9: Documentation (Not Started)
- [ ] API documentation (XML comments)
- [ ] Usage examples
- [ ] Migration guide from query-spec (if anyone used it)
- [ ] Performance benchmarks (vs EF Core, vs raw SQL)

## Open Questions and Considerations

### 1. Change Tracking
**Question**: Should `DataContext` support change tracking like EF Core?

**Options**:
- **No tracking**: User must explicitly call Add/Update/Remove (simpler, explicit)
- **Automatic tracking**: Track entities loaded from queries, detect changes on SaveChanges (complex, EF Core-like)

**Recommendation**: Start without change tracking (simpler), add later if needed.

### 2. Primary Key Discovery
**Question**: How do we identify primary key properties for Update/Delete operations?

**Options**:
- **Convention**: Assume property named "Id" or "{TypeName}Id"
- **Attribute**: `[PrimaryKey]` attribute on property
- **Source Generator**: Generate metadata for entities at compile-time
- **Manual configuration**: User configures in DataContext

**Recommendation**: Start with convention (Id property), add attribute support later.

### 3. Navigation Properties
**Question**: Should we support navigation properties (relationships)?

**Options**:
- **No support**: User writes explicit joins (simpler)
- **Manual joins**: User adds Join expressions to QueryCommand
- **Automatic joins**: Detect navigation properties, auto-generate joins (EF Core-like)

**Recommendation**: Start without navigation support, add explicit Join support first.

### 4. Async Enumeration
**Question**: Should IDataSet<T> support `IAsyncEnumerable<T>`?

**Current**: `IQueryable<T>` is synchronous enumeration
**Option**: Add `IAsyncQueryable<T>` or `ToAsyncEnumerable()` extension

**Recommendation**: Add async enumeration support for large result sets.

### 5. Multiple Result Sets
**Question**: How to handle stored procedures or queries that return multiple result sets?

**Options**:
- **Not supported**: Commands return single result type
- **Tuple results**: `IDataCommand<(T1, T2, T3)>` with tuple results
- **Custom result class**: User defines class with multiple properties

**Recommendation**: Not supported initially, add via custom result class later.

### 6. Bulk Update/Delete
**Question**: Should we add `BulkUpdateCommand<T>` and `BulkDeleteCommand`?

**Current**: Only BulkInsert is defined

**Recommendation**: Add BulkUpdate and BulkDelete for consistency.

### 7. Transactions
**Question**: How should transactions work across multiple commands?

**Options**:
- **Per-connection**: Use IDataConnection.BeginTransaction()
- **Per-context**: DataContext.BeginTransaction()
- **Implicit**: SaveChanges wraps all queued commands in transaction

**Recommendation**: Start with per-connection, add per-context later.

## Common Patterns to Follow

### Creating a New Command Type

```csharp
// 1. Define generic command class
[TypeOption(typeof(DataCommands), "MyCommand")]
public sealed class MyCommand<TInput, TResult> : DataCommandBase<TResult, TInput>
{
    public MyCommand(string containerName, TInput data)
        : base(id: 7, name: "MyCommand", containerName, data)
    {
    }

    // Add command-specific properties
    public MyCommandOptions Options { get; init; } = new();
}

// 2. Implement visitor method in translators
public async Task<IGenericResult<IConnectionCommand>> VisitMyCommand<TInput, TResult>(
    MyCommand<TInput, TResult> command,
    CancellationToken ct)
{
    // Build domain-specific command
}
```

### Creating a New Operator

```csharp
// Define operator as TypeOption
[TypeOption(typeof(FilterOperators), "Between")]
public sealed class BetweenOperator : FilterOperatorBase
{
    public BetweenOperator() : base(
        id: 15,
        name: "Between",
        sqlOperator: "BETWEEN",
        odataOperator: "between", // (if OData supports it)
        requiresValue: true)
    {
    }

    public override string FormatSqlParameter(string paramName) => $"@{paramName}_min AND @{paramName}_max";

    public override string FormatODataValue(object? value)
    {
        // Handle tuple or range value
        if (value is (object min, object max))
            return $"{min} and {max}";
        return base.FormatODataValue(value);
    }
}
```

### Creating a New Translator

```csharp
// 1. Define service type
[ServiceTypeOption(typeof(DataCommandTranslators), "GraphQL")]
public sealed class GraphQLTranslatorType :
    DataCommandTranslatorBase<GraphQLDataCommandTranslator, GraphQLTranslatorConfiguration>,
    IDataCommandTranslatorType
{
    public static GraphQLTranslatorType Instance { get; } = new();

    private GraphQLTranslatorType() : base(id: 4, name: "GraphQL", domainName: "GraphQL") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IDataCommandTranslator, GraphQLDataCommandTranslator>();
        services.AddSingleton<GraphQLTranslatorConfiguration>();
    }
}

// 2. Implement translator
public sealed class GraphQLDataCommandTranslator : IDataCommandTranslator
{
    public string DomainName => "GraphQL";

    public async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext containerContext,
        CancellationToken ct)
    {
        // Implement visitor pattern
        var visitor = new GraphQLCommandVisitor(containerContext);
        return await command.AcceptAsync(visitor, ct);
    }
}
```

## Testing Strategy

### Unit Tests
- Test each command type individually
- Test FilterOperators TypeCollection (all operators)
- Test LINQ expression decomposition
- Test translator output (SQL, REST, File)
- Mock connections for testing

### Integration Tests
- Test with real SQL connection
- Test with real REST API
- Test with real file system
- Test DataContext end-to-end
- Test type inference from LINQ

### Performance Tests
- Benchmark LINQ decomposition time
- Benchmark translation time
- Compare to EF Core query performance
- Compare to raw SQL performance
- Measure boxing/unboxing overhead (should be zero)

## Next Steps to Start Implementation

1. **Create Project Structure**:
   - Create `FractalDataWorks.Data.DataCommands.Abstractions` project (netstandard2.0)
   - Create `FractalDataWorks.Data.DataCommands` project (netstandard2.0;net10.0)
   - Add references to Collections, ServiceTypes, Results, Messages

2. **Implement Core Abstractions**:
   - Start with `IDataCommand`, `IDataCommand<TResult>`, `IDataCommand<TResult, TInput>`
   - Add `DataCommandBase` classes
   - Create `DataCommands` TypeCollection
   - Test source generator creates collection

3. **Implement FilterOperators**:
   - Create `FilterOperatorBase`
   - Create `FilterOperators` TypeCollection
   - Add operators: Equal, NotEqual, GreaterThan, etc. as TypeOptions
   - Verify no switch statements needed

4. **Implement First Command**:
   - Start with `QueryCommand<T>` (simplest)
   - Add all expression properties (Filter, Projection, etc.)
   - Test instantiation and type safety

5. **Implement First Translator**:
   - Start with `SqlDataCommandTranslator`
   - Implement just QueryCommand translation
   - Test SQL generation from QueryCommand
   - Verify visitor pattern works

6. **Test End-to-End**:
   - Create simple DataContext
   - Write LINQ query
   - Convert to QueryCommand
   - Translate to SQL
   - Execute and verify results

## Resources and References

### External Documentation
- FractalDataWorks Developer Kit: `../Developer-Kit-private/CLAUDE.md`
- TypeCollections: `../Developer-Kit-private/src/FractalDataWorks.Collections/README.md`
- ServiceTypes: `../Developer-Kit-private/src/FractalDataWorks.ServiceTypes/README.md`
- Railway-Oriented Programming: `../Developer-Kit-private/src/FractalDataWorks.Results/README.md`

### Related Worktrees
- Configuration: `../Developer-Kit-configuration` (multi-tenant config patterns)
- Transformations: `../Developer-Kit-transformations` (data transformation pipelines)
- FastEndpoints: `../Developer-Kit-fastendpoints` (REST API using DataCommands)

### Existing Codebase References
- IGenericConnection: `src/FractalDataWorks.Services.Connections.Abstractions/IGenericConnection.cs`
- IDataStore: `src/FractalDataWorks.Data.DataStores.Abstractions/IDataStore.cs`
- ServiceBase: `src/FractalDataWorks.Services.Abstractions/ServiceBase.cs`

## Critical Reminders

1. **ALWAYS use TypeCollections instead of enums** - This is non-negotiable
2. **NEVER use switch statements** - Use properties or visitor pattern
3. **Railway-Oriented Programming** - All methods return `IGenericResult` or `IGenericResult<T>`
4. **Source-generated logging** - Use `[LoggerMessage]` attribute
5. **No Async suffix** - Method names should not end in "Async"
6. **No Core pattern** - Don't use method names like `ValidateCore`
7. **Constructor-based properties** - In TypeCollections, set properties in constructor, not abstract
8. **Multi-targeting** - Collections/ServiceTypes must target `netstandard2.0;net10.0`

## Summary

This worktree contains a **complete architectural design** for DataCommands - a generic, type-safe, universal data command system that works across all connection types (SQL, REST, File, GraphQL). The architecture:

- Uses generics to eliminate boxing/unboxing
- Provides compile-time type safety and IntelliSense
- Follows all CLAUDE.md patterns (TypeCollections, no enums, no switch statements)
- Offers EF Core-like API via DataContext
- Supports type inference from LINQ expressions
- Works with any connection type through translator pattern

**Status**: Design complete, ready to start implementation in Phase 2.

**First Implementation Task**: Create project structure and implement core abstractions (`IDataCommand`, `DataCommandBase`, `DataCommands` TypeCollection).
