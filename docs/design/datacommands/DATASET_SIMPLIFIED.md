# DataSet Simplified - Configuration-Driven Approach

## What You Actually Need (Not EF Core Clone)

You want to:
1. **Define datasets** via configuration (schema, location, connection)
2. **Execute DataCommands** against those datasets
3. **No DbContext/DbSet ceremony** - just direct command execution

## Simplified Architecture

### 1. DataSet = Schema + Location (Configuration Item)

```csharp
namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// Represents a configured dataset - the schema and location of data.
/// This is a CONFIGURATION ITEM, not a queryable collection.
/// </summary>
public interface IDataSet
{
    /// <summary>
    /// Dataset name (logical identifier).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Physical container name (table, file, endpoint).
    /// </summary>
    string ContainerName { get; }

    /// <summary>
    /// Connection to use for this dataset.
    /// </summary>
    string ConnectionName { get; }

    /// <summary>
    /// Schema definition.
    /// </summary>
    IDataSchema Schema { get; }

    /// <summary>
    /// Data format (Table, JSON, CSV, Parquet, Rest).
    /// </summary>
    DataFormat Format { get; }
}
```

### 2. Configuration Defines Datasets

```json
{
    "DataSets": {
        "Customers": {
            "ContainerName": "Customers",
            "ConnectionName": "CustomerDb",
            "Format": "Table",
            "Schema": {
                "Columns": [
                    { "Name": "Id", "Type": "int", "IsIdentity": true },
                    { "Name": "Name", "Type": "string", "MaxLength": 200 },
                    { "Name": "Email", "Type": "string", "MaxLength": 100 },
                    { "Name": "IsActive", "Type": "bool" }
                ],
                "PrimaryKey": ["Id"]
            }
        },
        "OrderHistory": {
            "ContainerName": "orders.parquet",
            "ConnectionName": "DataLake",
            "Format": "Parquet"
        },
        "Products": {
            "ContainerName": "https://api.example.com/products",
            "ConnectionName": "ProductApi",
            "Format": "Rest",
            "TranslatorLanguage": "GraphQL"
        }
    }
}
```

### 3. Direct Command Execution (No DbContext)

```csharp
public class CustomerService
{
    private readonly IDataConnectionProvider _connectionProvider;
    private readonly IDataSetRegistry _dataSetRegistry;

    public async Task<IGenericResult<IEnumerable<Customer>>> GetActiveCustomers()
    {
        // Get dataset configuration
        var dataset = _dataSetRegistry.GetDataSet("Customers");

        // Build command
        var command = new QueryCommand<Customer>(dataset.ContainerName)
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

        // Get connection from dataset configuration
        var connectionResult = await _connectionProvider.GetConnectionAsync(dataset.ConnectionName);
        if (!connectionResult.IsSuccess)
            return GenericResult<IEnumerable<Customer>>.Failure(connectionResult.Message);

        // Execute command
        var result = await connectionResult.Value.ExecuteAsync(command);
        return result;
    }
}
```

## Or Even Simpler: DataSet Knows How to Execute

```csharp
namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// DataSet that can execute commands directly.
/// </summary>
public interface IDataSet
{
    string Name { get; }
    string ContainerName { get; }
    string ConnectionName { get; }
    IDataSchema Schema { get; }

    /// <summary>
    /// Execute a command against this dataset.
    /// Dataset knows its connection and container.
    /// </summary>
    Task<IGenericResult<TResult>> ExecuteAsync<TResult>(
        IDataCommand<TResult> command,
        CancellationToken ct = default);
}

// Usage - even simpler!
public class CustomerService
{
    private readonly IDataSetRegistry _dataSetRegistry;

    public async Task<IGenericResult<IEnumerable<Customer>>> GetActiveCustomers()
    {
        var dataset = _dataSetRegistry.GetDataSet("Customers");

        var command = new QueryCommand<Customer>(dataset.ContainerName)
        {
            Filter = new FilterExpression { /* ... */ }
        };

        // DataSet executes the command using its configured connection
        return await dataset.ExecuteAsync(command);
    }
}
```

## DataSet as TypeOption (Configuration + Behavior)

```csharp
namespace MyApp.Data.Datasets;

/// <summary>
/// Customer dataset definition.
/// Combines configuration (schema, connection) with behavior (execute commands).
/// </summary>
[TypeOption(typeof(DataSets), "Customers")]
public sealed class CustomerDataSet : DataSetBase
{
    public CustomerDataSet(IDataConnectionProvider connectionProvider)
        : base(
            id: 1,
            name: "Customers",
            containerName: "Customers",
            connectionName: "CustomerDb",
            format: DataFormat.Table,
            connectionProvider)
    {
        Schema = new DataSchema
        {
            Columns =
            [
                new DataColumn { Name = "Id", DataType = typeof(int), IsIdentity = true },
                new DataColumn { Name = "Name", DataType = typeof(string), MaxLength = 200 },
                new DataColumn { Name = "Email", DataType = typeof(string), MaxLength = 100 },
                new DataColumn { Name = "IsActive", DataType = typeof(bool) }
            ],
            PrimaryKey = ["Id"]
        };
    }

    public override IDataSchema Schema { get; }
}

// Usage - super simple!
public class CustomerService
{
    public async Task<IGenericResult<IEnumerable<Customer>>> GetActiveCustomers()
    {
        var command = new QueryCommand<Customer>(DataSets.Customers.ContainerName)
        {
            Filter = /* ... */
        };

        // DataSet knows its connection and executes
        return await DataSets.Customers.ExecuteAsync(command);
    }
}
```

## Key Differences from My Original Proposal

| Aspect | My Original (EF Core-like) | Simplified (Your Need) |
|--------|---------------------------|------------------------|
| **Purpose** | Queryable collections (DbSet) | Schema + location config |
| **LINQ Support** | Custom query provider, IQueryable | Direct DataCommand building |
| **Context** | DataContext with properties | DataSetRegistry lookup |
| **Execution** | Via context through provider | Direct via dataset or provider |
| **Configuration** | Code-based mapping | JSON/configuration-driven |
| **Complexity** | High (custom LINQ provider) | Low (explicit commands) |

## Recommended Naming

**Projects**:
```
FractalDataWorks.DataCommands.Abstractions/    ← Commands, operators, expressions
FractalDataWorks.DataCommands/                  ← Command implementations
FractalDataWorks.DataSets.Abstractions/         ← IDataSet, IDataSchema (configuration)
FractalDataWorks.DataSets/                      ← DataSet implementations
```

**No "Collections" project needed** - you're not creating queryable collections, just executing commands against configured datasets.

## Complete Flow - Simplified

```
Configuration (JSON)
    ↓
DataSet definitions loaded
    ├── Customers → ContainerName="Customers", ConnectionName="CustomerDb"
    ├── Orders → ContainerName="Orders", ConnectionName="CustomerDb"
    └── Products → ContainerName="products.parquet", ConnectionName="DataLake"
    ↓
User code
    ├── Gets dataset: DataSets.Customers
    ├── Builds command: QueryCommand<Customer>(DataSets.Customers.ContainerName)
    ├── Executes: DataSets.Customers.ExecuteAsync(command)
    │       ↓
    │   DataSet gets connection from ConnectionProvider
    │       ↓
    │   Connection gets translator (based on config)
    │       ↓
    │   Translator converts command to SQL/REST/etc.
    │       ↓
    │   Result returned
    ↓
IEnumerable<Customer>
```

## What This Gives You

1. ✅ **Configuration-driven**: Datasets defined in JSON/config
2. ✅ **Schema metadata**: Know columns, types, constraints
3. ✅ **Connection mapping**: Dataset → Connection → Translator
4. ✅ **Direct execution**: No DbContext ceremony
5. ✅ **Type-safe commands**: Generic DataCommands
6. ✅ **Future-proof**: New datasets = new config entries

## No EF Core Cloning

- ❌ No DbContext
- ❌ No DbSet<T>
- ❌ No custom LINQ provider
- ❌ No IQueryable magic
- ✅ Just: DataSet (config) + DataCommand (explicit) = Result

Is this more aligned with what you're thinking?
