# DataCommands Architecture (CORRECTION)

## Critical Correction

**IMPORTANT**: This architecture replaces the previous "Query Specification" terminology which was too restrictive.

The previous proposal incorrectly focused only on querying (SELECT operations). In reality, data connections need to support **all data operations**:

- **Query** (SELECT)
- **Insert** (INSERT)
- **Update** (UPDATE)
- **Delete** (DELETE)
- **Upsert** (MERGE / INSERT ON CONFLICT)
- **BulkInsert** (ETL bulk operations)

This document presents the corrected architecture using the **DataCommands TypeCollection pattern**.

## Architecture Overview

```
LINQ Expression
    ↓
LinqDataCommandBuilder (decomposes expression into IDataCommand)
    ↓
IDataCommand (universal intermediate representation)
    ↓
DataCommandTranslator (domain-specific)
    ↓
IConnectionCommand (SQL/REST/File/GraphQL specific)
    ↓
IDataConnection.ExecuteAsync()
```

### Key Insight: TypeCollection Pattern

Instead of separate interfaces for each operation type, we use **TypeCollection with DataCommands**:

```csharp
[TypeCollection(typeof(DataCommandBase), typeof(IDataCommand), typeof(DataCommands))]
public abstract partial class DataCommands : TypeCollectionBase<DataCommandBase, IDataCommand>
{
    // Source generator creates:
    // - DataCommands.Query (static property)
    // - DataCommands.Insert (static property)
    // - DataCommands.Update (static property)
    // - DataCommands.Delete (static property)
    // - DataCommands.Upsert (static property)
    // - DataCommands.BulkInsert (static property)
    // - DataCommands.All() returns FrozenSet
    // - DataCommands.GetByName("Query") with O(1) lookup
}
```

Each TypeOption breaks down the operation into **common architectural components** that translators can handle uniformly.

## Core Abstractions

### 1. Base Command Interface

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// Base interface for all data commands.
/// Represents the universal intermediate format between LINQ expressions
/// and domain-specific connection commands (SQL, REST, File, etc.).
/// </summary>
public interface IDataCommand
{
    /// <summary>
    /// The type of operation this command represents.
    /// </summary>
    DataCommandType CommandType { get; }

    /// <summary>
    /// The container (table/endpoint/file) this command targets.
    /// </summary>
    string ContainerName { get; }

    /// <summary>
    /// Additional metadata for the command (timeouts, hints, etc.).
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// The source type (for type-safe execution).
    /// </summary>
    Type SourceType { get; }

    /// <summary>
    /// The result type (for type-safe execution).
    /// </summary>
    Type ResultType { get; }
}

/// <summary>
/// Base abstract class for data commands.
/// Provides common infrastructure for all command types.
/// </summary>
public abstract class DataCommandBase : IDataCommand
{
    protected DataCommandBase(
        int id,
        string name,
        DataCommandType commandType,
        string containerName,
        Type sourceType,
        Type resultType)
    {
        Id = id;
        Name = name;
        CommandType = commandType;
        ContainerName = containerName;
        SourceType = sourceType;
        ResultType = resultType;
        Metadata = new Dictionary<string, object>();
    }

    public int Id { get; }
    public string Name { get; }
    public DataCommandType CommandType { get; }
    public string ContainerName { get; }
    public Type SourceType { get; }
    public Type ResultType { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Enum for command types (used for dispatch and validation).
/// </summary>
public enum DataCommandType
{
    Query = 1,
    Insert = 2,
    Update = 3,
    Delete = 4,
    Upsert = 5,
    BulkInsert = 6
}
```

### 2. DataCommands TypeCollection

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// TypeCollection for all data command types.
/// Source generator discovers all [TypeOption] attributes and creates static properties.
/// </summary>
[TypeCollection(typeof(DataCommandBase), typeof(IDataCommand), typeof(DataCommands))]
public abstract partial class DataCommands : TypeCollectionBase<DataCommandBase, IDataCommand>
{
    // ⚠️ NEVER manually create constructor - source generated!
    // ⚠️ NEVER create empty value - source generated!

    // Source generator creates:
    // public static QueryCommand Query { get; }
    // public static InsertCommand Insert { get; }
    // public static UpdateCommand Update { get; }
    // public static DeleteCommand Delete { get; }
    // public static UpsertCommand Upsert { get; }
    // public static BulkInsertCommand BulkInsert { get; }
    //
    // public static FrozenSet<IDataCommand> All() { get; }
    // public static IDataCommand? GetByName(string name) { get; }
    // public static IDataCommand? GetById(int id) { get; }
}
```

## TypeOptions for Each Operation

### 1. Query Command (SELECT)

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Query command for SELECT operations.
/// Breaks down into common components: Filter, Projection, Ordering, Paging.
/// </summary>
[TypeOption(typeof(DataCommands), "Query")]
public sealed class QueryCommand : DataCommandBase
{
    // Source generator reads constructor args to populate DataCommands.Query
    public QueryCommand()
        : base(
            id: 1,
            name: "Query",
            commandType: DataCommandType.Query,
            containerName: string.Empty,
            sourceType: typeof(object),
            resultType: typeof(object))
    {
    }

    /// <summary>
    /// Filter expression (WHERE clause).
    /// Common architecture: PropertyName, Operator, Value, LogicalOperator
    /// </summary>
    public IFilterExpression? Filter { get; init; }

    /// <summary>
    /// Projection expression (SELECT fields).
    /// Common architecture: SourceField, TargetField, TransformExpression
    /// </summary>
    public IProjectionExpression? Projection { get; init; }

    /// <summary>
    /// Ordering expression (ORDER BY).
    /// Common architecture: FieldName, Direction (Asc/Desc), NullsFirst/Last
    /// </summary>
    public IOrderingExpression? Ordering { get; init; }

    /// <summary>
    /// Paging expression (OFFSET/LIMIT).
    /// Common architecture: Skip, Take
    /// </summary>
    public IPagingExpression? Paging { get; init; }

    /// <summary>
    /// Aggregation expression (GROUP BY, aggregates).
    /// Common architecture: GroupByFields, Aggregates (Count, Sum, Avg, etc.)
    /// </summary>
    public IAggregationExpression? Aggregation { get; init; }

    /// <summary>
    /// Join expressions (INNER/LEFT/RIGHT JOIN).
    /// Common architecture: JoinType, LeftField, RightField, OnExpression
    /// </summary>
    public IReadOnlyList<IJoinExpression> Joins { get; init; } = [];
}

/// <summary>
/// Filter expression - common across all domains.
/// </summary>
public interface IFilterExpression
{
    IReadOnlyList<FilterCondition> Conditions { get; }
    LogicalOperator LogicalOperator { get; } // AND / OR
}

public sealed class FilterCondition
{
    public required string PropertyName { get; init; }
    public required FilterOperator Operator { get; init; } // Eq, Ne, Gt, Lt, Contains, etc.
    public object? Value { get; init; }
}

public enum FilterOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    In,
    NotIn,
    IsNull,
    IsNotNull
}

public enum LogicalOperator
{
    And,
    Or
}
```

### 2. Insert Command

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Insert command for INSERT operations.
/// Breaks down into common components: Data (field-value pairs), ReturnIdentity flag.
/// </summary>
[TypeOption(typeof(DataCommands), "Insert")]
public sealed class InsertCommand : DataCommandBase
{
    public InsertCommand()
        : base(
            id: 2,
            name: "Insert",
            commandType: DataCommandType.Insert,
            containerName: string.Empty,
            sourceType: typeof(object),
            resultType: typeof(object))
    {
    }

    /// <summary>
    /// Data to insert.
    /// Common architecture: Dictionary of field names to values.
    /// Translators convert to domain-specific format:
    /// - SQL: INSERT INTO table (field1, field2) VALUES (@value1, @value2)
    /// - REST: POST /endpoint with JSON body { field1: value1, field2: value2 }
    /// - File: Append row to CSV/JSON with field mappings
    /// </summary>
    public IReadOnlyDictionary<string, object?> Data { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Whether to return the generated identity value.
    /// Common architecture: Boolean flag.
    /// Translators handle:
    /// - SQL: OUTPUT INSERTED.Id or RETURNING id
    /// - REST: Parse Location header or response body for new ID
    /// - File: Return row number or generated GUID
    /// </summary>
    public bool ReturnIdentity { get; init; } = true;

    /// <summary>
    /// Conflict resolution strategy (for databases that support it).
    /// </summary>
    public InsertConflictStrategy ConflictStrategy { get; init; } = InsertConflictStrategy.Error;
}

public enum InsertConflictStrategy
{
    Error,      // Fail on conflict (default)
    Ignore,     // Ignore conflicts (INSERT IGNORE)
    Replace     // Replace on conflict (REPLACE INTO)
}
```

### 3. Update Command

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Update command for UPDATE operations.
/// Breaks down into common components: UpdatedData, Filter.
/// </summary>
[TypeOption(typeof(DataCommands), "Update")]
public sealed class UpdateCommand : DataCommandBase
{
    public UpdateCommand()
        : base(
            id: 3,
            name: "Update",
            commandType: DataCommandType.Update,
            containerName: string.Empty,
            sourceType: typeof(object),
            resultType: typeof(int)) // Returns affected row count
    {
    }

    /// <summary>
    /// Data to update.
    /// Common architecture: Dictionary of field names to new values.
    /// Translators convert:
    /// - SQL: UPDATE table SET field1 = @value1, field2 = @value2
    /// - REST: PATCH /endpoint/:id with JSON body { field1: value1, field2: value2 }
    /// - File: Update matching rows in CSV/JSON
    /// </summary>
    public IReadOnlyDictionary<string, object?> UpdatedData { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Filter to identify records to update (WHERE clause).
    /// Common architecture: Same as QueryCommand.Filter
    /// </summary>
    public IFilterExpression? Filter { get; init; }

    /// <summary>
    /// Whether to return updated row count.
    /// </summary>
    public bool ReturnAffectedCount { get; init; } = true;
}
```

### 4. Delete Command

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Delete command for DELETE operations.
/// Breaks down into common components: Filter.
/// </summary>
[TypeOption(typeof(DataCommands), "Delete")]
public sealed class DeleteCommand : DataCommandBase
{
    public DeleteCommand()
        : base(
            id: 4,
            name: "Delete",
            commandType: DataCommandType.Delete,
            containerName: string.Empty,
            sourceType: typeof(object),
            resultType: typeof(int)) // Returns affected row count
    {
    }

    /// <summary>
    /// Filter to identify records to delete (WHERE clause).
    /// Common architecture: Same as QueryCommand.Filter
    /// Translators convert:
    /// - SQL: DELETE FROM table WHERE condition
    /// - REST: DELETE /endpoint/:id or DELETE /endpoint with query params
    /// - File: Remove matching rows from CSV/JSON
    /// </summary>
    public IFilterExpression? Filter { get; init; }

    /// <summary>
    /// Whether to return deleted row count.
    /// </summary>
    public bool ReturnAffectedCount { get; init; } = true;

    /// <summary>
    /// Soft delete configuration (if supported).
    /// </summary>
    public SoftDeleteOptions? SoftDelete { get; init; }
}

/// <summary>
/// Soft delete configuration.
/// Instead of physically deleting, updates a flag column.
/// </summary>
public sealed class SoftDeleteOptions
{
    public required string FlagColumnName { get; init; } // e.g., "IsDeleted"
    public object DeletedValue { get; init; } = true; // Value to set (true, 1, DateTime.UtcNow, etc.)
}
```

### 5. Upsert Command

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// Upsert command for MERGE / INSERT ON CONFLICT operations.
/// Breaks down into common components: Data, ConflictFields, UpdateOnConflict.
/// </summary>
[TypeOption(typeof(DataCommands), "Upsert")]
public sealed class UpsertCommand : DataCommandBase
{
    public UpsertCommand()
        : base(
            id: 5,
            name: "Upsert",
            commandType: DataCommandType.Upsert,
            containerName: string.Empty,
            sourceType: typeof(object),
            resultType: typeof(object))
    {
    }

    /// <summary>
    /// Data to insert or update.
    /// Common architecture: Dictionary of field names to values.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Data { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Fields to check for conflicts (unique key).
    /// Common architecture: List of field names.
    /// Translators convert:
    /// - SQL: MERGE ... ON (field1 = @field1 AND field2 = @field2)
    /// - SQL: INSERT ... ON CONFLICT (field1, field2) DO UPDATE SET ...
    /// - REST: Check if exists, then POST or PATCH
    /// - File: Find matching row by key fields, update or append
    /// </summary>
    public IReadOnlyList<string> ConflictFields { get; init; } = [];

    /// <summary>
    /// Fields to update on conflict (if different from insert values).
    /// If null, uses Data values for both insert and update.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? UpdateOnConflict { get; init; }

    /// <summary>
    /// Whether to return the identity (new or existing).
    /// </summary>
    public bool ReturnIdentity { get; init; } = true;
}
```

### 6. BulkInsert Command

```csharp
namespace FractalDataWorks.Data.DataCommands;

/// <summary>
/// BulkInsert command for ETL bulk operations.
/// Breaks down into common components: Records, BatchSize, ErrorHandling.
/// </summary>
[TypeOption(typeof(DataCommands), "BulkInsert")]
public sealed class BulkInsertCommand : DataCommandBase
{
    public BulkInsertCommand()
        : base(
            id: 6,
            name: "BulkInsert",
            commandType: DataCommandType.BulkInsert,
            containerName: string.Empty,
            sourceType: typeof(object),
            resultType: typeof(BulkInsertResult))
    {
    }

    /// <summary>
    /// Records to insert.
    /// Common architecture: List of dictionaries (each record is field-value pairs).
    /// Translators convert:
    /// - SQL: Bulk insert via TVP, BULK INSERT, or batched INSERTs
    /// - REST: Batch POST requests (may need chunking based on API limits)
    /// - File: Append multiple rows to CSV/JSON
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Records { get; init; } = [];

    /// <summary>
    /// Batch size for chunked operations.
    /// Common architecture: Int32 value.
    /// SQL: Insert 1000 at a time
    /// REST: POST 100 records per request
    /// File: Buffer size for write operations
    /// </summary>
    public int BatchSize { get; init; } = 1000;

    /// <summary>
    /// Error handling strategy.
    /// </summary>
    public BulkErrorHandling ErrorHandling { get; init; } = BulkErrorHandling.StopOnFirstError;

    /// <summary>
    /// Whether to use transactions (if supported).
    /// </summary>
    public bool UseTransaction { get; init; } = true;
}

public enum BulkErrorHandling
{
    StopOnFirstError,   // Rollback entire operation on first error
    ContinueOnError,    // Skip failed records, continue with rest
    CollectErrors       // Collect all errors, return summary
}

/// <summary>
/// Result of bulk insert operation.
/// </summary>
public sealed class BulkInsertResult
{
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public IReadOnlyList<BulkInsertError> Errors { get; init; } = [];
}

public sealed class BulkInsertError
{
    public int RecordIndex { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> FailedRecord { get; init; } = new Dictionary<string, object?>();
}
```

## DataCommand Translators (Domain-Specific)

### Translator Interface

```csharp
namespace FractalDataWorks.Data.DataCommands.Abstractions;

/// <summary>
/// Translates universal IDataCommand to domain-specific IConnectionCommand.
/// Each domain (SQL, REST, File, GraphQL) implements its own translator.
/// </summary>
public interface IDataCommandTranslator
{
    /// <summary>
    /// Domain name (Sql, Rest, File, GraphQL).
    /// </summary>
    string DomainName { get; }

    /// <summary>
    /// Supported command types for this domain.
    /// Example: File domain may not support aggregations.
    /// </summary>
    DataCommandCapabilities GetCapabilities();

    /// <summary>
    /// Validates whether the command can be translated for this domain.
    /// </summary>
    IGenericResult ValidateCommand(IDataCommand command);

    /// <summary>
    /// Translates IDataCommand to domain-specific IConnectionCommand.
    /// </summary>
    Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext containerContext,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Capabilities for data command execution.
/// </summary>
[Flags]
public enum DataCommandCapabilities
{
    None = 0,
    Query = 1 << 0,
    Insert = 1 << 1,
    Update = 1 << 2,
    Delete = 1 << 3,
    Upsert = 1 << 4,
    BulkInsert = 1 << 5,
    Aggregation = 1 << 6,
    Joins = 1 << 7,
    Transactions = 1 << 8,

    // Common combinations
    BasicCRUD = Query | Insert | Update | Delete,
    FullSQL = Query | Insert | Update | Delete | Upsert | BulkInsert | Aggregation | Joins | Transactions,
    RestAPI = Query | Insert | Update | Delete,
    FileSystem = Query | Insert | BulkInsert
}
```

### SQL Translator (Stub)

```csharp
namespace FractalDataWorks.Data.DataCommands.Translators;

/// <summary>
/// STUB IMPLEMENTATION - Translates IDataCommand to SQL commands.
/// </summary>
/// <remarks>
/// Shows how common architecture components map to SQL:
/// - QueryCommand.Filter → WHERE clause
/// - QueryCommand.Projection → SELECT fields
/// - QueryCommand.Ordering → ORDER BY clause
/// - QueryCommand.Paging → OFFSET/FETCH
/// - InsertCommand.Data → INSERT INTO ... VALUES
/// - UpdateCommand.UpdatedData + Filter → UPDATE ... SET ... WHERE
/// - DeleteCommand.Filter → DELETE FROM ... WHERE
/// - UpsertCommand → MERGE or INSERT ON CONFLICT
/// - BulkInsertCommand → BULK INSERT or TVP
/// </remarks>
public sealed class SqlDataCommandTranslator : IDataCommandTranslator
{
    public string DomainName => "Sql";

    public DataCommandCapabilities GetCapabilities()
    {
        return DataCommandCapabilities.FullSQL;
    }

    public IGenericResult ValidateCommand(IDataCommand command)
    {
        if (command == null)
            return GenericResult.Failure("Command cannot be null");

        // All command types supported in SQL
        return GenericResult.Success();
    }

    public async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext containerContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Dispatch based on command type
            var sqlText = command.CommandType switch
            {
                DataCommandType.Query => BuildQuerySql((QueryCommand)command, containerContext),
                DataCommandType.Insert => BuildInsertSql((InsertCommand)command, containerContext),
                DataCommandType.Update => BuildUpdateSql((UpdateCommand)command, containerContext),
                DataCommandType.Delete => BuildDeleteSql((DeleteCommand)command, containerContext),
                DataCommandType.Upsert => BuildUpsertSql((UpsertCommand)command, containerContext),
                DataCommandType.BulkInsert => BuildBulkInsertSql((BulkInsertCommand)command, containerContext),
                _ => throw new NotSupportedException($"Command type {command.CommandType} not supported")
            };

            var parameters = ExtractParameters(command);

            var connectionCommand = new StubSqlConnectionCommand
            {
                SqlText = sqlText,
                Parameters = parameters,
                CommandTimeout = TimeSpan.FromSeconds(30),
                Metadata = new Dictionary<string, object>
                {
                    { "GeneratedAt", DateTime.UtcNow },
                    { "Translator", DomainName },
                    { "CommandType", command.CommandType }
                }
            };

            return await Task.FromResult(GenericResult<IConnectionCommand>.Success(connectionCommand));
        }
        catch (Exception ex)
        {
            return GenericResult<IConnectionCommand>.Failure($"Failed to translate command: {ex.Message}");
        }
    }

    private string BuildQuerySql(QueryCommand command, IContainerContext context)
    {
        // TODO: Use SQL ScriptDom for proper SQL AST generation
        var sql = new StringBuilder();

        // SELECT clause
        sql.Append("SELECT ");
        if (command.Projection != null && command.Projection.Fields.Any())
            sql.Append(string.Join(", ", command.Projection.Fields.Select(f => $"[{f.SourceName}]")));
        else
            sql.Append("*");

        // FROM clause
        sql.Append($" FROM [{command.ContainerName}]");

        // WHERE clause (filter)
        if (command.Filter != null && command.Filter.Conditions.Any())
        {
            sql.Append(" WHERE ");
            var conditions = command.Filter.Conditions.Select(c =>
                $"[{c.PropertyName}] {GetSqlOperator(c.Operator)} {GetParameterPlaceholder(c.PropertyName, c.Operator)}");
            var logicalOp = command.Filter.LogicalOperator == LogicalOperator.Or ? " OR " : " AND ";
            sql.Append(string.Join(logicalOp, conditions));
        }

        // ORDER BY clause
        if (command.Ordering != null && command.Ordering.OrderedFields.Any())
        {
            sql.Append(" ORDER BY ");
            sql.Append(string.Join(", ", command.Ordering.OrderedFields.Select(f =>
                $"[{f.FieldName}] {(f.Direction == SortDirection.Ascending ? "ASC" : "DESC")}")));
        }

        // OFFSET/FETCH (paging)
        if (command.Paging != null)
        {
            if (command.Paging.Skip.HasValue || command.Paging.Take.HasValue)
            {
                if (command.Ordering == null || !command.Ordering.OrderedFields.Any())
                    sql.Append(" ORDER BY (SELECT NULL)"); // Dummy ORDER BY required for OFFSET

                if (command.Paging.Skip.HasValue)
                    sql.Append($" OFFSET {command.Paging.Skip.Value} ROWS");
                else
                    sql.Append(" OFFSET 0 ROWS");

                if (command.Paging.Take.HasValue)
                    sql.Append($" FETCH NEXT {command.Paging.Take.Value} ROWS ONLY");
            }
        }

        return sql.ToString();
    }

    private string BuildInsertSql(InsertCommand command, IContainerContext context)
    {
        // INSERT INTO table (field1, field2, field3) VALUES (@field1, @field2, @field3)
        var sql = new StringBuilder();
        sql.Append($"INSERT INTO [{command.ContainerName}] (");
        sql.Append(string.Join(", ", command.Data.Keys.Select(k => $"[{k}]")));
        sql.Append(") VALUES (");
        sql.Append(string.Join(", ", command.Data.Keys.Select(k => $"@{k}")));
        sql.Append(")");

        if (command.ReturnIdentity)
            sql.Append("; SELECT SCOPE_IDENTITY()"); // SQL Server specific

        return sql.ToString();
    }

    private string BuildUpdateSql(UpdateCommand command, IContainerContext context)
    {
        // UPDATE table SET field1 = @field1, field2 = @field2 WHERE condition
        var sql = new StringBuilder();
        sql.Append($"UPDATE [{command.ContainerName}] SET ");
        sql.Append(string.Join(", ", command.UpdatedData.Keys.Select(k => $"[{k}] = @upd_{k}")));

        if (command.Filter != null && command.Filter.Conditions.Any())
        {
            sql.Append(" WHERE ");
            var conditions = command.Filter.Conditions.Select(c =>
                $"[{c.PropertyName}] {GetSqlOperator(c.Operator)} @filter_{c.PropertyName}");
            var logicalOp = command.Filter.LogicalOperator == LogicalOperator.Or ? " OR " : " AND ";
            sql.Append(string.Join(logicalOp, conditions));
        }

        return sql.ToString();
    }

    private string BuildDeleteSql(DeleteCommand command, IContainerContext context)
    {
        // DELETE FROM table WHERE condition
        var sql = new StringBuilder();

        // Check for soft delete
        if (command.SoftDelete != null)
        {
            // Convert to UPDATE statement
            sql.Append($"UPDATE [{command.ContainerName}] SET [{command.SoftDelete.FlagColumnName}] = @softDeleteValue");
        }
        else
        {
            sql.Append($"DELETE FROM [{command.ContainerName}]");
        }

        if (command.Filter != null && command.Filter.Conditions.Any())
        {
            sql.Append(" WHERE ");
            var conditions = command.Filter.Conditions.Select(c =>
                $"[{c.PropertyName}] {GetSqlOperator(c.Operator)} @{c.PropertyName}");
            var logicalOp = command.Filter.LogicalOperator == LogicalOperator.Or ? " OR " : " AND ";
            sql.Append(string.Join(logicalOp, conditions));
        }

        return sql.ToString();
    }

    private string BuildUpsertSql(UpsertCommand command, IContainerContext context)
    {
        // MERGE or INSERT ... ON CONFLICT (depends on SQL dialect)
        // This is SQL Server MERGE syntax (stub)
        var sql = new StringBuilder();
        sql.Append($"MERGE [{command.ContainerName}] AS target ");
        sql.Append("USING (VALUES (");
        sql.Append(string.Join(", ", command.Data.Keys.Select(k => $"@{k}")));
        sql.Append(")) AS source (");
        sql.Append(string.Join(", ", command.Data.Keys));
        sql.Append(") ON (");
        sql.Append(string.Join(" AND ", command.ConflictFields.Select(f => $"target.[{f}] = source.{f}")));
        sql.Append(") WHEN MATCHED THEN UPDATE SET ");

        var updateFields = command.UpdateOnConflict ?? command.Data;
        sql.Append(string.Join(", ", updateFields.Keys.Select(k => $"[{k}] = source.{k}")));

        sql.Append(" WHEN NOT MATCHED THEN INSERT (");
        sql.Append(string.Join(", ", command.Data.Keys.Select(k => $"[{k}]")));
        sql.Append(") VALUES (");
        sql.Append(string.Join(", ", command.Data.Keys.Select(k => $"source.{k}")));
        sql.Append(");");

        if (command.ReturnIdentity)
            sql.Append(" SELECT SCOPE_IDENTITY();");

        return sql.ToString();
    }

    private string BuildBulkInsertSql(BulkInsertCommand command, IContainerContext context)
    {
        // For SQL Server, would use TVP (Table-Valued Parameter) or BULK INSERT
        // Stub shows batched INSERT approach
        var sql = new StringBuilder();

        // Batch inserts (simplified - real implementation would use TVP)
        sql.Append($"-- BULK INSERT {command.Records.Count} records into [{command.ContainerName}]");
        sql.AppendLine();
        sql.AppendLine($"-- BatchSize: {command.BatchSize}");
        sql.AppendLine($"-- ErrorHandling: {command.ErrorHandling}");
        sql.AppendLine();

        // Example: First batch only (stub)
        var batch = command.Records.Take(command.BatchSize);
        foreach (var (record, index) in batch.Select((r, i) => (r, i)))
        {
            sql.Append($"INSERT INTO [{command.ContainerName}] (");
            sql.Append(string.Join(", ", record.Keys.Select(k => $"[{k}]")));
            sql.Append(") VALUES (");
            sql.Append(string.Join(", ", record.Keys.Select(k => $"@batch{index}_{k}")));
            sql.Append(");");
            sql.AppendLine();
        }

        return sql.ToString();
    }

    private string GetSqlOperator(FilterOperator op)
    {
        return op switch
        {
            FilterOperator.Equal => "=",
            FilterOperator.NotEqual => "<>",
            FilterOperator.GreaterThan => ">",
            FilterOperator.GreaterThanOrEqual => ">=",
            FilterOperator.LessThan => "<",
            FilterOperator.LessThanOrEqual => "<=",
            FilterOperator.Contains => "LIKE",
            FilterOperator.StartsWith => "LIKE",
            FilterOperator.EndsWith => "LIKE",
            FilterOperator.In => "IN",
            FilterOperator.NotIn => "NOT IN",
            FilterOperator.IsNull => "IS NULL",
            FilterOperator.IsNotNull => "IS NOT NULL",
            _ => throw new NotSupportedException($"Operator {op} not supported")
        };
    }

    private string GetParameterPlaceholder(string propertyName, FilterOperator op)
    {
        if (op == FilterOperator.IsNull || op == FilterOperator.IsNotNull)
            return string.Empty;

        if (op == FilterOperator.Contains)
            return $"'%' + @{propertyName} + '%'";
        if (op == FilterOperator.StartsWith)
            return $"@{propertyName} + '%'";
        if (op == FilterOperator.EndsWith)
            return $"'%' + @{propertyName}";

        return $"@{propertyName}";
    }

    private Dictionary<string, object?> ExtractParameters(IDataCommand command)
    {
        var parameters = new Dictionary<string, object?>();

        switch (command)
        {
            case QueryCommand query:
                if (query.Filter != null)
                {
                    foreach (var condition in query.Filter.Conditions)
                    {
                        if (condition.Operator != FilterOperator.IsNull &&
                            condition.Operator != FilterOperator.IsNotNull)
                        {
                            parameters[$"@{condition.PropertyName}"] = condition.Value;
                        }
                    }
                }
                break;

            case InsertCommand insert:
                foreach (var (key, value) in insert.Data)
                    parameters[$"@{key}"] = value;
                break;

            case UpdateCommand update:
                foreach (var (key, value) in update.UpdatedData)
                    parameters[$"@upd_{key}"] = value;
                if (update.Filter != null)
                {
                    foreach (var condition in update.Filter.Conditions)
                        parameters[$"@filter_{condition.PropertyName}"] = condition.Value;
                }
                break;

            case DeleteCommand delete:
                if (delete.SoftDelete != null)
                    parameters["@softDeleteValue"] = delete.SoftDelete.DeletedValue;
                if (delete.Filter != null)
                {
                    foreach (var condition in delete.Filter.Conditions)
                        parameters[$"@{condition.PropertyName}"] = condition.Value;
                }
                break;

            case UpsertCommand upsert:
                foreach (var (key, value) in upsert.Data)
                    parameters[$"@{key}"] = value;
                break;

            case BulkInsertCommand bulk:
                // Extract parameters for first batch (stub)
                var batch = bulk.Records.Take(bulk.BatchSize);
                foreach (var (record, index) in batch.Select((r, i) => (r, i)))
                {
                    foreach (var (key, value) in record)
                        parameters[$"@batch{index}_{key}"] = value;
                }
                break;
        }

        return parameters;
    }
}

/// <summary>
/// Stub SQL connection command.
/// TODO: Replace with actual SqlConnectionCommand from Connections project.
/// </summary>
internal sealed class StubSqlConnectionCommand : IConnectionCommand
{
    public string SqlText { get; init; } = string.Empty;
    public Dictionary<string, object?> Parameters { get; init; } = new();
    public TimeSpan CommandTimeout { get; init; }
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
```

### REST Translator (Stub)

```csharp
namespace FractalDataWorks.Data.DataCommands.Translators;

/// <summary>
/// STUB IMPLEMENTATION - Translates IDataCommand to REST API calls.
/// </summary>
/// <remarks>
/// Shows how common architecture maps to REST:
/// - QueryCommand.Filter → $filter query parameter (OData style)
/// - QueryCommand.Projection → $select query parameter
/// - QueryCommand.Ordering → $orderby query parameter
/// - QueryCommand.Paging → $skip and $top parameters
/// - InsertCommand.Data → POST with JSON body
/// - UpdateCommand → PATCH with JSON body and filter in URL
/// - DeleteCommand → DELETE with filter in URL
/// - UpsertCommand → Check existence, then POST or PATCH
/// - BulkInsertCommand → Batched POST requests
/// </remarks>
public sealed class RestDataCommandTranslator : IDataCommandTranslator
{
    public string DomainName => "Rest";

    public DataCommandCapabilities GetCapabilities()
    {
        // REST typically doesn't support aggregations/joins server-side
        return DataCommandCapabilities.RestAPI;
    }

    public IGenericResult ValidateCommand(IDataCommand command)
    {
        if (command == null)
            return GenericResult.Failure("Command cannot be null");

        // Check for unsupported operations
        if (command is QueryCommand query)
        {
            if (query.Aggregation != null)
                return GenericResult.Failure("REST APIs typically don't support server-side aggregation");
            if (query.Joins.Any())
                return GenericResult.Failure("REST APIs typically don't support joins");
        }

        return GenericResult.Success();
    }

    public async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext containerContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (url, method, body) = command.CommandType switch
            {
                DataCommandType.Query => (BuildQueryUrl((QueryCommand)command, containerContext), "GET", null),
                DataCommandType.Insert => (BuildInsertUrl((InsertCommand)command, containerContext), "POST", BuildInsertBody((InsertCommand)command)),
                DataCommandType.Update => (BuildUpdateUrl((UpdateCommand)command, containerContext), "PATCH", BuildUpdateBody((UpdateCommand)command)),
                DataCommandType.Delete => (BuildDeleteUrl((DeleteCommand)command, containerContext), "DELETE", null),
                DataCommandType.Upsert => (BuildUpsertUrl((UpsertCommand)command, containerContext), "PUT", BuildUpsertBody((UpsertCommand)command)),
                DataCommandType.BulkInsert => (BuildBulkInsertUrl((BulkInsertCommand)command, containerContext), "POST", BuildBulkInsertBody((BulkInsertCommand)command)),
                _ => throw new NotSupportedException($"Command type {command.CommandType} not supported")
            };

            var headers = BuildHeaders(containerContext);

            var connectionCommand = new StubRestConnectionCommand
            {
                Url = url,
                Method = method,
                Body = body,
                Headers = headers,
                Metadata = new Dictionary<string, object>
                {
                    { "GeneratedAt", DateTime.UtcNow },
                    { "Translator", DomainName },
                    { "CommandType", command.CommandType }
                }
            };

            return await Task.FromResult(GenericResult<IConnectionCommand>.Success(connectionCommand));
        }
        catch (Exception ex)
        {
            return GenericResult<IConnectionCommand>.Failure($"Failed to translate command: {ex.Message}");
        }
    }

    private string BuildQueryUrl(QueryCommand command, IContainerContext context)
    {
        // GET /api/customers?$filter=Country eq 'USA'&$select=Id,Name&$orderby=Name asc&$skip=0&$top=50
        var url = $"{context.ContainerName}";
        var queryParams = new List<string>();

        // $filter
        if (command.Filter != null && command.Filter.Conditions.Any())
        {
            var filterExpressions = command.Filter.Conditions.Select(c =>
                $"{c.PropertyName} {GetODataOperator(c.Operator)} {FormatODataValue(c.Value, c.Operator)}");
            var logicalOp = command.Filter.LogicalOperator == LogicalOperator.Or ? " or " : " and ";
            var filterString = string.Join(logicalOp, filterExpressions);
            queryParams.Add($"$filter={Uri.EscapeDataString(filterString)}");
        }

        // $select
        if (command.Projection != null && command.Projection.Fields.Any())
        {
            var selectFields = string.Join(",", command.Projection.Fields.Select(f => f.SourceName));
            queryParams.Add($"$select={selectFields}");
        }

        // $orderby
        if (command.Ordering != null && command.Ordering.OrderedFields.Any())
        {
            var orderByFields = command.Ordering.OrderedFields.Select(f =>
                $"{f.FieldName} {(f.Direction == SortDirection.Ascending ? "asc" : "desc")}");
            queryParams.Add($"$orderby={string.Join(",", orderByFields)}");
        }

        // $skip and $top
        if (command.Paging != null)
        {
            if (command.Paging.Skip.HasValue)
                queryParams.Add($"$skip={command.Paging.Skip.Value}");
            if (command.Paging.Take.HasValue)
                queryParams.Add($"$top={command.Paging.Take.Value}");
        }

        if (queryParams.Any())
            url += "?" + string.Join("&", queryParams);

        return url;
    }

    private string BuildInsertUrl(InsertCommand command, IContainerContext context)
    {
        // POST /api/customers
        return command.ContainerName;
    }

    private string? BuildInsertBody(InsertCommand command)
    {
        // { "name": "Acme Corp", "email": "contact@acme.com" }
        return JsonSerializer.Serialize(command.Data);
    }

    private string BuildUpdateUrl(UpdateCommand command, IContainerContext context)
    {
        // PATCH /api/customers/:id or PATCH /api/customers with filter params
        var url = command.ContainerName;

        // If filter is a simple Id = value, extract to URL
        if (command.Filter?.Conditions.Count == 1)
        {
            var condition = command.Filter.Conditions.First();
            if (condition.PropertyName.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                condition.Operator == FilterOperator.Equal)
            {
                url += $"/{condition.Value}";
            }
        }

        return url;
    }

    private string? BuildUpdateBody(UpdateCommand command)
    {
        return JsonSerializer.Serialize(command.UpdatedData);
    }

    private string BuildDeleteUrl(DeleteCommand command, IContainerContext context)
    {
        // DELETE /api/customers/:id
        var url = command.ContainerName;

        if (command.Filter?.Conditions.Count == 1)
        {
            var condition = command.Filter.Conditions.First();
            if (condition.PropertyName.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                condition.Operator == FilterOperator.Equal)
            {
                url += $"/{condition.Value}";
            }
        }

        return url;
    }

    private string BuildUpsertUrl(UpsertCommand command, IContainerContext context)
    {
        // PUT /api/customers/:id
        return command.ContainerName;
    }

    private string? BuildUpsertBody(UpsertCommand command)
    {
        return JsonSerializer.Serialize(command.Data);
    }

    private string BuildBulkInsertUrl(BulkInsertCommand command, IContainerContext context)
    {
        // POST /api/customers/bulk
        return $"{command.ContainerName}/bulk";
    }

    private string? BuildBulkInsertBody(BulkInsertCommand command)
    {
        return JsonSerializer.Serialize(new
        {
            records = command.Records,
            batchSize = command.BatchSize,
            errorHandling = command.ErrorHandling.ToString()
        });
    }

    private Dictionary<string, string> BuildHeaders(IContainerContext context)
    {
        return new Dictionary<string, string>
        {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" }
        };
    }

    private string GetODataOperator(FilterOperator op)
    {
        return op switch
        {
            FilterOperator.Equal => "eq",
            FilterOperator.NotEqual => "ne",
            FilterOperator.GreaterThan => "gt",
            FilterOperator.GreaterThanOrEqual => "ge",
            FilterOperator.LessThan => "lt",
            FilterOperator.LessThanOrEqual => "le",
            FilterOperator.Contains => "contains",
            FilterOperator.StartsWith => "startswith",
            FilterOperator.EndsWith => "endswith",
            FilterOperator.In => "in",
            FilterOperator.IsNull => "eq null",
            FilterOperator.IsNotNull => "ne null",
            _ => throw new NotSupportedException($"Operator {op} not supported in OData")
        };
    }

    private string FormatODataValue(object? value, FilterOperator op)
    {
        if (op == FilterOperator.IsNull || op == FilterOperator.IsNotNull)
            return string.Empty;

        if (value == null)
            return "null";

        return value switch
        {
            string str => $"'{str}'",
            int or long or short => value.ToString()!,
            decimal or double or float => value.ToString()!,
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
            Guid guid => $"guid'{guid}'",
            _ => $"'{value}'"
        };
    }
}

/// <summary>
/// Stub REST connection command.
/// TODO: Replace with actual RestConnectionCommand from Connections project.
/// </summary>
internal sealed class StubRestConnectionCommand : IConnectionCommand
{
    public string Url { get; init; } = string.Empty;
    public string Method { get; init; } = "GET";
    public string? Body { get; init; }
    public Dictionary<string, string> Headers { get; init; } = new();
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
```

## Complete Flow Example

### User LINQ Query → DataCommand → Translation → Execution

```csharp
// 1. User writes LINQ query
var customers = await dbContext.Customers
    .Where(c => c.Country == "USA" && c.IsActive)
    .OrderBy(c => c.Name)
    .Skip(0)
    .Take(50)
    .ToListAsync();

// 2. LinqDataCommandBuilder decomposes to QueryCommand
var queryCommand = new QueryCommand
{
    ContainerName = "Customers",
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition { PropertyName = "Country", Operator = FilterOperator.Equal, Value = "USA" },
            new FilterCondition { PropertyName = "IsActive", Operator = FilterOperator.Equal, Value = true }
        ],
        LogicalOperator = LogicalOperator.And
    },
    Ordering = new OrderingExpression
    {
        OrderedFields = [new OrderedField { FieldName = "Name", Direction = SortDirection.Ascending }]
    },
    Paging = new PagingExpression { Skip = 0, Take = 50 }
};

// 3. Get appropriate translator based on connection domain
var translator = _translatorProvider.GetTranslator("Sql");

// 4. Translate QueryCommand to SQL
var sqlCommandResult = await translator.TranslateAsync(queryCommand, containerContext);
// Produces: SELECT * FROM [Customers] WHERE [Country] = @Country AND [IsActive] = @IsActive ORDER BY [Name] ASC OFFSET 0 ROWS FETCH NEXT 50 ROWS ONLY

// 5. Execute via connection
var connection = await _connectionProvider.GetConnectionAsync("CustomerDb");
var result = await connection.ExecuteAsync(sqlCommandResult.Value);
```

## Benefits of DataCommands Architecture

1. **Universal Representation**: One command structure works for all domains (SQL, REST, File, GraphQL)
2. **Type Safety**: TypeCollection ensures compile-time discovery of all command types
3. **Extensibility**: Add new command types via `[TypeOption]` without modifying existing code
4. **Testability**: Commands are data objects that can be easily inspected and tested
5. **Domain Independence**: Business logic works with IDataCommand, not SQL strings
6. **Translation Flexibility**: Each domain translates commands to optimal native format
7. **Common Architecture**: All operations break down into reusable components (Filter, Projection, etc.)

## Conclusion

This **DataCommands** architecture corrects the overly-restrictive "Query Specification" terminology by:

1. Using **TypeCollection pattern** with DataCommands containing all operation types
2. Supporting **all data operations**: Query, Insert, Update, Delete, Upsert, BulkInsert
3. Breaking each operation into **common architectural components** that translators can handle uniformly
4. Enabling **domain-agnostic** data access through universal command representation
5. Maintaining **type safety** and **discoverability** through source-generated collections

The common architecture components (Filter, Projection, Ordering, Paging, etc.) translate naturally to each domain's native format, whether SQL WHERE clauses, OData query parameters, or file filtering logic.
