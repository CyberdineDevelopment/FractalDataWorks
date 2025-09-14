using System.Collections.Generic;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Represents an abstract data command that can be translated to provider-specific commands.
/// </summary>
public interface IDataCommand
{
    /// <summary>
    /// Gets the command type (e.g., "Query", "Insert", "Update", "Delete").
    /// </summary>
    string CommandType { get; }

    /// <summary>
    /// Gets the entity or table name.
    /// </summary>
    string EntityName { get; }

    /// <summary>
    /// Gets the command parameters.
    /// </summary>
    IReadOnlyDictionary<string, object> Parameters { get; }

    /// <summary>
    /// Gets the filter criteria for queries and deletes.
    /// </summary>
    IReadOnlyDictionary<string, object> Filters { get; }

    /// <summary>
    /// Gets the field values for inserts and updates.
    /// </summary>
    IReadOnlyDictionary<string, object> Values { get; }
}