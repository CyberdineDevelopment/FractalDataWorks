using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Provider interface for managing connection factories and creating connections.
/// </summary>
public interface IConnectionProvider
{
    /// <summary>
    /// Gets all registered connection type names.
    /// </summary>
    IEnumerable<string> GetSupportedConnectionTypes();

    /// <summary>
    /// Registers a connection factory.
    /// </summary>
    void RegisterFactory(IConnectionFactory factory);

    /// <summary>
    /// Creates a connection using the appropriate factory.
    /// </summary>
    Task<IFdwConnection> Create(
        IConnectionConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests if a connection type is supported.
    /// </summary>
    bool IsConnectionTypeSupported(string connectionTypeName);
}