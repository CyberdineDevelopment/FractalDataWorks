using FractalDataWorks;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions.Commands;

/// <summary>
/// Command interface for managing connections (list, remove, etc.).
/// </summary>
public interface IConnectionManagementCommand : IConnectionCommand
{
    /// <summary>
    /// Gets the management operation to perform.
    /// </summary>
    ConnectionManagementOperation Operation { get; }
    
    /// <summary>
    /// Gets the connection name (optional, depending on operation).
    /// </summary>
    string? ConnectionName { get; }
}
