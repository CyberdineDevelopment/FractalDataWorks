# Where Do Translators Belong?

## Answer: Translators belong WITH the connections that use them!

## Architecture Principle: Connection Owns Translator

Based on the inverted translator architecture:
```
Connection<TTranslator> owns its translator
```

Each connection package provides its own translator implementation.

## Package Structure

### Core Packages (No Translators)
```
FractalDataWorks.Commands.Data.Abstractions
├── IDataCommandTranslator.cs          ← Interface ONLY
└── DataCommandTranslators.cs          ← Hybrid collection (compile-time + runtime)

FractalDataWorks.Commands.Data
└── Commands/, Operators/, Expressions/ ← Universal representations (no translation)
```

### Connection Packages (With Translators)

#### SQL Connection Package
```
FractalDataWorks.Services.Connections.Sql/
├── SqlConnection.cs
├── SqlConnectionOptions.cs
└── Translators/
    └── SqlDataCommandTranslator.cs    ← Translates DataCommand → SQL
```

**SqlDataCommandTranslator.cs:**
```csharp
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Services.Connections.Sql;

public class SqlDataCommandTranslator : DataCommandTranslatorBase
{
    public SqlDataCommandTranslator()
        : base(id: 1, name: "Sql", domainName: "Sql")
    {
    }

    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        // Translate QueryCommand → SELECT statement
        if (command is QueryCommand query)
        {
            var sql = BuildSelectStatement(query);
            var parameters = ExtractParameters(query);

            return Result.Success<IConnectionCommand>(
                new SqlCommand(sql, parameters));
        }

        // Translate InsertCommand → INSERT statement
        if (command is InsertCommand insert)
        {
            var sql = BuildInsertStatement(insert);
            var parameters = ExtractParameters(insert);

            return Result.Success<IConnectionCommand>(
                new SqlCommand(sql, parameters));
        }

        // ... handle other command types
    }

    private string BuildSelectStatement(QueryCommand query)
    {
        var builder = new StringBuilder("SELECT ");

        // Projection (SELECT clause)
        if (query.Projection != null)
        {
            builder.Append(string.Join(", ", query.Projection.Fields.Select(f => f.PropertyName)));
        }
        else
        {
            builder.Append("*");
        }

        // Container (FROM clause)
        builder.Append($" FROM {query.ContainerName}");

        // Filter (WHERE clause)
        if (query.Filter != null)
        {
            builder.Append(" WHERE ");
            builder.Append(BuildWhereClause(query.Filter));
        }

        // Ordering (ORDER BY clause)
        if (query.Ordering != null)
        {
            builder.Append(" ORDER BY ");
            var orderBy = query.Ordering.OrderedFields
                .Select(f => $"{f.PropertyName} {f.Direction.SqlKeyword}");
            builder.Append(string.Join(", ", orderBy));
        }

        // Paging (OFFSET/FETCH)
        if (query.Paging != null)
        {
            builder.Append($" OFFSET {query.Paging.Skip} ROWS");
            builder.Append($" FETCH NEXT {query.Paging.Take} ROWS ONLY");
        }

        return builder.ToString();
    }

    private string BuildWhereClause(IFilterExpression filter)
    {
        var conditions = filter.Conditions
            .Select(c => $"{c.PropertyName} {c.Operator.SqlOperator} {c.Operator.FormatSqlParameter(c.PropertyName)}");

        var logicalOp = filter.LogicalOperator?.SqlOperator ?? "AND";
        return string.Join($" {logicalOp} ", conditions);
    }
}
```

**SqlConnection.cs:**
```csharp
public class SqlConnection : Connection<SqlDataCommandTranslator>
{
    private readonly string _connectionString;

    static SqlConnection()
    {
        // Register translator for runtime discovery
        DataCommandTranslators.Register("Sql", typeof(SqlDataCommandTranslator));
    }

    public SqlConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public override async Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
        IDataCommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        // 1. Use translator to convert DataCommand → SqlCommand
        var translator = new SqlDataCommandTranslator();
        var translationResult = await translator.TranslateAsync(command, cancellationToken);

        if (!translationResult.IsSuccess)
            return Result.Failure<TResult>(translationResult.Messages);

        var sqlCommand = (SqlCommand)translationResult.Value;

        // 2. Execute SQL against database
        using var connection = new SqlConnection(_connectionString);
        using var dbCommand = new SqlCommand(sqlCommand.CommandText, connection);

        // Add parameters
        foreach (var param in sqlCommand.Parameters)
        {
            dbCommand.Parameters.AddWithValue(param.Key, param.Value);
        }

        await connection.OpenAsync(cancellationToken);

        // 3. Execute and return typed result
        using var reader = await dbCommand.ExecuteReaderAsync(cancellationToken);
        var results = MapToEntities<TResult>(reader);

        return Result.Success(results);
    }
}
```

#### HTTP/REST Connection Package
```
FractalDataWorks.Services.Connections.Http/
├── HttpConnection.cs
├── HttpConnectionOptions.cs
└── Translators/
    ├── RestDataCommandTranslator.cs       ← Translates DataCommand → OData HTTP
    └── GraphQLDataCommandTranslator.cs    ← Translates DataCommand → GraphQL HTTP
```

**RestDataCommandTranslator.cs:**
```csharp
public class RestDataCommandTranslator : DataCommandTranslatorBase
{
    public RestDataCommandTranslator()
        : base(id: 2, name: "Rest", domainName: "Http")
    {
    }

    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command is QueryCommand query)
        {
            var url = BuildODataUrl(query);

            return Result.Success<IConnectionCommand>(
                new HttpCommand(HttpMethod.Get, url));
        }

        if (command is InsertCommand insert)
        {
            var url = $"/{insert.ContainerName}";
            var body = JsonSerializer.Serialize(insert.Data);

            return Result.Success<IConnectionCommand>(
                new HttpCommand(HttpMethod.Post, url, body));
        }

        // ... handle other command types
    }

    private string BuildODataUrl(QueryCommand query)
    {
        var queryParams = new List<string>();

        // $filter
        if (query.Filter != null)
        {
            var filter = BuildODataFilter(query.Filter);
            queryParams.Add($"$filter={filter}");
        }

        // $select
        if (query.Projection != null)
        {
            var select = string.Join(",", query.Projection.Fields.Select(f => f.PropertyName));
            queryParams.Add($"$select={select}");
        }

        // $orderby
        if (query.Ordering != null)
        {
            var orderby = string.Join(",",
                query.Ordering.OrderedFields.Select(f => $"{f.PropertyName} {f.Direction.ODataKeyword}"));
            queryParams.Add($"$orderby={orderby}");
        }

        // $skip and $top
        if (query.Paging != null)
        {
            queryParams.Add($"$skip={query.Paging.Skip}");
            queryParams.Add($"$top={query.Paging.Take}");
        }

        return $"/{query.ContainerName}?{string.Join("&", queryParams)}";
    }

    private string BuildODataFilter(IFilterExpression filter)
    {
        var conditions = filter.Conditions
            .Select(c => $"{c.PropertyName} {c.Operator.ODataOperator} {c.Operator.FormatODataValue(c.Value)}");

        var logicalOp = filter.LogicalOperator?.ODataOperator ?? "and";
        return string.Join($" {logicalOp} ", conditions);
    }
}
```

#### File Connection Package
```
FractalDataWorks.Services.Connections.File/
├── FileConnection.cs
├── FileConnectionOptions.cs
└── Translators/
    └── FileDataCommandTranslator.cs       ← Translates DataCommand → File operations
```

## Registration Flow

### 1. Compile-Time Registration (via [TypeOption])
If you want translators discovered at compile time, mark them:
```csharp
[TypeOption(typeof(DataCommandTranslators), "Sql")]
public class SqlDataCommandTranslator : DataCommandTranslatorBase
{
    // ...
}
```

Source generator creates:
```csharp
public partial class DataCommandTranslators
{
    public static SqlDataCommandTranslator Sql { get; } = new();
}
```

### 2. Runtime Registration (by connections)
Connections register their translators when first loaded:
```csharp
static SqlConnection()
{
    DataCommandTranslators.Register("Sql", typeof(SqlDataCommandTranslator));
}
```

### 3. Hybrid Lookup
```csharp
// Can find both compile-time and runtime registered translators
var translatorType = DataCommandTranslators.GetTranslatorType("Sql");
var translator = (IDataCommandTranslator)Activator.CreateInstance(translatorType);
```

## Benefits of This Architecture

### ✅ Modular Packages
- Core packages have NO dependencies on specific protocols
- Connection packages are independently deployable
- NuGet: Install only what you need
  - Need SQL? → `Install-Package FractalDataWorks.Services.Connections.Sql`
  - Need REST? → `Install-Package FractalDataWorks.Services.Connections.Http`

### ✅ Domain Knowledge Isolation
- SQL translator knows SQL specifics (T-SQL, PostgreSQL, MySQL dialects)
- REST translator knows OData, JSON, HTTP specifics
- GraphQL translator knows GraphQL query language
- File translator knows CSV, JSON, XML formats

### ✅ Third-Party Extensibility
- Anyone can create a new connection package
- Just implement IDataCommandTranslator
- Register with DataCommandTranslators.Register()
- No changes to core packages needed

### ✅ Type Safety
```csharp
// Compiler ensures translator matches connection
var sqlConn = new SqlConnection<SqlDataCommandTranslator>(connString);
var httpConn = new HttpConnection<RestDataCommandTranslator>(baseUrl);

// Can't do this - compile error!
var invalid = new SqlConnection<RestDataCommandTranslator>(connString);
```

### ✅ Testing
- Test translators independently
- Mock connections without translators
- Test universal commands without any connection

## Summary

**Where do translators belong?**

→ **In the connection package that uses them!**

```
Commands.Data.Abstractions    → IDataCommandTranslator interface
Services.Connections.Sql       → SqlDataCommandTranslator implementation
Services.Connections.Http      → RestDataCommandTranslator, GraphQLTranslator
Services.Connections.File      → FileDataCommandTranslator
```

This follows the principle: **"Connection owns its translator"** from the inverted translator architecture.
