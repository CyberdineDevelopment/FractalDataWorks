using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Factory interface for creating connections.
/// </summary>
public interface IConnectionFactory
{
    /// <summary>
    /// Gets the connection type name this factory supports.
    /// </summary>
    string ConnectionTypeName { get; }

    /// <summary>
    /// Creates a connection with the specified configuration.
    /// </summary>
    Task<IGenericConnection> Create(
        IConnectionConfiguration configuration,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic factory interface for creating typed connections.
/// </summary>
public interface IConnectionFactory<TConnection, TConfiguration> : IConnectionFactory
    where TConnection : IGenericConnection
    where TConfiguration : IConnectionConfiguration
{
    /// <summary>
    /// Creates a typed connection with the specified configuration.
    /// </summary>
    Task<TConnection> Create(
        TConfiguration configuration,
        CancellationToken cancellationToken = default);
}