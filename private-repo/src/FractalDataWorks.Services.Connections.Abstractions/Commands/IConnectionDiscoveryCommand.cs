using FractalDataWorks;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions.Commands;

/// <summary>
/// Command interface for discovering connection schemas and metadata.
/// </summary>
public interface IConnectionDiscoveryCommand : IConnectionCommand
{
    /// <summary>
    /// Gets the name of the connection to discover.
    /// </summary>
    string ConnectionName { get; }
    
    /// <summary>
    /// Gets the starting path for schema discovery (optional).
    /// </summary>
    string? StartPath { get; }
    
    /// <summary>
    /// Gets the discovery options.
    /// </summary>
    ConnectionDiscoveryOptions Options { get; }
}
