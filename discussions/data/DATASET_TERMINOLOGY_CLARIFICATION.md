# DataSet Terminology Clarification

> **✅ ISSUE RESOLVED**
>
> This document correctly identifies and resolves the terminology confusion between:
> - ~~**IDataSet<T>** as queryable collection (EF Core DbSet-like)~~ - **DECLINED**
> - **IDataSet** as schema/metadata definition - **ACCEPTED**
>
> **Outcome**: DataSet refers to metadata/schema only. The queryable collection concept remains undefined (may be IDataCollection<T> or something else in future).

## The Mismatch

**My View of IDataSet<T>**: I used "DataSet" to mean an **EF Core DbSet-like collection** for querying:
```csharp
// My usage (like EF Core DbSet<T>)
public class AppDataContext : DataContext
{
    public IDataSet<Customer> Customers => Set<Customer>(); // Queryable collection
}

var query = _context.Customers.Where(c => c.IsActive); // IQueryable<T>
```

**Your View of DataSet**: You see "DataSet" as the **schema/metadata** of a data structure:
```csharp
// Your concept: Schema definition
public class CustomerDataSet
{
    public string Name => "Customers";
    public DataSchema Schema { get; } // Column definitions, types, constraints
    public string ContainerName { get; } // "Customers" table/collection
    public string ConnectionName { get; } // "CustomerDb"
    public DataFormat Format { get; } // Table/JSON/CSV/Parquet
}
```

**You're right** - there's a semantic conflict. "DataSet" should represent the **schema/structure/metadata** of data, not a queryable collection.

## Terminology Alignment

### What I Called "DataSet" Should Be "DataCollection" or "DataTable"

**My IDataSet<T>** was really an **IDataCollection<T>** or **IDataTable<T>**:

```csharp
// Better name: IDataCollection<T> (queryable collection of entities)
public interface IDataCollection<T> : IQueryable<T> where T : class
{
    string Name { get; }
    void Add(T entity);
    void Remove(T entity);
    void Update(T entity);
    Task<IGenericResult> SaveChangesAsync(CancellationToken ct = default);
}

// Usage in context
public class AppDataContext : DataContext
{
    public IDataCollection<Customer> Customers => Collection<Customer>();
}
```

### What You Mean by "DataSet" - Schema/Metadata Definition

**Your DataSet** represents the **structure and location** of data:

```csharp
namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// Represents the schema, structure, and metadata of a data container.
/// Defines WHAT the data looks like and WHERE it lives.
/// </summary>
public interface IDataSet
{
    /// <summary>
    /// Logical name of this dataset (e.g., "Customers", "Orders").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Physical container name (table name, file name, collection name).
    /// </summary>
    string ContainerName { get; }

    /// <summary>
    /// Connection this dataset belongs to.
    /// </summary>
    string ConnectionName { get; }

    /// <summary>
    /// Schema definition (columns, types, constraints).
    /// </summary>
    IDataSchema Schema { get; }

    /// <summary>
    /// Format of the data (Table, JSON, CSV, Parquet, etc.).
    /// </summary>
    DataFormat Format { get; }

    /// <summary>
    /// Metadata about the dataset.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Schema definition for a dataset.
/// </summary>
public interface IDataSchema
{
    /// <summary>
    /// Columns/fields in this schema.
    /// </summary>
    IReadOnlyList<IDataColumn> Columns { get; }

    /// <summary>
    /// Primary key column(s).
    /// </summary>
    IReadOnlyList<string> PrimaryKey { get; }

    /// <summary>
    /// Constraints (unique, foreign key, etc.).
    /// </summary>
    IReadOnlyList<IDataConstraint> Constraints { get; }
}

/// <summary>
/// Column/field definition.
/// </summary>
public interface IDataColumn
{
    string Name { get; }
    Type DataType { get; }
    bool IsNullable { get; }
    bool IsIdentity { get; }
    object? DefaultValue { get; }
    int? MaxLength { get; }
    int? Precision { get; }
    int? Scale { get; }
}
```

### How DataSet Ties to Configuration

**Configuration defines the dataset mapping**:

```csharp
/// <summary>
/// Dataset configuration - maps entity types to physical containers.
/// </summary>
public class DataSetConfiguration
{
    /// <summary>
    /// Entity type (e.g., typeof(Customer)).
    /// </summary>
    public Type EntityType { get; set; }

    /// <summary>
    /// Logical dataset name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Physical container name (table/collection/file).
    /// </summary>
    public string ContainerName { get; set; }

    /// <summary>
    /// Which connection this dataset belongs to.
    /// </summary>
    public string ConnectionName { get; set; }

    /// <summary>
    /// Schema definition (could be auto-discovered or manually defined).
    /// </summary>
    public IDataSchema? Schema { get; set; }

    /// <summary>
    /// Data format (for file-based datasets).
    /// </summary>
    public DataFormat Format { get; set; } = DataFormat.Table;
}

// Example configuration
services.ConfigureDataSet<Customer>(ds =>
{
    ds.Name = "Customers";
    ds.ContainerName = "Customers"; // SQL table name
    ds.ConnectionName = "CustomerDb";
    ds.Schema = new DataSchema
    {
        Columns = [
            new DataColumn { Name = "Id", DataType = typeof(int), IsIdentity = true },
            new DataColumn { Name = "Name", DataType = typeof(string), MaxLength = 200 },
            new DataColumn { Name = "Email", DataType = typeof(string), MaxLength = 100 },
            new DataColumn { Name = "IsActive", DataType = typeof(bool) }
        ],
        PrimaryKey = ["Id"]
    };
});

// Or in JSON configuration
{
    "DataSets": {
        "Customers": {
            "EntityType": "MyApp.Domain.Customer",
            "ContainerName": "Customers",
            "ConnectionName": "CustomerDb",
            "Format": "Table",
            "Schema": {
                "Columns": [
                    { "Name": "Id", "DataType": "Int32", "IsIdentity": true },
                    { "Name": "Name", "DataType": "String", "MaxLength": 200 },
                    { "Name": "Email", "DataType": "String", "MaxLength": 100 }
                ],
                "PrimaryKey": ["Id"]
            }
        },
        "OrderHistory": {
            "EntityType": "MyApp.Domain.Order",
            "ContainerName": "orders.parquet", // File-based!
            "ConnectionName": "DataLake",
            "Format": "Parquet"
        }
    }
}
```

## Corrected Architecture with Proper Terminology

### 1. IDataSet = Schema/Metadata (Your Definition)

```csharp
namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// Represents the schema and metadata of a dataset.
/// Defines WHAT the data looks like and WHERE it lives.
/// </summary>
public interface IDataSet
{
    string Name { get; }
    string ContainerName { get; }
    string ConnectionName { get; }
    IDataSchema Schema { get; }
    DataFormat Format { get; }
}

/// <summary>
/// Registry of all configured datasets.
/// </summary>
[TypeCollection(typeof(DataSetBase), typeof(IDataSet), typeof(DataSets))]
public abstract partial class DataSets : TypeCollectionBase<DataSetBase, IDataSet>
{
    // Source generator creates:
    // - DataSets.Customers (static property)
    // - DataSets.Orders (static property)
    // - DataSets.All() method
    // - DataSets.GetByName("Customers") method
}
```

### 2. IDataCollection<T> = Queryable Collection (My Original IDataSet)

```csharp
namespace FractalDataWorks.Data.Collections.Abstractions;

/// <summary>
/// Queryable collection of entities (like EF Core DbSet<T>).
/// Provides IQueryable interface for LINQ queries.
/// </summary>
public interface IDataCollection<T> : IQueryable<T> where T : class
{
    /// <summary>
    /// Dataset this collection represents.
    /// </summary>
    IDataSet DataSet { get; }

    /// <summary>
    /// Add entity (queues for insert).
    /// </summary>
    void Add(T entity);

    /// <summary>
    /// Remove entity (queues for delete).
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Update entity (queues for update).
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Execute queued changes.
    /// </summary>
    Task<IGenericResult> SaveChangesAsync(CancellationToken ct = default);
}
```

### 3. DataContext Uses Collections (Not Sets)

```csharp
namespace FractalDataWorks.Data.Contexts.Abstractions;

/// <summary>
/// Base class for data contexts (like EF Core DbContext).
/// Provides collections for querying and modifying data.
/// </summary>
public abstract class DataContext : IDisposable, IAsyncDisposable
{
    private readonly IDataConnectionProvider _connectionProvider;
    private readonly IDataSetRegistry _dataSetRegistry;

    protected DataContext(
        IDataConnectionProvider connectionProvider,
        IDataSetRegistry dataSetRegistry)
    {
        _connectionProvider = connectionProvider;
        _dataSetRegistry = dataSetRegistry;
    }

    /// <summary>
    /// Get queryable collection for entity type.
    /// Collection is backed by configured dataset.
    /// </summary>
    protected IDataCollection<T> Collection<T>() where T : class
    {
        // Get dataset configuration for entity type
        var dataSet = _dataSetRegistry.GetDataSetForEntity<T>();

        // Create collection backed by dataset
        return new DataCollection<T>(_connectionProvider, dataSet);
    }

    public void Dispose() { /* cleanup */ }
    public ValueTask DisposeAsync() { /* cleanup */ return ValueTask.CompletedTask; }
}

// User-defined context
public class AppDataContext : DataContext
{
    public AppDataContext(
        IDataConnectionProvider connectionProvider,
        IDataSetRegistry dataSetRegistry)
        : base(connectionProvider, dataSetRegistry)
    {
    }

    // Collections (queryable), not Sets (schema)
    public IDataCollection<Customer> Customers => Collection<Customer>();
    public IDataCollection<Order> Orders => Collection<Order>();
    public IDataCollection<Product> Products => Collection<Product>();
}

// Usage
var query = _context.Customers // IDataCollection<Customer> (queryable)
    .Where(c => c.IsActive)
    .OrderBy(c => c.Name);

// Behind the scenes:
// - Customers collection is backed by DataSets.Customers (schema)
// - DataSets.Customers knows: ContainerName="Customers", ConnectionName="CustomerDb"
// - Query executes against that connection/container
```

### 4. DataSet Registry (Configuration Management)

```csharp
namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// Registry of dataset configurations.
/// Maps entity types to datasets.
/// </summary>
public interface IDataSetRegistry
{
    /// <summary>
    /// Get dataset for entity type.
    /// </summary>
    IDataSet GetDataSetForEntity<T>() where T : class;

    /// <summary>
    /// Get dataset by name.
    /// </summary>
    IDataSet? GetDataSet(string name);

    /// <summary>
    /// Register dataset configuration.
    /// </summary>
    void Register<T>(DataSetConfiguration configuration) where T : class;

    /// <summary>
    /// All registered datasets.
    /// </summary>
    IEnumerable<IDataSet> GetAllDataSets();
}
```

## Complete Example: Dataset Definition and Usage

### Step 1: Define Dataset (Schema/Metadata)

```csharp
namespace MyApp.Data;

/// <summary>
/// Dataset definition for Customer entity.
/// Defines schema and where the data lives.
/// </summary>
[TypeOption(typeof(DataSets), "Customers")]
public sealed class CustomerDataSet : DataSetBase
{
    public CustomerDataSet()
        : base(
            id: 1,
            name: "Customers",
            containerName: "Customers",
            connectionName: "CustomerDb",
            format: DataFormat.Table)
    {
        Schema = new DataSchema
        {
            Columns =
            [
                new DataColumn { Name = "Id", DataType = typeof(int), IsIdentity = true },
                new DataColumn { Name = "Name", DataType = typeof(string), MaxLength = 200, IsNullable = false },
                new DataColumn { Name = "Email", DataType = typeof(string), MaxLength = 100, IsNullable = false },
                new DataColumn { Name = "Country", DataType = typeof(string), MaxLength = 50 },
                new DataColumn { Name = "IsActive", DataType = typeof(bool), DefaultValue = true }
            ],
            PrimaryKey = ["Id"]
        };
    }

    public override IDataSchema Schema { get; }
}

// Source generator creates:
// DataSets.Customers (static property pointing to CustomerDataSet)
```

### Step 2: Register Dataset with Entity Type

```csharp
// In Startup/Program.cs
services.AddDataContext<AppDataContext>(options =>
{
    // Map entity types to datasets
    options.MapEntity<Customer>(DataSets.Customers);
    options.MapEntity<Order>(DataSets.Orders);
    options.MapEntity<Product>(DataSets.Products);
});
```

### Step 3: Use in Context

```csharp
public class AppDataContext : DataContext
{
    public IDataCollection<Customer> Customers => Collection<Customer>();
    // Collection<Customer>() internally:
    // - Gets DataSets.Customers (schema definition)
    // - Knows: ContainerName="Customers", ConnectionName="CustomerDb"
    // - Creates queryable collection backed by that dataset
}

// Usage
var activeCustomers = await _context.Customers
    .Where(c => c.IsActive && c.Country == "USA")
    .OrderBy(c => c.Name)
    .ToListAsync();

// Execution:
// 1. Customers collection knows it's backed by DataSets.Customers
// 2. DataSets.Customers says: ContainerName="Customers", ConnectionName="CustomerDb"
// 3. Query builder creates: QueryCommand<Customer>("Customers")
// 4. Connection from "CustomerDb" gets translator
// 5. Translator converts to SQL: SELECT * FROM Customers WHERE IsActive=1 AND Country='USA'...
// 6. Result returned as IEnumerable<Customer>
```

## Configuration Example with Multiple Formats

```json
{
    "DataSets": {
        "Customers": {
            "EntityType": "MyApp.Domain.Customer",
            "ContainerName": "Customers",
            "ConnectionName": "CustomerDb",
            "Format": "Table",
            "Schema": {
                "Columns": [
                    { "Name": "Id", "DataType": "Int32", "IsIdentity": true },
                    { "Name": "Name", "DataType": "String", "MaxLength": 200 }
                ],
                "PrimaryKey": ["Id"]
            }
        },
        "OrderHistory": {
            "EntityType": "MyApp.Domain.OrderHistory",
            "ContainerName": "orders.parquet",
            "ConnectionName": "DataLake",
            "Format": "Parquet",
            "Schema": {
                "Columns": [
                    { "Name": "OrderId", "DataType": "Int32" },
                    { "Name": "CustomerId", "DataType": "Int32" },
                    { "Name": "OrderDate", "DataType": "DateTime" },
                    { "Name": "Total", "DataType": "Decimal", "Precision": 18, "Scale": 2 }
                ]
            }
        },
        "ProductCatalog": {
            "EntityType": "MyApp.Domain.Product",
            "ContainerName": "https://api.example.com/products",
            "ConnectionName": "ProductApi",
            "Format": "Rest",
            "TranslatorLanguage": "GraphQL"
        }
    }
}
```

## Corrected Terminology Summary

| Term | Meaning | Example |
|------|---------|---------|
| **IDataSet** | Schema/metadata definition | `CustomerDataSet` with columns, constraints |
| **IDataCollection<T>** | Queryable collection (like DbSet) | `context.Customers.Where(...)` |
| **DataContext** | Entry point (like DbContext) | `AppDataContext` with collection properties |
| **DataSetConfiguration** | Maps entity to dataset | `Customer` → `DataSets.Customers` |
| **IDataSetRegistry** | Configuration registry | Maps `typeof(Customer)` → `CustomerDataSet` |

## Project Naming Corrections

**Old (Confusing)**:
```
FractalDataWorks.Data.DataSets.Abstractions/  (was queryable collections - WRONG)
FractalDataWorks.Data.DataSets/
```

**New (Correct)**:
```
FractalDataWorks.Data.DataSets.Abstractions/       ← Schema/metadata definitions
    ├── IDataSet.cs
    ├── IDataSchema.cs
    ├── IDataColumn.cs
    ├── DataSetBase.cs
    └── DataSets.cs (TypeCollection)

FractalDataWorks.Data.Collections.Abstractions/    ← Queryable collections (NEW)
    ├── IDataCollection{T}.cs
    └── DataContext.cs

FractalDataWorks.Data.Collections/                  ← Collection implementations (NEW)
    ├── DataCollection{T}.cs
    └── FractalDataWorksQueryProvider.cs
```

## How They Connect

```
Configuration
    ↓
DataSetRegistry
    ├── Maps: typeof(Customer) → DataSets.Customers
    ├── DataSets.Customers = CustomerDataSet (schema)
    │       ├── ContainerName: "Customers"
    │       ├── ConnectionName: "CustomerDb"
    │       └── Schema: { Columns, PrimaryKey, etc. }
    │
DataContext
    ↓
Collection<Customer>()
    ├── Gets: DataSets.Customers (from registry)
    ├── Creates: IDataCollection<Customer>
    │       ├── Backed by: DataSets.Customers schema
    │       ├── Uses: "CustomerDb" connection
    │       └── Targets: "Customers" container
    │
User writes LINQ
    ↓
context.Customers.Where(c => c.IsActive)
    ↓
QueryProvider converts to QueryCommand<Customer>
    ├── ContainerName: "Customers" (from dataset)
    ├── Filter: IsActive = true
    │
Executes via connection "CustomerDb"
    ↓
Returns IEnumerable<Customer>
```

Your view is correct: **DataSet = schema/metadata**, not a queryable collection!
