# IQueryable Entry Point in FractalDataWorks

> **⚠️ HISTORICAL DOCUMENT - CONCEPT EVOLVED**
>
> **~~DataSet as queryable collection API (IDataSet<T> : IQueryable<T>)~~** - This concept was **declined**.
>
> **Clarification**: The term "DataSet" in FractalDataWorks refers to **metadata/schema definitions** (what the data IS), not a queryable collection API like EF Core's DbSet<T>.
>
> This document remains for historical context showing the evolution of thought around LINQ integration, but the DataSet-as-collection approach was not pursued.

## The Question: What is "dbContext"?

In my generic DataCommands examples, I used:
```csharp
var query = dbContext.Customers.Where(c => c.IsActive);
```

But `dbContext` is Entity Framework Core terminology. **What's the equivalent in FractalDataWorks architecture?**

## Answer: IDataStore or DataSet<T>

Looking at the existing architecture, we have several options:

### Option 1: IDataStore (Existing Abstraction)

From the existing codebase, `IDataStore` is the abstraction for queryable data sources:

```csharp
// From FractalDataWorks.Data.DataStores.Abstractions
public interface IDataStore
{
    string Name { get; }

    /// <summary>
    /// Get queryable dataset for type T.
    /// </summary>
    IQueryable<T> GetQueryable<T>() where T : class;

    Task<IGenericResult> SaveChangesAsync(CancellationToken ct = default);
}

// Usage
IDataStore customerStore = await dataStoreProvider.GetStoreAsync("CustomerDatabase");

// This is our "dbContext" equivalent!
IQueryable<Customer> query = customerStore.GetQueryable<Customer>()
    .Where(c => c.IsActive && c.Country == "USA")
    .OrderBy(c => c.Name)
    .Skip(0)
    .Take(50);

// Convert LINQ to DataCommand
var commandResult = LinqDataCommandBuilder.FromQueryable(query);
var command = commandResult.Value; // QueryCommand<Customer>

// Execute
var result = await connection.ExecuteAsync(command);
```

### Option 2: DataSet<T> (Collection-like API)

Alternative: Provide a collection-like API that's more intuitive:

```csharp
namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// Typed dataset that provides IQueryable<T> interface.
/// This is our "DbSet<T>" equivalent.
/// </summary>
public interface IDataSet<T> : IQueryable<T> where T : class
{
    string Name { get; }

    /// <summary>
    /// Add entity to dataset (queues for insert).
    /// </summary>
    void Add(T entity);

    /// <summary>
    /// Remove entity from dataset (queues for delete).
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Update entity in dataset (queues for update).
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Execute queued changes.
    /// </summary>
    Task<IGenericResult> SaveChangesAsync(CancellationToken ct = default);
}

// Usage - looks like EF Core!
IDataSet<Customer> customers = dataStore.Set<Customer>();

IQueryable<Customer> query = customers
    .Where(c => c.IsActive && c.Country == "USA")
    .OrderBy(c => c.Name)
    .Skip(0)
    .Take(50);

// Convert to DataCommand
var commandResult = LinqDataCommandBuilder.FromQueryable(query);
var command = commandResult.Value;

// Execute
var result = await connection.ExecuteAsync(command);
```

### Option 3: DataContext (Recommended - Familiar API)

**Most user-friendly approach**: Create a `DataContext` abstraction similar to EF Core's `DbContext`:

```csharp
namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for defining typed data contexts.
/// Similar to Entity Framework's DbContext, but works across all connection types.
/// </summary>
public abstract class DataContext : IDisposable, IAsyncDisposable
{
    private readonly IDataConnectionProvider _connectionProvider;
    private readonly string _connectionName;

    protected DataContext(IDataConnectionProvider connectionProvider, string connectionName)
    {
        _connectionProvider = connectionProvider;
        _connectionName = connectionName;
    }

    /// <summary>
    /// Get dataset for entity type T.
    /// </summary>
    protected IDataSet<T> Set<T>() where T : class
    {
        return new DataSet<T>(_connectionProvider, _connectionName);
    }

    /// <summary>
    /// Execute DataCommand directly.
    /// </summary>
    public async Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
        IDataCommand<TResult> command,
        CancellationToken ct = default)
    {
        var connectionResult = await _connectionProvider.GetConnectionAsync(_connectionName, ct);
        if (!connectionResult.IsSuccess)
            return GenericResult<TResult>.Failure(connectionResult.Message);

        return await connectionResult.Value.ExecuteAsync(command, ct);
    }

    public void Dispose() { /* cleanup */ }
    public ValueTask DisposeAsync() { /* async cleanup */ return ValueTask.CompletedTask; }
}
```

### User-Defined DataContext

```csharp
namespace MyApp.Data;

/// <summary>
/// Application data context - defines all datasets.
/// Looks and feels like EF Core DbContext!
/// </summary>
public class AppDataContext : DataContext
{
    public AppDataContext(IDataConnectionProvider connectionProvider)
        : base(connectionProvider, "AppDatabase")
    {
    }

    // Define datasets (like DbSet<T> in EF Core)
    public IDataSet<Customer> Customers => Set<Customer>();
    public IDataSet<Order> Orders => Set<Order>();
    public IDataSet<Product> Products => Set<Product>();
}

// Registration in DI
services.AddScoped<AppDataContext>();
```

## Complete Flow with DataContext

```csharp
public class CustomerService
{
    private readonly AppDataContext _context;

    public CustomerService(AppDataContext context)
    {
        _context = context;
    }

    public async Task<IGenericResult<IEnumerable<Customer>>> GetActiveCustomersAsync(
        string country,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        // 1. Write LINQ query (familiar EF Core-like syntax!)
        IQueryable<Customer> linqQuery = _context.Customers
            .Where(c => c.IsActive && c.Country == country)
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(take);

        // 2. Convert LINQ to generic DataCommand (type inferred!)
        var commandResult = LinqDataCommandBuilder.FromQueryable(linqQuery);
        if (!commandResult.IsSuccess)
            return GenericResult<IEnumerable<Customer>>.Failure(commandResult.Message);

        // 3. Execute via context (type-safe!)
        var result = await _context.ExecuteAsync(commandResult.Value, ct);
        // Returns IGenericResult<IEnumerable<Customer>> - NO CASTING!

        return result;
    }

    public async Task<IGenericResult<int>> CreateCustomerAsync(
        Customer customer,
        CancellationToken ct = default)
    {
        // Create typed insert command
        var command = new InsertCommand<Customer>("Customers", customer);

        // Execute via context
        var result = await _context.ExecuteAsync(command, ct);
        // Returns IGenericResult<int> - NO CASTING!

        return result;
    }
}
```

## DataSet<T> Implementation

```csharp
namespace FractalDataWorks.Data.DataSets;

/// <summary>
/// Implementation of IDataSet<T> that provides IQueryable interface.
/// Internally builds DataCommands for execution.
/// </summary>
internal sealed class DataSet<T> : IDataSet<T> where T : class
{
    private readonly IDataConnectionProvider _connectionProvider;
    private readonly string _connectionName;
    private readonly FractalDataWorksQueryProvider _queryProvider;

    public DataSet(IDataConnectionProvider connectionProvider, string connectionName)
    {
        _connectionProvider = connectionProvider;
        _connectionName = connectionName;
        _queryProvider = new FractalDataWorksQueryProvider(connectionProvider, connectionName);
    }

    public string Name => typeof(T).Name;

    // IQueryable<T> implementation
    public Type ElementType => typeof(T);
    public Expression Expression => Expression.Constant(this);
    public IQueryProvider Provider => _queryProvider;

    public IEnumerator<T> GetEnumerator()
    {
        // Execute query when enumerated
        var command = LinqDataCommandBuilder.FromQueryable(this.AsQueryable()).Value;
        var result = _connectionProvider.GetConnectionAsync(_connectionName)
            .Result.Value
            .ExecuteAsync(command)
            .Result;

        return result.Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Modification methods
    public void Add(T entity)
    {
        // Queue insert command
        var command = new InsertCommand<T>(Name, entity);
        _queryProvider.QueueCommand(command);
    }

    public void Remove(T entity)
    {
        // Queue delete command
        var command = new DeleteCommand(Name)
        {
            Filter = BuildEntityFilter(entity)
        };
        _queryProvider.QueueCommand(command);
    }

    public void Update(T entity)
    {
        // Queue update command
        var command = new UpdateCommand<T>(Name, entity)
        {
            Filter = BuildEntityFilter(entity)
        };
        _queryProvider.QueueCommand(command);
    }

    public async Task<IGenericResult> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _queryProvider.ExecuteQueuedCommandsAsync(ct);
    }

    private IFilterExpression BuildEntityFilter(T entity)
    {
        // Build filter based on primary key
        // TODO: Use reflection or source generator to find PK properties
        var pkProperty = typeof(T).GetProperty("Id");
        var pkValue = pkProperty?.GetValue(entity);

        return new FilterExpression
        {
            Conditions = [new FilterCondition
            {
                PropertyName = pkProperty?.Name ?? "Id",
                Operator = FilterOperators.Equal,
                Value = pkValue
            }]
        };
    }
}
```

## Custom LINQ Provider (FractalDataWorksQueryProvider)

```csharp
namespace FractalDataWorks.Data.DataSets;

/// <summary>
/// Custom LINQ provider that builds DataCommands from LINQ expressions.
/// This is what makes IQueryable<T> work with our DataCommands!
/// </summary>
internal sealed class FractalDataWorksQueryProvider : IQueryProvider
{
    private readonly IDataConnectionProvider _connectionProvider;
    private readonly string _connectionName;
    private readonly List<IDataCommand> _queuedCommands = new();

    public FractalDataWorksQueryProvider(
        IDataConnectionProvider connectionProvider,
        string connectionName)
    {
        _connectionProvider = connectionProvider;
        _connectionName = connectionName;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        // Generic version handles this
        throw new NotImplementedException();
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        // Return queryable that wraps this provider
        return new DataSetQuery<TElement>(this, expression);
    }

    public object? Execute(Expression expression)
    {
        // Generic version handles this
        throw new NotImplementedException();
    }

    public TResult Execute<TResult>(Expression expression)
    {
        // Convert expression to DataCommand
        var commandResult = LinqDataCommandBuilder.FromExpression<TResult>(expression);
        if (!commandResult.IsSuccess)
            throw new InvalidOperationException(commandResult.Message?.Text);

        var command = commandResult.Value;

        // Execute command
        var connectionResult = _connectionProvider.GetConnectionAsync(_connectionName).Result;
        if (!connectionResult.IsSuccess)
            throw new InvalidOperationException(connectionResult.Message?.Text);

        var result = connectionResult.Value.ExecuteAsync(command).Result;
        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Message?.Text);

        return (TResult)result.Value;
    }

    public void QueueCommand(IDataCommand command)
    {
        _queuedCommands.Add(command);
    }

    public async Task<IGenericResult> ExecuteQueuedCommandsAsync(CancellationToken ct)
    {
        var connectionResult = await _connectionProvider.GetConnectionAsync(_connectionName, ct);
        if (!connectionResult.IsSuccess)
            return connectionResult;

        foreach (var command in _queuedCommands)
        {
            var result = await connectionResult.Value.ExecuteAsync(command, ct);
            if (!result.IsSuccess)
                return result;
        }

        _queuedCommands.Clear();
        return GenericResult.Success();
    }
}
```

## Summary: What is "dbContext"?

In FractalDataWorks architecture:

| EF Core | FractalDataWorks | Purpose |
|---------|------------------|---------|
| `DbContext` | `DataContext` | Main entry point for data access |
| `DbSet<T>` | `IDataSet<T>` | Queryable collection of entities |
| `IQueryable<T>` | `IQueryable<T>` | Same - LINQ queries |
| Entity tracking | Not implemented | Could add change tracking |

**Recommended approach**: Use `DataContext` with `IDataSet<T>` properties for familiar, EF Core-like API:

```csharp
public class AppDataContext : DataContext
{
    public IDataSet<Customer> Customers => Set<Customer>();
    public IDataSet<Order> Orders => Set<Order>();
}

// Usage - looks like EF Core!
var customers = await _context.Customers
    .Where(c => c.IsActive)
    .OrderBy(c => c.Name)
    .ToListAsync();
```

Behind the scenes, it converts to generic `DataCommand<T>` and executes through `IDataConnection` - but the user doesn't see that complexity!
