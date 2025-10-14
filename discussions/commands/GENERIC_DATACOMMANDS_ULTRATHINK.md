# Generic DataCommands Architecture - ULTRATHINK

## Problem: Boxing/Unboxing and Lost Type Safety

**Current Issue:**
```csharp
public interface IDataCommand
{
    Type SourceType { get; }  // ❌ Runtime type checking
    Type ResultType { get; }  // ❌ Runtime type checking
}

// Usage forces casting:
var command = new QueryCommand { SourceType = typeof(Customer), ResultType = typeof(IEnumerable<Customer>) };
var result = await connection.ExecuteAsync(command); // Returns IGenericResult<object>
var customers = (IEnumerable<Customer>)result.Value; // ❌ Boxing/unboxing, runtime cast
```

**Problems:**
1. ❌ Boxing/unboxing overhead
2. ❌ No compile-time type safety
3. ❌ Runtime casting errors possible
4. ❌ No IntelliSense for result types
5. ❌ Can't infer types from LINQ expressions

## Solution: Generic Command Hierarchy

### Type Pattern Analysis

Let's analyze what each command type needs:

| Command | Input Type | Output Type | Pattern |
|---------|-----------|-------------|---------|
| **Query** | Filter/Projection (not data) | `IEnumerable<T>` | `IDataCommand<IEnumerable<T>>` |
| **Insert** | `T` entity | `int` identity or `T` | `IDataCommand<TReturn, T>` |
| **Update** | `T` entity or partial | `int` affected count or `T` | `IDataCommand<TReturn, T>` |
| **Delete** | Filter (not data) | `int` affected count | `IDataCommand<int>` |
| **Upsert** | `T` entity | `int` identity or `T` | `IDataCommand<TReturn, T>` |
| **BulkInsert** | `IEnumerable<T>` | `BulkInsertResult` | `IDataCommand<BulkInsertResult, IEnumerable<T>>` |

**Pattern Insight**: We need **3 levels of generics**:

1. **Non-generic** (`IDataCommand`) - Marker interface, common properties
2. **Single-generic** (`IDataCommand<TResult>`) - Commands that return typed results
3. **Double-generic** (`IDataCommand<TResult, TInput>`) - Commands that accept typed input and return typed results

## Architecture Design

### 1. Generic Command Interfaces

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// Non-generic marker interface for all data commands.
/// Allows heterogeneous collections and base infrastructure.
/// </summary>
public interface IDataCommand
{
    string ContainerName { get; }
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Generic command that returns a typed result.
/// Eliminates boxing/unboxing and provides compile-time type safety.
/// </summary>
/// <typeparam name="TResult">The type of result returned by this command.</typeparam>
public interface IDataCommand<TResult> : IDataCommand
{
    // No additional members needed - the generic type parameter provides type safety
    // Execution will return IGenericResult<TResult> instead of IGenericResult<object>
}

/// <summary>
/// Generic command that accepts typed input and returns typed result.
/// Provides end-to-end type safety.
/// </summary>
/// <typeparam name="TResult">The type of result returned by this command.</typeparam>
/// <typeparam name="TInput">The type of input data for this command.</typeparam>
public interface IDataCommand<TResult, TInput> : IDataCommand<TResult>
{
    /// <summary>
    /// Typed input data for the command.
    /// No boxing - strongly typed!
    /// </summary>
    TInput Data { get; }
}
```

### 2. Generic Command Base Classes

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// Non-generic base class for TypeCollection registration.
/// TypeCollection pattern requires non-generic base for source generator.
/// </summary>
public abstract class DataCommandBase : IDataCommand
{
    protected DataCommandBase(int id, string name, string containerName)
    {
        Id = id;
        Name = name;
        ContainerName = containerName;
        Metadata = new Dictionary<string, object>();
    }

    public int Id { get; }
    public string Name { get; }
    public string ContainerName { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; protected init; }
}

/// <summary>
/// Generic base class for commands with typed results.
/// </summary>
public abstract class DataCommandBase<TResult> : DataCommandBase, IDataCommand<TResult>
{
    protected DataCommandBase(int id, string name, string containerName)
        : base(id, name, containerName)
    {
    }
}

/// <summary>
/// Generic base class for commands with typed input and results.
/// </summary>
public abstract class DataCommandBase<TResult, TInput> : DataCommandBase<TResult>, IDataCommand<TResult, TInput>
{
    protected DataCommandBase(int id, string name, string containerName, TInput data)
        : base(id, name, containerName)
    {
        Data = data;
    }

    public TInput Data { get; }
}
```

### 3. Concrete Generic Commands

#### QueryCommand<T>

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Generic query command that returns IEnumerable<T>.
/// No boxing - type-safe from LINQ to result!
/// </summary>
/// <typeparam name="T">The entity type to query.</typeparam>
public sealed class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public QueryCommand(string containerName)
        : base(id: 1, name: "Query", containerName)
    {
    }

    /// <summary>
    /// Filter expression (WHERE clause).
    /// </summary>
    public IFilterExpression? Filter { get; init; }

    /// <summary>
    /// Projection expression (SELECT fields).
    /// </summary>
    public IProjectionExpression? Projection { get; init; }

    /// <summary>
    /// Ordering expression (ORDER BY).
    /// </summary>
    public IOrderingExpression? Ordering { get; init; }

    /// <summary>
    /// Paging expression (OFFSET/LIMIT).
    /// </summary>
    public IPagingExpression? Paging { get; init; }

    /// <summary>
    /// Aggregation expression (GROUP BY, aggregates).
    /// </summary>
    public IAggregationExpression? Aggregation { get; init; }

    /// <summary>
    /// Join expressions.
    /// </summary>
    public IReadOnlyList<IJoinExpression> Joins { get; init; } = [];
}

// Usage - TYPE SAFE!
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions = [new FilterCondition { PropertyName = nameof(Customer.IsActive), Operator = FilterOperators.Equal, Value = true }]
    },
    Paging = new PagingExpression { Skip = 0, Take = 50 }
};

// Execute returns IGenericResult<IEnumerable<Customer>> - NO CASTING!
IGenericResult<IEnumerable<Customer>> result = await connection.ExecuteAsync(command);
foreach (var customer in result.Value)
{
    Console.WriteLine(customer.Name); // IntelliSense works!
}
```

#### InsertCommand<T>

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Generic insert command that accepts entity and returns identity.
/// Type-safe input and output!
/// </summary>
/// <typeparam name="T">The entity type to insert.</typeparam>
public sealed class InsertCommand<T> : DataCommandBase<int, T> // Returns int (identity), accepts T
{
    public InsertCommand(string containerName, T entity)
        : base(id: 2, name: "Insert", containerName, entity)
    {
    }

    /// <summary>
    /// Whether to return the generated identity value.
    /// </summary>
    public bool ReturnIdentity { get; init; } = true;

    /// <summary>
    /// Conflict resolution strategy.
    /// </summary>
    public InsertConflictStrategy ConflictStrategy { get; init; } = InsertConflictStrategy.Error;
}

// Usage - TYPE SAFE!
var newCustomer = new Customer
{
    Name = "Acme Corp",
    Email = "contact@acme.com",
    IsActive = true
};

var command = new InsertCommand<Customer>("Customers", newCustomer);

// Execute returns IGenericResult<int> - NO CASTING!
IGenericResult<int> result = await connection.ExecuteAsync(command);
int customerId = result.Value; // Type-safe!
```

#### InsertCommand<T> with Full Entity Return

```csharp
/// <summary>
/// Insert command that returns the full entity (with generated fields populated).
/// </summary>
public sealed class InsertCommandWithEntity<T> : DataCommandBase<T, T> // Returns T, accepts T
{
    public InsertCommandWithEntity(string containerName, T entity)
        : base(id: 2, name: "Insert", containerName, entity)
    {
    }
}

// Usage
var command = new InsertCommandWithEntity<Customer>("Customers", newCustomer);
IGenericResult<Customer> result = await connection.ExecuteAsync(command);
Customer insertedCustomer = result.Value; // Full entity with Id populated!
```

#### UpdateCommand<T>

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Generic update command.
/// Returns affected row count.
/// </summary>
/// <typeparam name="T">The entity type to update.</typeparam>
public sealed class UpdateCommand<T> : DataCommandBase<int, T> // Returns int (affected count), accepts T
{
    public UpdateCommand(string containerName, T entity)
        : base(id: 3, name: "Update", containerName, entity)
    {
    }

    /// <summary>
    /// Filter to identify records to update (WHERE clause).
    /// </summary>
    public IFilterExpression? Filter { get; init; }

    /// <summary>
    /// Whether to return updated row count.
    /// </summary>
    public bool ReturnAffectedCount { get; init; } = true;
}

// Usage - TYPE SAFE!
var updatedCustomer = existingCustomer with { Email = "newemail@acme.com" }; // Record with-expression

var command = new UpdateCommand<Customer>("Customers", updatedCustomer)
{
    Filter = new FilterExpression
    {
        Conditions = [new FilterCondition { PropertyName = nameof(Customer.Id), Operator = FilterOperators.Equal, Value = customerId }]
    }
};

IGenericResult<int> result = await connection.ExecuteAsync(command);
int affectedRows = result.Value; // Type-safe int, no casting!
```

#### DeleteCommand (non-generic result)

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Delete command returns affected row count (int).
/// No input entity needed (uses filter).
/// </summary>
public sealed class DeleteCommand : DataCommandBase<int> // Returns int, no input entity
{
    public DeleteCommand(string containerName)
        : base(id: 4, name: "Delete", containerName)
    {
    }

    /// <summary>
    /// Filter to identify records to delete (WHERE clause).
    /// </summary>
    public IFilterExpression? Filter { get; init; }

    /// <summary>
    /// Soft delete configuration.
    /// </summary>
    public SoftDeleteOptions? SoftDelete { get; init; }
}

// Usage
var command = new DeleteCommand("Customers")
{
    Filter = new FilterExpression
    {
        Conditions = [new FilterCondition { PropertyName = nameof(Customer.IsActive), Operator = FilterOperators.Equal, Value = false }]
    }
};

IGenericResult<int> result = await connection.ExecuteAsync(command);
int deletedCount = result.Value;
```

#### UpsertCommand<T>

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Generic upsert command (insert or update).
/// </summary>
public sealed class UpsertCommand<T> : DataCommandBase<int, T> // Returns int (identity), accepts T
{
    public UpsertCommand(string containerName, T entity, params string[] conflictFields)
        : base(id: 5, name: "Upsert", containerName, entity)
    {
        ConflictFields = conflictFields.ToList();
    }

    /// <summary>
    /// Fields to check for conflicts (unique key).
    /// </summary>
    public IReadOnlyList<string> ConflictFields { get; }

    /// <summary>
    /// Fields to update on conflict (if different from insert values).
    /// </summary>
    public IReadOnlyDictionary<string, object?>? UpdateOnConflict { get; init; }
}

// Usage - TYPE SAFE!
var customer = new Customer { Email = "contact@acme.com", Name = "Acme Corp" };

var command = new UpsertCommand<Customer>("Customers", customer, nameof(Customer.Email)) // Conflict on Email
{
    UpdateOnConflict = new Dictionary<string, object?>
    {
        { nameof(Customer.Name), customer.Name },
        { nameof(Customer.UpdatedAt), DateTime.UtcNow }
    }
};

IGenericResult<int> result = await connection.ExecuteAsync(command);
int customerId = result.Value;
```

#### BulkInsertCommand<T>

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Generic bulk insert command for ETL operations.
/// </summary>
public sealed class BulkInsertCommand<T> : DataCommandBase<BulkInsertResult, IEnumerable<T>> // Returns BulkInsertResult, accepts IEnumerable<T>
{
    public BulkInsertCommand(string containerName, IEnumerable<T> records)
        : base(id: 6, name: "BulkInsert", containerName, records)
    {
    }

    /// <summary>
    /// Batch size for chunked operations.
    /// </summary>
    public int BatchSize { get; init; } = 1000;

    /// <summary>
    /// Error handling strategy.
    /// </summary>
    public BulkErrorHandling ErrorHandling { get; init; } = BulkErrorHandling.StopOnFirstError;

    /// <summary>
    /// Whether to use transactions.
    /// </summary>
    public bool UseTransaction { get; init; } = true;
}

// Usage - TYPE SAFE!
var customers = new List<Customer>
{
    new Customer { Name = "Corp A", Email = "a@corp.com" },
    new Customer { Name = "Corp B", Email = "b@corp.com" },
    // ... 10,000 more
};

var command = new BulkInsertCommand<Customer>("Customers", customers)
{
    BatchSize = 1000,
    ErrorHandling = BulkErrorHandling.CollectErrors
};

IGenericResult<BulkInsertResult> result = await connection.ExecuteAsync(command);
Console.WriteLine($"Inserted: {result.Value.SuccessCount}, Failed: {result.Value.FailureCount}");
```

### 4. Generic Execute Methods

```csharp
namespace FractalDataWorks.Services.Connections.Abstractions;

public interface IDataConnection : IGenericConnection
{
    /// <summary>
    /// Execute non-generic command (fallback for dynamic scenarios).
    /// </summary>
    Task<IGenericResult<object>> ExecuteAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute generic command with typed result.
    /// No boxing/unboxing - type-safe from command to result!
    /// </summary>
    Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
        IDataCommand<TResult> command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute generic command with typed input and result.
    /// Full type safety throughout!
    /// </summary>
    Task<IGenericResult<TResult>> ExecuteAsync<TResult, TInput>(
        IDataCommand<TResult, TInput> command,
        CancellationToken cancellationToken = default);
}
```

### 5. Type Inference from LINQ

The real power: **type inference from LINQ expressions!**

```csharp
namespace FractalDataWorks.Data.DataCommands;

public static class LinqDataCommandBuilder
{
    /// <summary>
    /// Build query command from LINQ expression.
    /// Type is inferred from IQueryable<T>!
    /// </summary>
    public static IGenericResult<QueryCommand<T>> FromQueryable<T>(IQueryable<T> query)
    {
        // Analyze expression tree
        var visitor = new LinqExpressionVisitor();
        visitor.Visit(query.Expression);

        // Build typed query command
        var command = new QueryCommand<T>(GetContainerName<T>())
        {
            Filter = visitor.ExtractedFilter,
            Projection = visitor.ExtractedProjection,
            Ordering = visitor.ExtractedOrdering,
            Paging = visitor.ExtractedPaging
        };

        return GenericResult<QueryCommand<T>>.Success(command);
    }
}

// Usage - FULLY INFERRED!
IQueryable<Customer> query = dbContext.Customers
    .Where(c => c.IsActive && c.Country == "USA")
    .OrderBy(c => c.Name)
    .Skip(0)
    .Take(50);

// Type is inferred as QueryCommand<Customer>!
var commandResult = LinqDataCommandBuilder.FromQueryable(query);
var command = commandResult.Value;

// Execute - type-safe IGenericResult<IEnumerable<Customer>>!
var result = await connection.ExecuteAsync(command);

// No casting needed!
foreach (var customer in result.Value)
{
    Console.WriteLine(customer.Name);
}
```

## TypeCollection Integration

**Challenge**: TypeCollection source generator expects non-generic base types.

**Solution**: Use non-generic `DataCommandBase` for TypeCollection, but actual commands are generic.

```csharp
/// <summary>
/// TypeCollection tracks command types (not generic instances).
/// </summary>
[TypeCollection(typeof(DataCommandBase), typeof(IDataCommand), typeof(DataCommands))]
public abstract partial class DataCommands : TypeCollectionBase<DataCommandBase, IDataCommand>
{
    // Source generator creates:
    // public static readonly DataCommandBase Query; // Represents QueryCommand<T> type
    // public static readonly DataCommandBase Insert; // Represents InsertCommand<T> type
    // etc.
}
```

The TypeCollection knows about command **types** (Query, Insert, Update, etc.), but actual command **instances** are generic and type-safe!

## Benefits of Generic Approach

### 1. Zero Boxing/Unboxing

```csharp
// ❌ Old way: Boxing
var result = await connection.ExecuteAsync(command); // Returns IGenericResult<object>
var customers = (IEnumerable<Customer>)result.Value; // BOXING!

// ✅ New way: No boxing
var result = await connection.ExecuteAsync(command); // Returns IGenericResult<IEnumerable<Customer>>
var customers = result.Value; // NO CAST! Type-safe!
```

### 2. Compile-Time Type Safety

```csharp
// ❌ Old way: Runtime errors possible
var command = new QueryCommand { ResultType = typeof(Customer) }; // Oops, should be IEnumerable<Customer>!
var result = await connection.ExecuteAsync(command);
var customers = (IEnumerable<Customer>)result.Value; // RUNTIME ERROR!

// ✅ New way: Compile-time checking
var command = new QueryCommand<Customer>("Customers"); // Type enforced at compile time!
var result = await connection.ExecuteAsync(command); // Returns IGenericResult<IEnumerable<Customer>>
// Compiler ensures types match!
```

### 3. IntelliSense Support

```csharp
// ❌ Old way: No IntelliSense
var result = await connection.ExecuteAsync(command); // result.Value is object
// No IntelliSense when accessing result.Value

// ✅ New way: Full IntelliSense
var result = await connection.ExecuteAsync(command); // result.Value is IEnumerable<Customer>
foreach (var customer in result.Value)
{
    customer. // <- IntelliSense shows Customer properties!
}
```

### 4. Type Inference

```csharp
// ✅ Types inferred from LINQ!
var query = dbContext.Customers.Where(c => c.IsActive);
var command = LinqDataCommandBuilder.FromQueryable(query); // QueryCommand<Customer> inferred!
var result = await connection.ExecuteAsync(command); // IGenericResult<IEnumerable<Customer>> inferred!
```

### 5. Overload Resolution

```csharp
// Compiler automatically selects correct overload:

var queryCommand = new QueryCommand<Customer>("Customers");
await connection.ExecuteAsync(queryCommand); // Calls ExecuteAsync<IEnumerable<Customer>>

var insertCommand = new InsertCommand<Customer>("Customers", newCustomer);
await connection.ExecuteAsync(insertCommand); // Calls ExecuteAsync<int, Customer>

var deleteCommand = new DeleteCommand("Customers");
await connection.ExecuteAsync(deleteCommand); // Calls ExecuteAsync<int>
```

## Implementation Strategy

### Phase 1: Add Generic Interfaces (Non-Breaking)
- Add `IDataCommand<TResult>` and `IDataCommand<TResult, TInput>`
- Both inherit from `IDataCommand` (maintains compatibility)
- Old non-generic code still works

### Phase 2: Add Generic Execute Overloads
- Add generic `ExecuteAsync<TResult>` and `ExecuteAsync<TResult, TInput>`
- Keep non-generic `ExecuteAsync` for compatibility
- New code uses generic overloads, old code unchanged

### Phase 3: Migrate Command Types
- Convert `QueryCommand` → `QueryCommand<T>`
- Convert `InsertCommand` → `InsertCommand<T>`
- etc.
- Old non-generic versions deprecated but still available

### Phase 4: Update LINQ Builder
- Enhance `LinqDataCommandBuilder` to use generic commands
- Infer types from `IQueryable<T>`
- Type-safe end-to-end

## Complete Flow Example

```csharp
// 1. User writes LINQ query
var linqQuery = dbContext.Customers
    .Where(c => c.Country == "USA" && c.IsActive)
    .OrderBy(c => c.Name)
    .Skip(0)
    .Take(50);

// 2. Build generic command from LINQ (type inferred!)
var commandResult = LinqDataCommandBuilder.FromQueryable(linqQuery);
// Result: IGenericResult<QueryCommand<Customer>>

// 3. Get connection
var connectionResult = await connectionProvider.GetConnectionAsync("CustomerDb");
var connection = connectionResult.Value;

// 4. Execute with full type safety
var queryResult = await connection.ExecuteAsync(commandResult.Value);
// Result: IGenericResult<IEnumerable<Customer>> - NO CASTING!

// 5. Use results with IntelliSense
if (queryResult.IsSuccess)
{
    foreach (var customer in queryResult.Value)
    {
        Console.WriteLine($"{customer.Name} - {customer.Email}"); // IntelliSense works!
    }
}

// ZERO BOXING, ZERO CASTING, FULL TYPE SAFETY!
```

## Comparison Table

| Aspect | Old (Non-Generic) | New (Generic) |
|--------|------------------|---------------|
| **Result Type** | `IGenericResult<object>` | `IGenericResult<T>` |
| **Casting Required** | ✅ Yes, always | ❌ No, never |
| **Boxing/Unboxing** | ✅ Yes | ❌ No |
| **Compile-Time Safety** | ❌ No | ✅ Yes |
| **IntelliSense** | ❌ Limited | ✅ Full |
| **Type Inference** | ❌ Not possible | ✅ From LINQ |
| **Runtime Errors** | ⚠️ Possible | ❌ Prevented |
| **Performance** | Slower (boxing) | Faster (no boxing) |

## Conclusion

Using generic DataCommands provides:

1. ✅ **Zero boxing/unboxing** - Better performance
2. ✅ **Compile-time type safety** - Catch errors early
3. ✅ **Full IntelliSense** - Better developer experience
4. ✅ **Type inference** - Less boilerplate
5. ✅ **Automatic overload resolution** - Compiler picks correct Execute method
6. ✅ **Non-breaking** - Can be added alongside existing non-generic code

The architecture maintains TypeCollection pattern (non-generic base for source generator) while providing full generic type safety at the command level.

**One generic parameter, zero casts, full type safety!**
