using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for all data commands (Query, Insert, Update, Delete, Upsert).
/// </summary>
public abstract class DataCommandBase : IDataCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the connection to route this command to.</param>
    /// <param name="query">The LINQ expression representing the query/operation.</param>
    /// <param name="commandType">The type of command (Query, Insert, Update, Delete, Upsert, etc.).</param>
    /// <param name="targetContainer">Optional target container (table, view, etc.) for this command.</param>
    protected DataCommandBase(string connectionName, Expression? query, string commandType, object? targetContainer = null)
    {
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        Query = query;
        CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
        TargetContainer = targetContainer;
        Metadata = new Dictionary<string, object>(StringComparer.Ordinal);
        Timeout = null;
        CommandId = Guid.NewGuid();
        CorrelationId = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the name of the connection to route this command to.
    /// </summary>
    public string ConnectionName { get; }

    /// <summary>
    /// Gets the LINQ expression representing the query/operation.
    /// </summary>
    public Expression? Query { get; }

    /// <summary>
    /// Gets the type of command (Query, Insert, Update, Delete, Upsert, Count, Exists, etc.).
    /// </summary>
    public string CommandType { get; }

    /// <summary>
    /// Gets the optional target container (table, view, collection, etc.) for this command.
    /// </summary>
    public object? TargetContainer { get; }

    /// <summary>
    /// Gets metadata dictionary for additional command properties.
    /// Used to pass translator-specific options like paging, conflict handling, etc.
    /// </summary>
    public IDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets or sets the optional timeout for this command.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

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
    /// Validates this command.
    /// </summary>
    /// <returns>A GenericResult indicating whether validation succeeded.</returns>
    public virtual IGenericResult Validate()
    {
        return GenericResult.Success();
    }
}
