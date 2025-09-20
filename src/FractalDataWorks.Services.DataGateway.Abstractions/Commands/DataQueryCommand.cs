using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Commands;

/// <summary>
/// Represents a data query command that encapsulates a LINQ expression for execution across different connection types.
/// This command serves as the bridge between universal DataSet queries and connection-specific execution.
/// </summary>
/// <remarks>
/// The DataQueryCommand contains all the information needed to execute a data query:
/// - The LINQ expression tree that represents the query logic
/// - The target dataset metadata for schema validation
/// - The expected result type for proper mapping
/// - Additional execution context and options
/// </remarks>
public sealed class DataQueryCommand : IConnectionCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataQueryCommand"/> class.
    /// </summary>
    /// <param name="expression">The LINQ expression tree representing the query.</param>
    /// <param name="dataSet">The target dataset for the query.</param>
    /// <param name="resultType">The expected result type after query execution.</param>
    public DataQueryCommand(Expression expression, IDataSet dataSet, Type resultType)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        DataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
        ResultType = resultType ?? throw new ArgumentNullException(nameof(resultType));
        CommandId = Guid.NewGuid();
        CorrelationId = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    public Guid CommandId { get; }

    /// <summary>
    /// Gets the correlation identifier for tracking related operations.
    /// </summary>
    public Guid CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the configuration associated with this command.
    /// </summary>
    public IFdwConfiguration? Configuration { get; init; }

    /// <summary>
    /// Gets the LINQ expression tree that represents the query logic.
    /// </summary>
    /// <remarks>
    /// This expression tree contains the complete query logic including:
    /// - Where clauses for filtering
    /// - Select projections for field selection
    /// - OrderBy clauses for sorting
    /// - Take/Skip for pagination
    /// - Join operations (future enhancement)
    /// </remarks>
    public Expression Expression { get; }

    /// <summary>
    /// Gets the target dataset for this query.
    /// </summary>
    /// <remarks>
    /// The dataset provides schema information, field mappings, and connection compatibility
    /// information that query translators use to generate appropriate connection-specific queries.
    /// </remarks>
    public IDataSet DataSet { get; }

    /// <summary>
    /// Gets the expected result type after query execution.
    /// </summary>
    /// <remarks>
    /// This type information is used by result mappers to properly deserialize and map
    /// connection-specific results back to the expected .NET types.
    /// </remarks>
    public Type ResultType { get; }

    /// <summary>
    /// Gets additional execution options for this query.
    /// </summary>
    /// <remarks>
    /// These options can control various aspects of query execution such as:
    /// - Timeout values
    /// - Caching behavior
    /// - Transaction isolation levels
    /// - Connection pooling preferences
    /// </remarks>
    public QueryExecutionOptions Options { get; init; } = QueryExecutionOptions.Default;

    /// <summary>
    /// Gets execution context information for this query.
    /// </summary>
    /// <remarks>
    /// Context information includes metadata about the execution environment,
    /// user identity, correlation IDs, and other contextual data that may be
    /// needed for logging, auditing, or connection-specific behavior.
    /// </remarks>
    public QueryExecutionContext Context { get; init; } = QueryExecutionContext.Empty;

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A FdwResult containing the validation result.</returns>
    public IFdwResult<ValidationResult> Validate()
    {
        var validationResult = new ValidationResult();

        if (Expression == null)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(Expression), "Expression cannot be null"));
        }

        if (DataSet == null)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(DataSet), "DataSet cannot be null"));
        }

        if (ResultType == null)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(ResultType), "ResultType cannot be null"));
        }

        return FdwResult<ValidationResult>.Success(validationResult);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"DataQueryCommand[{CommandId}]: {DataSet.Name} query created at {Timestamp:yyyy-MM-dd HH:mm:ss}";
    }
}

/// <summary>
/// Execution options that control how a data query is executed.
/// </summary>
/// <remarks>
/// These options provide fine-grained control over query execution behavior,
/// allowing applications to optimize performance, reliability, and resource usage
/// based on their specific requirements.
/// </remarks>
public sealed record QueryExecutionOptions
{
    /// <summary>
    /// Gets the default execution options.
    /// </summary>
    public static readonly QueryExecutionOptions Default = new();

    /// <summary>
    /// Gets or sets the timeout for query execution.
    /// </summary>
    /// <remarks>
    /// If not specified, the connection's default timeout will be used.
    /// </remarks>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the query results should be cached.
    /// </summary>
    /// <remarks>
    /// When enabled, query results may be cached based on the query signature
    /// and data freshness requirements. Default is false.
    /// </remarks>
    public bool EnableCaching { get; init; } = false;

    /// <summary>
    /// Gets or sets the cache duration for query results.
    /// </summary>
    /// <remarks>
    /// Only relevant when EnableCaching is true. If not specified,
    /// a default cache duration will be used based on the data source characteristics.
    /// </remarks>
    public TimeSpan? CacheDuration { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of rows to return.
    /// </summary>
    /// <remarks>
    /// This provides a safety limit to prevent accidentally returning huge result sets.
    /// If not specified, no artificial limit is applied beyond what's in the query expression.
    /// </remarks>
    public int? MaxRows { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to track query execution metrics.
    /// </summary>
    /// <remarks>
    /// When enabled, detailed execution metrics will be collected for monitoring
    /// and performance analysis. Default is true.
    /// </remarks>
    public bool TrackMetrics { get; init; } = true;
}

/// <summary>
/// Execution context information for a data query.
/// </summary>
/// <remarks>
/// Context provides additional metadata about the execution environment
/// that may be needed for logging, auditing, security, or connection-specific behavior.
/// </remarks>
public sealed record QueryExecutionContext
{
    /// <summary>
    /// Gets an empty execution context.
    /// </summary>
    public static readonly QueryExecutionContext Empty = new();

    /// <summary>
    /// Gets or sets the correlation ID for this query execution.
    /// </summary>
    /// <remarks>
    /// Used for tracking queries across distributed systems and logging correlation.
    /// </remarks>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets or sets the user identity associated with this query.
    /// </summary>
    /// <remarks>
    /// May be used for authorization, auditing, or connection-specific user context.
    /// </remarks>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets or sets the tenant ID for multi-tenant scenarios.
    /// </summary>
    /// <remarks>
    /// Used in multi-tenant applications to ensure data isolation and proper routing.
    /// </remarks>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets or sets additional custom properties for this execution context.
    /// </summary>
    /// <remarks>
    /// Allows applications to pass custom metadata that may be needed by
    /// specific connection types or query translators.
    /// </remarks>
    public Dictionary<string, object> Properties { get; init; } = new(StringComparer.Ordinal);
}