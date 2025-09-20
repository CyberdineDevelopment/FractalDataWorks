# FractalDataWorks.Services.Connections.MsSql Documentation

## Overview

The FractalDataWorks.Services.Connections.MsSql library provides a complete implementation of SQL Server connectivity within the framework. It demonstrates the full pattern for implementing connection types, including command translation, expression parsing, result mapping, and comprehensive logging.

## Core Components

### MsSqlConnectionType

The ServiceType definition for SQL Server connections:

```csharp
public sealed class MsSqlConnectionType : ConnectionTypeBase<IFdwConnection, MsSqlConfiguration, IMsSqlConnectionFactory>
{
    public static MsSqlConnectionType Instance { get; } = new();

    private MsSqlConnectionType() : base(2, "MsSql", "Database Connections") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMsSqlConnectionFactory, MsSqlConnectionFactory>();
        services.AddScoped<MsSqlService>();
        services.AddScoped<MsSqlCommandTranslator>();
        services.AddScoped<ExpressionTranslator>();
    }
}
```

**Key Features:**
- Singleton instance pattern
- Automatic service registration
- Category-based organization ("Database Connections")
- ID-based identification for lookups

### MsSqlExternalConnection

The main connection implementation:

```csharp
public class MsSqlExternalConnection : ConnectionServiceBase<IConnectionCommand, MsSqlConfiguration, MsSqlExternalConnection>
{
    private SqlConnection _sqlConnection;
    private readonly MsSqlCommandTranslator _translator;

    protected override async Task<IFdwResult> OpenCoreAsync()
    {
        _sqlConnection = new SqlConnection(Configuration.ConnectionString);
        await _sqlConnection.OpenAsync();
        return FdwResult.Success();
    }

    public override async Task<IFdwResult<T>> Execute<T>(IConnectionCommand command)
    {
        var translationResult = _translator.Translate(command);
        // Execute SQL and map results
    }
}
```

**Features:**
- Inherits from ConnectionServiceBase for standard behavior
- Manages SqlConnection lifecycle
- Integrates command translation
- Comprehensive error handling

### MsSqlService

Service implementation for SQL Server operations:

```csharp
public class MsSqlService : ServiceBase<IConnectionCommand, MsSqlConfiguration, MsSqlService>
{
    private readonly MsSqlExternalConnection _connection;

    public override async Task<IFdwResult> Execute(IConnectionCommand command)
    {
        return await _connection.Execute(command);
    }
}
```

## Configuration

### MsSqlConfiguration

Configuration class with FluentValidation:

```csharp
public class MsSqlConfiguration : IConnectionConfiguration
{
    public string ConnectionString { get; set; }
    public int CommandTimeout { get; set; } = 30;
    public bool EnableRetryLogic { get; set; } = true;
    public int MaxRetryCount { get; set; } = 3;
    public bool UseAzureAuthentication { get; set; }
    public string ApplicationName { get; set; }
    public bool MultipleActiveResultSets { get; set; } = true;
}
```

### MsSqlConfigurationValidator

FluentValidation rules:

```csharp
public class MsSqlConfigurationValidator : AbstractValidator<MsSqlConfiguration>
{
    public MsSqlConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required");

        RuleFor(x => x.CommandTimeout)
            .GreaterThan(0)
            .LessThanOrEqualTo(3600);

        RuleFor(x => x.MaxRetryCount)
            .InclusiveBetween(0, 10)
            .When(x => x.EnableRetryLogic);
    }
}
```

## Command System

### Command Types

**MsSqlConnectionTestCommand**: Tests connection validity
```csharp
public class MsSqlConnectionTestCommand : IConnectionCommand
{
    public string TestQuery { get; set; } = "SELECT 1";
    public int TimeoutSeconds { get; set; } = 5;
}
```

**MsSqlExternalConnectionCreateCommand**: Creates database objects
```csharp
public class MsSqlExternalConnectionCreateCommand : IConnectionCreateCommand
{
    public string DatabaseName { get; set; }
    public string Collation { get; set; }
    public RecoveryModel RecoveryModel { get; set; }
}
```

**MsSqlExternalConnectionDiscoveryCommand**: Discovers database objects
```csharp
public class MsSqlExternalConnectionDiscoveryCommand : IConnectionDiscoveryCommand
{
    public SchemaDiscoveryType DiscoveryType { get; set; }
    public string SchemaName { get; set; }
    public bool IncludeSystemObjects { get; set; }
}
```

**MsSqlExternalConnectionManagementCommand**: Management operations
```csharp
public class MsSqlExternalConnectionManagementCommand : IConnectionManagementCommand
{
    public BackupType BackupType { get; set; }
    public string BackupPath { get; set; }
    public bool WithCompression { get; set; }
}
```

## Command Translation

### MsSqlCommandTranslator

Translates abstract commands to T-SQL:

```csharp
public class MsSqlCommandTranslator
{
    public SqlTranslationResult Translate(IConnectionCommand command)
    {
        return command switch
        {
            IDataQueryCommand query => TranslateQuery(query),
            IDataUpdateCommand update => TranslateUpdate(update),
            IDataDeleteCommand delete => TranslateDelete(delete),
            _ => TranslateGeneric(command)
        };
    }

    private SqlTranslationResult TranslateQuery(IDataQueryCommand command)
    {
        var translator = new TSqlQueryTranslator();
        return translator.BuildSelectStatement(command);
    }
}
```

### ExpressionTranslator

Converts LINQ expressions to SQL:

```csharp
public class ExpressionTranslator
{
    public string Translate(Expression expression)
    {
        return expression switch
        {
            BinaryExpression binary => TranslateBinary(binary),
            MethodCallExpression method => TranslateMethodCall(method),
            MemberExpression member => TranslateMember(member),
            _ => throw new NotSupportedException($"Expression type {expression.NodeType} not supported")
        };
    }
}
```

### TSqlQueryTranslator

Specialized T-SQL query builder:

```csharp
public class TSqlQueryTranslator
{
    public SqlTranslationResult BuildSelectStatement(IDataQueryCommand command)
    {
        var builder = new StringBuilder();
        builder.Append("SELECT ");

        if (command.Distinct) builder.Append("DISTINCT ");
        if (command.Top.HasValue) builder.Append($"TOP {command.Top} ");

        BuildColumnList(builder, command.Columns);
        BuildFromClause(builder, command.From);
        BuildWhereClause(builder, command.Where);
        BuildOrderByClause(builder, command.OrderBy);

        return new SqlTranslationResult
        {
            Sql = builder.ToString(),
            Parameters = ExtractParameters(command)
        };
    }
}
```

## Result Mapping

### SqlServerResultMapper

Maps SQL results to domain objects:

```csharp
public class SqlServerResultMapper
{
    public T MapSingle<T>(SqlDataReader reader)
    {
        if (!reader.Read()) return default(T);
        return MapRow<T>(reader);
    }

    public IEnumerable<T> MapMultiple<T>(SqlDataReader reader)
    {
        while (reader.Read())
        {
            yield return MapRow<T>(reader);
        }
    }

    private T MapRow<T>(SqlDataReader reader)
    {
        var instance = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var columnName = GetColumnName(property);
            if (reader.HasColumn(columnName))
            {
                var value = reader[columnName];
                property.SetValue(instance, ConvertValue(value, property.PropertyType));
            }
        }

        return instance;
    }
}
```

## Connection Metadata

### MsSqlConnectionMetadata

Provides SQL Server metadata:

```csharp
public class MsSqlConnectionMetadata : IConnectionMetadata
{
    public string SystemName => "Microsoft SQL Server";
    public string Version { get; private set; }
    public string ServerInfo { get; private set; }
    public string DatabaseName { get; private set; }
    public IReadOnlyDictionary<string, object> Capabilities { get; private set; }

    public static async Task<MsSqlConnectionMetadata> CollectAsync(SqlConnection connection)
    {
        var metadata = new MsSqlConnectionMetadata
        {
            Version = await GetServerVersion(connection),
            ServerInfo = connection.DataSource,
            DatabaseName = connection.Database,
            Capabilities = await GetServerCapabilities(connection)
        };

        return metadata;
    }
}
```

## Factory Implementation

### IMsSqlConnectionFactory

Factory interface:

```csharp
public interface IMsSqlConnectionFactory : IConnectionFactory
{
    Task<IFdwResult<MsSqlExternalConnection>> CreateTypedConnectionAsync(MsSqlConfiguration configuration);
}
```

### MsSqlConnectionFactory

Concrete factory implementation:

```csharp
public class MsSqlConnectionFactory : IMsSqlConnectionFactory
{
    private readonly ILogger<MsSqlConnectionFactory> _logger;
    private readonly MsSqlCommandTranslator _translator;

    public async Task<IFdwResult<IFdwConnection>> CreateConnectionAsync(IConnectionConfiguration configuration)
    {
        if (configuration is not MsSqlConfiguration sqlConfig)
            return FdwResult<IFdwConnection>.Failure("Invalid configuration type");

        var connection = new MsSqlExternalConnection(sqlConfig, _translator, _logger);

        if (sqlConfig.AutoOpen)
        {
            var openResult = await connection.OpenAsync();
            if (openResult.Error)
                return FdwResult<IFdwConnection>.Failure(openResult.Message);
        }

        return FdwResult<IFdwConnection>.Success(connection);
    }
}
```

## Logging

### MsSqlServiceLog

Service-level logging:

```csharp
public static class MsSqlServiceLog
{
    private static readonly Action<ILogger, string, Exception> _executingCommand =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1001, "ExecutingCommand"),
            "Executing SQL command: {CommandText}");

    public static void ExecutingCommand(ILogger logger, string commandText)
        => _executingCommand(logger, commandText, null);
}
```

### MsSqlExternalConnectionLog

Connection-level logging:

```csharp
public static class MsSqlExternalConnectionLog
{
    public static void ConnectionOpened(ILogger logger, string database);
    public static void ConnectionClosed(ILogger logger, string database);
    public static void CommandExecuted(ILogger logger, string commandText, int rowsAffected);
    public static void TransactionStarted(ILogger logger, IsolationLevel level);
}
```

### MsSqlCommandTranslatorLog

Translation logging:

```csharp
public static class MsSqlCommandTranslatorLog
{
    public static void TranslatingCommand(ILogger logger, string commandType);
    public static void TranslationCompleted(ILogger logger, string sql);
    public static void TranslationFailed(ILogger logger, Exception ex);
}
```

## Advanced Features

### Connection Pooling

Built-in connection pooling configuration:

```csharp
public class MsSqlConfiguration
{
    public int MinPoolSize { get; set; } = 0;
    public int MaxPoolSize { get; set; } = 100;
    public bool Pooling { get; set; } = true;
    public int ConnectionLifetime { get; set; } = 0;
}
```

### Retry Logic

Automatic retry for transient failures:

```csharp
public class SqlRetryPolicy
{
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var retryCount = 0;
        while (retryCount < MaxRetries)
        {
            try
            {
                return await operation();
            }
            catch (SqlException ex) when (IsTransient(ex))
            {
                retryCount++;
                await Task.Delay(GetBackoffDelay(retryCount));
            }
        }
    }
}
```

### Transaction Support

Comprehensive transaction management:

```csharp
public class MsSqlExternalConnection
{
    private SqlTransaction _currentTransaction;

    public async Task<IFdwResult> BeginTransactionAsync(IsolationLevel level)
    {
        _currentTransaction = await _sqlConnection.BeginTransactionAsync(level);
        return FdwResult.Success();
    }

    public async Task<IFdwResult> CommitAsync()
    {
        await _currentTransaction.CommitAsync();
        return FdwResult.Success();
    }
}
```

## Best Practices

1. **Always use parameterized queries** to prevent SQL injection
2. **Configure appropriate timeouts** based on operation types
3. **Enable connection pooling** for web applications
4. **Use async methods** for all database operations
5. **Implement retry logic** for transient failures
6. **Dispose connections properly** using using statements
7. **Log all SQL operations** for debugging and auditing

## Integration Examples

### With Entity Framework Core

```csharp
services.AddDbContext<MyDbContext>(options =>
{
    var config = Configuration.GetSection("Connections:MainDatabase").Get<MsSqlConfiguration>();
    options.UseSqlServer(config.ConnectionString);
});
```

### With Dapper

```csharp
public class DapperRepository
{
    private readonly IFdwConnectionProvider _provider;

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql)
    {
        var connectionResult = await _provider.GetConnection("MainDatabase");
        if (connectionResult.Error) return Enumerable.Empty<T>();

        using var connection = connectionResult.Value as MsSqlExternalConnection;
        return await connection.QueryAsync<T>(sql);
    }
}
```