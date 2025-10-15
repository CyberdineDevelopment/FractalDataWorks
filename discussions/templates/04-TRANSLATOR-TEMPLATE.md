# Translator Project Template Documentation

## Document Purpose

This guide provides comprehensive documentation for creating FractalDataWorks Translators, which convert universal `IDataCommand` instances into domain-specific `IConnectionCommand` instances.

**Key Principle**: Translators are SEPARATE, REUSABLE components in `FractalDataWorks.Commands.Data.Translators` project.

---

## 1. What is a Translator?

### Definition

A translator is a component that converts universal `IDataCommand` objects into backend-specific `IConnectionCommand` objects. It bridges the gap between domain-agnostic data operations and specific data source protocols.

```
IDataCommand (universal)
    ↓
IDataCommandTranslator.TranslateAsync()
    ↓
IConnectionCommand (backend-specific)
```

### Examples

- **SQL Translator**: `IDataCommand` → SQL string with parameters
- **REST Translator**: `IDataCommand` → HTTP request with OData query
- **GraphQL Translator**: `IDataCommand` → GraphQL query
- **File Translator**: `IDataCommand` → File operations

### Key Responsibilities

1. **Command Translation**: Convert DataCommands to backend-specific formats
2. **Expression Walking**: Traverse filter, projection, ordering, paging expressions
3. **Type Mapping**: Map C# types to backend types
4. **Parameter Binding**: Create safe, parameterized queries
5. **Error Handling**: Return `IGenericResult<IConnectionCommand>` with validation

---

## 2. Translator Components

### Inverted Architecture: Connections Own Translators

**CRITICAL**: Translators belong in connection implementation projects, NOT in separate projects.

```
FractalDataWorks.Services.Connections.Http/
    ├── HttpConnection.cs
    ├── HttpConnectionType.cs
    ├── HttpConfiguration.cs
    └── Translators/              ← Translators live HERE!
        ├── RestTranslator.cs
        ├── GraphQLTranslator.cs
        └── GrpcTranslator.cs

FractalDataWorks.Services.Connections.MsSql/
    ├── MsSqlConnection.cs
    ├── MsSqlConnectionType.cs
    ├── MsSqlConfiguration.cs
    └── Translators/              ← Translators live HERE!
        ├── TSqlTranslator.cs
        └── SqlKataTranslator.cs
```

### Why This Structure?

**Benefits**:
1. One connection type can support multiple command languages (HTTP: REST/GraphQL/gRPC)
2. Swap implementations easily (T-SQL vs SQL Kata) via configuration
3. Shared infrastructure (HttpClientFactory for all HTTP translators)
4. Future-proof (new translators just drop in)
5. Configuration-driven selection

### Required Files per Translator

1. **Translator Class**: Implements `IDataCommandTranslator`
2. **Base Class** (optional): Extends `DataCommandTranslatorBase`
3. **Registration**: In connection type's `Register()` method
4. **Visitor Methods**: One per command type

---

## 3. IDataCommandTranslator Interface

### Interface Definition

```csharp
namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Interface for data command translators.
/// Translators convert universal IDataCommand to domain-specific IConnectionCommand.
/// </summary>
public interface IDataCommandTranslator
{
    /// <summary>
    /// Gets the domain name this translator targets (Sql, Rest, File, GraphQL, etc.).
    /// </summary>
    string DomainName { get; }

    /// <summary>
    /// Translates a data command to a connection-specific command.
    /// </summary>
    /// <param name="command">The data command to translate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the translated connection command.</returns>
    Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default);
}
```

### Base Class (Optional)

```csharp
namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Abstract base class for data command translators.
/// Used by TypeCollection source generators for compile-time discovery.
/// </summary>
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

---

## 4. Visitor Pattern (Avoid Switch Statements)

### Pattern Matching Dispatch

**Don't use switch on command type** - use pattern matching to dispatch to visitor methods:

```csharp
public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
    IDataCommand command,
    CancellationToken cancellationToken = default)
{
    return command switch
    {
        QueryCommand<object> queryCmd => await VisitQueryAsync(queryCmd, cancellationToken),
        InsertCommand<object> insertCmd => await VisitInsertAsync(insertCmd, cancellationToken),
        UpdateCommand<object> updateCmd => await VisitUpdateAsync(updateCmd, cancellationToken),
        DeleteCommand deleteCmd => await VisitDeleteAsync(deleteCmd, cancellationToken),
        UpsertCommand<object> upsertCmd => await VisitUpsertAsync(upsertCmd, cancellationToken),
        BulkInsertCommand<object> bulkCmd => await VisitBulkInsertAsync(bulkCmd, cancellationToken),
        _ => GenericResult<IConnectionCommand>.Failure(
            $"Command type '{command.GetType().Name}' is not supported by {DomainName} translator")
    };
}
```

### Visitor Methods

Each command type gets its own visitor method:

```csharp
private async Task<IGenericResult<IConnectionCommand>> VisitQueryAsync<T>(
    QueryCommand<T> command,
    CancellationToken cancellationToken)
{
    // Build SELECT statement
}

private async Task<IGenericResult<IConnectionCommand>> VisitInsertAsync<T>(
    InsertCommand<T> command,
    CancellationToken cancellationToken)
{
    // Build INSERT statement
}

private async Task<IGenericResult<IConnectionCommand>> VisitUpdateAsync<T>(
    UpdateCommand<T> command,
    CancellationToken cancellationToken)
{
    // Build UPDATE statement
}

private async Task<IGenericResult<IConnectionCommand>> VisitDeleteAsync(
    DeleteCommand command,
    CancellationToken cancellationToken)
{
    // Build DELETE statement
}
```

---

## 5. Project Dependencies

### Required Package References

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core abstractions -->
    <ProjectReference Include="..\FractalDataWorks.Commands.Data.Abstractions\FractalDataWorks.Commands.Data.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Services.Connections.Abstractions\FractalDataWorks.Services.Connections.Abstractions.csproj" />

    <!-- TypeCollection support -->
    <ProjectReference Include="..\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj" />

    <!-- Results pattern -->
    <ProjectReference Include="..\FractalDataWorks.Results\FractalDataWorks.Results.csproj" />
  </ItemGroup>
</Project>
```

### Namespace Imports

```csharp
using FractalDataWorks.Commands.Data.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Results;
using System.Threading;
using System.Threading.Tasks;
```

---

## 6. Hybrid Registration Pattern

### Runtime Registration (Primary Method)

Connections register their translators when the connection type is registered:

```csharp
namespace FractalDataWorks.Services.Connections.Http;

[ServiceTypeOption(typeof(ConnectionTypes), "Http")]
public sealed class HttpConnectionType : ConnectionTypeBase, IConnectionType
{
    public override void Register(IServiceCollection services)
    {
        // Register connection
        services.AddScoped<HttpConnection>();

        // Register HttpClientFactory (shared across all HTTP translators!)
        services.AddHttpClient("FractalDataWorks");

        // Register translators (connection brings its own!)
        DataCommandTranslators.Register("Rest", typeof(RestTranslator));
        DataCommandTranslators.Register("GraphQL", typeof(GraphQLTranslator));
        DataCommandTranslators.Register("Grpc", typeof(GrpcTranslator));
    }

    // Connection advertises which translators it supports
    public override IReadOnlyList<string> SupportedTranslators =>
        new[] { "Rest", "GraphQL", "Grpc" };
}
```

### Compile-Time Registration (Optional)

For translators you want discovered at compile time:

```csharp
[TypeOption(typeof(DataCommandTranslators), "TSql")]
public class TSqlTranslator : DataCommandTranslatorBase
{
    public TSqlTranslator()
        : base(id: 1, name: "TSql", domainName: "Sql")
    {
    }

    // Implementation...
}
```

### Hybrid Collection

The `DataCommandTranslators` class supports BOTH registration mechanisms:

```csharp
/// <summary>
/// Hybrid collection of data command translators.
/// Combines compile-time discovery (TypeCollection) with runtime registration.
/// </summary>
[TypeCollection(typeof(DataCommandTranslatorBase), typeof(IDataCommandTranslator), typeof(DataCommandTranslators))]
public abstract partial class DataCommandTranslators : TypeCollectionBase<DataCommandTranslatorBase, IDataCommandTranslator>
{
    // Runtime-registered translators (connections register these at startup)
    private static readonly ConcurrentDictionary<string, Type> _runtimeTranslators = new();

    /// <summary>
    /// Registers a translator at runtime (called by connection types).
    /// </summary>
    public static void Register(string name, Type translatorType)
    {
        if (!typeof(IDataCommandTranslator).IsAssignableFrom(translatorType))
            throw new ArgumentException($"Type {translatorType.Name} must implement IDataCommandTranslator");

        _runtimeTranslators[name] = translatorType;
    }

    /// <summary>
    /// Gets a translator type by domain name.
    /// </summary>
    public static Type? GetTranslatorType(string domainName)
    {
        if (_runtimeTranslators.TryGetValue(domainName, out var runtimeType))
            return runtimeType;

        // Future: Check compile-time translators when source generator adds them
        return null;
    }
}
```

---

## 7. Expression Translation

### FilterExpression → WHERE / $filter

**SQL Example**:
```csharp
private IGenericResult<string> BuildFilter(IFilterExpression filter)
{
    var conditions = new List<string>();

    // Build individual conditions
    foreach (var condition in filter.Conditions)
    {
        var paramName = $"p{_parameterIndex++}";

        // No switch statement - operator knows its SQL representation!
        var sqlCondition = $"{condition.PropertyName} {condition.Operator.SqlOperator} @{paramName}";
        conditions.Add(sqlCondition);

        _parameters[paramName] = condition.Value!;
    }

    // Build nested filters
    foreach (var nested in filter.NestedFilters ?? Array.Empty<IFilterExpression>())
    {
        var nestedResult = BuildFilter(nested);
        if (!nestedResult.IsSuccess)
            return nestedResult;

        conditions.Add($"({nestedResult.Value})");
    }

    // Combine with logical operator (AND / OR)
    var logic = filter.Logic == LogicalOperator.And ? " AND " : " OR ";
    return GenericResult<string>.Success(string.Join(logic, conditions));
}
```

**OData Example**:
```csharp
private IGenericResult<string> BuildODataFilter(IFilterExpression filter)
{
    var conditions = new List<string>();

    foreach (var condition in filter.Conditions)
    {
        // No switch statement - operator knows its OData representation!
        var odataCondition = $"{condition.PropertyName} {condition.Operator.ODataOperator} {condition.Operator.FormatODataValue(condition.Value)}";
        conditions.Add(odataCondition);
    }

    foreach (var nested in filter.NestedFilters ?? Array.Empty<IFilterExpression>())
    {
        var nestedResult = BuildODataFilter(nested);
        if (!nestedResult.IsSuccess)
            return nestedResult;

        conditions.Add($"({nestedResult.Value})");
    }

    var logic = filter.Logic == LogicalOperator.And ? " and " : " or ";
    return GenericResult<string>.Success(string.Join(logic, conditions));
}
```

### ProjectionExpression → SELECT / $select

**SQL Example**:
```csharp
private string BuildProjection(IProjectionExpression projection)
{
    var fields = projection.Fields.Select(f =>
    {
        if (!string.IsNullOrEmpty(f.Alias))
            return $"{f.PropertyName} AS {f.Alias}";
        return f.PropertyName;
    });

    return string.Join(", ", fields);
}
```

**OData Example**:
```csharp
private string BuildODataSelect(IProjectionExpression projection)
{
    return string.Join(",", projection.Fields.Select(f => f.PropertyName));
}
```

### OrderingExpression → ORDER BY / $orderby

**SQL Example**:
```csharp
private string BuildOrdering(IOrderingExpression ordering)
{
    var orderFields = ordering.OrderedFields.Select(f =>
    {
        var direction = f.Direction == SortDirection.Ascending ? "ASC" : "DESC";
        return $"{f.PropertyName} {direction}";
    });

    return $"ORDER BY {string.Join(", ", orderFields)}";
}
```

**OData Example**:
```csharp
private string BuildODataOrderBy(IOrderingExpression ordering)
{
    var orderFields = ordering.OrderedFields.Select(f =>
    {
        var direction = f.Direction == SortDirection.Ascending ? "asc" : "desc";
        return $"{f.PropertyName} {direction}";
    });

    return string.Join(",", orderFields);
}
```

### PagingExpression → OFFSET/FETCH / $skip/$top

**SQL Server Example**:
```csharp
private string BuildPaging(IPagingExpression paging)
{
    // SQL Server 2012+ syntax
    return $"OFFSET {paging.Skip} ROWS FETCH NEXT {paging.Take} ROWS ONLY";
}
```

**PostgreSQL Example**:
```csharp
private string BuildPaging(IPagingExpression paging)
{
    return $"LIMIT {paging.Take} OFFSET {paging.Skip}";
}
```

**OData Example**:
```csharp
private void AddPagingParameters(Dictionary<string, string> queryParams, IPagingExpression paging)
{
    queryParams["$skip"] = paging.Skip.ToString();
    queryParams["$top"] = paging.Take.ToString();
}
```

---

## 8. Operator Handling (No Switch Statements!)

### The Problem with Switch Statements

**DON'T DO THIS** (Old Pattern):
```csharp
// ❌ BAD - Switch statement anti-pattern
string sqlOp = condition.Operator switch
{
    FilterOperator.Equal => "=",
    FilterOperator.NotEqual => "<>",
    FilterOperator.GreaterThan => ">",
    FilterOperator.Contains => "LIKE",
    // ... 20 more cases
};
```

### The FilterOperators TypeCollection Solution

**DO THIS** (Current Pattern):
```csharp
// ✅ GOOD - No switch statement!
var sqlCondition = $"{condition.PropertyName} {condition.Operator.SqlOperator} @{paramName}";
var odataCondition = $"{condition.PropertyName} {condition.Operator.ODataOperator} {condition.Operator.FormatODataValue(condition.Value)}";
```

### How It Works

**Operator Base Class**:
```csharp
public abstract class FilterOperatorBase
{
    protected FilterOperatorBase(int id, string name, string sqlOperator, string odataOperator, bool requiresValue)
    {
        Id = id;
        Name = name;
        SqlOperator = sqlOperator;        // Direct property access!
        ODataOperator = odataOperator;    // No switch needed!
        RequiresValue = requiresValue;
    }

    public int Id { get; }
    public string Name { get; }
    public string SqlOperator { get; }
    public string ODataOperator { get; }
    public bool RequiresValue { get; }

    public virtual string FormatSqlParameter(string paramName) => $"@{paramName}";
    public abstract string FormatODataValue(object? value);
}
```

**Concrete Operator Example**:
```csharp
[TypeOption(typeof(FilterOperators), "Equal")]
public sealed class EqualOperator : FilterOperatorBase
{
    public EqualOperator()
        : base(
            id: 1,
            name: "Equal",
            sqlOperator: "=",      // SQL representation
            odataOperator: "eq",   // OData representation
            requiresValue: true)
    {
    }

    public override string FormatODataValue(object? value)
    {
        return value switch
        {
            string str => $"'{str.Replace("'", "''")}'",
            int or long => value.ToString()!,
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
            _ => $"'{value}'"
        };
    }
}
```

**Usage in Translator**:
```csharp
// No switch statement - just property access!
var op = FilterOperators.GetByName(condition.Operator.Name);
sql.Append($"{condition.PropertyName} {op.SqlOperator} @{paramName}");
```

### Special Operator Cases

**Operators that Don't Require Values**:
```csharp
if (!condition.Operator.RequiresValue)
{
    // IS NULL, IS NOT NULL
    return $"{condition.PropertyName} {condition.Operator.SqlOperator}";
}
```

**Operators with Custom Parameter Formatting**:
```csharp
public class ContainsOperator : FilterOperatorBase
{
    public override string FormatSqlParameter(string paramName)
    {
        // LIKE needs wildcards
        return $"'%' + @{paramName} + '%'";
    }
}
```

---

## 9. Template Parameters (For Project Templates)

### Template Variables

When creating a project template for translators, use these parameters:

```json
{
  "TranslatorName": "TSql",
  "DomainCategory": "Sql",
  "TargetFramework": "netstandard2.0",
  "IncludeExpressionVisitors": true,
  "GenerateParameterBinding": true
}
```

### Generated Class Template

```csharp
namespace FractalDataWorks.Services.Connections.{{DomainCategory}};

/// <summary>
/// Translates DataCommands to {{TranslatorName}} format.
/// </summary>
public class {{TranslatorName}}Translator : DataCommandTranslatorBase
{
    public {{TranslatorName}}Translator()
        : base(id: {{Id}}, name: "{{TranslatorName}}", domainName: "{{DomainCategory}}")
    {
    }

    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        return command switch
        {
            QueryCommand<object> queryCmd => await VisitQueryAsync(queryCmd, cancellationToken),
            InsertCommand<object> insertCmd => await VisitInsertAsync(insertCmd, cancellationToken),
            UpdateCommand<object> updateCmd => await VisitUpdateAsync(updateCmd, cancellationToken),
            DeleteCommand deleteCmd => await VisitDeleteAsync(deleteCmd, cancellationToken),
            _ => GenericResult<IConnectionCommand>.Failure(
                $"Command type '{command.GetType().Name}' is not supported")
        };
    }

    // Visitor methods...
}
```

---

## 10. Complete Examples

### SQL Translator (T-SQL)

**Full Implementation**:
```csharp
namespace FractalDataWorks.Services.Connections.MsSql.Translators;

public class TSqlTranslator : DataCommandTranslatorBase
{
    private readonly Dictionary<string, object> _parameters = new();
    private int _parameterIndex = 0;

    public TSqlTranslator()
        : base(id: 1, name: "TSql", domainName: "Sql")
    {
    }

    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        // Reset state for new translation
        _parameters.Clear();
        _parameterIndex = 0;

        return command switch
        {
            QueryCommand<object> queryCmd => await VisitQueryAsync(queryCmd, cancellationToken),
            InsertCommand<object> insertCmd => await VisitInsertAsync(insertCmd, cancellationToken),
            UpdateCommand<object> updateCmd => await VisitUpdateAsync(updateCmd, cancellationToken),
            DeleteCommand deleteCmd => await VisitDeleteAsync(deleteCmd, cancellationToken),
            _ => GenericResult<IConnectionCommand>.Failure(
                $"Command type '{command.GetType().Name}' is not supported by {DomainName} translator")
        };
    }

    private async Task<IGenericResult<IConnectionCommand>> VisitQueryAsync<T>(
        QueryCommand<T> command,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder();

        // SELECT clause
        sql.Append("SELECT ");
        if (command.Projection != null)
        {
            sql.Append(BuildProjection(command.Projection));
        }
        else
        {
            sql.Append("*");
        }

        // FROM clause
        sql.Append($" FROM [{command.ContainerName}]");

        // WHERE clause
        if (command.Filter != null)
        {
            var whereResult = BuildFilter(command.Filter);
            if (!whereResult.IsSuccess)
                return GenericResult<IConnectionCommand>.Failure(whereResult.Message);

            sql.Append($" WHERE {whereResult.Value}");
        }

        // ORDER BY clause
        if (command.Ordering != null)
        {
            sql.Append($" {BuildOrdering(command.Ordering)}");
        }

        // OFFSET/FETCH (paging)
        if (command.Paging != null)
        {
            sql.Append($" {BuildPaging(command.Paging)}");
        }

        var connectionCommand = new SqlConnectionCommand
        {
            CommandText = sql.ToString(),
            CommandType = System.Data.CommandType.Text,
            Parameters = _parameters
        };

        return await Task.FromResult(
            GenericResult<IConnectionCommand>.Success(connectionCommand));
    }

    private IGenericResult<string> BuildFilter(IFilterExpression filter)
    {
        var conditions = new List<string>();

        foreach (var condition in filter.Conditions)
        {
            var conditionResult = BuildCondition(condition);
            if (!conditionResult.IsSuccess)
                return conditionResult;

            conditions.Add(conditionResult.Value);
        }

        foreach (var nested in filter.NestedFilters ?? Array.Empty<IFilterExpression>())
        {
            var nestedResult = BuildFilter(nested);
            if (!nestedResult.IsSuccess)
                return nestedResult;

            conditions.Add($"({nestedResult.Value})");
        }

        var logic = filter.Logic == LogicalOperator.And ? " AND " : " OR ";
        return GenericResult<string>.Success(string.Join(logic, conditions));
    }

    private IGenericResult<string> BuildCondition(FilterCondition condition)
    {
        var paramName = $"p{_parameterIndex++}";

        // Handle operators that don't need values (IS NULL, IS NOT NULL)
        if (!condition.Operator.RequiresValue)
        {
            return GenericResult<string>.Success(
                $"[{condition.PropertyName}] {condition.Operator.SqlOperator}");
        }

        // Add parameter
        _parameters[paramName] = condition.Value!;

        // Format condition (operator knows its own SQL representation!)
        var formattedParam = condition.Operator.FormatSqlParameter(paramName);
        return GenericResult<string>.Success(
            $"[{condition.PropertyName}] {condition.Operator.SqlOperator} {formattedParam}");
    }

    private string BuildProjection(IProjectionExpression projection)
    {
        var fields = projection.Fields.Select(f =>
        {
            if (!string.IsNullOrEmpty(f.Alias))
                return $"[{f.PropertyName}] AS [{f.Alias}]";
            return $"[{f.PropertyName}]";
        });

        return string.Join(", ", fields);
    }

    private string BuildOrdering(IOrderingExpression ordering)
    {
        var orderFields = ordering.OrderedFields.Select(f =>
        {
            var direction = f.Direction == SortDirection.Ascending ? "ASC" : "DESC";
            return $"[{f.PropertyName}] {direction}";
        });

        return $"ORDER BY {string.Join(", ", orderFields)}";
    }

    private string BuildPaging(IPagingExpression paging)
    {
        return $"OFFSET {paging.Skip} ROWS FETCH NEXT {paging.Take} ROWS ONLY";
    }
}
```

### REST Translator (OData)

**Full Implementation**:
```csharp
namespace FractalDataWorks.Services.Connections.Http.Translators;

public class RestTranslator : DataCommandTranslatorBase
{
    public RestTranslator()
        : base(id: 2, name: "Rest", domainName: "Http")
    {
    }

    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        return command switch
        {
            QueryCommand<object> queryCmd => await VisitQueryAsync(queryCmd, cancellationToken),
            InsertCommand<object> insertCmd => await VisitInsertAsync(insertCmd, cancellationToken),
            UpdateCommand<object> updateCmd => await VisitUpdateAsync(updateCmd, cancellationToken),
            DeleteCommand deleteCmd => await VisitDeleteAsync(deleteCmd, cancellationToken),
            _ => GenericResult<IConnectionCommand>.Failure(
                $"Command type '{command.GetType().Name}' is not supported by {DomainName} translator")
        };
    }

    private async Task<IGenericResult<IConnectionCommand>> VisitQueryAsync<T>(
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
        var baseUrl = command.ContainerName;
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
            // No switch statement - operator knows its OData representation!
            var odataCondition = $"{condition.PropertyName} {condition.Operator.ODataOperator} {condition.Operator.FormatODataValue(condition.Value)}";
            conditions.Add(odataCondition);
        }

        foreach (var nested in filter.NestedFilters ?? Array.Empty<IFilterExpression>())
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
        var orderFields = ordering.OrderedFields.Select(f =>
        {
            var direction = f.Direction == SortDirection.Ascending ? "asc" : "desc";
            return $"{f.PropertyName} {direction}";
        });

        return string.Join(",", orderFields);
    }

    private async Task<IGenericResult<IConnectionCommand>> VisitInsertAsync<T>(
        InsertCommand<T> command,
        CancellationToken cancellationToken)
    {
        var connectionCommand = new HttpConnectionCommand
        {
            Url = command.ContainerName,
            Method = "POST",
            Body = JsonSerializer.Serialize(command.Data),
            ContentType = "application/json"
        };

        return await Task.FromResult(
            GenericResult<IConnectionCommand>.Success(connectionCommand));
    }
}
```

---

## 11. Common Patterns

### Expression Visitor Pattern

**Base Visitor Structure**:
```csharp
public abstract class ExpressionVisitor
{
    protected abstract string VisitPropertyReference(string propertyName);
    protected abstract string VisitParameterPlaceholder(string paramName);
    protected abstract string VisitLogicalOperator(LogicalOperator logicalOp);

    public IGenericResult<string> Visit(IFilterExpression filter)
    {
        var conditions = new List<string>();

        foreach (var condition in filter.Conditions)
        {
            var conditionResult = VisitCondition(condition);
            if (!conditionResult.IsSuccess)
                return conditionResult;

            conditions.Add(conditionResult.Value);
        }

        foreach (var nested in filter.NestedFilters ?? Array.Empty<IFilterExpression>())
        {
            var nestedResult = Visit(nested);
            if (!nestedResult.IsSuccess)
                return nestedResult;

            conditions.Add($"({nestedResult.Value})");
        }

        var logic = VisitLogicalOperator(filter.Logic);
        return GenericResult<string>.Success(string.Join(logic, conditions));
    }

    protected abstract IGenericResult<string> VisitCondition(FilterCondition condition);
}
```

### StringBuilder for SQL Generation

```csharp
private async Task<IGenericResult<IConnectionCommand>> VisitQueryAsync<T>(
    QueryCommand<T> command,
    CancellationToken cancellationToken)
{
    var sql = new StringBuilder(1024); // Pre-allocate capacity

    sql.Append("SELECT ");
    // ... build query

    return GenericResult<IConnectionCommand>.Success(new SqlConnectionCommand
    {
        CommandText = sql.ToString()
    });
}
```

### Parameter Collection

```csharp
private readonly Dictionary<string, object> _parameters = new();
private int _parameterIndex = 0;

private string AddParameter(object value)
{
    var paramName = $"p{_parameterIndex++}";
    _parameters[paramName] = value;
    return paramName;
}
```

### Error Handling for Unsupported Operations

```csharp
if (command.Aggregation != null)
{
    return GenericResult<IConnectionCommand>.Failure(
        $"Aggregation is not supported by {DomainName} translator");
}

if (command.Join != null)
{
    return GenericResult<IConnectionCommand>.Failure(
        $"Joins are not supported by {DomainName} translator");
}
```

---

## 12. Common Mistakes

### Mistake 1: Using Switch Statements on Command Type

**DON'T**:
```csharp
// ❌ BAD
if (command is QueryCommand)
{
    // ...
}
else if (command is InsertCommand)
{
    // ...
}
```

**DO**:
```csharp
// ✅ GOOD
return command switch
{
    QueryCommand<object> queryCmd => await VisitQueryAsync(queryCmd, ct),
    InsertCommand<object> insertCmd => await VisitInsertAsync(insertCmd, ct),
    _ => GenericResult<IConnectionCommand>.Failure("Unsupported command type")
};
```

### Mistake 2: Not Implementing All Command Visitors

Always implement all core command types even if just to return "not supported":

```csharp
private async Task<IGenericResult<IConnectionCommand>> VisitUpsertAsync<T>(
    UpsertCommand<T> command,
    CancellationToken cancellationToken)
{
    return GenericResult<IConnectionCommand>.Failure(
        $"Upsert is not supported by {DomainName} translator");
}
```

### Mistake 3: Incorrect Parameter Binding

**DON'T** (SQL Injection Risk):
```csharp
// ❌ BAD - Direct value concatenation
sql.Append($"WHERE Name = '{condition.Value}'");
```

**DO**:
```csharp
// ✅ GOOD - Parameterized
var paramName = AddParameter(condition.Value);
sql.Append($"WHERE Name = @{paramName}");
```

### Mistake 4: Missing Null Checks on Optional Expressions

```csharp
// ❌ BAD - Null reference exception
var whereClause = BuildFilter(command.Filter);

// ✅ GOOD - Null check
if (command.Filter != null)
{
    var whereResult = BuildFilter(command.Filter);
    if (whereResult.IsSuccess)
    {
        sql.Append($" WHERE {whereResult.Value}");
    }
}
```

### Mistake 5: Not Resetting State Between Translations

```csharp
// ❌ BAD - Parameters accumulate across translations
private readonly Dictionary<string, object> _parameters = new();

public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(...)
{
    // Parameters from previous translation still here!
}

// ✅ GOOD - Reset state
public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(...)
{
    _parameters.Clear();
    _parameterIndex = 0;
    // Now fresh for this translation
}
```

---

## 13. Testing Translators

### Unit Tests

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
                Conditions = new[]
                {
                    new FilterCondition
                    {
                        PropertyName = "IsActive",
                        Operator = FilterOperators.Equal,
                        Value = true
                    }
                }
            }
        };

        // Act
        var result = await _translator.TranslateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sqlCommand = result.Value as SqlConnectionCommand;
        sqlCommand.Should().NotBeNull();
        sqlCommand!.CommandText.Should().Contain("WHERE [IsActive] = @p0");
        sqlCommand.Parameters.Should().ContainKey("p0");
        sqlCommand.Parameters["p0"].Should().Be(true);
    }
}
```

---

## 14. Configuration-Driven Translator Selection

### Connection Configuration

```csharp
public interface IConnectionConfiguration
{
    string ConnectionName { get; }
    string ConnectionType { get; }
    string TranslatorLanguage { get; set; }  // User picks translator!
}
```

### Example Configurations

**HTTP with REST**:
```json
{
    "ConnectionName": "ProductApi",
    "ConnectionType": "Http",
    "BaseUrl": "https://api.example.com",
    "TranslatorLanguage": "Rest"
}
```

**HTTP with GraphQL**:
```json
{
    "ConnectionName": "ProductApi",
    "ConnectionType": "Http",
    "BaseUrl": "https://api.example.com/graphql",
    "TranslatorLanguage": "GraphQL"
}
```

**SQL with T-SQL**:
```json
{
    "ConnectionName": "CustomerDb",
    "ConnectionType": "Sql",
    "ConnectionString": "...",
    "TranslatorLanguage": "TSql"
}
```

**SQL with SQL Kata**:
```json
{
    "ConnectionName": "CustomerDb",
    "ConnectionType": "Sql",
    "ConnectionString": "...",
    "TranslatorLanguage": "SqlKata"
}
```

---

## 15. Summary

### Key Principles

1. **Translators belong WITH connections** - Not in separate projects
2. **No switch statements** - Use visitor pattern and operator properties
3. **Runtime registration** - Connections register their own translators
4. **Configuration-driven** - Users select translator via config
5. **Type-safe** - Generic `Connection<TTranslator>` pattern

### Benefits

- One connection type, multiple command languages
- Swap implementations without code changes
- Shared infrastructure (HttpClientFactory, connection pools)
- Future-proof (new translators just drop in)
- Configuration-driven selection

### Quick Checklist

- [ ] Translator class in `Translators/` subfolder of connection project
- [ ] Extends `DataCommandTranslatorBase`
- [ ] Implements visitor methods for each command type
- [ ] Uses operator properties (no switch statements)
- [ ] Registered in connection type's `Register()` method
- [ ] Supports configuration-driven selection
- [ ] Unit tests for all translation scenarios

---

**Last Updated**: January 2025
**Version**: 1.0.0 (Initial Release)
**Architecture**: Inverted Translator Architecture (Connections Own Translators)
