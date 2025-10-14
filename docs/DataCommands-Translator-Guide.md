# DataCommands Translator Guide

## Table of Contents
- [Introduction](#introduction)
- [Translator Architecture](#translator-architecture)
- [Creating a Basic Translator](#creating-a-basic-translator)
- [SQL Translators](#sql-translators)
- [REST/OData Translators](#restodata-translators)
- [File Translators](#file-translators)
- [Translator Organization](#translator-organization)
- [Testing Translators](#testing-translators)
- [Advanced Topics](#advanced-topics)

---

## Introduction

**Translators** convert universal `IDataCommand` instances into backend-specific `IConnectionCommand` instances. They bridge the gap between the domain-agnostic command layer and specific data source protocols.

### Key Responsibilities

1. **Command Translation**: Convert DataCommands to backend-specific formats
2. **Expression Walking**: Traverse filter, projection, ordering expressions
3. **Type Mapping**: Map C# types to backend types
4. **Parameter Binding**: Create safe, parameterized queries
5. **Error Handling**: Return `IGenericResult<IConnectionCommand>` with validation

### Translator Flow

```
IDataCommand (universal)
    ↓
IDataCommandTranslator.TranslateAsync()
    ↓
Expression Walking & Building
    ↓
IConnectionCommand (backend-specific)
```

---

## Translator Architecture

### Interface Definition

```csharp
public interface IDataCommandTranslator
{
    /// <summary>
    /// Gets the domain name this translator targets.
    /// </summary>
    string DomainName { get; }

    /// <summary>
    /// Translates a data command to a connection-specific command.
    /// </summary>
    Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default);
}
```

### Base Class

```csharp
public abstract class DataCommandTranslatorBase : IDataCommandTranslator
{
    protected DataCommandTranslatorBase(int id, string name, string domainName)
    {
        Id = id;
        Name = name;
        DomainName = domainName;
    }

    public int Id { get; }
    public string Name { get; }
    public string DomainName { get; }

    public abstract Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default);
}
```

### Hybrid Registration

Translators support both compile-time and runtime registration:

**Compile-Time (TypeCollection):**
```csharp
[TypeOption(typeof(DataCommandTranslators), "TSql")]
public class TSqlTranslator : DataCommandTranslatorBase
{
    // Discovered by source generator
}
```

**Runtime (Connection-Provided):**
```csharp
// Connection registers its translator during initialization
DataCommandTranslators.Register("GraphQL", typeof(GraphQLTranslator));
```

---

## Creating a Basic Translator

### Step 1: Define Translator Class

```csharp
using FractalDataWorks.Commands.Data.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Commands.Data.Translators.Sql;

/// <summary>
/// Translates DataCommands to T-SQL (SQL Server) commands.
/// </summary>
public class TSqlTranslator : DataCommandTranslatorBase
{
    public TSqlTranslator()
        : base(id: 1, name: "TSql", domainName: "Sql")
    {
    }

    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        // Implementation here
        return await Task.FromResult(
            GenericResult<IConnectionCommand>.Failure("Not implemented"));
    }
}
```

### Step 2: Implement Command Dispatch

Use pattern matching to dispatch to specific handlers:

```csharp
public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
    IDataCommand command,
    CancellationToken cancellationToken = default)
{
    return command switch
    {
        QueryCommand<object> queryCmd => await TranslateQueryAsync(queryCmd, cancellationToken),
        InsertCommand<object> insertCmd => await TranslateInsertAsync(insertCmd, cancellationToken),
        UpdateCommand<object> updateCmd => await TranslateUpdateAsync(updateCmd, cancellationToken),
        DeleteCommand deleteCmd => await TranslateDeleteAsync(deleteCmd, cancellationToken),
        _ => GenericResult<IConnectionCommand>.Failure(
            $"Command type '{command.GetType().Name}' is not supported by {DomainName} translator")
    };
}
```

**Note:** The `<object>` in pattern matching is a placeholder - the actual generic type is preserved.

### Step 3: Implement Query Translation

```csharp
private async Task<IGenericResult<IConnectionCommand>> TranslateQueryAsync<T>(
    QueryCommand<T> command,
    CancellationToken cancellationToken)
{
    var sqlBuilder = new StringBuilder();

    // SELECT clause
    sqlBuilder.Append("SELECT ");
    if (command.Projection != null)
    {
        sqlBuilder.Append(BuildProjection(command.Projection));
    }
    else
    {
        sqlBuilder.Append("*");
    }

    // FROM clause
    sqlBuilder.Append($" FROM {command.ContainerName}");

    // WHERE clause
    if (command.Filter != null)
    {
        var whereResult = BuildFilter(command.Filter);
        if (!whereResult.IsSuccess)
            return GenericResult<IConnectionCommand>.Failure(whereResult.Message);

        sqlBuilder.Append($" WHERE {whereResult.Value}");
    }

    // ORDER BY clause
    if (command.Ordering != null)
    {
        sqlBuilder.Append($" {BuildOrdering(command.Ordering)}");
    }

    // OFFSET/FETCH (paging)
    if (command.Paging != null)
    {
        sqlBuilder.Append($" {BuildPaging(command.Paging)}");
    }

    // Create connection command
    var connectionCommand = new SqlConnectionCommand
    {
        CommandText = sqlBuilder.ToString(),
        CommandType = System.Data.CommandType.Text,
        Parameters = _parameters // Collected during building
    };

    return await Task.FromResult(
        GenericResult<IConnectionCommand>.Success(connectionCommand));
}
```

---

## SQL Translators

### Building WHERE Clauses

```csharp
private readonly Dictionary<string, object> _parameters = new();
private int _parameterIndex = 0;

private IGenericResult<string> BuildFilter(IFilterExpression filter)
{
    var conditions = new List<string>();

    // Build individual conditions
    foreach (var condition in filter.Conditions)
    {
        var conditionResult = BuildCondition(condition);
        if (!conditionResult.IsSuccess)
            return conditionResult;

        conditions.Add(conditionResult.Value);
    }

    // Build nested filters
    foreach (var nested in filter.NestedFilters ?? [])
    {
        var nestedResult = BuildFilter(nested);
        if (!nestedResult.IsSuccess)
            return nestedResult;

        conditions.Add($"({nestedResult.Value})");
    }

    // Combine with logical operator
    var logic = filter.Logic == LogicalOperator.And ? " AND " : " OR ";
    return GenericResult<string>.Success(string.Join(logic, conditions));
}

private IGenericResult<string> BuildCondition(FilterCondition condition)
{
    var paramName = $"p{_parameterIndex++}";

    // Handle operators that don't need values
    if (!condition.Operator.RequiresValue)
    {
        return GenericResult<string>.Success(
            $"{condition.PropertyName} {condition.Operator.SqlOperator}");
    }

    // Handle special operators
    if (condition.Operator == FilterOperators.In)
    {
        return BuildInCondition(condition, paramName);
    }

    // Standard operators
    _parameters[paramName] = condition.Value!;

    var formattedParam = condition.Operator.FormatSqlParameter(paramName);
    return GenericResult<string>.Success(
        $"{condition.PropertyName} {condition.Operator.SqlOperator} {formattedParam}");
}

private IGenericResult<string> BuildInCondition(FilterCondition condition, string paramName)
{
    if (condition.Value is not IEnumerable<object> values)
    {
        return GenericResult<string>.Failure(
            "IN operator requires IEnumerable<object> value");
    }

    var paramNames = new List<string>();
    var valuesList = values.ToList();

    for (int i = 0; i < valuesList.Count; i++)
    {
        var currentParamName = $"{paramName}_{i}";
        _parameters[currentParamName] = valuesList[i];
        paramNames.Add($"@{currentParamName}");
    }

    return GenericResult<string>.Success(
        $"{condition.PropertyName} IN ({string.Join(", ", paramNames)})");
}
```

### Building SELECT Clauses

```csharp
private string BuildProjection(IProjectionExpression projection)
{
    var fields = projection.Fields
        .Select(f =>
        {
            if (!string.IsNullOrEmpty(f.Alias))
                return $"{f.PropertyName} AS {f.Alias}";
            return f.PropertyName;
        });

    return string.Join(", ", fields);
}
```

### Building ORDER BY Clauses

```csharp
private string BuildOrdering(IOrderingExpression ordering)
{
    var orderFields = ordering.OrderedFields
        .Select(f =>
        {
            var direction = f.Direction == SortDirection.Ascending ? "ASC" : "DESC";
            return $"{f.PropertyName} {direction}";
        });

    return $"ORDER BY {string.Join(", ", orderFields)}";
}
```

### Building OFFSET/FETCH Clauses

```csharp
private string BuildPaging(IPagingExpression paging)
{
    // SQL Server 2012+ syntax
    return $"OFFSET {paging.Skip} ROWS FETCH NEXT {paging.Take} ROWS ONLY";
}
```

### Complete SQL Translator Example

See: `src/FractalDataWorks.Commands.Data.Translators/Sql/TSqlTranslator.cs`

---

## REST/OData Translators

### OData Query String Building

```csharp
public class ODataTranslator : DataCommandTranslatorBase
{
    public ODataTranslator()
        : base(id: 2, name: "OData", domainName: "Rest")
    {
    }

    private async Task<IGenericResult<IConnectionCommand>> TranslateQueryAsync<T>(
        QueryCommand<T> command,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, string>();

        // $filter
        if (command.Filter != null)
        {
            var filterResult = BuildODataFilter(command.Filter);
            if (!filterResult.IsSuccess)
                return GenericResult<IConnectionCommand>.Failure(filterResult.Message);

            queryParams["$filter"] = filterResult.Value;
        }

        // $select
        if (command.Projection != null)
        {
            queryParams["$select"] = BuildODataSelect(command.Projection);
        }

        // $orderby
        if (command.Ordering != null)
        {
            queryParams["$orderby"] = BuildODataOrderBy(command.Ordering);
        }

        // $skip and $top
        if (command.Paging != null)
        {
            queryParams["$skip"] = command.Paging.Skip.ToString();
            queryParams["$top"] = command.Paging.Take.ToString();
        }

        // Build URL
        var baseUrl = command.ContainerName; // Assume container is endpoint path
        var queryString = string.Join("&",
            queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

        var fullUrl = string.IsNullOrEmpty(queryString)
            ? baseUrl
            : $"{baseUrl}?{queryString}";

        var connectionCommand = new HttpConnectionCommand
        {
            Url = fullUrl,
            Method = "GET"
        };

        return await Task.FromResult(
            GenericResult<IConnectionCommand>.Success(connectionCommand));
    }

    private IGenericResult<string> BuildODataFilter(IFilterExpression filter)
    {
        var conditions = new List<string>();

        foreach (var condition in filter.Conditions)
        {
            var propertyName = condition.PropertyName;
            var operatorStr = condition.Operator.ODataOperator;
            var value = condition.Operator.FormatODataValue(condition.Value);

            conditions.Add($"{propertyName} {operatorStr} {value}");
        }

        foreach (var nested in filter.NestedFilters ?? [])
        {
            var nestedResult = BuildODataFilter(nested);
            if (!nestedResult.IsSuccess)
                return nestedResult;

            conditions.Add($"({nestedResult.Value})");
        }

        var logic = filter.Logic == LogicalOperator.And ? " and " : " or ";
        return GenericResult<string>.Success(string.Join(logic, conditions));
    }

    private string BuildODataSelect(IProjectionExpression projection)
    {
        return string.Join(",", projection.Fields.Select(f => f.PropertyName));
    }

    private string BuildODataOrderBy(IOrderingExpression ordering)
    {
        var orderFields = ordering.OrderedFields
            .Select(f =>
            {
                var direction = f.Direction == SortDirection.Ascending ? "asc" : "desc";
                return $"{f.PropertyName} {direction}";
            });

        return string.Join(",", orderFields);
    }
}
```

### REST POST/PUT/DELETE

```csharp
private async Task<IGenericResult<IConnectionCommand>> TranslateInsertAsync<T>(
    InsertCommand<T> command,
    CancellationToken cancellationToken)
{
    var connectionCommand = new HttpConnectionCommand
    {
        Url = command.ContainerName,
        Method = "POST",
        Body = SerializeEntity(command.Data),
        ContentType = "application/json"
    };

    return await Task.FromResult(
        GenericResult<IConnectionCommand>.Success(connectionCommand));
}

private async Task<IGenericResult<IConnectionCommand>> TranslateUpdateAsync<T>(
    UpdateCommand<T> command,
    CancellationToken cancellationToken)
{
    // Assume entity has Id property for URL
    var id = GetEntityId(command.Data);

    var connectionCommand = new HttpConnectionCommand
    {
        Url = $"{command.ContainerName}/{id}",
        Method = "PUT",
        Body = SerializeEntity(command.Data),
        ContentType = "application/json"
    };

    return await Task.FromResult(
        GenericResult<IConnectionCommand>.Success(connectionCommand));
}

private async Task<IGenericResult<IConnectionCommand>> TranslateDeleteAsync(
    DeleteCommand command,
    CancellationToken cancellationToken)
{
    // Build query string from filter
    var filterQuery = "";
    if (command.Filter != null)
    {
        var filterResult = BuildODataFilter(command.Filter);
        if (!filterResult.IsSuccess)
            return GenericResult<IConnectionCommand>.Failure(filterResult.Message);

        filterQuery = $"?$filter={Uri.EscapeDataString(filterResult.Value)}";
    }

    var connectionCommand = new HttpConnectionCommand
    {
        Url = $"{command.ContainerName}{filterQuery}",
        Method = "DELETE"
    };

    return await Task.FromResult(
        GenericResult<IConnectionCommand>.Success(connectionCommand));
}
```

---

## File Translators

### JSON File Translator

```csharp
public class JsonFileTranslator : DataCommandTranslatorBase
{
    public JsonFileTranslator()
        : base(id: 3, name: "JsonFile", domainName: "File")
    {
    }

    private async Task<IGenericResult<IConnectionCommand>> TranslateQueryAsync<T>(
        QueryCommand<T> command,
        CancellationToken cancellationToken)
    {
        // File translator passes expressions to connection for in-memory filtering
        var connectionCommand = new FileConnectionCommand
        {
            FilePath = command.ContainerName, // Container is file path
            Operation = FileOperation.Read,
            FilterExpression = command.Filter,
            ProjectionExpression = command.Projection,
            OrderingExpression = command.Ordering,
            PagingExpression = command.Paging
        };

        return await Task.FromResult(
            GenericResult<IConnectionCommand>.Success(connectionCommand));
    }

    private async Task<IGenericResult<IConnectionCommand>> TranslateInsertAsync<T>(
        InsertCommand<T> command,
        CancellationToken cancellationToken)
    {
        var connectionCommand = new FileConnectionCommand
        {
            FilePath = command.ContainerName,
            Operation = FileOperation.Append,
            Data = command.Data
        };

        return await Task.FromResult(
            GenericResult<IConnectionCommand>.Success(connectionCommand));
    }
}
```

**Key Difference:** File translators often pass expressions to the connection for in-memory evaluation, since files don't have query engines.

---

## Translator Organization

### Recommended Project Structure

```
FractalDataWorks.Commands.Data.Translators/
├── Common/
│   ├── TranslatorBase.cs
│   ├── ExpressionWalker.cs
│   └── TypeMapper.cs
├── Sql/
│   ├── SqlTranslatorBase.cs
│   ├── TSqlTranslator.cs         [TypeOption("TSql")]
│   ├── PostgreSqlTranslator.cs   [TypeOption("PostgreSql")]
│   ├── MySqlTranslator.cs        [TypeOption("MySql")]
│   └── SqliteTranslator.cs       [TypeOption("Sqlite")]
├── Rest/
│   ├── ODataTranslator.cs        [TypeOption("OData")]
│   └── GraphQLTranslator.cs      [TypeOption("GraphQL")]
└── NoSql/
    ├── MongoDbTranslator.cs      [TypeOption("MongoDb")]
    └── CosmosDbTranslator.cs     [TypeOption("CosmosDb")]
```

### Shared Translation Logic

```csharp
// Common/ExpressionWalker.cs
public abstract class ExpressionWalker
{
    protected abstract string GetPropertyReference(string propertyName);
    protected abstract string GetParameterPlaceholder(string paramName);

    public IGenericResult<string> WalkFilter(IFilterExpression filter)
    {
        // Shared logic for walking filter expressions
        // Override methods provide backend-specific formatting
    }
}

// Sql/SqlExpressionWalker.cs
public class SqlExpressionWalker : ExpressionWalker
{
    protected override string GetPropertyReference(string propertyName)
        => $"[{propertyName}]"; // SQL Server brackets

    protected override string GetParameterPlaceholder(string paramName)
        => $"@{paramName}";
}

// Rest/ODataExpressionWalker.cs
public class ODataExpressionWalker : ExpressionWalker
{
    protected override string GetPropertyReference(string propertyName)
        => propertyName; // No escaping needed

    protected override string GetParameterPlaceholder(string paramName)
        => throw new NotSupportedException("OData uses inline values");
}
```

---

## Testing Translators

### Unit Testing Translation Logic

```csharp
using Xunit;
using FluentAssertions;

public class TSqlTranslatorTests
{
    private readonly TSqlTranslator _translator = new();

    [Fact]
    public async Task TranslateQuery_WithSimpleFilter_GeneratesCorrectSql()
    {
        // Arrange
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

        // Act
        var result = await _translator.TranslateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sqlCommand = result.Value as SqlConnectionCommand;
        sqlCommand.Should().NotBeNull();
        sqlCommand!.CommandText.Should().Contain("WHERE IsActive = @");
        sqlCommand.Parameters.Should().ContainKey("p0");
        sqlCommand.Parameters["p0"].Should().Be(true);
    }

    [Fact]
    public async Task TranslateQuery_WithComplexFilter_GeneratesCorrectSql()
    {
        // Arrange
        var command = new QueryCommand<Customer>("Customers")
        {
            Filter = new FilterExpression
            {
                Logic = LogicalOperator.Or,
                Conditions =
                [
                    new FilterCondition
                    {
                        PropertyName = "Status",
                        Operator = FilterOperators.Equal,
                        Value = "Premium"
                    }
                ],
                NestedFilters =
                [
                    new FilterExpression
                    {
                        Logic = LogicalOperator.And,
                        Conditions =
                        [
                            new FilterCondition
                            {
                                PropertyName = "TotalOrders",
                                Operator = FilterOperators.GreaterThan,
                                Value = 100
                            },
                            new FilterCondition
                            {
                                PropertyName = "LastOrderDate",
                                Operator = FilterOperators.GreaterThan,
                                Value = new DateTime(2024, 1, 1)
                            }
                        ]
                    }
                ]
            }
        };

        // Act
        var result = await _translator.TranslateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sqlCommand = result.Value as SqlConnectionCommand;
        sqlCommand!.CommandText.Should().Contain("WHERE Status = @p0 OR (TotalOrders > @p1 AND LastOrderDate > @p2)");
    }

    [Fact]
    public async Task TranslateQuery_WithPaging_GeneratesOffsetFetch()
    {
        // Arrange
        var command = new QueryCommand<Customer>("Customers")
        {
            Paging = new PagingExpression { Skip = 20, Take = 10 }
        };

        // Act
        var result = await _translator.TranslateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sqlCommand = result.Value as SqlConnectionCommand;
        sqlCommand!.CommandText.Should().Contain("OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY");
    }
}
```

### Integration Testing with Real Connections

```csharp
public class TSqlTranslatorIntegrationTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;
    private readonly TSqlTranslator _translator = new();

    public TSqlTranslatorIntegrationTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EndToEnd_QueryWithFilter_ReturnsExpectedResults()
    {
        // Arrange
        await _fixture.SeedDataAsync();

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

        // Act
        var translateResult = await _translator.TranslateAsync(command);
        translateResult.IsSuccess.Should().BeTrue();

        var sqlCommand = translateResult.Value as SqlConnectionCommand;
        var executeResult = await _fixture.Connection.ExecuteAsync(sqlCommand!);

        // Assert
        executeResult.IsSuccess.Should().BeTrue();
        var customers = executeResult.Value as IEnumerable<Customer>;
        customers.Should().NotBeEmpty();
        customers.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());
    }
}
```

---

## Advanced Topics

### Dialect-Specific Translation

Handle different SQL dialects:

```csharp
public abstract class SqlTranslatorBase : DataCommandTranslatorBase
{
    protected abstract string BuildPagingClause(IPagingExpression paging);
    protected abstract string EscapeIdentifier(string identifier);

    protected SqlTranslatorBase(int id, string name, string domainName)
        : base(id, name, domainName)
    {
    }
}

// SQL Server
public class TSqlTranslator : SqlTranslatorBase
{
    protected override string BuildPagingClause(IPagingExpression paging)
        => $"OFFSET {paging.Skip} ROWS FETCH NEXT {paging.Take} ROWS ONLY";

    protected override string EscapeIdentifier(string identifier)
        => $"[{identifier}]";
}

// PostgreSQL
public class PostgreSqlTranslator : SqlTranslatorBase
{
    protected override string BuildPagingClause(IPagingExpression paging)
        => $"LIMIT {paging.Take} OFFSET {paging.Skip}";

    protected override string EscapeIdentifier(string identifier)
        => $"\"{identifier}\"";
}

// MySQL
public class MySqlTranslator : SqlTranslatorBase
{
    protected override string BuildPagingClause(IPagingExpression paging)
        => $"LIMIT {paging.Take} OFFSET {paging.Skip}";

    protected override string EscapeIdentifier(string identifier)
        => $"`{identifier}`";
}
```

### Type Mapping

Map C# types to backend types:

```csharp
public class SqlTypeMapper
{
    private static readonly Dictionary<Type, string> _typeMap = new()
    {
        [typeof(int)] = "INT",
        [typeof(long)] = "BIGINT",
        [typeof(string)] = "NVARCHAR(MAX)",
        [typeof(DateTime)] = "DATETIME2",
        [typeof(bool)] = "BIT",
        [typeof(decimal)] = "DECIMAL(18,2)",
        [typeof(Guid)] = "UNIQUEIDENTIFIER"
    };

    public string GetSqlType(Type clrType)
    {
        if (_typeMap.TryGetValue(clrType, out var sqlType))
            return sqlType;

        throw new NotSupportedException($"Type {clrType.Name} is not supported");
    }
}
```

### Query Optimization Hints

Add translator-specific optimization hints:

```csharp
public class OptimizedQueryCommand<T> : QueryCommand<T>
{
    public Dictionary<string, object> TranslatorHints { get; init; } = new();
}

// Usage
var command = new OptimizedQueryCommand<Customer>("Customers")
{
    TranslatorHints =
    {
        ["ForceIndex"] = "IX_Customer_Email",
        ["NoLock"] = true,
        ["MaxDop"] = 4
    }
};

// Translator uses hints
if (command is OptimizedQueryCommand<T> optimized)
{
    if (optimized.TranslatorHints.TryGetValue("ForceIndex", out var index))
    {
        sqlBuilder.Append($" WITH (INDEX({index}))");
    }
}
```

### Caching Translated Commands

Cache frequently-used translations:

```csharp
public class CachedTranslator : IDataCommandTranslator
{
    private readonly IDataCommandTranslator _innerTranslator;
    private readonly IMemoryCache _cache;

    public CachedTranslator(IDataCommandTranslator innerTranslator, IMemoryCache cache)
    {
        _innerTranslator = innerTranslator;
        _cache = cache;
    }

    public async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey(command);

        if (_cache.TryGetValue<IConnectionCommand>(cacheKey, out var cached))
        {
            return GenericResult<IConnectionCommand>.Success(cached!);
        }

        var result = await _innerTranslator.TranslateAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromMinutes(10));
        }

        return result;
    }

    private string GenerateCacheKey(IDataCommand command)
    {
        // Generate hash based on command structure
        // Be careful with parameter values!
    }
}
```

---

## Best Practices

### 1. Use Parameterized Queries

```csharp
// ✅ Good - parameterized
WHERE Email = @email

// ❌ Bad - SQL injection risk
WHERE Email = 'user@example.com'
```

### 2. Validate Commands Before Translation

```csharp
public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
    IDataCommand command,
    CancellationToken cancellationToken = default)
{
    // Validate command structure
    if (string.IsNullOrWhiteSpace(command.ContainerName))
    {
        return GenericResult<IConnectionCommand>.Failure(
            "Container name is required");
    }

    // Proceed with translation
    return command switch { /* ... */ };
}
```

### 3. Handle Unsupported Features Gracefully

```csharp
if (command.Aggregation != null && command.Aggregation.Aggregates.Any(a => a.Function == AggregateFunction.Median))
{
    return GenericResult<IConnectionCommand>.Failure(
        $"MEDIAN aggregate is not supported by {DomainName}");
}
```

### 4. Log Translation for Debugging

```csharp
_logger.LogDebug("Translating {CommandType} for {Container}: {Sql}",
    command.GetType().Name,
    command.ContainerName,
    sqlBuilder.ToString());
```

### 5. Optimize for Common Patterns

```csharp
// Cache compiled regex patterns
private static readonly Regex PropertyNamePattern =
    new(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

// Reuse StringBuilder
private readonly StringBuilder _sqlBuilder = new(1024);
```

---

## Next Steps

- **[Examples](DataCommands-Examples.md)** - See complete translator examples
- **[API Reference](../src/FractalDataWorks.Commands.Data.Translators/README.md)** - Translator API docs
- **Sample Translators**: `src/FractalDataWorks.Commands.Data.Translators/`

---

**Last Updated**: October 2024
**Version**: 1.0.0 (Initial Release)
