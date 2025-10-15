# DataCommand Implementation Template Documentation

## Table of Contents
1. [What is a DataCommand?](#1-what-is-a-datacommand)
2. [DataCommand Hierarchy](#2-datacommand-hierarchy)
3. [Adding a New DataCommand](#3-adding-a-new-datacommand)
4. [Expression System](#4-expression-system)
5. [Dependencies](#5-dependencies)
6. [TypeCollection Integration](#6-typecollection-integration)
7. [Category Assignment](#7-category-assignment)
8. [Template Parameters](#8-template-parameters-item-template)
9. [Complete Examples](#9-complete-examples)
10. [Common Patterns](#10-common-patterns)
11. [Common Mistakes](#11-common-mistakes)

---

## 1. What is a DataCommand?

A **DataCommand** is a universal data operation representation that works across all connection types:
- **SQL databases** (via SQL translators)
- **REST APIs** (via OData or custom REST translators)
- **File systems** (CSV, JSON, XML)
- **GraphQL endpoints**
- **Any other data source** (custom translators)

### Key Benefits
- **Type-safe**: Generic type parameters eliminate casting (`IEnumerable<Customer>` not `object`)
- **Universal**: Same command works across SQL, REST, File, GraphQL
- **Composable**: Expressions can be combined (filter + ordering + paging)
- **Extensible**: Add custom commands without modifying framework

### How It Works
```
QueryCommand<Customer> -> Translator -> SQL: SELECT * FROM Customers
                                     -> REST: GET /api/customers
                                     -> File: Read customers.csv
                                     -> GraphQL: query { customers {...} }
```

---

## 2. DataCommand Hierarchy

DataCommands have a **3-level generic hierarchy**:

```csharp
// Level 1: Non-generic marker (used by TypeCollection source generator)
public interface IDataCommand : IGenericCommand
{
    string ContainerName { get; }  // Table, collection, endpoint, file path
    IReadOnlyDictionary<string, object> Metadata { get; }
}

// Level 2: Typed result (no input data)
public interface IDataCommand<TResult> : IDataCommand
{
    // TResult provides compile-time type safety for results
    // No additional members - type parameter is the contract
}

// Level 3: Typed input and result
public interface IDataCommand<TResult, TInput> : IDataCommand<TResult>
{
    TInput Data { get; }  // Input data for the command
}
```

### Base Classes
Commands inherit from one of three base classes:

```csharp
// Non-generic base (rarely used directly)
public abstract class DataCommandBase : IDataCommand
{
    protected DataCommandBase(int id, string name, string containerName, IGenericCommandCategory category)
}

// Typed result base (for queries, deletes)
public abstract class DataCommandBase<TResult> : DataCommandBase, IDataCommand<TResult>
{
    protected DataCommandBase(int id, string name, string containerName, IGenericCommandCategory category)
}

// Typed input and result base (for inserts, updates)
public abstract class DataCommandBase<TResult, TInput> : DataCommandBase<TResult>, IDataCommand<TResult, TInput>
{
    protected DataCommandBase(int id, string name, string containerName, IGenericCommandCategory category, TInput data)
    public TInput Data { get; }
}
```

### When to Use Each Base Class

| Base Class | Use When | Examples |
|------------|----------|----------|
| `DataCommandBase<TResult>` | Command returns typed result, no input data | `QueryCommand<T>`, `DeleteCommand` |
| `DataCommandBase<TResult, TInput>` | Command requires typed input data | `InsertCommand<T>`, `UpdateCommand<T>` |
| `DataCommandBase` (non-generic) | Rare - only for marker interfaces | N/A - use generic variants |

---

## 3. Adding a New DataCommand

### Required Steps

1. **Choose the correct base class** (see table above)
2. **Add `[TypeOption]` attribute** for TypeCollection discovery
3. **Add command-specific properties** (Filter, Projection, etc.)
4. **Set category in constructor** (Query, Insert, Update, Delete, Custom)
5. **Use init-only properties** for immutability
6. **Use collection expressions** `[]` for list initialization

### Step-by-Step Example: Creating UpsertCommand

```csharp
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Upsert command for insert-or-update operations (MERGE/UPSERT).
/// Returns the number of affected rows.
/// </summary>
/// <typeparam name="T">The type of entity to upsert.</typeparam>
[TypeOption(typeof(DataCommands), "Upsert")]  // REQUIRED: TypeCollection attribute
public sealed class UpsertCommand<T> : DataCommandBase<int, T>  // Typed input + result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertCommand{T}"/> class.
    /// </summary>
    /// <param name="containerName">The name of the container to upsert into.</param>
    /// <param name="data">The entity to upsert.</param>
    public UpsertCommand(string containerName, T data)
        : base(
            id: 5,  // MUST be unique across all DataCommands
            name: "Upsert",  // MUST match TypeOption attribute
            containerName,
            DataCommandCategory.Insert,  // Choose appropriate category
            data)
    {
    }

    /// <summary>
    /// Gets or sets the match conditions for determining if record exists.
    /// If record matches, UPDATE; otherwise INSERT.
    /// </summary>
    public IFilterExpression? MatchConditions { get; init; }  // init-only property
}
```

### ID Assignment Strategy

Each command needs a **unique ID** within the DataCommands collection:

```csharp
// Existing IDs (DO NOT REUSE):
// 1 = QueryCommand
// 2 = InsertCommand
// 3 = UpdateCommand
// 4 = DeleteCommand

// New commands should use sequential IDs starting from 5:
// 5 = UpsertCommand
// 6 = BulkInsertCommand
// 7 = MergeCommand
// etc.
```

---

## 4. Expression System

DataCommands use **expression objects** to represent SQL-like clauses universally:

### FilterExpression (WHERE clause)

```csharp
// Interface (in Abstractions)
public interface IFilterExpression
{
    IReadOnlyList<FilterCondition> Conditions { get; }
    LogicalOperator? LogicalOperator { get; }  // AND/OR
}

// Implementation (in Commands.Data)
public sealed class FilterExpression : IFilterExpression
{
    public required IReadOnlyList<FilterCondition> Conditions { get; init; }
    public LogicalOperator? LogicalOperator { get; init; }
}

// Usage
var filter = new FilterExpression
{
    Conditions = [
        new FilterCondition
        {
            PropertyName = "IsActive",
            Operator = FilterOperators.Equal,  // TypeCollection - no enums!
            Value = true
        },
        new FilterCondition
        {
            PropertyName = "Name",
            Operator = FilterOperators.Contains,
            Value = "Acme"
        }
    ],
    LogicalOperator = LogicalOperator.And
};
```

### FilterCondition

```csharp
public sealed record FilterCondition
{
    public required string PropertyName { get; init; }
    public required FilterOperatorBase Operator { get; init; }  // TypeCollection, not enum!
    public object? Value { get; init; }
}
```

### FilterOperators (TypeCollection)

**Key Innovation**: Operators are TypeCollection instances, not enums. This eliminates switch statements!

```csharp
// Usage: No switch statements needed!
var condition = new FilterCondition
{
    PropertyName = "Status",
    Operator = FilterOperators.Equal,  // Type-safe, no magic strings
    Value = "Active"
};

// Direct property access in translators
var sqlCondition = $"[{condition.PropertyName}] {condition.Operator.SqlOperator} @{condition.PropertyName}";
// Result: "[Status] = @Status"

var odataFilter = $"{condition.PropertyName} {condition.Operator.ODataOperator} {condition.Operator.FormatODataValue(condition.Value)}";
// Result: "Status eq 'Active'"
```

Available operators (from `FilterOperators` TypeCollection):
- `Equal` (=, eq)
- `NotEqual` (<>, ne)
- `GreaterThan` (>, gt)
- `GreaterThanOrEqual` (>=, ge)
- `LessThan` (<, lt)
- `LessThanOrEqual` (<=, le)
- `Contains` (LIKE %x%, contains)
- `StartsWith` (LIKE x%, startswith)
- `EndsWith` (LIKE %x, endswith)
- `IsNull` (IS NULL, null)
- `IsNotNull` (IS NOT NULL, not null)
- `In` (IN, in)

### ProjectionExpression (SELECT clause)

```csharp
// Interface
public interface IProjectionExpression
{
    IReadOnlyList<ProjectionField> Fields { get; }
}

// Field record
public sealed record ProjectionField
{
    public required string PropertyName { get; init; }
    public string? Alias { get; init; }
}

// Usage
var projection = new ProjectionExpression
{
    Fields = [
        new ProjectionField { PropertyName = "Id" },
        new ProjectionField { PropertyName = "Name" },
        new ProjectionField { PropertyName = "Email", Alias = "ContactEmail" }
    ]
};
```

### OrderingExpression (ORDER BY clause)

```csharp
// Interface
public interface IOrderingExpression
{
    IReadOnlyList<OrderedField> OrderedFields { get; }
}

// Field record
public sealed record OrderedField
{
    public required string PropertyName { get; init; }
    public required SortDirection Direction { get; init; }  // EnhancedEnum
}

// Usage
var ordering = new OrderingExpression
{
    OrderedFields = [
        new OrderedField
        {
            PropertyName = "Name",
            Direction = SortDirection.Ascending
        },
        new OrderedField
        {
            PropertyName = "CreatedDate",
            Direction = SortDirection.Descending
        }
    ]
};
```

### PagingExpression (SKIP/TAKE)

```csharp
// Interface
public interface IPagingExpression
{
    int Skip { get; }
    int? Take { get; }
}

// Usage
var paging = new PagingExpression
{
    Skip = 0,     // Offset
    Take = 50     // Limit (null = no limit)
};
```

### AggregationExpression (GROUP BY - Future)

```csharp
// Interface
public interface IAggregationExpression
{
    IReadOnlyList<string> GroupByFields { get; }
    IReadOnlyDictionary<string, string> Aggregations { get; }
}

// Note: Full aggregation support planned for future phases
```

### JoinExpression (JOIN - Future)

```csharp
// Interface
public interface IJoinExpression
{
    string TargetContainerName { get; }
    string JoinType { get; }  // INNER, LEFT, RIGHT, FULL
    IReadOnlyList<(string LeftField, string RightField)> JoinConditions { get; }
}

// Note: Full join support planned for future phases
```

---

## 5. Dependencies

### Project References

Commands must reference the Abstractions project:

```xml
<ItemGroup>
  <!-- Core Dependencies -->
  <ProjectReference Include="..\FractalDataWorks.Commands.Data.Abstractions" />
  <ProjectReference Include="..\FractalDataWorks.Collections" />

  <!-- Source Generator (Analyzer only) -->
  <ProjectReference Include="..\FractalDataWorks.Collections.SourceGenerators"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false"
                    PrivateAssets="all" />
</ItemGroup>
```

### Required Using Statements

```csharp
using FractalDataWorks.Collections.Attributes;  // For [TypeOption]
using FractalDataWorks.Commands.Data.Abstractions;  // For base classes and interfaces
```

### Optional Using Statements

```csharp
using System.Collections.Generic;  // For IReadOnlyList, IReadOnlyDictionary
using FractalDataWorks.Commands.Abstractions;  // For IGenericCommandCategory
```

---

## 6. TypeCollection Integration

### How DataCommands TypeCollection Works

The `DataCommands` class is a **TypeCollection** that provides compile-time discovery of all DataCommand types:

```csharp
[TypeCollection(typeof(DataCommandBase), typeof(IDataCommand), typeof(DataCommands))]
public abstract partial class DataCommands : TypeCollectionBase<DataCommandBase, IDataCommand>
{
    // Source generator creates:
    // - Static properties for each [TypeOption] command
    // - All() method (returns all command types)
    // - GetByName(string name) method
    // - GetById(int id) method
}
```

### Source Generator Output (Conceptual)

The source generator scans for all classes with `[TypeOption(typeof(DataCommands), "CommandName")]` and generates:

```csharp
// Generated code (conceptual - actual output is more complex)
public abstract partial class DataCommands
{
    // Static properties
    public static TypeOption Query { get; } = new TypeOption(1, "Query", typeof(QueryCommand<>));
    public static TypeOption Insert { get; } = new TypeOption(2, "Insert", typeof(InsertCommand<>));
    public static TypeOption Update { get; } = new TypeOption(3, "Update", typeof(UpdateCommand<>));
    public static TypeOption Delete { get; } = new TypeOption(4, "Delete", typeof(DeleteCommand));

    // Methods
    public static IEnumerable<TypeOption> All() { ... }
    public static TypeOption? GetByName(string name) { ... }
    public static TypeOption? GetById(int id) { ... }
}
```

### Usage in Application Code

```csharp
// Access commands via TypeCollection
var queryType = DataCommands.Query;
var insertType = DataCommands.Insert;

// Get all command types
var allCommands = DataCommands.All();

// Find by name
var updateType = DataCommands.GetByName("Update");

// Find by ID
var deleteType = DataCommands.GetById(4);
```

### Critical Requirements

1. **[TypeOption] attribute is MANDATORY** - Without it, source generator won't discover the command
2. **Name must match** - `[TypeOption(typeof(DataCommands), "Query")]` requires `name: "Query"` in constructor
3. **ID must be unique** - No two commands can share the same ID

---

## 7. Category Assignment

### DataCommandCategory Pattern

Categories define execution characteristics (transactions, caching, mutations):

```csharp
internal sealed class DataCommandCategory : IGenericCommandCategory
{
    // Predefined categories
    public static readonly DataCommandCategory Query =
        new(1, "Query", requiresTransaction: false, isMutation: false, isCacheable: true);

    public static readonly DataCommandCategory Insert =
        new(2, "Insert", requiresTransaction: true, isMutation: true, isCacheable: false);

    public static readonly DataCommandCategory Update =
        new(3, "Update", requiresTransaction: true, isMutation: true, isCacheable: false);

    public static readonly DataCommandCategory Delete =
        new(4, "Delete", requiresTransaction: true, isMutation: true, isCacheable: false);

    private DataCommandCategory(int id, string name, bool requiresTransaction, bool isMutation, bool isCacheable)
    {
        Id = id;
        Name = name;
        RequiresTransaction = requiresTransaction;
        IsMutation = isMutation;
        IsCacheable = isCacheable;
        SupportsStreaming = !isMutation;
        ExecutionPriority = 50;
    }
}
```

### Choosing the Right Category

| Category | When to Use | Characteristics |
|----------|-------------|-----------------|
| **Query** | Read operations (SELECT) | No transaction, cacheable, supports streaming |
| **Insert** | Create operations (INSERT) | Requires transaction, mutation, not cacheable |
| **Update** | Modify operations (UPDATE) | Requires transaction, mutation, not cacheable |
| **Delete** | Remove operations (DELETE) | Requires transaction, mutation, not cacheable |

### Custom Categories (Future)

For advanced scenarios, you may need custom categories:

```csharp
// Example: BulkInsert might need custom category with different ExecutionPriority
public static readonly DataCommandCategory BulkInsert =
    new(5, "BulkInsert", requiresTransaction: true, isMutation: true, isCacheable: false)
    {
        ExecutionPriority = 100  // Higher priority than regular Insert
    };
```

---

## 8. Template Parameters (Item Template)

When creating a Visual Studio or Rider item template, use these substitution parameters:

### Required Parameters

```xml
<CommandName>                 <!-- e.g., "Upsert", "BulkInsert", "Merge" -->
<ResultType>                  <!-- Generic parameter: T, int, IEnumerable<T>, etc. -->
<Category>                    <!-- Query, Insert, Update, Delete, Custom -->
<UniqueId>                    <!-- Integer: unique ID for this command -->
```

### Optional Parameters

```xml
<InputType>                   <!-- For TInput commands: T, IEnumerable<T>, etc. -->
<HasFilter>                   <!-- bool: true if command supports Filter property -->
<HasProjection>               <!-- bool: true if command supports Projection property -->
<HasOrdering>                 <!-- bool: true if command supports Ordering property -->
<HasPaging>                   <!-- bool: true if command supports Paging property -->
<HasAggregation>              <!-- bool: true if command supports Aggregation property -->
<HasJoins>                    <!-- bool: true if command supports Joins property -->
```

### Template Structure

```csharp
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace $rootnamespace$;

/// <summary>
/// $CommandName$ command for ...
/// </summary>
[TypeOption(typeof(DataCommands), "$CommandName$")]
public sealed class $CommandName$Command<T> : DataCommandBase<$ResultType$, $InputType$>
{
    public $CommandName$Command(string containerName, $InputType$ data)
        : base(id: $UniqueId$, name: "$CommandName$", containerName, DataCommandCategory.$Category$, data)
    {
    }

    // Conditional properties based on template parameters
#if ($HasFilter$ == true)
    public IFilterExpression? Filter { get; init; }
#endif

#if ($HasProjection$ == true)
    public IProjectionExpression? Projection { get; init; }
#endif

#if ($HasOrdering$ == true)
    public IOrderingExpression? Ordering { get; init; }
#endif

#if ($HasPaging$ == true)
    public IPagingExpression? Paging { get; init; }
#endif
}
```

---

## 9. Complete Examples

### Example 1: QueryCommand (Existing)

Full implementation with all expression types:

```csharp
using System.Collections.Generic;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Query command for retrieving data (SELECT operation).
/// Returns an enumerable collection of typed results - no casting needed!
/// </summary>
/// <typeparam name="T">The type of entity to query.</typeparam>
[TypeOption(typeof(DataCommands), "Query")]
public sealed class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public QueryCommand(string containerName)
        : base(id: 1, name: "Query", containerName, DataCommandCategory.Query)
    {
    }

    /// <summary>
    /// Gets or sets the filter expression (WHERE clause).
    /// </summary>
    public IFilterExpression? Filter { get; init; }

    /// <summary>
    /// Gets or sets the projection expression (SELECT clause).
    /// </summary>
    public IProjectionExpression? Projection { get; init; }

    /// <summary>
    /// Gets or sets the ordering expression (ORDER BY clause).
    /// </summary>
    public IOrderingExpression? Ordering { get; init; }

    /// <summary>
    /// Gets or sets the paging expression (SKIP/TAKE).
    /// </summary>
    public IPagingExpression? Paging { get; init; }

    /// <summary>
    /// Gets or sets the aggregation expression (GROUP BY).
    /// </summary>
    public IAggregationExpression? Aggregation { get; init; }

    /// <summary>
    /// Gets or sets the join expressions (JOIN clauses).
    /// </summary>
    public IReadOnlyList<IJoinExpression> Joins { get; init; } = [];
}
```

### Example 2: UpsertCommand (New Command)

Upsert = Insert if not exists, Update if exists:

```csharp
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Upsert command for insert-or-update operations (MERGE/UPSERT).
/// If record matching MatchConditions exists, UPDATE; otherwise INSERT.
/// Returns the number of affected rows.
/// </summary>
/// <typeparam name="T">The type of entity to upsert.</typeparam>
/// <remarks>
/// <para>
/// This command represents a universal UPSERT/MERGE operation.
/// Translators convert it to:
/// <list type="bullet">
/// <item>SQL: MERGE statement</item>
/// <item>REST: PUT with upsert semantics</item>
/// <item>File: Find and update or append</item>
/// </list>
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var customer = new Customer { Id = 123, Name = "Acme Corp", IsActive = true };
/// var command = new UpsertCommand&lt;Customer&gt;("Customers", customer)
/// {
///     MatchConditions = new FilterExpression {
///         Conditions = [
///             new FilterCondition {
///                 PropertyName = "Id",
///                 Operator = FilterOperators.Equal,
///                 Value = 123
///             }
///         ]
///     }
/// };
///
/// var result = await connection.ExecuteAsync(command);
/// // result.Value is int (1 if inserted, 1 if updated)
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataCommands), "Upsert")]
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
    public UpsertCommand(string containerName, T data)
        : base(id: 5, name: "Upsert", containerName, DataCommandCategory.Insert, data)
    {
    }

    /// <summary>
    /// Gets or sets the match conditions for determining if record exists.
    /// If record matches, UPDATE; otherwise INSERT.
    /// </summary>
    public IFilterExpression? MatchConditions { get; init; }
}
```

### Example 3: BulkInsertCommand (New Command)

Insert multiple records in a single operation:

```csharp
using System.Collections.Generic;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Bulk insert command for inserting multiple records efficiently.
/// Returns a result with count of inserted records and any errors.
/// </summary>
/// <typeparam name="T">The type of entity to insert.</typeparam>
/// <remarks>
/// <para>
/// This command represents optimized bulk insert operations.
/// Translators convert it to:
/// <list type="bullet">
/// <item>SQL: Bulk INSERT or table-valued parameters</item>
/// <item>REST: POST array with batch semantics</item>
/// <item>File: Append multiple records</item>
/// </list>
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var customers = new List&lt;Customer&gt;
/// {
///     new Customer { Name = "Acme Corp", IsActive = true },
///     new Customer { Name = "Beta Inc", IsActive = true },
///     new Customer { Name = "Gamma LLC", IsActive = false }
/// };
///
/// var command = new BulkInsertCommand&lt;Customer&gt;("Customers", customers)
/// {
///     BatchSize = 1000,
///     ContinueOnError = false
/// };
///
/// var result = await connection.ExecuteAsync(command);
/// // result.Value.InsertedCount = 3
/// // result.Value.Errors = []
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataCommands), "BulkInsert")]
public sealed class BulkInsertCommand<T> : DataCommandBase<BulkInsertResult, IEnumerable<T>>
{
    public BulkInsertCommand(string containerName, IEnumerable<T> data)
        : base(id: 6, name: "BulkInsert", containerName, DataCommandCategory.Insert, data)
    {
    }

    /// <summary>
    /// Gets or sets the batch size for bulk operations.
    /// Null means use translator's default.
    /// </summary>
    public int? BatchSize { get; init; }

    /// <summary>
    /// Gets or sets whether to continue inserting if some records fail.
    /// Default is false (stop on first error).
    /// </summary>
    public bool ContinueOnError { get; init; }
}

/// <summary>
/// Result of a bulk insert operation.
/// </summary>
public sealed record BulkInsertResult
{
    /// <summary>
    /// Gets the number of records successfully inserted.
    /// </summary>
    public required int InsertedCount { get; init; }

    /// <summary>
    /// Gets any errors that occurred during bulk insert.
    /// </summary>
    public IReadOnlyList<BulkInsertError> Errors { get; init; } = [];
}

/// <summary>
/// Error that occurred during bulk insert.
/// </summary>
public sealed record BulkInsertError
{
    /// <summary>
    /// Gets the zero-based index of the record that failed.
    /// </summary>
    public required int RecordIndex { get; init; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public required string ErrorMessage { get; init; }
}
```

### Example 4: MergeCommand (New Command)

Advanced merge with update/insert/delete based on match:

```csharp
using System.Collections.Generic;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Merge command for advanced MERGE operations with update/insert/delete.
/// Returns the number of affected rows per operation type.
/// </summary>
/// <typeparam name="T">The type of entity to merge.</typeparam>
[TypeOption(typeof(DataCommands), "Merge")]
public sealed class MergeCommand<T> : DataCommandBase<MergeResult, IEnumerable<T>>
{
    public MergeCommand(string containerName, IEnumerable<T> data)
        : base(id: 7, name: "Merge", containerName, DataCommandCategory.Update, data)
    {
    }

    /// <summary>
    /// Gets or sets the match conditions for determining if record exists.
    /// </summary>
    public required IFilterExpression MatchConditions { get; init; }

    /// <summary>
    /// Gets or sets the action when matched (UPDATE or DELETE).
    /// </summary>
    public MergeMatchAction WhenMatched { get; init; } = MergeMatchAction.Update;

    /// <summary>
    /// Gets or sets the action when not matched (INSERT or IGNORE).
    /// </summary>
    public MergeNotMatchAction WhenNotMatched { get; init; } = MergeNotMatchAction.Insert;

    /// <summary>
    /// Gets or sets whether to delete records that exist in target but not in source.
    /// </summary>
    public bool DeleteNotInSource { get; init; }
}

/// <summary>
/// Action to take when merge finds matching record.
/// </summary>
public enum MergeMatchAction
{
    Update,
    Delete,
    Ignore
}

/// <summary>
/// Action to take when merge doesn't find matching record.
/// </summary>
public enum MergeNotMatchAction
{
    Insert,
    Ignore
}

/// <summary>
/// Result of a merge operation.
/// </summary>
public sealed record MergeResult
{
    public required int InsertedCount { get; init; }
    public required int UpdatedCount { get; init; }
    public required int DeletedCount { get; init; }
}
```

---

## 10. Common Patterns

### Pattern 1: Init-Only Properties for Immutability

Always use `init` setters for command properties:

```csharp
// CORRECT
public IFilterExpression? Filter { get; init; }

// WRONG - Allows mutation after construction
public IFilterExpression? Filter { get; set; }
```

### Pattern 2: Collection Expressions

Use modern C# collection expressions `[]` instead of `new List<T>()`:

```csharp
// CORRECT - Modern C# 12+
public IReadOnlyList<IJoinExpression> Joins { get; init; } = [];

// WRONG - Old style
public IReadOnlyList<IJoinExpression> Joins { get; init; } = new List<IJoinExpression>();
```

### Pattern 3: Required vs Optional Properties

Use `required` modifier for properties that must be set:

```csharp
// Required - MUST be set during initialization
public required IFilterExpression MatchConditions { get; init; }

// Optional - Can be null
public IFilterExpression? Filter { get; init; }
```

### Pattern 4: Nullable Reference Types

Enable nullable reference types and annotate correctly:

```csharp
// Nullable - property can be null
public IFilterExpression? Filter { get; init; }

// Non-nullable - property cannot be null
public required IFilterExpression MatchConditions { get; init; }
```

### Pattern 5: Property-Based Configuration

Commands should be configured via properties, not methods:

```csharp
// CORRECT - Declarative property initialization
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression { ... },
    Ordering = new OrderingExpression { ... },
    Paging = new PagingExpression { Skip = 0, Take = 50 }
};

// WRONG - Imperative method calls (not the DataCommand pattern)
var command = new QueryCommand<Customer>("Customers");
command.SetFilter(...);
command.SetOrdering(...);
```

### Pattern 6: XML Documentation

Always provide comprehensive XML documentation:

```csharp
/// <summary>
/// Brief description of the command.
/// </summary>
/// <typeparam name="T">Description of the generic parameter.</typeparam>
/// <remarks>
/// <para>
/// Detailed explanation of what the command does.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var command = new MyCommand&lt;Customer&gt;("Customers");
/// var result = await connection.ExecuteAsync(command);
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataCommands), "MyCommand")]
public sealed class MyCommand<T> : DataCommandBase<T>
{
    /// <summary>
    /// Description of what this property does.
    /// </summary>
    public IFilterExpression? Filter { get; init; }
}
```

---

## 11. Common Mistakes

### Mistake 1: Forgetting [TypeOption] Attribute

**WRONG:**
```csharp
// Missing attribute - source generator won't discover this!
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
    ...
}
```

**CORRECT:**
```csharp
[TypeOption(typeof(DataCommands), "Upsert")]  // REQUIRED
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
    ...
}
```

### Mistake 2: Wrong Base Class for Generic Parameters

**WRONG:**
```csharp
// InsertCommand needs TInput, but this uses DataCommandBase<TResult> without TInput
public sealed class InsertCommand<T> : DataCommandBase<int>
{
    public T Data { get; }  // Manually adding Data - BAD!
}
```

**CORRECT:**
```csharp
// Use DataCommandBase<TResult, TInput> when command needs input data
public sealed class InsertCommand<T> : DataCommandBase<int, T>
{
    public InsertCommand(string containerName, T data)
        : base(id: 2, name: "Insert", containerName, DataCommandCategory.Insert, data)
    {
    }
    // Data property inherited from base class
}
```

### Mistake 3: Mismatched Name in Attribute vs Constructor

**WRONG:**
```csharp
[TypeOption(typeof(DataCommands), "Upsert")]  // Says "Upsert"
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
    public UpsertCommand(string containerName, T data)
        : base(id: 5, name: "UpsertData", ...)  // Says "UpsertData" - MISMATCH!
}
```

**CORRECT:**
```csharp
[TypeOption(typeof(DataCommands), "Upsert")]  // Matches constructor
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
    public UpsertCommand(string containerName, T data)
        : base(id: 5, name: "Upsert", ...)  // Same name
}
```

### Mistake 4: Forgetting Category in Constructor

**WRONG:**
```csharp
public UpsertCommand(string containerName, T data)
    : base(id: 5, name: "Upsert", containerName, null, data)  // Null category!
{
}
```

**CORRECT:**
```csharp
public UpsertCommand(string containerName, T data)
    : base(id: 5, name: "Upsert", containerName, DataCommandCategory.Insert, data)
{
}
```

### Mistake 5: Not Using Init-Only Properties

**WRONG:**
```csharp
public IFilterExpression? Filter { get; set; }  // Mutable!
```

**CORRECT:**
```csharp
public IFilterExpression? Filter { get; init; }  // Immutable after initialization
```

### Mistake 6: Duplicate IDs

**WRONG:**
```csharp
// Multiple commands with same ID
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
    public UpsertCommand(...) : base(id: 2, ...)  // ID 2 already used by InsertCommand!
}
```

**CORRECT:**
```csharp
// Unique ID
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
    public UpsertCommand(...) : base(id: 5, ...)  // Unique ID
}
```

### Mistake 7: Using Enums Instead of TypeCollections

**WRONG:**
```csharp
public enum FilterOperator  // DON'T create enums for operators!
{
    Equal,
    NotEqual,
    GreaterThan
}

// This leads to switch statements everywhere
var condition = new FilterCondition
{
    PropertyName = "Status",
    Operator = FilterOperator.Equal  // Enum
};

// Translator needs switch statement - BAD!
string sqlOp = condition.Operator switch
{
    FilterOperator.Equal => "=",
    FilterOperator.NotEqual => "<>",
    ...
};
```

**CORRECT:**
```csharp
// Use TypeCollection - operators know their own representations
var condition = new FilterCondition
{
    PropertyName = "Status",
    Operator = FilterOperators.Equal  // TypeCollection instance
};

// No switch needed - direct property access!
string sqlOp = condition.Operator.SqlOperator;  // "="
```

### Mistake 8: Not Sealing Command Classes

**WRONG:**
```csharp
public class UpsertCommand<T> : DataCommandBase<int, T>  // Not sealed - allows inheritance
{
}
```

**CORRECT:**
```csharp
public sealed class UpsertCommand<T> : DataCommandBase<int, T>  // Sealed - no inheritance
{
}
```

### Mistake 9: Missing Namespace Import

**WRONG:**
```csharp
// Missing using statement
namespace FractalDataWorks.Commands.Data;

[TypeOption(...)]  // Compiler error: TypeOption not found
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
}
```

**CORRECT:**
```csharp
using FractalDataWorks.Collections.Attributes;  // REQUIRED for [TypeOption]

namespace FractalDataWorks.Commands.Data;

[TypeOption(typeof(DataCommands), "Upsert")]
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
}
```

### Mistake 10: Forgetting XML Documentation

**WRONG:**
```csharp
[TypeOption(typeof(DataCommands), "Upsert")]
public sealed class UpsertCommand<T> : DataCommandBase<int, T>  // No XML doc
{
    public IFilterExpression? MatchConditions { get; init; }  // No XML doc
}
```

**CORRECT:**
```csharp
/// <summary>
/// Upsert command for insert-or-update operations.
/// </summary>
/// <typeparam name="T">The type of entity to upsert.</typeparam>
[TypeOption(typeof(DataCommands), "Upsert")]
public sealed class UpsertCommand<T> : DataCommandBase<int, T>
{
    /// <summary>
    /// Gets or sets the match conditions for determining if record exists.
    /// </summary>
    public IFilterExpression? MatchConditions { get; init; }
}
```

---

## Summary Checklist

When creating a new DataCommand, verify:

- [ ] Correct base class chosen (`DataCommandBase<TResult>` or `DataCommandBase<TResult, TInput>`)
- [ ] `[TypeOption(typeof(DataCommands), "CommandName")]` attribute added
- [ ] Unique ID assigned in constructor
- [ ] Name in constructor matches TypeOption attribute
- [ ] Appropriate category assigned (Query, Insert, Update, Delete)
- [ ] All properties use `init` setters (not `set`)
- [ ] Collection properties use `[]` syntax
- [ ] Class is `sealed`
- [ ] XML documentation provided for class and all properties
- [ ] Required using statements included:
  - `using FractalDataWorks.Collections.Attributes;`
  - `using FractalDataWorks.Commands.Data.Abstractions;`
- [ ] Project references correct:
  - `FractalDataWorks.Commands.Data.Abstractions`
  - `FractalDataWorks.Collections`
  - `FractalDataWorks.Collections.SourceGenerators` (OutputItemType="Analyzer")

---

## Additional Resources

- **Architecture Documentation**: See root `ARCHITECTURE_SUMMARY.md` for overall architecture
- **Continuation Guide**: See root `CONTINUATION_GUIDE.md` for project continuation
- **Source Code Examples**:
  - `src/FractalDataWorks.Commands.Data/Commands/QueryCommand.cs`
  - `src/FractalDataWorks.Commands.Data/Commands/InsertCommand.cs`
  - `src/FractalDataWorks.Commands.Data/Commands/UpdateCommand.cs`
  - `src/FractalDataWorks.Commands.Data/Commands/DeleteCommand.cs`

---

**Document Version**: 1.0
**Last Updated**: 2025-10-14
**Maintained By**: FractalDataWorks Development Team
