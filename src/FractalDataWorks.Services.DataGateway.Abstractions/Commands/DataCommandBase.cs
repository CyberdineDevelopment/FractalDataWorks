using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Commands;

/// <summary>
/// Base class for all data commands (Query, Insert, Update, Delete, Upsert).
/// </summary>
public abstract class DataCommandBase : IDataGatewayCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the connection to route this command to.</param>
    /// <param name="commandType">The type of command (Query, Insert, Update, Delete, Upsert, etc.).</param>
    /// <param name="targetContainer">Optional target container (table, view, etc.) for this command.</param>
    protected DataCommandBase(string connectionName, string commandType, object? targetContainer = null)
    {
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
        TargetContainer = targetContainer;
        Metadata = new Dictionary<string, object>(StringComparer.Ordinal);
        Timeout = null;
    }

    /// <summary>
    /// Gets the name of the connection to route this command to.
    /// </summary>
    public string ConnectionName { get; }

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
    public Dictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets or sets the optional timeout for this command.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}
