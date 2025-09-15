using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Base interface for all FractalDataWorks connections.
/// </summary>
public interface IFdwConnection : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this connection instance.
    /// </summary>
    string ConnectionId { get; }

    /// <summary>
    /// Gets the provider name (e.g., "MsSql", "PostgreSql", "RestApi").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Executes a command on this connection.
    /// </summary>
    Task<IFractalResult> Execute(
        IDataCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command on this connection and returns a typed result.
    /// </summary>
    Task<IFractalResult<TResult>> Execute<TResult>(
        IDataCommand command,
        CancellationToken cancellationToken = default);
}