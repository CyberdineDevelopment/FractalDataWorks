using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions.Commands;
using FractalDataWorks.Services.DataGateway.Abstractions.Models;

namespace FractalDataWorks.Services.DataGateway.Services;

/// <summary>
/// Interface for the main data provider service that routes commands to named connections.
/// </summary>
/// <remarks>
/// IDataGateway serves as the consumer-facing entry point for all data operations within
/// the FractalDataWorks framework. It accepts provider-agnostic DataCommandBase commands
/// and routes them to the appropriate external data connections based on the ConnectionName
/// property of each command.
/// 
/// This service abstracts away the complexity of managing multiple data connections and
/// provides a unified interface for data operations across different data stores such as
/// SQL databases, NoSQL databases, file systems, REST APIs, and other external systems.
/// </remarks>
public interface IDataGateway : IFdwService<DataCommandBase>
{
    /// <summary>
    /// Executes a data command against the specified connection and returns a typed result.
    /// </summary>
    /// <typeparam name="T">The expected return type of the command execution.</typeparam>
    /// <param name="command">The data command to execute containing the connection name and operation details.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult&lt;T&gt; with the command execution result or error information.
    /// </returns>
    Task<IFdwResult<T>> Execute<T>(DataCommandBase command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers the schema structure of a data store starting from an optional path.
    /// </summary>
    /// <param name="connectionName">The name of the connection to discover schema for.</param>
    /// <param name="startPath">Optional starting path for schema discovery. If null, discovers from root.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult with a collection of discovered data containers and their metadata.
    /// </returns>
    Task<IFdwResult<IEnumerable<DataContainer>>> DiscoverSchema(
        string connectionName,
        DataPath? startPath = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata about the available connections and their capabilities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult with a dictionary of connection names and their metadata.
    /// </returns>
    Task<IFdwResult<IDictionary<string, object>>> GetConnectionsInfo(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a data command and returns a typed result.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <typeparam name="TResult">The type of result expected from the command execution.</typeparam>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IFdwResult<TResult>> Execute<TResult>(IDataCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a data command without returning a specific result type.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IFdwResult> Execute(IDataCommand command, CancellationToken cancellationToken = default);
}
