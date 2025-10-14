# DataCommands Architecture Overview

## Table of Contents
- [Introduction](#introduction)
- [What Problem Does It Solve?](#what-problem-does-it-solve)
- [Core Concepts](#core-concepts)
- [Architecture Layers](#architecture-layers)
- [Key Design Decisions](#key-design-decisions)
- [How It Works](#how-it-works)
- [Comparison with Other Approaches](#comparison-with-other-approaches)

---

## Introduction

**DataCommands** is a universal data command system that provides a single, type-safe API for working with data across **any** backend - SQL databases, REST APIs, file systems, GraphQL endpoints, and more.

### Key Features

✅ **Universal API**: Write once, run anywhere (SQL, REST, File, GraphQL)
✅ **Type-Safe**: Full compile-time type checking with generics
✅ **Zero Boxing**: No object casting or boxing overhead
✅ **LINQ Integration**: Familiar LINQ syntax for queries
✅ **Extensible**: Add new commands and translators without changing existing code
✅ **Railway-Oriented**: All operations return `IGenericResult<T>` for explicit error handling
✅ **No Switch Statements**: Uses TypeCollections and visitor pattern for clean dispatch

---

## What Problem Does It Solve?

### The Problem

Modern applications need to work with data from multiple sources:

```csharp
// Different APIs for different data sources
var sqlCustomers = await _dbContext.Customers.Where(c => c.IsActive).ToListAsync();
var restCustomers = await _httpClient.GetFromJsonAsync<Customer[]>("/api/customers?active=true");
var fileCustomers = await _fileStore.ReadJsonAsync<Customer[]>("customers.json");
```

**Issues:**
- ❌ Different APIs for each source (EF Core, HttpClient, File I/O)
- ❌ Hard to switch data sources
- ❌ Difficult to test (can't mock database easily)
- ❌ Code duplication across layers
- ❌ No unified error handling

### The Solution

**One API, many backends:**

```csharp
// Same API for all data sources!
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = "IsActive",
                Operator = FilterOperators.Equal,
                Value = true
            }
        ]
    }
};

// Works with SQL
var sqlResult = await sqlConnection.ExecuteAsync(command);

// Works with REST
var restResult = await restConnection.ExecuteAsync(command);

// Works with Files
var fileResult = await fileConnection.ExecuteAsync(command);
```

**Benefits:**
- ✅ Single API for all data sources
- ✅ Easy to switch backends (change connection, not code)
- ✅ Testable (mock connections, not databases)
- ✅ Consistent error handling via `IGenericResult<T>`
- ✅ Type-safe at compile time

---

## Core Concepts

### 1. Commands

**Commands** represent data operations:

- `QueryCommand<T>` - Retrieve data (SELECT)
- `InsertCommand<T>` - Add new data (INSERT)
- `UpdateCommand<T>` - Modify existing data (UPDATE)
- `DeleteCommand` - Remove data (DELETE)

**Key Point:** Commands are **universal** - they don't know about SQL, REST, or any specific backend.

### 2. Expressions

**Expressions** describe query logic:

- `FilterExpression` - WHERE conditions
- `ProjectionExpression` - SELECT fields
- `OrderingExpression` - ORDER BY
- `PagingExpression` - SKIP/TAKE
- `AggregationExpression` - GROUP BY, COUNT, SUM
- `JoinExpression` - JOIN tables

**Key Point:** Expressions are **declarative** - they say WHAT you want, not HOW to get it.

### 3. Translators

**Translators** convert commands to backend-specific operations:

- `SqlTranslator` → SQL statements
- `RestTranslator` → HTTP requests with OData
- `FileTranslator` → File I/O operations
- `GraphQLTranslator` → GraphQL queries

**Key Point:** Translators are **pluggable** - add new backends without changing commands.

### 4. Connections

**Connections** execute translated commands:

- `SqlConnection` - Executes SQL via ADO.NET
- `HttpConnection` - Executes HTTP requests
- `FileConnection` - Executes file operations

**Key Point:** Connections own their translators - they know how to execute their specific protocol.

---

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                     Application Layer                        │
│  (Business logic uses universal DataCommands API)           │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                    Commands Layer                            │
│  QueryCommand, InsertCommand, UpdateCommand, DeleteCommand  │
│  FilterExpression, ProjectionExpression, OrderingExpression │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                   Translator Layer                           │
│  SqlTranslator, RestTranslator, FileTranslator, etc.        │
│  (Converts commands to backend-specific formats)            │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                   Connection Layer                           │
│  SqlConnection, HttpConnection, FileConnection              │
│  (Executes backend-specific operations)                     │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                    Data Sources                              │
│  SQL Server, PostgreSQL, REST APIs, Files, GraphQL, etc.    │
└─────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

**Commands Layer:**
- Define WHAT operation to perform
- Express query logic declaratively
- Provide type safety with generics
- No knowledge of backends

**Translator Layer:**
- Convert universal commands to backend-specific formats
- Handle backend-specific syntax (SQL dialects, OData versions)
- Map expressions to backend constructs
- Return `IConnectionCommand` for execution

**Connection Layer:**
- Execute backend-specific commands
- Handle connection management, retries, timeouts
- Convert results back to typed objects
- Return `IGenericResult<T>` with success/failure

---

## Key Design Decisions

### 1. TypeCollections Instead of Enums

**Why?** Extensibility without recompilation.

**Before (Enum - Bad):**
```csharp
public enum FilterOperator
{
    Equal,
    NotEqual,
    GreaterThan
}

// Need switch statement
string sql = operator switch
{
    FilterOperator.Equal => "=",
    FilterOperator.NotEqual => "<>",
    _ => throw new NotSupportedException()
};
```

**After (TypeCollection - Good):**
```csharp
// Each operator knows its SQL representation
public sealed class EqualOperator : FilterOperatorBase
{
    public override string SqlOperator => "=";
    public override string ODataOperator => "eq";
}

// No switch needed - just access property!
string sql = FilterOperators.Equal.SqlOperator; // "="
```

**Benefits:**
- ✅ Add new operators without modifying existing code
- ✅ Each operator encapsulates its own behavior
- ✅ No switch statements
- ✅ Source generator creates static collection

### 2. Three-Level Generic Hierarchy

**Why?** Type safety without boxing.

```csharp
// Level 1: Non-generic (for TypeCollection compatibility)
public interface IDataCommand { }

// Level 2: Generic result type
public interface IDataCommand<TResult> : IDataCommand { }

// Level 3: Generic result and input types
public interface IDataCommand<TResult, TInput> : IDataCommand<TResult>
{
    TInput Data { get; }
}
```

**Usage:**
```csharp
// Query - only result type
QueryCommand<Customer> implements IDataCommand<IEnumerable<Customer>>

// Insert - both input and result types
InsertCommand<Customer> implements IDataCommand<int, Customer>
```

**Benefits:**
- ✅ No boxing/unboxing overhead
- ✅ Compile-time type checking
- ✅ Full IntelliSense support
- ✅ Type inference from LINQ

### 3. Visitor Pattern for Translation

**Why?** Avoid switch statements, enable extensibility.

**Bad (Switch Statement):**
```csharp
public IConnectionCommand Translate(IDataCommand command)
{
    return command.CommandType switch
    {
        "Query" => TranslateQuery((QueryCommand)command),
        "Insert" => TranslateInsert((InsertCommand)command),
        _ => throw new NotSupportedException()
    };
}
```

**Good (Visitor Pattern):**
```csharp
public interface IDataCommandTranslator
{
    Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken ct);
}

// Commands know how to accept visitors
public class QueryCommand<T> : DataCommandBase<IEnumerable<T>>
{
    public override Task<IGenericResult<IConnectionCommand>> AcceptAsync(
        IDataCommandTranslator translator,
        CancellationToken ct)
    {
        return translator.VisitQueryAsync(this, ct);
    }
}
```

**Benefits:**
- ✅ No switch statements
- ✅ Type-safe dispatch
- ✅ Easy to add new translators
- ✅ Commands don't know about translators

### 4. Railway-Oriented Programming

**Why?** Explicit error handling without exceptions.

```csharp
// All operations return IGenericResult<T>
public async Task<IGenericResult<IEnumerable<Customer>>> GetActiveCustomersAsync()
{
    var command = new QueryCommand<Customer>("Customers")
    {
        Filter = new FilterExpression
        {
            Conditions = [/* ... */]
        }
    };

    var result = await _connection.ExecuteAsync(command);

    // Check success
    if (!result.IsSuccess)
    {
        _logger.LogError("Query failed: {Message}", result.Message);
        return result; // Propagate failure
    }

    return result; // Success with data
}
```

**Benefits:**
- ✅ Explicit error handling (no hidden exceptions)
- ✅ Composable with Bind/Map/Match
- ✅ Clear success/failure paths
- ✅ Easy to test

---

## How It Works

### End-to-End Flow

```
1. User writes LINQ query
   ↓
2. Custom query provider builds QueryCommand<T>
   ↓
3. Application calls connection.ExecuteAsync(command)
   ↓
4. Connection selects appropriate translator
   ↓
5. Translator converts command to backend-specific format
   ↓
6. Connection executes translated command
   ↓
7. Connection returns IGenericResult<T> with data or error
```

### Example Walkthrough

**Step 1: Define Command**
```csharp
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = "IsActive",
                Operator = FilterOperators.Equal,
                Value = true
            }
        ]
    },
    Ordering = new OrderingExpression
    {
        OrderedFields =
        [
            new OrderedField
            {
                PropertyName = "Name",
                Direction = SortDirection.Ascending
            }
        ]
    },
    Paging = new PagingExpression { Skip = 0, Take = 50 }
};
```

**Step 2: Execute via Connection**
```csharp
var result = await sqlConnection.ExecuteAsync(command);
```

**Step 3: Translator Converts to SQL**
```sql
SELECT * FROM Customers
WHERE IsActive = @IsActive
ORDER BY Name ASC
OFFSET 0 ROWS FETCH NEXT 50 ROWS ONLY
```

**Step 4: Connection Executes SQL**
```csharp
// Internally uses ADO.NET
using var sqlCommand = new SqlCommand(sql, connection);
sqlCommand.Parameters.AddWithValue("@IsActive", true);
var reader = await sqlCommand.ExecuteReaderAsync();
```

**Step 5: Return Results**
```csharp
if (result.IsSuccess)
{
    var customers = result.Value; // IEnumerable<Customer>
    foreach (var customer in customers)
    {
        Console.WriteLine(customer.Name);
    }
}
```

---

## Comparison with Other Approaches

### vs. Entity Framework Core

| Feature | DataCommands | EF Core |
|---------|-------------|---------|
| Data Sources | SQL, REST, Files, GraphQL, etc. | SQL databases only |
| Type Safety | ✅ Full generics | ✅ Full generics |
| LINQ Support | ✅ Custom provider | ✅ Built-in |
| Change Tracking | ❌ Not included | ✅ Automatic |
| Migrations | ❌ Not included | ✅ Code-first |
| Testability | ✅✅ Easy (mock connections) | ⚠️ Requires InMemory provider |
| Performance | ✅✅ No tracking overhead | ⚠️ Tracking has overhead |
| Learning Curve | ⚠️ New concepts | ✅ Well-known |

### vs. Dapper

| Feature | DataCommands | Dapper |
|---------|-------------|--------|
| Data Sources | SQL, REST, Files, GraphQL, etc. | SQL databases only |
| Type Safety | ✅ Full generics | ✅ Generic mapping |
| Query Building | ✅ Type-safe expressions | ❌ String SQL |
| SQL Injection | ✅ Protected by design | ⚠️ Manual parameterization |
| Performance | ✅ Similar | ✅ Micro-ORM speed |
| Flexibility | ✅✅ Change backends | ❌ SQL only |

### vs. Repository Pattern

| Feature | DataCommands | Repository |
|---------|-------------|------------|
| Abstraction Level | ✅ Universal commands | ⚠️ Per-entity repositories |
| Code Duplication | ✅ None (shared commands) | ❌ Lots (per repository) |
| Testability | ✅✅ Mock connections | ✅ Mock repositories |
| Flexibility | ✅✅ Swap backends | ⚠️ Still coupled to backend |
| Complexity | ⚠️ More concepts | ✅ Simpler |

---

## When to Use DataCommands

### ✅ Good Fit

- Multi-backend applications (SQL + REST + Files)
- Microservices with various data sources
- Applications that need to switch backends
- Highly testable codebases
- Domain-driven design with universal commands
- API gateways routing to different backends

### ⚠️ Consider Alternatives

- Simple CRUD apps with single SQL database (use EF Core)
- Need change tracking and migrations (use EF Core)
- Team unfamiliar with command pattern
- Need mature ecosystem (EF Core has more tooling)

---

## Next Steps

- **[Developer Guide](DataCommands-Developer-Guide.md)** - Learn to build commands
- **[Translator Guide](DataCommands-Translator-Guide.md)** - Learn to build translators
- **[Examples](DataCommands-Examples.md)** - See practical examples
- **[API Reference](../src/FractalDataWorks.Commands.Data.Abstractions/README.md)** - Detailed API docs

---

## Project Structure

```
FractalDataWorks.Commands.Data.Abstractions/   (netstandard2.0)
├── Commands/                  # Command interfaces and base classes
├── Expressions/               # Query expression models
├── Operators/                 # Filter operators (TypeCollection)
├── Translators/              # Translator interfaces
└── Messages/                 # Error messages

FractalDataWorks.Commands.Data/               (netstandard2.0;net10.0)
├── Commands/                  # Concrete command implementations
├── Expressions/               # Expression implementations
└── Operators/                # Operator implementations

FractalDataWorks.Commands.Data.Translators/   (To be created)
├── Sql/                      # SQL translators
├── Rest/                     # REST/OData translators
└── NoSql/                    # NoSQL translators
```

---

## Additional Resources

- **Design Documents**: `docs/design/datacommands/`
- **Samples**: `samples/DataCommands/`
- **Architecture Decisions**: `ARCHITECTURE_SUMMARY.md`
- **GitHub Issues**: [Report issues](https://github.com/CyberdineDevelopment/Developer-Kit-Private/issues)

---

**Last Updated**: October 2024
**Version**: 1.0.0 (Initial Release)
