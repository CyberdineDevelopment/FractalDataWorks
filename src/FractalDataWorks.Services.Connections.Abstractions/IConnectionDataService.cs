using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for connection data services that handle data operations across external systems.
/// Extends IFractalService with connection-specific data operations.
/// </summary>
public interface IConnectionDataService : IGenericService
{
    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    string ServiceId { get; }

    /// <summary>
    /// Creates a new connection with the specified configuration.
    /// </summary>
    /// <param name="configuration">The connection configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the connection creation result.</returns>
    Task<IGenericResult<string>> CreateConnectionAsync(IConnectionConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing connection by ID.
    /// </summary>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the connection retrieval result.</returns>
    Task<IGenericResult<IGenericConnection>> GetConnectionAsync(string connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all active connections managed by this service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the list of connection IDs.</returns>
    Task<IGenericResult<IEnumerable<string>>> ListConnectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a connection from the service.
    /// </summary>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the removal result.</returns>
    Task<IGenericResult> RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connectivity for all managed connections.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the health check results.</returns>
    Task<IGenericResult<IDictionary<string, bool>>> HealthCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a connection command and returns a typed result.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <typeparam name="TResult">The type of result expected from the command execution.</typeparam>
    /// <param name="command">The connection command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IGenericResult<TResult>> Execute<TResult>(IConnectionCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a connection command without returning a specific result type.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <param name="command">The connection command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IGenericResult> Execute(IConnectionCommand command, CancellationToken cancellationToken = default);
}