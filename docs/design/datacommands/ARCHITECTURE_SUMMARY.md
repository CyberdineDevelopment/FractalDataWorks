# DataCommands Architecture Summary (FINAL)

## Document Purpose

This is the **definitive summary** of the final DataCommands architecture incorporating all improvements:
1. **Correct naming**: `FractalDataWorks.Commands.Data` (not `Data.DataCommands`)
2. **Inverted translator architecture**: Connections own their translators
3. **Generic connections**: `Connection<TTranslator>` pattern
4. **Configuration-driven**: User selects translator language in config

Read this FIRST to understand the complete, final architecture.

## Critical Architectural Decisions (FINAL)

### 1. Naming: Commands.Data (FINAL DECISION)

**Project Names**:
```
FractalDataWorks.Commands.Data.Abstractions     (netstandard2.0)
FractalDataWorks.Commands.Data                  (netstandard2.0;net10.0)
FractalDataWorks.Data.DataSets.Abstractions     (netstandard2.0)
FractalDataWorks.Data.DataSets                  (net10.0)
```

**Rationale**:
- ✅ **IDataCommand extends IGenericCommand** - Makes inheritance clear
- ✅ **Commands namespace unifies** - All command types (data, service, workflow) under one roof
- ✅ **`.Commands.Data` not `.Data.Commands`** - Commands come first (primary concern)
- ✅ **Import clarity**: `using FractalDataWorks.Commands.Data;` - clear what you're importing
- ✅ **Future-proof**: Can add `Commands.Service`, `Commands.Workflow` as siblings

**User's reasoning**: "datacommands should be usable everywhere commands are submittable as an IGenericCommand. that seems kind of broken if commands and datacommands are siblings"

**Namespace Hierarchy**:
```
FractalDataWorks
    ├── Commands
    │   ├── Abstractions (IGenericCommand - already exists)
    │   └── Data
    │       ├── Abstractions (IDataCommand : IGenericCommand)
    │       └── (implementations)
    ├── Data
    │   ├── DataSets (user-facing API)
    │   └── DataStores (existing)
    └── Services
        └── Connections (where translators live!)
```

### 2. Inverted Translator Architecture (MAJOR IMPROVEMENT)

**Before (WRONG)**:
```
Separate Translators Project
    ↓
ServiceTypeCollection registration
    ↓
Connection fetches translator from IoC
    ↓
Problem: One connection type can't support multiple languages
```

**After (CORRECT)**:
```
Connections BRING their own translators
    ↓
Translators live in connection implementation projects
    ↓
Configuration selects which translator to use
    ↓
Benefit: One HTTP connection, multiple languages (REST/GraphQL/gRPC)
```

**Key Insight**: HTTP connection could speak REST, GraphQL, gRPC - user picks via configuration!

**Project Structure**:
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

**Benefits**:
1. ✅ One connection type, multiple command languages
2. ✅ Swap implementations (T-SQL → SQL Kata) via config
3. ✅ Shared infrastructure (HttpClientFactory for all HTTP translators)
4. ✅ Future-proof (new translators just drop in)

### 3. Configuration-Driven Translator Selection

**Configuration includes translator choice**:
```csharp
public interface IConnectionConfiguration
{
    string ConnectionName { get; }
    string ConnectionType { get; }
    string TranslatorLanguage { get; set; }  // ← User picks!
}

// Example: HTTP connection
{
    "ConnectionName": "ProductApi",
    "ConnectionType": "Http",
    "BaseUrl": "https://api.example.com",
    "TranslatorLanguage": "GraphQL"  // Could be "Rest", "GraphQL", "Grpc"
}

// Example: SQL connection
{
    "ConnectionName": "CustomerDb",
    "ConnectionType": "Sql",
    "ConnectionString": "...",
    "TranslatorLanguage": "SqlKata"  // Could be "TSql", "SqlKata", "Dapper"
}
```

### 4. Hybrid Translators Collection (Compile-Time + Runtime)

**Not pure ServiceTypeCollection** - hybrid pattern:

```csharp
[TypeCollection(typeof(DataCommandTranslatorBase), typeof(IDataCommandTranslator), typeof(DataCommandTranslators))]
public abstract partial class DataCommandTranslators : TypeCollectionBase<DataCommandTranslatorBase, IDataCommandTranslator>
{
    // Source generator discovers compile-time translators (if any exist with [TypeOption])

    // ALSO: Runtime registration for connection-provided translators
    private static readonly ConcurrentDictionary<string, IDataCommandTranslator> _runtimeTranslators = new();

    /// <summary>
    /// Connections register their translators at startup.
    /// </summary>
    public static void Register(string name, IDataCommandTranslator translator)
    {
        _runtimeTranslators[name] = translator;
    }

    /// <summary>
    /// Get translator by name (checks both compile-time and runtime).
    /// </summary>
    public static IDataCommandTranslator? GetTranslator(string name)
    {
        // Check compile-time discovered first
        var translator = GetByName(name);
        if (translator != null) return translator;

        // Check runtime-registered
        return _runtimeTranslators.TryGetValue(name, out var runtimeTranslator)
            ? runtimeTranslator
            : null;
    }
}
```

### 5. Generic Connection Pattern: Connection<TTranslator>

**Connections can be generic over translator type**:

```csharp
public class HttpConnection<TTranslator> : ConnectionBase<HttpConfiguration>, IDataConnection
    where TTranslator : IDataCommandTranslator
{
    private readonly TTranslator _translator;
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpConnection(
        HttpConfiguration configuration,
        TTranslator translator,
        IHttpClientFactory httpClientFactory)
        : base(configuration)
    {
        _translator = translator;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
        IDataCommand<TResult> command,
        CancellationToken ct = default)
    {
        // Use injected translator
        var translationResult = await _translator.TranslateAsync(command, GetContainerContext(), ct);
        // ... execute HTTP request
    }
}

// Registration makes translator explicit
services.AddScoped<HttpConnection<GraphQLTranslator>>();
// OR
services.AddScoped<HttpConnection<RestTranslator>>();
```

**Benefit**: Translator type is **explicit at registration** rather than runtime string lookup!

### 6. Connection Types Register Their Translators

```csharp
[ServiceTypeOption(typeof(ConnectionTypes), "Http")]
public sealed class HttpConnectionType :
    ConnectionTypeBase<HttpConnection, HttpConfiguration, IHttpConnectionFactory>,
    IConnectionType
{
    public override void Register(IServiceCollection services)
    {
        // Register connection
        services.AddScoped<HttpConnection>();
        services.AddScoped<IHttpConnectionFactory, HttpConnectionFactory>();

        // Register HttpClientFactory (shared across all HTTP translators!)
        services.AddHttpClient("FractalDataWorks");

        // Register translators (connection brings its own!)
        DataCommandTranslators.Register("Rest", new RestTranslator());
        DataCommandTranslators.Register("GraphQL", new GraphQLTranslator());
        DataCommandTranslators.Register("Grpc", new GrpcTranslator());
    }

    // Connection advertises which translators it supports
    public override IReadOnlyList<string> SupportedTranslators => ["Rest", "GraphQL", "Grpc"];
}
```

## Complete Project Structure (FINAL)

```
FractalDataWorks.Commands.Abstractions/          ← EXISTING (has IGenericCommand)
    └── IGenericCommand.cs

FractalDataWorks.Commands.Data.Abstractions/     ← NEW (netstandard2.0)
    ├── Commands/
    │   ├── IDataCommand.cs                      ← Extends IGenericCommand
    │   ├── IDataCommand{TResult}.cs
    │   ├── IDataCommand{TResult,TInput}.cs
    │   ├── DataCommandBase.cs
    │   ├── DataCommandBase{TResult}.cs
    │   ├── DataCommandBase{TResult,TInput}.cs
    │   └── DataCommands.cs                      ← TypeCollection
    │
    ├── Operators/
    │   ├── FilterOperatorBase.cs
    │   ├── FilterOperators.cs                   ← TypeCollection
    │   ├── LogicalOperator.cs                   ← EnhancedEnum
    │   └── SortDirection.cs                     ← EnhancedEnum
    │
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
    │
    ├── Translators/
    │   ├── IDataCommandTranslator.cs
    │   ├── DataCommandTranslatorBase.cs
    │   └── DataCommandTranslators.cs            ← Hybrid collection (TypeCollection + runtime)
    │
    └── Messages/
        └── DataCommandMessages.cs

FractalDataWorks.Commands.Data/                  ← NEW (netstandard2.0;net10.0)
    ├── Commands/
    │   ├── QueryCommand{T}.cs                   ← [TypeOption]
    │   ├── InsertCommand{T}.cs
    │   ├── UpdateCommand{T}.cs
    │   ├── DeleteCommand.cs
    │   ├── UpsertCommand{T}.cs
    │   └── BulkInsertCommand{T}.cs
    │
    ├── Operators/
    │   ├── EqualOperator.cs                     ← [TypeOption]
    │   ├── NotEqualOperator.cs
    │   ├── ContainsOperator.cs
    │   └── ... (more operators)
    │
    ├── Expressions/
    │   ├── FilterExpression.cs
    │   ├── ProjectionExpression.cs
    │   ├── OrderingExpression.cs
    │   ├── PagingExpression.cs
    │   ├── AggregationExpression.cs
    │   └── JoinExpression.cs
    │
    └── Builders/
        ├── LinqDataCommandBuilder.cs
        └── LinqExpressionVisitor.cs

FractalDataWorks.Services.Connections.Http/      ← UPDATE EXISTING
    ├── HttpConnection.cs
    ├── HttpConnection{TTranslator}.cs           ← Generic version
    ├── HttpConnectionType.cs
    ├── HttpConfiguration.cs
    └── Translators/                             ← ADD THIS FOLDER
        ├── RestTranslator.cs
        ├── GraphQLTranslator.cs
        └── GrpcTranslator.cs

FractalDataWorks.Services.Connections.MsSql/     ← UPDATE EXISTING
    ├── MsSqlConnection.cs
    ├── MsSqlConnection{TTranslator}.cs
    ├── MsSqlConnectionType.cs
    ├── MsSqlConfiguration.cs
    └── Translators/                             ← ADD THIS FOLDER
        ├── TSqlTranslator.cs
        └── SqlKataTranslator.cs

FractalDataWorks.Data.DataSets.Abstractions/     ← NEW (netstandard2.0)
    ├── DataContext.cs
    ├── IDataSet{T}.cs
    └── IFractalDataWorksQueryProvider.cs

FractalDataWorks.Data.DataSets/                  ← NEW (net10.0)
    ├── DataSet{T}.cs
    ├── FractalDataWorksQueryProvider.cs
    └── DataSetQuery{T}.cs
```

## Complete Flow (FINAL)

```
User Code
    ↓
1. Configuration loaded:
   {
       "ConnectionType": "Http",
       "BaseUrl": "https://api.example.com/graphql",
       "TranslatorLanguage": "GraphQL"  ← User picks!
   }
    ↓
2. User writes LINQ:
   var query = _context.Products.Where(p => p.IsActive);
    ↓
3. DataSet<T> (implements IQueryable<T>)
    ↓
4. FractalDataWorksQueryProvider (custom LINQ provider)
    ↓
5. LinqDataCommandBuilder.FromQueryable(query)
    ↓
6. Builds: QueryCommand<Product> : IDataCommand<IEnumerable<Product>>
   {
       Filter = new FilterExpression {
           Conditions = [new FilterCondition {
               PropertyName = "IsActive",
               Operator = FilterOperators.Equal,
               Value = true
           }]
       }
   }
    ↓
7. await _context.ExecuteAsync(command)
    ↓
8. Gets connection: HttpConnection<GraphQLTranslator>
    ↓
9. HttpConnection uses injected GraphQLTranslator
    ↓
10. GraphQLTranslator.TranslateAsync(command)
    ↓
11. Builds: HttpConnectionCommand {
        Url = "https://api.example.com/graphql",
        Method = "POST",
        Body = "query { products(where: { isActive: true }) { id name } }"
    }
    ↓
12. HttpConnection.ExecuteAsync(httpCommand)
    ↓
13. Uses HttpClientFactory to execute request
    ↓
14. Returns: IGenericResult<IEnumerable<Product>>
    ↓
15. result.Value (typed as IEnumerable<Product> - NO CASTING!)
```

## Real-World Example: Swapping Translator

**Scenario**: Start with REST, switch to GraphQL later.

**Day 1: REST configuration**:
```json
{
    "ConnectionName": "ProductApi",
    "ConnectionType": "Http",
    "BaseUrl": "https://api.example.com",
    "TranslatorLanguage": "Rest"
}
```

**Result**: REST translator generates OData query:
```
GET /api/products?$filter=isActive eq true&$select=id,name&$orderby=name asc
```

**Day 30: Switch to GraphQL** (no code changes!):
```json
{
    "ConnectionName": "ProductApi",
    "ConnectionType": "Http",
    "BaseUrl": "https://api.example.com/graphql",
    "TranslatorLanguage": "GraphQL"
}
```

**Result**: GraphQL translator generates GraphQL query:
```graphql
query {
    products(where: { isActive: true }, orderBy: { name: ASC }) {
        id
        name
    }
}
```

**No code changes** - just configuration!

## Migration Path: Adding SQL Kata

**Step 1**: Create translator in connection project:
```csharp
// FractalDataWorks.Services.Connections.MsSql/Translators/SqlKataTranslator.cs

public class SqlKataTranslator : DataCommandTranslatorBase
{
    private readonly SqlKata.Compilers.SqlServerCompiler _compiler = new();

    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext context,
        CancellationToken ct)
    {
        var query = new SqlKata.Query(command.ContainerName);

        // Use SQL Kata fluent API
        if (command is QueryCommand queryCmd && queryCmd.Filter != null)
        {
            foreach (var condition in queryCmd.Filter.Conditions)
            {
                query.Where(condition.PropertyName, condition.Operator.SqlOperator, condition.Value);
            }
        }

        var compiled = _compiler.Compile(query);

        return GenericResult<IConnectionCommand>.Success(new SqlConnectionCommand
        {
            SqlText = compiled.Sql,
            Parameters = compiled.NamedBindings
        });
    }
}
```

**Step 2**: Register in connection type:
```csharp
public override void Register(IServiceCollection services)
{
    services.AddScoped<MsSqlConnection>();

    DataCommandTranslators.Register("TSql", new TSqlTranslator());
    DataCommandTranslators.Register("SqlKata", new SqlKataTranslator());  // ← Add it!
}

public override IReadOnlyList<string> SupportedTranslators => ["TSql", "SqlKata"];
```

**Step 3**: User picks in configuration:
```json
{
    "TranslatorLanguage": "SqlKata"
}
```

**Done!** No changes to connection code, command definitions, user queries, or anything else.

## Key Benefits Summary

### 1. One Connection, Multiple Languages
HTTP connection can speak REST, GraphQL, gRPC - user picks via config.

### 2. Swap Implementations Without Code Changes
SQL connection can use T-SQL, SQL Kata, Dapper - just change config.

### 3. Shared Infrastructure
All HTTP translators share HttpClientFactory - no duplication.

### 4. Future-Proof
New command language? Add a class in `Translators/` folder. No registration ceremony.

### 5. Configuration-Driven
User controls translator selection via configuration, not code.

### 6. Type-Safe
Generic `Connection<TTranslator>` makes translator type explicit at registration.

### 7. Clear Namespace Hierarchy
`Commands.Data` makes relationship to `IGenericCommand` clear.

## Agent's Question Answered

**Q**: Should I create new parallel structure alongside existing `Data.DataSets.Abstractions`?

**A**: **YES - Option A (Create New Parallel Structure)**

The existing `FractalDataWorks.Data.DataSets.Abstractions` with `IDataQuery`, `WhereClause`, etc. is an **older, incomplete implementation** from the main branch.

This new architecture is the **correct, final design**.

**Projects to CREATE**:
1. ✅ `FractalDataWorks.Commands.Data.Abstractions` (NEW - netstandard2.0)
2. ✅ `FractalDataWorks.Commands.Data` (NEW - netstandard2.0;net10.0)
3. ✅ `FractalDataWorks.Data.DataSets` (NEW implementation - net10.0)

**Projects to UPDATE** (add `Translators/` subfolder):
4. ⚠️ `FractalDataWorks.Services.Connections.Http`
5. ⚠️ `FractalDataWorks.Services.Connections.MsSql`
6. ⚠️ Any other connection types

The new system will run in parallel with the old system. The old system can be deprecated/removed later.

## Implementation Checklist

- [ ] Create `Commands.Data.Abstractions` project
- [ ] Create `Commands.Data` project
- [ ] Create `DataSets` project (new implementation)
- [ ] Update connection projects to add `Translators/` folders
- [ ] Implement hybrid `DataCommandTranslators` collection
- [ ] Add `TranslatorLanguage` to `IConnectionConfiguration`
- [ ] Implement `Connection<TTranslator>` generic pattern
- [ ] Update connection types to register translators
- [ ] Test configuration-driven translator selection

## Conclusion

This architecture provides:
- ✅ Clear naming: `Commands.Data` shows relationship to `IGenericCommand`
- ✅ Inverted control: Connections own translators
- ✅ Configuration-driven: User picks translator language
- ✅ Type-safe: Generic connections
- ✅ Shared infrastructure: HttpClientFactory for all HTTP translators
- ✅ Future-proof: New translators drop in easily
- ✅ No code changes: Swap translators via config

**This is the final, correct architecture.**
