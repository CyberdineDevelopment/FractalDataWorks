# FractalDataWorks Data Architecture Refactoring Plan

## Core Principles

1. **TypeCollections Everywhere**: No switch statements or enums - use TypeCollections and EnhancedEnums
2. **No Async Suffix**: Methods returning Task don't need Async suffix
3. **Messages & Results**: All operations return `IGenericResult` with structured messages
4. **Structured Logging**: Use `[LoggerMessage]` attributes for high-performance logging
5. **No Defensive Programming**: Trust the compiler and type system
6. **No Exceptions for Expected Conditions**: Use Result pattern with Messages

## Part 1: Commands Domain Architecture

### Project Structure

```
FractalDataWorks.Commands.Abstractions/
├── ICommand.cs
├── ICommandType.cs
├── CommandTypeBase.cs
├── CommandTypes.cs                      # TypeCollection
├── ICommandTranslator.cs
├── ITranslatorType.cs                   # TypeOption for translators
├── TranslatorTypeBase.cs
├── TranslatorTypes.cs                   # TypeCollection for translators
├── Commands/
│   ├── IDataCommand.cs
│   ├── IQueryCommand.cs
│   ├── IMutationCommand.cs
│   └── IBulkCommand.cs
├── Messages/
│   ├── CommandMessage.cs
│   ├── CommandNullMessage.cs
│   ├── TranslationFailedMessage.cs
│   └── UnsupportedCommandMessage.cs
└── Logging/
    └── CommandLog.cs

FractalDataWorks.Commands/
├── CommandProvider.cs
├── TranslatorProvider.cs
└── CommandBuilder.cs
```

### Key Interfaces

```csharp
namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Base command type with metadata.
/// </summary>
public interface ICommandType : ITypeOption<ICommandType>
{
    /// <summary>
    /// Gets the command category (Query, Mutation, Bulk, etc).
    /// </summary>
    CommandCategory Category { get; }

    /// <summary>
    /// Gets the supported translator types for this command.
    /// </summary>
    IReadOnlyCollection<ITranslatorType> SupportedTranslators { get; }
}

/// <summary>
/// Command translator type with capabilities.
/// </summary>
public interface ITranslatorType : ITypeOption<ITranslatorType>
{
    /// <summary>
    /// Gets the source format this translator handles.
    /// </summary>
    IDataFormat SourceFormat { get; }

    /// <summary>
    /// Gets the target format this translator produces.
    /// </summary>
    IDataFormat TargetFormat { get; }

    /// <summary>
    /// Gets the translation capabilities.
    /// </summary>
    TranslationCapabilities Capabilities { get; }

    /// <summary>
    /// Creates a translator instance.
    /// </summary>
    IGenericResult<ICommandTranslator> CreateTranslator(IServiceProvider services);
}

/// <summary>
/// Command categories as EnhancedEnum.
/// </summary>
[EnhancedEnumCollection(typeof(CommandCategory), typeof(ICommandCategory))]
public abstract class CommandCategory : EnhancedEnumBase<CommandCategory, ICommandCategory>
{
    public abstract bool RequiresTransaction { get; }
    public abstract bool SupportsStreaming { get; }
}
```

## Part 2: Data Domain Architecture

### Project Structure

```
FractalDataWorks.Data.Abstractions/
├── IDataType.cs
├── DataTypeBase.cs
├── DataTypes.cs                         # TypeCollection
├── IDataFormat.cs                       # EnhancedEnum for formats
├── DataFormatBase.cs
├── DataFormats.cs                       # EnhancedEnum collection
├── IDataLocation.cs
├── IDataSchema.cs
├── Messages/
│   ├── DataMessage.cs
│   ├── SchemaValidationMessage.cs
│   ├── FormatMismatchMessage.cs
│   └── LocationNotFoundMessage.cs
└── Logging/
    └── DataLog.cs

FractalDataWorks.Data.DataSets.Abstractions/
├── IDataSet.cs
├── IDataSetType.cs
├── DataSetTypeBase.cs
├── DataSetTypes.cs                      # TypeCollection
├── Messages/
│   └── DataSetMessages.cs
└── Logging/
    └── DataSetLog.cs

FractalDataWorks.Data.DataContainers.Abstractions/
├── IDataContainer.cs
├── IDataContainerType.cs
├── DataContainerTypeBase.cs
├── DataContainerTypes.cs                # TypeCollection
├── IContainerFormat.cs                  # EnhancedEnum
├── ContainerFormatBase.cs
├── ContainerFormats.cs
├── Messages/
│   └── ContainerMessages.cs
└── Logging/
    └── ContainerLog.cs
```

### Data Formats as EnhancedEnum

```csharp
namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Data format definition.
/// </summary>
public interface IDataFormat : IEnhancedEnum<IDataFormat>
{
    /// <summary>
    /// Gets whether this format supports schema discovery.
    /// </summary>
    bool SupportsSchemaDiscovery { get; }

    /// <summary>
    /// Gets whether this format supports streaming.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets the MIME type for this format.
    /// </summary>
    string MimeType { get; }
}

/// <summary>
/// Base class for data formats.
/// </summary>
public abstract class DataFormatBase : EnhancedEnumBase<DataFormatBase, IDataFormat>, IDataFormat
{
    public abstract bool SupportsSchemaDiscovery { get; }
    public abstract bool SupportsStreaming { get; }
    public abstract string MimeType { get; }

    protected DataFormatBase(int id, string name) : base(id, name) { }
}

/// <summary>
/// Collection of data formats.
/// </summary>
[EnhancedEnumCollection(typeof(DataFormatBase), typeof(IDataFormat), typeof(DataFormats))]
public partial class DataFormats
{
    // Source generator creates static instances
}
```

## Part 3: Implementation Projects

### SQL Implementation

```
FractalDataWorks.Data.Sql/
├── SqlDataFormat.cs                     # IDataFormat implementation
├── SqlCommandType.cs                    # ICommandType implementation
├── SqlTranslatorType.cs                 # ITranslatorType implementation
├── Translators/
│   ├── SqlCommandTranslator.cs
│   ├── SqlExpressionVisitor.cs
│   └── SqlParameterMapper.cs
├── Commands/
│   ├── SqlQueryCommand.cs
│   └── SqlMutationCommand.cs
├── Messages/
│   ├── SqlMessage.cs
│   ├── SqlSyntaxErrorMessage.cs
│   └── SqlConnectionFailedMessage.cs
└── Logging/
    └── SqlTranslatorLog.cs
```

### SQL Translator Implementation

```csharp
namespace FractalDataWorks.Data.Sql;

/// <summary>
/// SQL translator type definition.
/// </summary>
public sealed class SqlTranslatorType : TranslatorTypeBase, IEnumOption<SqlTranslatorType>
{
    public static SqlTranslatorType Instance { get; } = new();

    private SqlTranslatorType() : base(
        id: 1,
        name: "SqlTranslator",
        sourceFormat: DataFormats.Linq,
        targetFormat: DataFormats.Sql)
    {
    }

    public override TranslationCapabilities Capabilities => new()
    {
        SupportsProjection = true,
        SupportsFiltering = true,
        SupportsOrdering = true,
        SupportsPaging = true,
        SupportsJoins = true,
        SupportsGrouping = true,
        SupportsAggregation = true
    };

    public override IGenericResult<ICommandTranslator> CreateTranslator(IServiceProvider services)
    {
        var translator = services.GetService<SqlCommandTranslator>();
        return translator != null
            ? GenericResult.Success<ICommandTranslator>(translator)
            : GenericResult.Failure<ICommandTranslator>(SqlMessages.TranslatorNotRegistered());
    }
}

/// <summary>
/// SQL command translator with proper result/message handling.
/// </summary>
public sealed class SqlCommandTranslator : ICommandTranslator
{
    private readonly ILogger<SqlCommandTranslator> _logger;

    public SqlCommandTranslator(ILogger<SqlCommandTranslator> logger)
    {
        _logger = logger;
    }

    public IGenericResult<ICommand> Translate(Expression expression, IDataContainer container)
    {
        SqlTranslatorLog.TranslationStarted(_logger, expression.Type.Name);

        var visitor = new SqlExpressionVisitor(container.Schema);
        var result = visitor.Visit(expression);

        if (!visitor.IsValid)
        {
            SqlTranslatorLog.TranslationFailed(_logger, visitor.ErrorMessage);
            return GenericResult.Failure<ICommand>(
                SqlMessages.TranslationFailed(visitor.ErrorMessage));
        }

        var command = new SqlQueryCommand
        {
            CommandText = visitor.GeneratedSql,
            Parameters = visitor.Parameters
        };

        SqlTranslatorLog.TranslationCompleted(_logger, command.CommandText);
        return GenericResult.Success<ICommand>(command);
    }
}
```

### Messages Implementation

```csharp
namespace FractalDataWorks.Data.Sql.Messages;

/// <summary>
/// Base SQL message class.
/// </summary>
[MessageCollection("SqlMessages")]
public abstract class SqlMessage : MessageTemplate<MessageSeverity>, IDataMessage
{
    protected SqlMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "Sql", message, code, null, null) { }
}

/// <summary>
/// Translation failed message.
/// </summary>
public sealed class TranslationFailedMessage : SqlMessage
{
    public TranslationFailedMessage(string reason)
        : base(1001, "TranslationFailed", MessageSeverity.Error,
               $"Failed to translate expression to SQL: {reason}", "SQL_TRANS_001") { }
}

/// <summary>
/// Source-generated message factory.
/// </summary>
public static partial class SqlMessages
{
    public static TranslationFailedMessage TranslationFailed(string reason) => new(reason);
    public static TranslatorNotRegisteredMessage TranslatorNotRegistered() => new();
}
```

### Structured Logging

```csharp
namespace FractalDataWorks.Data.Sql.Logging;

/// <summary>
/// High-performance structured logging for SQL translator.
/// </summary>
public static partial class SqlTranslatorLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Debug,
        Message = "Starting SQL translation for expression type {ExpressionType}")]
    public static partial void TranslationStarted(ILogger logger, string expressionType);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug,
        Message = "SQL translation completed: {GeneratedSql}")]
    public static partial void TranslationCompleted(ILogger logger, string generatedSql);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning,
        Message = "SQL translation failed: {ErrorMessage}")]
    public static partial void TranslationFailed(ILogger logger, string errorMessage);
}
```

## Part 4: DataGateway Integration

### DataGateway Without Switch Statements

```csharp
namespace FractalDataWorks.Services.DataGateway;

public sealed class DataGatewayService : ServiceBase<IDataGatewayCommand, IDataGatewayConfiguration, DataGatewayService>,
    IDataGateway
{
    private readonly ITranslatorProvider _translatorProvider;
    private readonly IConnectionProvider _connectionProvider;
    private readonly IDataSetProvider _dataSetProvider;

    public DataGatewayService(
        ILogger<DataGatewayService> logger,
        IDataGatewayConfiguration configuration,
        ITranslatorProvider translatorProvider,
        IConnectionProvider connectionProvider,
        IDataSetProvider dataSetProvider)
        : base(logger, configuration)
    {
        _translatorProvider = translatorProvider;
        _connectionProvider = connectionProvider;
        _dataSetProvider = dataSetProvider;
    }

    public Task<IGenericResult<T>> Execute<T>(
        Expression<Func<IQueryable<T>, IQueryable<T>>> query,
        DataSetReference dataSet,
        CancellationToken cancellationToken = default)
    {
        DataGatewayLog.ExecutionStarted(Logger, dataSet.Name);

        // Get dataset configuration
        var datasetResult = _dataSetProvider.GetDataSet(dataSet);
        if (!datasetResult.IsSuccess)
        {
            DataGatewayLog.DataSetNotFound(Logger, dataSet.Name);
            return Task.FromResult(GenericResult.Failure<T>(datasetResult.Message));
        }

        // Select optimal container
        var containerResult = SelectOptimalContainer(datasetResult.Value, query);
        if (!containerResult.IsSuccess)
        {
            DataGatewayLog.ContainerSelectionFailed(Logger, containerResult.Message);
            return Task.FromResult(GenericResult.Failure<T>(containerResult.Message));
        }

        // Get translator based on container format (TypeCollection lookup, no switch!)
        var translatorResult = _translatorProvider.GetTranslator(
            DataFormats.Linq,
            containerResult.Value.Format);

        if (!translatorResult.IsSuccess)
        {
            DataGatewayLog.TranslatorNotFound(Logger, containerResult.Value.Format.Name);
            return Task.FromResult(GenericResult.Failure<T>(
                DataGatewayMessages.NoTranslatorAvailable(containerResult.Value.Format)));
        }

        // Translate expression to command
        var commandResult = translatorResult.Value.Translate(query.Body, containerResult.Value);
        if (!commandResult.IsSuccess)
        {
            DataGatewayLog.TranslationFailed(Logger, commandResult.Message);
            return Task.FromResult(GenericResult.Failure<T>(commandResult.Message));
        }

        // Execute via connection
        return ExecuteCommand<T>(commandResult.Value, containerResult.Value, cancellationToken);
    }

    private IGenericResult<IDataContainer> SelectOptimalContainer(IDataSet dataset, Expression query)
    {
        // Use TypeCollection pattern to select container
        var selector = ContainerSelectors.GetForQuery(query);
        return selector.SelectContainer(dataset.Containers);
    }
}
```

### Container Selection as TypeCollection

```csharp
namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Container selector type for optimal container selection.
/// </summary>
public interface IContainerSelectorType : ITypeOption<IContainerSelectorType>
{
    /// <summary>
    /// Gets the priority for this selector.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Determines if this selector can handle the query.
    /// </summary>
    IGenericResult<bool> CanHandle(Expression query);

    /// <summary>
    /// Selects the optimal container.
    /// </summary>
    IGenericResult<IDataContainer> SelectContainer(IReadOnlyCollection<IDataContainer> containers);
}

/// <summary>
/// TypeCollection for container selectors.
/// </summary>
[ServiceTypeCollection(typeof(ContainerSelectorTypeBase), typeof(IContainerSelectorType), typeof(ContainerSelectors))]
public partial class ContainerSelectors
{
    public static IContainerSelectorType GetForQuery(Expression query)
    {
        // TypeCollection handles selection based on capabilities
        foreach (var selector in All.OrderByDescending(s => s.Priority))
        {
            var canHandleResult = selector.CanHandle(query);
            if (canHandleResult.IsSuccess && canHandleResult.Value)
            {
                return selector;
            }
        }

        return DefaultContainerSelector.Instance;
    }
}
```

## Part 5: Usage Examples

### Example 1: Simple LINQ Query

```csharp
// User writes LINQ
var gateway = provider.GetService<IDataGateway>();
var result = await gateway.Execute<User>(
    q => q.Where(u => u.Age > 18)
          .OrderBy(u => u.Name)
          .Take(10),
    DataSets.Users
);

if (!result.IsSuccess)
{
    _logger.LogWarning("Query failed: {Message}", result.Message);
    return result;
}

// The gateway:
// 1. Uses DataSetProvider to get dataset config (no exceptions)
// 2. Uses ContainerSelectors TypeCollection to pick optimal container
// 3. Uses TranslatorProvider to get appropriate translator
// 4. Translator returns GenericResult with command or failure message
// 5. ConnectionProvider executes command and returns result
```

### Example 2: Command Building with TypeCollections

```csharp
// Build command using fluent API
var commandResult = DataCommands.CreateBulkInsert<User>()
    .Into(DataSets.Users)
    .WithData(users)
    .WithBatchSize(1000)
    .WithConflictStrategy(ConflictStrategies.Update) // EnhancedEnum
    .Build();

if (!commandResult.IsSuccess)
{
    return commandResult;
}

// Execute command
var result = await gateway.Execute(commandResult.Value);

// ConflictStrategies is an EnhancedEnum
[EnhancedEnumCollection(typeof(ConflictStrategyBase), typeof(IConflictStrategy))]
public abstract class ConflictStrategyBase : EnhancedEnumBase<ConflictStrategyBase, IConflictStrategy>
{
    public abstract IGenericResult<ICommand> GenerateResolutionCommand(ICommand original);
}

public sealed class UpdateStrategy : ConflictStrategyBase, IEnumOption<UpdateStrategy>
{
    public static UpdateStrategy Instance { get; } = new();

    public override IGenericResult<ICommand> GenerateResolutionCommand(ICommand original)
    {
        // Generate MERGE or UPSERT command
        return GenericResult.Success(mergeCommand);
    }
}
```

### Example 3: Multi-Container with Format Detection

```csharp
// Configure dataset with multiple containers
var configResult = DataSets.Configure("Orders")
    .AddContainer(ContainerTypes.SqlTable, "dbo.Orders")
    .AddContainer(ContainerTypes.JsonFile, "/cache/orders.json")
    .AddContainer(ContainerTypes.CsvFile, "/export/orders.csv")
    .Build();

if (!configResult.IsSuccess)
{
    return configResult;
}

// Query routes to best container automatically
var ordersResult = await gateway.Execute<Order>(
    q => q.Where(o => o.CustomerId == customerId),
    configResult.Value
);

// ContainerTypes is a TypeCollection that knows about all container types
// Each type knows its format, capabilities, and performance characteristics
```

## Part 6: Configuration

### appsettings.json

```json
{
  "Data": {
    "DataSets": {
      "Users": {
        "DataSetType": "Entity",
        "Schema": {
          "Fields": [
            { "Name": "Id", "Type": "Int32", "IsKey": true },
            { "Name": "Name", "Type": "String", "MaxLength": 100 },
            { "Name": "Email", "Type": "String", "MaxLength": 255 }
          ]
        },
        "Containers": {
          "Primary": {
            "ContainerType": "SqlTable",
            "Store": "MainDatabase",
            "Location": "dbo.Users",
            "Priority": 100
          },
          "Cache": {
            "ContainerType": "RedisHash",
            "Store": "CacheServer",
            "Location": "users:*",
            "Priority": 200,
            "MaxAge": 300
          }
        }
      }
    },
    "Translators": {
      "Default": {
        "LinqToSql": "SqlTranslator",
        "LinqToJson": "JsonTranslator",
        "LinqToHttp": "ODataTranslator"
      }
    }
  }
}
```

## Part 7: Registration and DI

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register Commands domain
foreach (var commandType in CommandTypes.All)
{
    commandType.Register(builder.Services);
}

// Register Translators
foreach (var translatorType in TranslatorTypes.All)
{
    translatorType.Register(builder.Services);
}

// Register Data types
foreach (var dataType in DataTypes.All)
{
    dataType.Register(builder.Services);
}

// Register DataSets
foreach (var dataSetType in DataSetTypes.All)
{
    dataSetType.Register(builder.Services);
}

// Register Containers
foreach (var containerType in DataContainerTypes.All)
{
    containerType.Register(builder.Services);
}

// Register DataGateway
builder.Services.AddScoped<IDataGateway, DataGatewayService>();
builder.Services.AddScoped<ITranslatorProvider, TranslatorProvider>();
builder.Services.AddScoped<IContainerProvider, ContainerProvider>();
```

## Part 8: Benefits of This Architecture

1. **No Switch Statements**: Everything uses TypeCollections for extensibility
2. **No Exceptions**: All operations return GenericResult with Messages
3. **Type Safety**: Compiler enforces correctness, no defensive coding
4. **Consistent Patterns**: Same patterns as existing Services
5. **High Performance**: Structured logging, no boxing, efficient lookups
6. **Extensibility**: Add new formats/translators without modifying core
7. **Testability**: Every component can be mocked/tested independently
8. **Clear Boundaries**: Each domain has clear responsibilities

## Implementation Order

### Phase 1: Foundation (Week 1)
1. Create Commands.Abstractions with TypeCollections
2. Create Data.Abstractions with EnhancedEnums for formats
3. Create base Messages and Logging infrastructure

### Phase 2: Core Implementations (Week 2-3)
1. Implement SQL translator and commands
2. Implement HTTP translator and commands
3. Implement JSON/CSV in-memory translators

### Phase 3: DataGateway Integration (Week 3-4)
1. Refactor existing DataGateway to use new patterns
2. Implement TranslatorProvider with TypeCollection lookup
3. Implement ContainerSelectors for optimal routing

### Phase 4: Testing & Documentation (Week 4-5)
1. Unit tests for all components
2. Integration tests for end-to-end scenarios
3. Update documentation with new patterns

### Phase 5: Migration (Week 5-6)
1. Create compatibility layer for existing code
2. Gradual migration of existing connections
3. Performance benchmarking and optimization

## Key Principles Enforced

1. **TypeCollections replace all switch/enum logic**
2. **No Async suffix on any methods**
3. **All operations return IGenericResult with Messages**
4. **Structured logging with [LoggerMessage] attributes**
5. **No null defensive programming - trust the type system**
6. **No exceptions for expected conditions - use Messages**
7. **Consistent with existing FractalDataWorks patterns**
8. **Every type is discoverable via source generators**