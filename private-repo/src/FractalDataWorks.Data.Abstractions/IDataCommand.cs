using System;
using System.Collections.Generic;
using System.Linq;using System.Linq.Expressions;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base interface for data commands that can be executed against data sources.
/// </summary>
public interface IDataCommand
{
    /// <summary>
    /// Gets the name of the connection to route this command to.
    /// </summary>
    string ConnectionName { get; }

    /// <summary>
    /// Gets the LINQ expression representing the query/operation.
    /// </summary>
    Expression? Query { get; }

    /// <summary>
    /// Gets the type of command (Query, Insert, Update, Delete, etc.).
    /// </summary>
    string CommandType { get; }

    /// <summary>
    /// Gets the optional target container (table, view, collection, etc.) for this command.
    /// </summary>
    object? TargetContainer { get; }

    /// <summary>
    /// Gets metadata dictionary for additional command properties.
    /// Used to pass translator-specific options like paging, conflict handling, etc.
    /// </summary>
    IDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the optional timeout for this command.
    /// </summary>
    TimeSpan? Timeout { get; }
}
