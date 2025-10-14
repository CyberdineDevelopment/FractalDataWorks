# DataCommands Visual Structure

## The Big Picture: Repository and Worktrees

```
D:\FractalDataworks\
│
├── Developer-Kit-private/           ← Main repo (develop branch)
│   ├── src/
│   │   ├── FractalDataWorks.Collections/
│   │   ├── FractalDataWorks.ServiceTypes/
│   │   ├── FractalDataWorks.Results/
│   │   ├── FractalDataWorks.Services.Connections.Abstractions/
│   │   └── ... (existing projects)
│   │
│   └── tests/
│       └── ... (existing test projects)
│
├── Developer-Kit-datacommands/      ← THIS WORKTREE (feature/datacommands-architecture)
│   │
│   ├── CONTINUATION_GUIDE.md        ← What to implement
│   ├── IMPLEMENTATION_DETAILS.md    ← How to implement
│   ├── DATACOMMANDS_ARCHITECTURE.md ← Main architecture
│   ├── TYPECOLLECTIONS_FIX.md       ← TypeCollections patterns
│   ├── GENERIC_DATACOMMANDS_ULTRATHINK.md ← Generics design
│   ├── QUERYABLE_ENTRY_POINT.md     ← DataContext design
│   │
│   ├── src/                         ← NEW PROJECTS TO CREATE
│   │   ├── FractalDataWorks.Data.DataCommands.Abstractions/
│   │   ├── FractalDataWorks.Data.DataCommands/
│   │   ├── FractalDataWorks.Data.DataCommands.Translators/
│   │   ├── FractalDataWorks.Data.DataSets.Abstractions/
│   │   └── FractalDataWorks.Data.DataSets/
│   │
│   └── tests/                       ← NEW TEST PROJECTS TO CREATE
│       ├── FractalDataWorks.Data.DataCommands.Abstractions.Tests/
│       ├── FractalDataWorks.Data.DataCommands.Tests/
│       ├── FractalDataWorks.Data.DataCommands.Translators.Tests/
│       ├── FractalDataWorks.Data.DataSets.Abstractions.Tests/
│       └── FractalDataWorks.Data.DataSets.Tests/
│
├── Developer-Kit-configuration/     ← Configuration worktree
├── Developer-Kit-scheduling/        ← Scheduling worktree
├── Developer-Kit-transformations/   ← Transformations worktree
├── Developer-Kit-fastendpoints/     ← FastEndpoints worktree
└── Developer-Kit-config-ui/         ← Config UI Generator worktree
```

## Inside DataCommands Worktree: Project Structure

```
Developer-Kit-datacommands/src/
│
├─── FractalDataWorks.Data.DataCommands.Abstractions/    (netstandard2.0 ONLY)
│    │
│    ├── Commands/
│    │   ├── IDataCommand.cs                    ← Non-generic marker interface
│    │   ├── IDataCommand{TResult}.cs           ← Generic with typed result
│    │   ├── IDataCommand{TResult,TInput}.cs    ← Generic with typed input & result
│    │   ├── DataCommandBase.cs                 ← Non-generic base (for TypeCollection)
│    │   ├── DataCommandBase{TResult}.cs        ← Generic base
│    │   ├── DataCommandBase{TResult,TInput}.cs ← Generic base with input
│    │   └── DataCommands.cs                    ← TypeCollection [TypeCollection(...)]
│    │
│    ├── Operators/
│    │   ├── FilterOperatorBase.cs              ← Base for all operators
│    │   ├── FilterOperators.cs                 ← TypeCollection [TypeCollection(...)]
│    │   ├── LogicalOperator.cs                 ← EnhancedEnum (And/Or)
│    │   └── SortDirection.cs                   ← EnhancedEnum (Asc/Desc)
│    │
│    ├── Expressions/
│    │   ├── IFilterExpression.cs               ← WHERE clause representation
│    │   ├── FilterCondition.cs                 ← Single filter condition
│    │   ├── IProjectionExpression.cs           ← SELECT clause representation
│    │   ├── ProjectionField.cs                 ← Single field projection
│    │   ├── IOrderingExpression.cs             ← ORDER BY representation
│    │   ├── OrderedField.cs                    ← Single ordered field
│    │   ├── IPagingExpression.cs               ← SKIP/TAKE representation
│    │   ├── IAggregationExpression.cs          ← GROUP BY representation
│    │   └── IJoinExpression.cs                 ← JOIN representation
│    │
│    ├── Translators/
│    │   ├── IDataCommandTranslator.cs          ← Translator interface
│    │   ├── DataCommandTranslatorBase{TService,TConfig}.cs ← Base for ServiceTypes
│    │   ├── DataCommandTranslators.cs          ← ServiceTypeCollection [ServiceTypeCollection(...)]
│    │   ├── IDataCommandTranslatorProvider.cs  ← Provider interface
│    │   └── CommandCapabilityBase.cs           ← Base for capabilities TypeCollection
│    │
│    ├── Messages/
│    │   └── DataCommandMessages.cs             ← MessageCollection
│    │
│    ├── FractalDataWorks.Data.DataCommands.Abstractions.csproj
│    └── README.md
│
│
├─── FractalDataWorks.Data.DataCommands/    (netstandard2.0;net10.0 multi-target)
│    │
│    ├── Commands/
│    │   ├── QueryCommand{T}.cs                 ← [TypeOption(typeof(DataCommands), "Query")]
│    │   ├── InsertCommand{T}.cs                ← [TypeOption(typeof(DataCommands), "Insert")]
│    │   ├── UpdateCommand{T}.cs                ← [TypeOption(typeof(DataCommands), "Update")]
│    │   ├── DeleteCommand.cs                   ← [TypeOption(typeof(DataCommands), "Delete")]
│    │   ├── UpsertCommand{T}.cs                ← [TypeOption(typeof(DataCommands), "Upsert")]
│    │   └── BulkInsertCommand{T}.cs            ← [TypeOption(typeof(DataCommands), "BulkInsert")]
│    │
│    ├── Operators/
│    │   ├── EqualOperator.cs                   ← [TypeOption(typeof(FilterOperators), "Equal")]
│    │   ├── NotEqualOperator.cs                ← [TypeOption(typeof(FilterOperators), "NotEqual")]
│    │   ├── ContainsOperator.cs                ← [TypeOption(typeof(FilterOperators), "Contains")]
│    │   ├── StartsWithOperator.cs              ← [TypeOption(typeof(FilterOperators), "StartsWith")]
│    │   ├── EndsWithOperator.cs                ← [TypeOption(typeof(FilterOperators), "EndsWith")]
│    │   ├── GreaterThanOperator.cs             ← [TypeOption(typeof(FilterOperators), "GreaterThan")]
│    │   ├── LessThanOperator.cs                ← [TypeOption(typeof(FilterOperators), "LessThan")]
│    │   ├── InOperator.cs                      ← [TypeOption(typeof(FilterOperators), "In")]
│    │   ├── IsNullOperator.cs                  ← [TypeOption(typeof(FilterOperators), "IsNull")]
│    │   └── IsNotNullOperator.cs               ← [TypeOption(typeof(FilterOperators), "IsNotNull")]
│    │
│    ├── Expressions/
│    │   ├── FilterExpression.cs                ← Implements IFilterExpression
│    │   ├── ProjectionExpression.cs            ← Implements IProjectionExpression
│    │   ├── OrderingExpression.cs              ← Implements IOrderingExpression
│    │   ├── PagingExpression.cs                ← Implements IPagingExpression
│    │   ├── AggregationExpression.cs           ← Implements IAggregationExpression
│    │   └── JoinExpression.cs                  ← Implements IJoinExpression
│    │
│    ├── Builders/
│    │   ├── LinqDataCommandBuilder.cs          ← Converts LINQ → DataCommand<T>
│    │   └── LinqExpressionVisitor.cs           ← Visits expression tree
│    │
│    ├── FractalDataWorks.Data.DataCommands.csproj
│    └── README.md
│
│
├─── FractalDataWorks.Data.DataCommands.Translators/    (net10.0)
│    │
│    ├── Sql/
│    │   ├── SqlDataCommandTranslator.cs        ← Implements IDataCommandTranslator
│    │   ├── SqlTranslatorType.cs               ← [ServiceTypeOption(typeof(DataCommandTranslators), "Sql")]
│    │   ├── SqlTranslatorConfiguration.cs      ← Configuration for SQL translator
│    │   ├── SqlCommandVisitor.cs               ← Visitor pattern implementation
│    │   └── SqlCommandBuilder.cs               ← Helper for building SQL strings
│    │
│    ├── Rest/
│    │   ├── RestDataCommandTranslator.cs       ← Implements IDataCommandTranslator
│    │   ├── RestTranslatorType.cs              ← [ServiceTypeOption(typeof(DataCommandTranslators), "Rest")]
│    │   ├── RestTranslatorConfiguration.cs     ← Configuration for REST translator
│    │   ├── RestCommandVisitor.cs              ← Visitor pattern implementation
│    │   └── ODataQueryBuilder.cs               ← Helper for building OData URLs
│    │
│    ├── File/
│    │   ├── FileDataCommandTranslator.cs       ← Implements IDataCommandTranslator
│    │   ├── FileTranslatorType.cs              ← [ServiceTypeOption(typeof(DataCommandTranslators), "File")]
│    │   ├── FileTranslatorConfiguration.cs     ← Configuration for File translator
│    │   └── FileCommandVisitor.cs              ← Visitor pattern implementation
│    │
│    ├── DataCommandTranslatorProvider.cs       ← Implements IDataCommandTranslatorProvider
│    ├── FractalDataWorks.Data.DataCommands.Translators.csproj
│    └── README.md
│
│
├─── FractalDataWorks.Data.DataSets.Abstractions/    (netstandard2.0)
│    │
│    ├── DataContext.cs                         ← Base class (like DbContext)
│    ├── IDataSet{T}.cs                         ← Interface (like DbSet<T>)
│    ├── IFractalDataWorksQueryProvider.cs      ← Custom LINQ provider interface
│    │
│    ├── Messages/
│    │   └── DataSetMessages.cs                 ← MessageCollection
│    │
│    ├── FractalDataWorks.Data.DataSets.Abstractions.csproj
│    └── README.md
│
│
└─── FractalDataWorks.Data.DataSets/    (net10.0)
     │
     ├── DataSet{T}.cs                          ← Implements IDataSet<T>, IQueryable<T>
     ├── FractalDataWorksQueryProvider.cs       ← Custom LINQ provider (converts to DataCommands)
     ├── DataSetQuery{T}.cs                     ← Helper for queryable implementation
     │
     ├── FractalDataWorks.Data.DataSets.csproj
     └── README.md
```

## Project Dependencies (Arrows = References)

```
┌──────────────────────────────────────────────────────────────────────────┐
│                      ABSTRACTIONS LAYER (netstandard2.0)                  │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│   DataCommands.Abstractions                DataSets.Abstractions          │
│   ┌────────────────────┐                   ┌─────────────────┐           │
│   │ IDataCommand       │                   │ DataContext     │           │
│   │ DataCommandBase    │                   │ IDataSet<T>     │           │
│   │ DataCommands       │                   └────────┬────────┘           │
│   │ FilterOperators    │                            │                     │
│   │ IExpression types  │                            │                     │
│   │ ITranslator        │                            │                     │
│   │ DataCommandTranslators│                         │                     │
│   └────────┬───────────┘                            │                     │
│            │                                         │                     │
│            │      References                         │ References          │
│            ├────────────────┐                        │                     │
│            ↓                ↓                        ↓                     │
│   ┌────────────────┐  ┌─────────────┐      ┌───────────────┐            │
│   │  Collections   │  │   Results   │      │  Collections  │            │
│   │  (TypeCollection)│ │(IGenericResult)│  │  ServiceTypes │            │
│   └────────────────┘  └─────────────┘      └───────────────┘            │
│                                                                            │
└──────────────────────────────────────────────────────────────────────────┘
                                    ↓ Referenced by ↓
┌──────────────────────────────────────────────────────────────────────────┐
│               IMPLEMENTATION LAYER (netstandard2.0;net10.0)               │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│   DataCommands                          DataSets                          │
│   ┌─────────────────────┐              ┌──────────────────┐              │
│   │ QueryCommand<T>     │              │ DataSet<T>       │              │
│   │ InsertCommand<T>    │              │ QueryProvider    │              │
│   │ UpdateCommand<T>    │              └────────┬─────────┘              │
│   │ DeleteCommand       │                       │                         │
│   │ EqualOperator       │                       │ References              │
│   │ ContainsOperator    │                       ↓                         │
│   │ FilterExpression    │              ┌────────────────────┐            │
│   │ LinqDataCommandBuilder│            │DataCommands.Abstractions│        │
│   └──────────┬──────────┘              └────────────────────┘            │
│              │                                                             │
│              │ References                                                  │
│              ↓                                                             │
│   ┌─────────────────────────────┐                                        │
│   │DataCommands.Abstractions    │                                        │
│   └─────────────────────────────┘                                        │
│                                                                            │
└──────────────────────────────────────────────────────────────────────────┘
                                    ↓ Referenced by ↓
┌──────────────────────────────────────────────────────────────────────────┐
│                    TRANSLATOR LAYER (net10.0)                             │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│   DataCommands.Translators                                                │
│   ┌────────────────────────────────────────────────┐                     │
│   │ SqlDataCommandTranslator                       │                     │
│   │ SqlTranslatorType [ServiceTypeOption]          │                     │
│   │                                                 │                     │
│   │ RestDataCommandTranslator                      │                     │
│   │ RestTranslatorType [ServiceTypeOption]         │                     │
│   │                                                 │                     │
│   │ FileDataCommandTranslator                      │                     │
│   │ FileTranslatorType [ServiceTypeOption]         │                     │
│   │                                                 │                     │
│   │ DataCommandTranslatorProvider                  │                     │
│   └────────────────────┬───────────────────────────┘                     │
│                        │                                                   │
│                        │ References                                        │
│                        ↓                                                   │
│   ┌────────────────────────────────┐                                     │
│   │ DataCommands.Abstractions      │                                     │
│   │ Services.Connections.Abstractions│                                   │
│   └────────────────────────────────┘                                     │
│                                                                            │
└──────────────────────────────────────────────────────────────────────────┘
                                    ↓ Used by ↓
┌──────────────────────────────────────────────────────────────────────────┐
│                          USER CODE                                        │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│   User's Application                                                      │
│   ┌────────────────────────────────────────────┐                         │
│   │ // Define context                          │                         │
│   │ public class AppDataContext : DataContext  │                         │
│   │ {                                          │                         │
│   │     public IDataSet<Customer> Customers    │                         │
│   │         => Set<Customer>();                │                         │
│   │                                            │                         │
│   │     public IDataSet<Order> Orders          │                         │
│   │         => Set<Order>();                   │                         │
│   │ }                                          │                         │
│   │                                            │                         │
│   │ // Use it (EF Core-like!)                 │                         │
│   │ var query = _context.Customers            │                         │
│   │     .Where(c => c.IsActive)               │                         │
│   │     .OrderBy(c => c.Name)                 │                         │
│   │     .Skip(0).Take(50);                    │                         │
│   │                                            │                         │
│   │ var command =                             │                         │
│   │     LinqDataCommandBuilder                │                         │
│   │         .FromQueryable(query).Value;      │                         │
│   │                                            │                         │
│   │ var result =                              │                         │
│   │     await _context.ExecuteAsync(command); │                         │
│   │                                            │                         │
│   │ // result.Value is IEnumerable<Customer>  │                         │
│   │ // NO CASTING!                            │                         │
│   └────────────────────────────────────────────┘                         │
│                                                                            │
└──────────────────────────────────────────────────────────────────────────┘
```

## Execution Flow (When User Runs Query)

```
User Code
    │
    ├─ _context.Customers.Where(c => c.IsActive)
    │  ↓
    │  [IDataSet<Customer> implements IQueryable<Customer>]
    │  ↓
    │  FractalDataWorksQueryProvider (custom LINQ provider)
    │  ↓
    │  LinqDataCommandBuilder.FromQueryable(query)
    │  ↓
    │  LinqExpressionVisitor (visits expression tree)
    │  ↓
    │  Builds: new QueryCommand<Customer>("Customers") {
    │      Filter = new FilterExpression {
    │          Conditions = [new FilterCondition {
    │              PropertyName = "IsActive",
    │              Operator = FilterOperators.Equal,
    │              Value = true
    │          }]
    │      }
    │  }
    │
    ├─ await _context.ExecuteAsync(command)
    │  ↓
    │  DataContext.ExecuteAsync<IEnumerable<Customer>>(command)
    │  ↓
    │  Gets connection from IDataConnectionProvider
    │  ↓
    │  IDataConnection.ExecuteAsync<IEnumerable<Customer>>(command)
    │  ↓
    │  Gets translator: DataCommandTranslatorProvider.GetTranslator("Sql")
    │  ↓
    │  SqlDataCommandTranslator.TranslateAsync(command)
    │  ↓
    │  SqlCommandVisitor.VisitQuery(command)
    │  ↓
    │  Builds: StubSqlConnectionCommand {
    │      SqlText = "SELECT * FROM [Customers] WHERE [IsActive] = @IsActive",
    │      Parameters = { ["@IsActive"] = true }
    │  }
    │  ↓
    │  SqlConnection.ExecuteAsync(sqlCommand)
    │  ↓
    │  Executes SQL query against database
    │  ↓
    │  Returns: IGenericResult<IEnumerable<Customer>>
    │  ↓
    ↓  [NO CASTING NEEDED!]
    │
    └─ result.Value (typed as IEnumerable<Customer>)
```

## Where Source Generators Fit

```
Build Time:
    │
    ├─ Collections.SourceGenerators (already exists)
    │  ↓
    │  Scans for [TypeCollection] attributes
    │  ↓
    │  Finds: DataCommands.cs with [TypeCollection(typeof(DataCommandBase), ...)]
    │  ↓
    │  Scans for [TypeOption] attributes
    │  ↓
    │  Finds: QueryCommand<T>, InsertCommand<T>, etc.
    │  ↓
    │  GENERATES: DataCommands partial class with:
    │      - Static constructor
    │      - Static properties (Query, Insert, Update, etc.)
    │      - All() method returning FrozenSet
    │      - GetByName() method
    │      - GetById() method
    │
    ├─ ServiceTypes.SourceGenerators (already exists)
    │  ↓
    │  Scans for [ServiceTypeCollection] attributes
    │  ↓
    │  Finds: DataCommandTranslators.cs with [ServiceTypeCollection(...)]
    │  ↓
    │  Scans for [ServiceTypeOption] attributes
    │  ↓
    │  Finds: SqlTranslatorType, RestTranslatorType, FileTranslatorType
    │  ↓
    │  GENERATES: DataCommandTranslators partial class with:
    │      - Static constructor
    │      - Static properties (Sql, Rest, File)
    │      - RegisterAll(IServiceCollection) method
    │      - All() method
    │      - GetByName() method
    │
    └─ Messages.SourceGenerators (already exists)
       ↓
       Scans for [MessageCollection] attributes
       ↓
       Finds: DataCommandMessages.cs
       ↓
       GENERATES: Message collection static class
```

## Summary: Three Main Layers

```
┌───────────────────────────────────────────────────────────────┐
│                    ABSTRACTIONS                                │
│  (Interfaces, base classes, TypeCollections/ServiceTypeCollections) │
│                                                                 │
│  • What operations exist (IDataCommand, IDataCommandTranslator)│
│  • What they return (IGenericResult<T>)                        │
│  • What expressions look like (IFilterExpression, etc.)        │
│  • TypeCollection definitions (DataCommands, FilterOperators)  │
│  • ServiceTypeCollection definitions (DataCommandTranslators)  │
└────────────────────────┬──────────────────────────────────────┘
                         ↓
┌───────────────────────────────────────────────────────────────┐
│                 IMPLEMENTATIONS                                │
│        (Concrete commands, operators, expressions)             │
│                                                                 │
│  • Specific commands (QueryCommand<T>, InsertCommand<T>)      │
│  • Specific operators (EqualOperator, ContainsOperator)       │
│  • Expression implementations (FilterExpression)               │
│  • LINQ builder (LinqDataCommandBuilder)                      │
│  • DataContext & DataSet (EF Core-like API)                   │
└────────────────────────┬──────────────────────────────────────┘
                         ↓
┌───────────────────────────────────────────────────────────────┐
│                   TRANSLATORS                                  │
│    (Convert universal commands to domain-specific)             │
│                                                                 │
│  • SQL translator (DataCommand → SQL string)                  │
│  • REST translator (DataCommand → OData URL)                  │
│  • File translator (DataCommand → file operations)            │
│  • Provider (routes to correct translator)                    │
└───────────────────────────────────────────────────────────────┘
```

Hope this helps visualize where everything lives!
