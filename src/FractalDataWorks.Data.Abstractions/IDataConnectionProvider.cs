using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Data.DataStores.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Provides data connections for executing data commands.
/// This provider manages connection lifecycle and provides type-safe connection retrieval.
/// </summary>
public interface IDataConnectionProvider
{
    /// <summary>
    /// Gets a connection by name (from configuration).
    /// </summary>
    /// <param name="connectionName">The name of the connection to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the connection instance.</returns>
    Task<IGenericResult<IGenericConnection>> GetConnectionAsync(
        string connectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a connection by name and attempts to cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The expected connection type.</typeparam>
    /// <param name="connectionName">The name of the connection to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the typed connection instance.</returns>
    Task<IGenericResult<T>> GetConnectionAsync<T>(
        string connectionName,
        CancellationToken cancellationToken = default)
        where T : class, IGenericConnection;

    /// <summary>
    /// Gets a connection from a DataStore instance.
    /// Creates a connection dynamically based on the DataStore's StoreType.
    /// </summary>
    /// <param name="dataStore">The data store to create a connection for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the connection instance.</returns>
    Task<IGenericResult<IGenericConnection>> GetConnectionFromStoreAsync(
        IDataStore dataStore,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests if a connection exists and is configured.
    /// </summary>
    /// <param name="connectionName">The name of the connection to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating whether the connection exists.</returns>
    Task<IGenericResult<bool>> HasConnectionAsync(
        string connectionName,
        CancellationToken cancellationToken = default);
}
