using System;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Models;

/// <summary>
/// Base interface for data commands that can be executed against data sources.
/// </summary>
public interface IDataCommand
{
    /// <summary>
    /// Gets the type of command (Query, Insert, Update, Delete, etc.).
    /// </summary>
    string CommandType { get; }

    /// <summary>
    /// Gets the optional timeout for this command.
    /// </summary>
    TimeSpan? Timeout { get; }
}
