using FractalDataWorks;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions.Commands;

/// <summary>
/// Command interface for creating connections.
/// </summary>
public interface IConnectionCreateCommand : IConnectionCommand
{
    /// <summary>
    /// Gets the name for the new connection.
    /// </summary>
    string ConnectionName { get; }
    
    /// <summary>
    /// Gets the provider type for the connection (e.g., "MsSql", "PostgreSQL").
    /// </summary>
    string ProviderType { get; }
    
    /// <summary>
    /// Gets the configuration for the connection.
    /// </summary>
    IConnectionConfiguration ConnectionConfiguration { get; }
}
