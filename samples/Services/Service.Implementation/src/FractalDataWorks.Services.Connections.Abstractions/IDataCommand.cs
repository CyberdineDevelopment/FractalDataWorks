using System.Collections.Generic;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Basic data command interface for sample.
/// NOTE: In production, use IDataCommand from FractalDataWorks.Services.Abstractions
/// </summary>
public interface IDataCommand
{
    /// <summary>
    /// Gets the command type.
    /// </summary>
    string CommandType { get; }

    /// <summary>
    /// Gets the entity name.
    /// </summary>
    string EntityName { get; }

    /// <summary>
    /// Gets the command parameters.
    /// </summary>
    IReadOnlyDictionary<string, object> Parameters { get; }
}