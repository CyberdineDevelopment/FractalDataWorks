using System;
using System.Linq.Expressions;

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
    /// Gets the optional timeout for this command.
    /// </summary>
    TimeSpan? Timeout { get; }
}
