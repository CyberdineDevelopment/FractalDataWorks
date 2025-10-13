using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for FractalDataWorks framework connections.
/// Provides a framework-specific interface for connection implementations.
/// </summary>
public interface IGenericConnection : IDisposable, IGenericService
{
    /// <summary>
    /// Executes a data command against the connection.
    /// </summary>
    /// <typeparam name="T">The type of the result expected from the command.</typeparam>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the command execution outcome and any data returned.</returns>
    Task<IGenericResult<T>> Execute<T>(IDataCommand command,CancellationToken cancellationToken);
    /// <summary>
    /// Executes a data command against the connection.
    /// </summary>
    /// <typeparam name="T">The type of the result expected from the command.</typeparam>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the command execution outcome and any data returned.</returns>
    Task<IGenericResult> Execute(IDataCommand command, CancellationToken cancellationToken);
}