# DataCommands TypeCollections Fix

## Problem

The DataCommands architecture stubs violate critical CLAUDE.md rules by using **enums and switch statements** instead of **TypeCollections and dispatch patterns**.

### Violations Found

1. ❌ `DataCommandType` enum - should be TypeCollection
2. ❌ Switch statements on `command.CommandType` - should use TypeCollection dispatch
3. ❌ `FilterOperator`, `LogicalOperator`, `SortDirection` enums - should be TypeCollections/EnhancedEnums
4. ❌ Translators not using ServiceTypeCollection for registration
5. ❌ Capabilities using [Flags] enum - should use alternative pattern

From CLAUDE.md:
> **"TypeCollections Replace Enums"** - Any time you have a fixed set of something, use TypeCollection instead of enum
> **"EnhancedEnum to Replace Switch Statements"** - When encountering enums, use EnhancedEnum to add properties and eliminate switch/if chains
> **"NEVER use switch statements on enums"** - use EnhancedEnum with properties

## Corrected Architecture

### 1. FilterOperator TypeCollection (Replaces Enum)

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// Base class for filter operators.
/// Replaces FilterOperator enum to add behavior and eliminate switch statements.
/// </summary>
public abstract class FilterOperatorBase
{
    protected FilterOperatorBase(int id, string name, string sqlOperator, string odataOperator, bool requiresValue)
    {
        Id = id;
        Name = name;
        SqlOperator = sqlOperator;
        ODataOperator = odataOperator;
        RequiresValue = requiresValue;
    }

    public int Id { get; }
    public string Name { get; }

    /// <summary>
    /// SQL representation (e.g., "=", "<>", "LIKE").
    /// No switch statements needed - property lookup!
    /// </summary>
    public string SqlOperator { get; }

    /// <summary>
    /// OData representation (e.g., "eq", "ne", "contains").
    /// No switch statements needed - property lookup!
    /// </summary>
    public string ODataOperator { get; }

    /// <summary>
    /// Whether this operator requires a value parameter.
    /// IS NULL and IS NOT NULL don't need values.
    /// </summary>
    public bool RequiresValue { get; }

    /// <summary>
    /// Format the parameter placeholder for SQL.
    /// Subclasses can override for special behavior (LIKE wildcards).
    /// </summary>
    public virtual string FormatSqlParameter(string paramName) => $"@{paramName}";

    /// <summary>
    /// Format the value for OData query strings.
    /// </summary>
    public abstract string FormatODataValue(object? value);
}

/// <summary>
/// TypeCollection for filter operators.
/// </summary>
[TypeCollection(typeof(FilterOperatorBase), typeof(FilterOperatorBase), typeof(FilterOperators))]
public abstract partial class FilterOperators : TypeCollectionBase<FilterOperatorBase, FilterOperatorBase>
{
    // Source generator creates:
    // public static EqualOperator Equal { get; }
    // public static NotEqualOperator NotEqual { get; }
    // public static ContainsOperator Contains { get; }
    // ... etc
    // public static FrozenSet<FilterOperatorBase> All()
    // public static FilterOperatorBase? GetByName(string name)
}
```

### 2. FilterOperator TypeOptions (Replaces Enum Values)

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Equal operator (=, eq).
/// </summary>
[TypeOption(typeof(FilterOperators), "Equal")]
public sealed class EqualOperator : FilterOperatorBase
{
    public EqualOperator() : base(
        id: 1,
        name: "Equal",
        sqlOperator: "=",
        odataOperator: "eq",
        requiresValue: true)
    {
    }

    public override string FormatODataValue(object? value)
    {
        if (value == null) return "null";
        return value switch
        {
            string str => $"'{str}'",
            int or long => value.ToString()!,
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
            _ => $"'{value}'"
        };
    }
}

/// <summary>
/// Not equal operator (<>, ne).
/// </summary>
[TypeOption(typeof(FilterOperators), "NotEqual")]
public sealed class NotEqualOperator : FilterOperatorBase
{
    public NotEqualOperator() : base(
        id: 2,
        name: "NotEqual",
        sqlOperator: "<>",
        odataOperator: "ne",
        requiresValue: true)
    {
    }

    public override string FormatODataValue(object? value)
    {
        // Same as Equal
        if (value == null) return "null";
        return value switch
        {
            string str => $"'{str}'",
            int or long => value.ToString()!,
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
            _ => $"'{value}'"
        };
    }
}

/// <summary>
/// Contains operator (LIKE '%value%', contains).
/// </summary>
[TypeOption(typeof(FilterOperators), "Contains")]
public sealed class ContainsOperator : FilterOperatorBase
{
    public ContainsOperator() : base(
        id: 3,
        name: "Contains",
        sqlOperator: "LIKE",
        odataOperator: "contains",
        requiresValue: true)
    {
    }

    // Override to add wildcards for SQL
    public override string FormatSqlParameter(string paramName) => $"'%' + @{paramName} + '%'";

    public override string FormatODataValue(object? value)
    {
        // OData contains() function handles wildcards itself
        return value switch
        {
            string str => $"'{str}'",
            _ => $"'{value}'"
        };
    }
}

/// <summary>
/// StartsWith operator (LIKE 'value%', startswith).
/// </summary>
[TypeOption(typeof(FilterOperators), "StartsWith")]
public sealed class StartsWithOperator : FilterOperatorBase
{
    public StartsWithOperator() : base(
        id: 4,
        name: "StartsWith",
        sqlOperator: "LIKE",
        odataOperator: "startswith",
        requiresValue: true)
    {
    }

    public override string FormatSqlParameter(string paramName) => $"@{paramName} + '%'";

    public override string FormatODataValue(object? value) => $"'{value}'";
}

/// <summary>
/// EndsWith operator (LIKE '%value', endswith).
/// </summary>
[TypeOption(typeof(FilterOperators), "EndsWith")]
public sealed class EndsWithOperator : FilterOperatorBase
{
    public EndsWithOperator() : base(
        id: 5,
        name: "EndsWith",
        sqlOperator: "LIKE",
        odataOperator: "endswith",
        requiresValue: true)
    {
    }

    public override string FormatSqlParameter(string paramName) => $"'%' + @{paramName}";

    public override string FormatODataValue(object? value) => $"'{value}'";
}

/// <summary>
/// IsNull operator (IS NULL, eq null).
/// </summary>
[TypeOption(typeof(FilterOperators), "IsNull")]
public sealed class IsNullOperator : FilterOperatorBase
{
    public IsNullOperator() : base(
        id: 6,
        name: "IsNull",
        sqlOperator: "IS NULL",
        odataOperator: "eq null",
        requiresValue: false) // No value needed!
    {
    }

    public override string FormatSqlParameter(string paramName) => string.Empty;
    public override string FormatODataValue(object? value) => string.Empty;
}

/// <summary>
/// IsNotNull operator (IS NOT NULL, ne null).
/// </summary>
[TypeOption(typeof(FilterOperators), "IsNotNull")]
public sealed class IsNotNullOperator : FilterOperatorBase
{
    public IsNotNullOperator() : base(
        id: 7,
        name: "IsNotNull",
        sqlOperator: "IS NOT NULL",
        odataOperator: "ne null",
        requiresValue: false)
    {
    }

    public override string FormatSqlParameter(string paramName) => string.Empty;
    public override string FormatODataValue(object? value) => string.Empty;
}

// Additional operators: GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, In, NotIn
// All follow same pattern with properties instead of switch statements
```

### 3. Usage in FilterCondition (No Switch Statements!)

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

public sealed class FilterCondition
{
    public required string PropertyName { get; init; }

    /// <summary>
    /// Operator is now a FilterOperatorBase (TypeCollection), not enum.
    /// Access properties directly, no switch statements!
    /// </summary>
    public required FilterOperatorBase Operator { get; init; }

    public object? Value { get; init; }
}

// ✅ CORRECT: Property access, no switch
public string BuildSqlCondition(FilterCondition condition)
{
    // Old way: switch (condition.Operator) { case FilterOperator.Equal => "="; ... }
    // New way: Direct property access!

    var paramPlaceholder = condition.Operator.RequiresValue
        ? condition.Operator.FormatSqlParameter(condition.PropertyName)
        : string.Empty;

    return $"[{condition.PropertyName}] {condition.Operator.SqlOperator} {paramPlaceholder}";
}

// ✅ CORRECT: Property access, no switch
public string BuildODataCondition(FilterCondition condition)
{
    var valueStr = condition.Operator.RequiresValue
        ? condition.Operator.FormatODataValue(condition.Value)
        : string.Empty;

    return $"{condition.PropertyName} {condition.Operator.ODataOperator} {valueStr}";
}

// Usage:
var condition = new FilterCondition
{
    PropertyName = "Name",
    Operator = FilterOperators.Contains, // Type-safe, no magic strings!
    Value = "Acme"
};

var sql = BuildSqlCondition(condition);
// Result: "[Name] LIKE '%' + @Name + '%'"
// No switch statement needed - operator knows how to format itself!
```

### 4. LogicalOperator EnhancedEnum (Simple Case)

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// Logical operator for combining filter conditions.
/// Simple enough for EnhancedEnum (only 2 options).
/// </summary>
public sealed class LogicalOperator : EnumOptionBase<LogicalOperator>
{
    public static readonly LogicalOperator And = new(1, "And", "AND", "and");
    public static readonly LogicalOperator Or = new(2, "Or", "OR", "or");

    private LogicalOperator(int id, string name, string sqlOperator, string odataOperator)
        : base(id, name)
    {
        SqlOperator = sqlOperator;
        ODataOperator = odataOperator;
    }

    /// <summary>
    /// SQL representation (AND / OR).
    /// No switch statements needed!
    /// </summary>
    public string SqlOperator { get; }

    /// <summary>
    /// OData representation (and / or).
    /// No switch statements needed!
    /// </summary>
    public string ODataOperator { get; }
}

// Usage:
var logicalOp = LogicalOperator.And;
var sqlJoin = logicalOp.SqlOperator; // "AND" - no switch!
var odataJoin = logicalOp.ODataOperator; // "and" - no switch!
```

### 5. SortDirection EnhancedEnum

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

public sealed class SortDirection : EnumOptionBase<SortDirection>
{
    public static readonly SortDirection Ascending = new(1, "Ascending", "ASC", "asc");
    public static readonly SortDirection Descending = new(2, "Descending", "DESC", "desc");

    private SortDirection(int id, string name, string sqlKeyword, string odataKeyword)
        : base(id, name)
    {
        SqlKeyword = sqlKeyword;
        ODataKeyword = odataKeyword;
    }

    public string SqlKeyword { get; }
    public string ODataKeyword { get; }
}

// Usage:
var direction = SortDirection.Ascending;
var sqlSort = $"ORDER BY [Name] {direction.SqlKeyword}"; // "ORDER BY [Name] ASC"
```

### 6. DataCommandTranslators ServiceTypeCollection

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// Base class for data command translators.
/// Registered via ServiceTypeCollection for DI.
/// </summary>
public abstract class DataCommandTranslatorBase<TService, TConfiguration> : ServiceTypeBase
    where TService : class, IDataCommandTranslator
    where TConfiguration : class, ITranslatorConfiguration
{
    protected DataCommandTranslatorBase(int id, string name, string domainName)
        : base(id, name, typeof(TService), typeof(TConfiguration))
    {
        DomainName = domainName;
    }

    public string DomainName { get; }

    public abstract void Register(IServiceCollection services);
}

/// <summary>
/// ServiceTypeCollection for translators.
/// Enables automatic registration of all translators.
/// </summary>
[ServiceTypeCollection(
    typeof(DataCommandTranslatorBase<,>),
    typeof(IDataCommandTranslator),
    typeof(DataCommandTranslators))]
public static partial class DataCommandTranslators
{
    // Source generator creates:
    // public static SqlTranslatorType Sql { get; }
    // public static RestTranslatorType Rest { get; }
    // public static FileTranslatorType File { get; }
    // public static void RegisterAll(IServiceCollection services)
}
```

### 7. Translator TypeOptions (ServiceTypeOptions)

```csharp
namespace FractalDataWorks.Data.DataCommands.Translators;

/// <summary>
/// SQL translator service type.
/// Automatically registered via ServiceTypeCollection.
/// </summary>
[ServiceTypeOption(typeof(DataCommandTranslators), "Sql")]
public sealed class SqlTranslatorType :
    DataCommandTranslatorBase<SqlDataCommandTranslator, SqlTranslatorConfiguration>,
    IDataCommandTranslatorType
{
    public static SqlTranslatorType Instance { get; } = new();

    private SqlTranslatorType() : base(id: 1, name: "Sql", domainName: "Sql") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IDataCommandTranslator, SqlDataCommandTranslator>();
        services.AddSingleton<SqlTranslatorConfiguration>();
    }
}

/// <summary>
/// REST translator service type.
/// </summary>
[ServiceTypeOption(typeof(DataCommandTranslators), "Rest")]
public sealed class RestTranslatorType :
    DataCommandTranslatorBase<RestDataCommandTranslator, RestTranslatorConfiguration>,
    IDataCommandTranslatorType
{
    public static RestTranslatorType Instance { get; } = new();

    private RestTranslatorType() : base(id: 2, name: "Rest", domainName: "Rest") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IDataCommandTranslator, RestDataCommandTranslator>();
        services.AddSingleton<RestTranslatorConfiguration>();
    }
}

/// <summary>
/// File translator service type.
/// </summary>
[ServiceTypeOption(typeof(DataCommandTranslators), "File")]
public sealed class FileTranslatorType :
    DataCommandTranslatorBase<FileDataCommandTranslator, FileTranslatorConfiguration>,
    IDataCommandTranslatorType
{
    public static FileTranslatorType Instance { get; } = new();

    private FileTranslatorType() : base(id: 3, name: "File", domainName: "File") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IDataCommandTranslator, FileDataCommandTranslator>();
        services.AddSingleton<FileTranslatorConfiguration>();
    }
}
```

### 8. Translator Registration (No Manual Code!)

```csharp
// In Program.cs or Startup
public void ConfigureServices(IServiceCollection services)
{
    // ✅ CORRECT: ServiceTypeCollection handles registration
    DataCommandTranslators.RegisterAll(services);

    // Source generator discovers all [ServiceTypeOption] attributes
    // and calls Register() on each one automatically!

    // No need for:
    // services.AddScoped<IDataCommandTranslator, SqlDataCommandTranslator>(); ❌
    // services.AddScoped<IDataCommandTranslator, RestDataCommandTranslator>(); ❌
    // services.AddScoped<IDataCommandTranslator, FileDataCommandTranslator>(); ❌
}
```

### 9. Translator Dispatch Pattern (No Switch Statements!)

```csharp
namespace FractalDataWorks.Data.DataCommands.Translators;

/// <summary>
/// Translator provider that dispatches to the correct translator.
/// Uses TypeCollection lookup instead of switch statements.
/// </summary>
public sealed class DataCommandTranslatorProvider : IDataCommandTranslatorProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataCommandTranslatorProvider> _logger;

    public DataCommandTranslatorProvider(
        IServiceProvider serviceProvider,
        ILogger<DataCommandTranslatorProvider> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IGenericResult<IDataCommandTranslator> GetTranslator(string domainName)
    {
        // ✅ CORRECT: TypeCollection lookup, no switch!
        var translatorType = DataCommandTranslators.All()
            .FirstOrDefault(t => t.DomainName.Equals(domainName, StringComparison.OrdinalIgnoreCase));

        if (translatorType == null)
        {
            DataCommandTranslatorProviderLog.TranslatorNotFound(_logger, domainName);
            return GenericResult<IDataCommandTranslator>.Failure(
                TranslatorMessages.TranslatorNotFound.WithData(domainName));
        }

        // Resolve from DI
        var translator = _serviceProvider.GetService(translatorType.ServiceType) as IDataCommandTranslator;
        if (translator == null)
        {
            DataCommandTranslatorProviderLog.TranslatorNotRegistered(_logger, domainName);
            return GenericResult<IDataCommandTranslator>.Failure(
                TranslatorMessages.TranslatorNotRegistered.WithData(domainName));
        }

        return GenericResult<IDataCommandTranslator>.Success(translator);
    }

    // ❌ OLD WAY: Switch statement (NEVER DO THIS)
    /*
    public IGenericResult<IDataCommandTranslator> GetTranslatorOld(string domainName)
    {
        IDataCommandTranslator translator = domainName switch
        {
            "Sql" => _serviceProvider.GetRequiredService<SqlDataCommandTranslator>(),
            "Rest" => _serviceProvider.GetRequiredService<RestDataCommandTranslator>(),
            "File" => _serviceProvider.GetRequiredService<FileDataCommandTranslator>(),
            _ => throw new NotSupportedException($"Domain {domainName} not supported")
        };

        return GenericResult<IDataCommandTranslator>.Success(translator);
    }
    */
}
```

### 10. Command Translation (No Switch on CommandType!)

```csharp
namespace FractalDataWorks.Data.DataCommands.Translators;

/// <summary>
/// SQL translator - NO SWITCH STATEMENTS.
/// Uses polymorphism and visitor pattern instead.
/// </summary>
public sealed class SqlDataCommandTranslator : IDataCommandTranslator
{
    private readonly ISqlCommandBuilder _commandBuilder;

    public string DomainName => "Sql";

    public async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext containerContext,
        CancellationToken cancellationToken = default)
    {
        // ✅ CORRECT: Visitor pattern, no switch on type
        var visitor = new SqlCommandVisitor(_commandBuilder, containerContext);
        var result = await command.AcceptAsync(visitor, cancellationToken);
        return result;
    }

    // ❌ OLD WAY: Switch statement (NEVER DO THIS)
    /*
    public async Task<IGenericResult<IConnectionCommand>> TranslateAsyncOld(
        IDataCommand command,
        IContainerContext containerContext,
        CancellationToken cancellationToken = default)
    {
        var sqlText = command.CommandType switch
        {
            DataCommandType.Query => BuildQuerySql((QueryCommand)command, containerContext),
            DataCommandType.Insert => BuildInsertSql((InsertCommand)command, containerContext),
            DataCommandType.Update => BuildUpdateSql((UpdateCommand)command, containerContext),
            // ...
            _ => throw new NotSupportedException()
        };

        // ...
    }
    */
}

/// <summary>
/// Visitor pattern for SQL command building.
/// Each command type knows how to accept visitors.
/// </summary>
public interface ISqlCommandVisitor
{
    Task<IGenericResult<IConnectionCommand>> VisitQuery(QueryCommand command, CancellationToken ct);
    Task<IGenericResult<IConnectionCommand>> VisitInsert(InsertCommand command, CancellationToken ct);
    Task<IGenericResult<IConnectionCommand>> VisitUpdate(UpdateCommand command, CancellationToken ct);
    Task<IGenericResult<IConnectionCommand>> VisitDelete(DeleteCommand command, CancellationToken ct);
    Task<IGenericResult<IConnectionCommand>> VisitUpsert(UpsertCommand command, CancellationToken ct);
    Task<IGenericResult<IConnectionCommand>> VisitBulkInsert(BulkInsertCommand command, CancellationToken ct);
}

/// <summary>
/// Update DataCommand interface to support visitor pattern.
/// </summary>
public interface IDataCommand
{
    // ... existing properties

    /// <summary>
    /// Accept visitor for translation (replaces switch statements).
    /// </summary>
    Task<IGenericResult<IConnectionCommand>> AcceptAsync<TVisitor>(
        TVisitor visitor,
        CancellationToken cancellationToken) where TVisitor : ISqlCommandVisitor;
}

/// <summary>
/// QueryCommand accepts visitors.
/// </summary>
[TypeOption(typeof(DataCommands), "Query")]
public sealed class QueryCommand : DataCommandBase
{
    // ... properties

    public override async Task<IGenericResult<IConnectionCommand>> AcceptAsync<TVisitor>(
        TVisitor visitor,
        CancellationToken cancellationToken)
    {
        return await visitor.VisitQuery(this, cancellationToken);
    }
}

/// <summary>
/// InsertCommand accepts visitors.
/// </summary>
[TypeOption(typeof(DataCommands), "Insert")]
public sealed class InsertCommand : DataCommandBase
{
    // ... properties

    public override async Task<IGenericResult<IConnectionCommand>> AcceptAsync<TVisitor>(
        TVisitor visitor,
        CancellationToken cancellationToken)
    {
        return await visitor.VisitInsert(this, cancellationToken);
    }
}

// Similar for Update, Delete, Upsert, BulkInsert
```

### 11. Capabilities - Alternative to [Flags] Enum

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// Capability definition base class.
/// Replaces [Flags] enum with TypeCollection.
/// </summary>
public abstract class CommandCapabilityBase
{
    protected CommandCapabilityBase(int id, string name, string description, int priority)
    {
        Id = id;
        Name = name;
        Description = description;
        Priority = priority;
    }

    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int Priority { get; }
}

/// <summary>
/// TypeCollection for capabilities.
/// </summary>
[TypeCollection(typeof(CommandCapabilityBase), typeof(CommandCapabilityBase), typeof(CommandCapabilities))]
public abstract partial class CommandCapabilities : TypeCollectionBase<CommandCapabilityBase, CommandCapabilityBase>
{
    // Source generator creates:
    // public static QueryCapability Query { get; }
    // public static InsertCapability Insert { get; }
    // public static UpdateCapability Update { get; }
    // ... etc
}

/// <summary>
/// Capability set for a translator.
/// Instead of [Flags] enum bitwise operations, use HashSet.
/// </summary>
public sealed class TranslatorCapabilitySet
{
    private readonly HashSet<CommandCapabilityBase> _capabilities;

    public TranslatorCapabilitySet(params CommandCapabilityBase[] capabilities)
    {
        _capabilities = new HashSet<CommandCapabilityBase>(capabilities);
    }

    public bool Supports(CommandCapabilityBase capability) => _capabilities.Contains(capability);

    public bool SupportsAll(params CommandCapabilityBase[] capabilities) =>
        capabilities.All(c => _capabilities.Contains(c));
}

// Usage:
public class SqlDataCommandTranslator : IDataCommandTranslator
{
    public TranslatorCapabilitySet GetCapabilities()
    {
        return new TranslatorCapabilitySet(
            CommandCapabilities.Query,
            CommandCapabilities.Insert,
            CommandCapabilities.Update,
            CommandCapabilities.Delete,
            CommandCapabilities.Upsert,
            CommandCapabilities.BulkInsert,
            CommandCapabilities.Aggregation,
            CommandCapabilities.Joins,
            CommandCapabilities.Transactions
        );
    }
}

// Check capabilities:
var capabilities = translator.GetCapabilities();
if (capabilities.Supports(CommandCapabilities.Aggregation))
{
    // Execute aggregation query
}
```

## Benefits of TypeCollection Approach

### Before (Enum + Switch):
```csharp
// ❌ Enum definition
public enum FilterOperator
{
    Equal,
    NotEqual,
    Contains,
    // ... 15+ values
}

// ❌ Switch statement for SQL (repeated in every translator!)
private string GetSqlOperator(FilterOperator op)
{
    return op switch
    {
        FilterOperator.Equal => "=",
        FilterOperator.NotEqual => "<>",
        FilterOperator.Contains => "LIKE",
        // ... 15+ cases
        _ => throw new NotSupportedException()
    };
}

// ❌ Another switch for OData (duplication!)
private string GetODataOperator(FilterOperator op)
{
    return op switch
    {
        FilterOperator.Equal => "eq",
        FilterOperator.NotEqual => "ne",
        FilterOperator.Contains => "contains",
        // ... 15+ cases
        _ => throw new NotSupportedException()
    };
}

// ❌ Yet another switch for parameter formatting
private string FormatParameter(FilterOperator op, string paramName)
{
    return op switch
    {
        FilterOperator.Contains => $"'%' + @{paramName} + '%'",
        FilterOperator.StartsWith => $"@{paramName} + '%'",
        // ... more cases
        _ => $"@{paramName}"
    };
}
```

**Problems:**
- Duplication across translators
- Adding new operator requires updating multiple switch statements
- No type safety (can forget a case)
- Can't add operator-specific behavior without more switch statements

### After (TypeCollection):
```csharp
// ✅ Operator knows its own SQL, OData, and parameter formatting
var condition = new FilterCondition
{
    PropertyName = "Name",
    Operator = FilterOperators.Contains,
    Value = "Acme"
};

// ✅ No switch statements - just property access!
var sqlCondition = $"[{condition.PropertyName}] {condition.Operator.SqlOperator} {condition.Operator.FormatSqlParameter(condition.PropertyName)}";
// Result: "[Name] LIKE '%' + @Name + '%'"

var odataCondition = $"{condition.PropertyName} {condition.Operator.ODataOperator} {condition.Operator.FormatODataValue(condition.Value)}";
// Result: "Name contains 'Acme'"
```

**Benefits:**
- Zero duplication (operator knows all its representations)
- Adding new operator: create one class with [TypeOption], done!
- Type safe (source generator ensures collection is complete)
- Extensible (operators can have methods, not just data)
- No switch statements anywhere

## Migration Checklist

### Phase 1: Replace Enums with TypeCollections
- [ ] Replace `FilterOperator` enum with `FilterOperators` TypeCollection
- [ ] Replace `LogicalOperator` enum with `LogicalOperator` EnhancedEnum
- [ ] Replace `SortDirection` enum with `SortDirection` EnhancedEnum
- [ ] Replace `DataCommandType` enum with TypeCollection (or remove entirely with visitor pattern)
- [ ] Replace `[Flags] DataCommandCapabilities` enum with TypeCollection + HashSet

### Phase 2: Remove Switch Statements
- [ ] Replace switch on `FilterOperator` with property access
- [ ] Replace switch on `LogicalOperator` with property access
- [ ] Replace switch on `SortDirection` with property access
- [ ] Replace switch on `DataCommandType` with visitor pattern

### Phase 3: Add ServiceTypeCollection for Translators
- [ ] Create `DataCommandTranslatorBase<,>` base class
- [ ] Create `DataCommandTranslators` ServiceTypeCollection
- [ ] Add `[ServiceTypeOption]` to SqlTranslatorType, RestTranslatorType, FileTranslatorType
- [ ] Replace manual registration with `DataCommandTranslators.RegisterAll(services)`

### Phase 4: Implement Visitor Pattern for Command Dispatch
- [ ] Add `ISqlCommandVisitor` interface
- [ ] Add `AcceptAsync<TVisitor>` method to `IDataCommand`
- [ ] Implement `AcceptAsync` in each command type (QueryCommand, InsertCommand, etc.)
- [ ] Create `SqlCommandVisitor` implementation
- [ ] Remove switch statements from translators

## Conclusion

This correction brings the DataCommands architecture into compliance with CLAUDE.md rules:

✅ **No enums** - TypeCollections and EnhancedEnums instead
✅ **No switch statements** - Property access and visitor pattern instead
✅ **ServiceTypeCollection for DI** - Automatic registration of translators
✅ **Type safe** - Source generators ensure completeness
✅ **Extensible** - Add new operators/commands via [TypeOption] attributes
✅ **No duplication** - Each type knows its own behavior

The architecture now follows the Developer Kit's philosophy of source-generated, type-safe, zero-reflection patterns.
