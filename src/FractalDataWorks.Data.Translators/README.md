# FractalDataWorks.Data.Translators

**Domain-specific translator implementations for universal data commands.**

## Overview

This project contains the concrete implementations of `IDataCommandTranslator` for various data store domains. Each translator converts universal `IDataCommand` objects into domain-specific `IConnectionCommand` objects that can be executed by the corresponding connection type.

### Supported Domains

- **MsSql** - Microsoft SQL Server translation (T-SQL)
- **Rest** - REST API translation (HTTP/OData)
- **GraphQL** - GraphQL API translation

### Architecture

```
IDataCommand (universal, domain-agnostic)
    ↓
Domain-Specific Translator
    ├── MsSqlDataCommandTranslator   → T-SQL
    ├── RestDataCommandTranslator    → HTTP/OData
    └── GraphQLDataCommandTranslator → GraphQL queries
    ↓
IConnectionCommand (domain-specific, executable)
```

## Project Structure

```
FractalDataWorks.Data.Translators/
├── MsSql/
│   ├── MsSqlDataCommandTranslator.cs      # SQL Server translator
│   ├── MsSqlQueryBuilder.cs               # SQL query construction
│   ├── MsSqlParameterBuilder.cs           # SQL parameter handling
│   └── Logging/
│       └── MsSqlTranslatorLog.cs          # Source-generated logging
├── Rest/
│   ├── RestDataCommandTranslator.cs       # REST API translator
│   ├── ODataQueryBuilder.cs               # OData query string construction
│   ├── HttpMethodResolver.cs              # Determine GET/POST/PUT/DELETE
│   └── Logging/
│       └── RestTranslatorLog.cs           # Source-generated logging
├── GraphQL/
│   ├── GraphQLDataCommandTranslator.cs    # GraphQL translator
│   ├── GraphQLQueryBuilder.cs             # GraphQL query construction
│   ├── GraphQLMutationBuilder.cs          # GraphQL mutation construction
│   └── Logging/
│       └── GraphQLTranslatorLog.cs        # Source-generated logging
└── FractalDataWorks.Data.Translators.csproj
```

## Translator Implementations

### MsSql Translator

Translates `IDataCommand` to T-SQL commands for Microsoft SQL Server.

**Features:**
- SELECT queries with WHERE, ORDER BY, JOIN support
- INSERT statements with identity return
- UPDATE statements with WHERE clauses
- DELETE statements with soft delete support
- MERGE (UPSERT) operations
- Bulk INSERT with table-valued parameters
- Parameter injection protection
- Query optimization hints

**Example Translation:**

```csharp
// Input: IDataCommand
var command = new QueryDataCommand
{
    ContainerName = "Users",
    Filter = new FilterExpression
    {
        Conditions = new[]
        {
            new FilterCondition
            {
                PropertyName = "Age",
                Operator = FilterOperators.GreaterThan,
                Value = 21
            }
        }
    },
    Ordering = new OrderingExpression
    {
        OrderBy = new[] { new OrderBy { PropertyName = "Name", Direction = SortDirection.Ascending } }
    },
    Paging = new PagingExpression { Skip = 0, Take = 10 }
};

// Output: SQL ConnectionCommand
// SQL: SELECT * FROM [Users] WHERE [Age] > @Age ORDER BY [Name] ASC OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY
// Parameters: { "@Age": 21 }
```

**Implementation:**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Commands.Data.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Data.Translators.MsSql;

/// <summary>
/// Translates IDataCommand to Microsoft SQL Server T-SQL commands.
/// </summary>
public sealed class MsSqlDataCommandTranslator : IDataCommandTranslator
{
    private readonly ILogger<MsSqlDataCommandTranslator> _logger;
    private readonly MsSqlQueryBuilder _queryBuilder;

    public MsSqlDataCommandTranslator(
        ILogger<MsSqlDataCommandTranslator> logger,
        MsSqlQueryBuilder queryBuilder)
    {
        _logger = logger;
        _queryBuilder = queryBuilder;
    }

    public string DomainName => "MsSql";

    public Task<IGenericResult<IConnectionCommand>> Translate(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
        {
            MsSqlTranslatorLog.CommandIsNull(_logger);
            return Task.FromResult(GenericResult<IConnectionCommand>.Failure("Command cannot be null"));
        }

        try
        {
            // Build SQL based on command type
            var sql = _queryBuilder.BuildQuery(command);
            var parameters = _queryBuilder.ExtractParameters(command);

            // Create SQL connection command
            var connectionCommand = new SqlConnectionCommand
            {
                CommandText = sql,
                Parameters = parameters,
                CommandTimeout = GetTimeout(command)
            };

            MsSqlTranslatorLog.CommandTranslated(_logger, command.ContainerName, sql);
            return Task.FromResult(GenericResult<IConnectionCommand>.Success(connectionCommand));
        }
        catch (Exception ex)
        {
            MsSqlTranslatorLog.TranslationFailed(_logger, command.ContainerName, ex.Message);
            return Task.FromResult(GenericResult<IConnectionCommand>.Failure(
                $"Failed to translate command for '{command.ContainerName}': {ex.Message}"));
        }
    }

    private static int GetTimeout(IDataCommand command)
    {
        return command.Metadata.TryGetValue("CommandTimeout", out var timeout)
            ? Convert.ToInt32(timeout)
            : 30; // Default 30 seconds
    }
}
```

**Query Builder:**

```csharp
public sealed class MsSqlQueryBuilder
{
    public string BuildQuery(IDataCommand command)
    {
        // Determine query type from command metadata
        var queryType = GetQueryType(command);

        return queryType switch
        {
            "SELECT" => BuildSelectQuery(command),
            "INSERT" => BuildInsertQuery(command),
            "UPDATE" => BuildUpdateQuery(command),
            "DELETE" => BuildDeleteQuery(command),
            "UPSERT" => BuildMergeQuery(command),
            _ => throw new NotSupportedException($"Query type '{queryType}' not supported")
        };
    }

    private string BuildSelectQuery(IDataCommand command)
    {
        var sql = new StringBuilder();
        sql.Append($"SELECT * FROM [{command.ContainerName}]");

        // Add WHERE clause
        if (command.Metadata.TryGetValue("Filter", out var filter) && filter is IFilterExpression filterExpr)
        {
            sql.Append(" WHERE ");
            sql.Append(BuildWhereClause(filterExpr));
        }

        // Add ORDER BY clause
        if (command.Metadata.TryGetValue("Ordering", out var ordering) && ordering is IOrderingExpression orderExpr)
        {
            sql.Append(" ORDER BY ");
            sql.Append(BuildOrderByClause(orderExpr));
        }

        // Add OFFSET/FETCH for paging
        if (command.Metadata.TryGetValue("Paging", out var paging) && paging is IPagingExpression pageExpr)
        {
            sql.Append($" OFFSET {pageExpr.Skip} ROWS FETCH NEXT {pageExpr.Take} ROWS ONLY");
        }

        return sql.ToString();
    }

    private string BuildWhereClause(IFilterExpression filter)
    {
        // Convert filter conditions to SQL WHERE clause
        // Example: PropertyName = "Age", Operator = GreaterThan, Value = 21
        // Result: [Age] > @Age
    }

    private string BuildOrderByClause(IOrderingExpression ordering)
    {
        // Convert ordering to SQL ORDER BY clause
        // Example: PropertyName = "Name", Direction = Ascending
        // Result: [Name] ASC
    }
}
```

### Rest Translator

Translates `IDataCommand` to HTTP/REST API requests with OData query syntax.

**Features:**
- GET requests with OData $filter, $select, $orderby, $top, $skip
- POST requests for inserts
- PATCH/PUT requests for updates
- DELETE requests
- JSON payload construction
- HTTP header management
- Authentication token injection

**Example Translation:**

```csharp
// Input: IDataCommand
var command = new QueryDataCommand
{
    ContainerName = "users",  // API endpoint
    Filter = new FilterExpression
    {
        Conditions = new[]
        {
            new FilterCondition
            {
                PropertyName = "Age",
                Operator = FilterOperators.GreaterThan,
                Value = 21
            }
        }
    },
    Paging = new PagingExpression { Skip = 0, Take = 10 }
};

// Output: HTTP ConnectionCommand
// Method: GET
// URL: /api/users?$filter=age gt 21&$top=10&$skip=0
// Headers: { "Accept": "application/json" }
```

**Implementation:**

```csharp
public sealed class RestDataCommandTranslator : IDataCommandTranslator
{
    private readonly ILogger<RestDataCommandTranslator> _logger;
    private readonly ODataQueryBuilder _queryBuilder;

    public string DomainName => "Rest";

    public Task<IGenericResult<IConnectionCommand>> Translate(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Determine HTTP method (GET/POST/PATCH/DELETE)
            var method = DetermineHttpMethod(command);

            // Build URL with OData query parameters
            var url = _queryBuilder.BuildUrl(command);

            // Build request body (for POST/PATCH)
            var body = method is "POST" or "PATCH"
                ? _queryBuilder.BuildRequestBody(command)
                : null;

            // Create HTTP connection command
            var connectionCommand = new HttpConnectionCommand
            {
                Method = method,
                Url = url,
                Body = body,
                Headers = new Dictionary<string, string>
                {
                    ["Accept"] = "application/json",
                    ["Content-Type"] = "application/json"
                }
            };

            RestTranslatorLog.CommandTranslated(_logger, method, url);
            return Task.FromResult(GenericResult<IConnectionCommand>.Success(connectionCommand));
        }
        catch (Exception ex)
        {
            RestTranslatorLog.TranslationFailed(_logger, command.ContainerName, ex.Message);
            return Task.FromResult(GenericResult<IConnectionCommand>.Failure(ex.Message));
        }
    }
}
```

### GraphQL Translator

Translates `IDataCommand` to GraphQL queries and mutations.

**Features:**
- Query generation with field selection
- Mutation generation for inserts/updates/deletes
- Variable injection
- Fragment support
- Nested object queries
- Pagination with cursor-based or offset-based approaches

**Example Translation:**

```csharp
// Input: IDataCommand
var command = new QueryDataCommand
{
    ContainerName = "users",
    Filter = new FilterExpression
    {
        Conditions = new[]
        {
            new FilterCondition
            {
                PropertyName = "age",
                Operator = FilterOperators.GreaterThan,
                Value = 21
            }
        }
    },
    Projection = new ProjectionExpression
    {
        Fields = new[] { "id", "name", "email" }
    }
};

// Output: GraphQL ConnectionCommand
// Query:
// query GetUsers($age: Int!) {
//   users(where: { age: { gt: $age } }) {
//     id
//     name
//     email
//   }
// }
// Variables: { "age": 21 }
```

**Implementation:**

```csharp
public sealed class GraphQLDataCommandTranslator : IDataCommandTranslator
{
    private readonly ILogger<GraphQLDataCommandTranslator> _logger;
    private readonly GraphQLQueryBuilder _queryBuilder;

    public string DomainName => "GraphQL";

    public Task<IGenericResult<IConnectionCommand>> Translate(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Determine operation type (query vs mutation)
            var operationType = DetermineOperationType(command);

            // Build GraphQL query/mutation
            var graphQL = operationType == "query"
                ? _queryBuilder.BuildQuery(command)
                : _queryBuilder.BuildMutation(command);

            // Extract variables
            var variables = _queryBuilder.ExtractVariables(command);

            // Create GraphQL connection command
            var connectionCommand = new GraphQLConnectionCommand
            {
                Query = graphQL,
                Variables = variables,
                OperationName = $"{operationType}_{command.ContainerName}"
            };

            GraphQLTranslatorLog.CommandTranslated(_logger, operationType, command.ContainerName);
            return Task.FromResult(GenericResult<IConnectionCommand>.Success(connectionCommand));
        }
        catch (Exception ex)
        {
            GraphQLTranslatorLog.TranslationFailed(_logger, command.ContainerName, ex.Message);
            return Task.FromResult(GenericResult<IConnectionCommand>.Failure(ex.Message));
        }
    }
}
```

## Registration

Translators are registered in the corresponding connection's `ServiceTypeOption`:

```csharp
// In FractalDataWorks.Services.Connections.MsSql
[ServiceTypeOption(typeof(ConnectionTypes), "MsSql")]
public sealed class MsSqlConnectionType : ConnectionTypeBase<...>
{
    public override void Configure(IConfiguration configuration)
    {
        // Register translator type in TypeCollection
        DataCommandTranslators.Register("MsSql", typeof(MsSqlDataCommandTranslator));
    }

    public override void Register(IServiceCollection services)
    {
        // Register translator in DI
        services.AddScoped<IDataCommandTranslator, MsSqlDataCommandTranslator>();

        // Register supporting services
        services.AddScoped<MsSqlQueryBuilder>();
        services.AddScoped<MsSqlParameterBuilder>();

        // Register connection
        services.AddScoped<IDataConnection, MsSqlConnection>();
    }
}
```

## Usage Examples

### Example 1: Using MsSql Translator

```csharp
// In DataGateway
public async Task<IGenericResult<T>> Execute<T>(IDataCommand command)
{
    // 1. Get translator
    var translator = await _translatorProvider.GetTranslator("MsSql");

    // 2. Translate to SQL
    var sqlCommand = await translator.Translate(command);
    // Result: SELECT * FROM [Users] WHERE [Age] > @Age

    // 3. Execute SQL
    return await connection.Execute<T>(sqlCommand);
}
```

### Example 2: Using Rest Translator

```csharp
// Same code, different connection
public async Task<IGenericResult<T>> Execute<T>(IDataCommand command)
{
    // 1. Get translator (determined by connection's DataStore.TranslatorType)
    var translator = await _translatorProvider.GetTranslator("Rest");

    // 2. Translate to HTTP
    var httpCommand = await translator.Translate(command);
    // Result: GET /api/users?$filter=age gt 21

    // 3. Execute HTTP request
    return await connection.Execute<T>(httpCommand);
}
```

### Example 3: Adding Custom Translator

```csharp
// 1. Implement IDataCommandTranslator
public sealed class MongoDbTranslator : IDataCommandTranslator
{
    public string DomainName => "MongoDB";

    public Task<IGenericResult<IConnectionCommand>> Translate(...)
    {
        // Convert to MongoDB query
        var mongoQuery = BuildMongoQuery(command);
        return Task.FromResult(GenericResult<IConnectionCommand>.Success(mongoQuery));
    }
}

// 2. Register in connection's ServiceTypeOption
[ServiceTypeOption(typeof(ConnectionTypes), "MongoDB")]
public sealed class MongoDbConnectionType : ConnectionTypeBase<...>
{
    public override void Configure(IConfiguration configuration)
    {
        DataCommandTranslators.Register("MongoDB", typeof(MongoDbTranslator));
    }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IDataCommandTranslator, MongoDbTranslator>();
        services.AddScoped<IDataConnection, MongoDbConnection>();
    }
}
```

## Best Practices

### Translation Design

✅ **DO**: Keep translation logic focused on single domain
✅ **DO**: Use parameterized queries to prevent injection attacks
✅ **DO**: Include detailed logging for translation operations
✅ **DO**: Return descriptive error messages
✅ **DO**: Support common operations (SELECT, INSERT, UPDATE, DELETE)

❌ **DON'T**: Mix multiple domain translations in one translator
❌ **DON'T**: Build queries with string concatenation (use parameters)
❌ **DON'T**: Throw exceptions - use Result pattern
❌ **DON'T**: Assume command metadata structure - validate first

### Performance

✅ **DO**: Cache compiled query templates when possible
✅ **DO**: Use StringBuilder for query construction
✅ **DO**: Minimize allocations in hot paths
✅ **DO**: Use ValueTask for synchronous operations

❌ **DON'T**: Perform I/O operations during translation
❌ **DON'T**: Cache connection-specific data in translator

### Security

✅ **DO**: Always use parameterized queries (SQL)
✅ **DO**: Validate and sanitize input values
✅ **DO**: Escape special characters in query strings
✅ **DO**: Limit query complexity (prevent DOS)

❌ **DON'T**: Concatenate user input into queries
❌ **DON'T**: Allow arbitrary SQL/code injection
❌ **DON'T**: Trust command metadata without validation

## Testing

### Unit Tests

```csharp
public class MsSqlTranslatorTests
{
    [Fact]
    public async Task Translate_SimpleQuery_GeneratesCorrectSql()
    {
        // Arrange
        var translator = new MsSqlDataCommandTranslator(logger, queryBuilder);
        var command = new QueryDataCommand
        {
            ContainerName = "Users",
            Filter = new FilterExpression
            {
                Conditions = new[]
                {
                    new FilterCondition
                    {
                        PropertyName = "Age",
                        Operator = FilterOperators.GreaterThan,
                        Value = 21
                    }
                }
            }
        };

        // Act
        var result = await translator.Translate(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var sqlCommand = result.Value as SqlConnectionCommand;
        sqlCommand.CommandText.ShouldContain("SELECT * FROM [Users]");
        sqlCommand.CommandText.ShouldContain("WHERE [Age] > @Age");
        sqlCommand.Parameters.ShouldContainKey("@Age");
        sqlCommand.Parameters["@Age"].ShouldBe(21);
    }

    [Fact]
    public async Task Translate_NullCommand_ReturnsFailure()
    {
        // Arrange
        var translator = new MsSqlDataCommandTranslator(logger, queryBuilder);

        // Act
        var result = await translator.Translate(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("cannot be null");
    }
}
```

## Target Frameworks

- .NET Standard 2.0
- .NET 10.0

## Dependencies

**NuGet Packages:**
- Microsoft.Extensions.Logging.Abstractions
- System.Text.Json (for REST/GraphQL JSON handling)

**Project References:**
- FractalDataWorks.Commands.Data.Abstractions - IDataCommandTranslator interface
- FractalDataWorks.Results - Result pattern
- FractalDataWorks.Services.Connections.Abstractions - IConnectionCommand

## Related Projects

- **FractalDataWorks.Commands.Data.Abstractions** - Translator interfaces and contracts
- **FractalDataWorks.Services.Data** - DataGateway that uses translators
- **FractalDataWorks.Services.Connections.MsSql** - SQL Server connection that executes translated SQL
- **FractalDataWorks.Services.Connections.Http** - HTTP connection that executes translated REST requests

---

**FractalDataWorks.Data.Translators** - Domain-specific translators for universal data command execution.
