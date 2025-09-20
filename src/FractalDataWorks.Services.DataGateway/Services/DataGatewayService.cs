using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DataGateway.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions.Commands;
using FractalDataWorks.Services.DataGateway.Abstractions.Models;
using FractalDataWorks.Services.DataGateway.Abstractions.Configuration;
using FractalDataWorks.Services.DataGateway.Logging;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace FractalDataWorks.Services.DataGateway.Services;

/// <summary>
/// Main data provider service implementation that routes commands to named connections.
/// </summary>
/// <remarks>
/// DataGatewayService serves as the primary entry point for all data operations within
/// the FractalDataWorks framework. It implements the service base pattern with proper
/// validation, logging, and error handling while delegating actual data operations
/// to external connection providers.
/// 
/// This service provides:
/// - Command validation and routing based on connection names
/// - Schema discovery capabilities across different data stores
/// - Connection health monitoring and metadata retrieval
/// - Consistent error handling and logging across all operations
/// </remarks>
public sealed class DataGatewayService : IDataGateway
{
    private readonly IExternalDataConnectionProvider _connectionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for this service.</param>
    /// <param name="configuration">The configuration for this service.</param>
    /// <param name="connectionProvider">The external data connection provider for routing commands.</param>
    /// <exception cref="ArgumentNullException">Thrown when connectionProvider is null.</exception>
    /// <remarks>
    /// The service uses constructor injection to receive its dependencies, following
    /// FractalDataWorks patterns for proper logging and configuration management.
    /// </remarks>
    public DataGatewayService(
        ILogger<DataGatewayService>? logger,
        DataStoreConfiguration configuration,
        IExternalDataConnectionProvider connectionProvider)
        : base(logger, configuration)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<T>> Execute<T>(DataCommandBase command, CancellationToken cancellationToken)
    {
        var validationResult = ValidateCommand<T>(command);
        if (validationResult != null)
        {
            return validationResult;
        }

        using (LogContext.PushProperty("Operation", "ExecuteCommand"))
        using (LogContext.PushProperty("CommandType", command.GetType().Name))
        using (LogContext.PushProperty("ConnectionName", command.ConnectionName ?? "(null)"))
        using (LogContext.PushProperty("CommandId", command.CommandId))
        using (LogContext.PushProperty("CorrelationId", command.CorrelationId))
        {
            DataGatewayServiceLog.RoutingCommand(Logger, command.GetType().Name, command.ConnectionName);

            try
            {
                return await ExecuteCommandWithValidation<T>(command, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                return HandleExecutionException<T>(command, ex);
            }
        }
    }

    /// <summary>
    /// Executes a data command with the specified return type - IDataGateway interface method.
    /// </summary>
    /// <typeparam name="T">The expected return type of the command execution.</typeparam>
    /// <param name="command">The data command to execute containing the connection name and operation details.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult&lt;T&gt; with the command execution result or error information.
    /// </returns>
    Task<IFdwResult<T>> IDataGateway.Execute<T>(DataCommandBase command, CancellationToken cancellationToken)
    {
        return Execute<T>(command, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IEnumerable<DataContainer>>> DiscoverSchema(
        string connectionName, 
        DataPath? startPath = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
        {
            var errorMessage = "Connection name cannot be null or empty";
            DataGatewayServiceLog.DiscoverSchemaInvalidConnectionName(Logger);
            return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
        }

        using (LogContext.PushProperty("Operation", nameof(DiscoverSchema)))
        using (LogContext.PushProperty("ConnectionName", connectionName))
        using (LogContext.PushProperty("StartPath", startPath?.ToString() ?? "null"))
        {
            DataGatewayServiceLog.DiscoveringSchema(Logger, connectionName, startPath?.ToString() ?? "root");

            try
            {
                var result = await _connectionProvider.DiscoverConnectionSchema(
                    connectionName, 
                    startPath, 
                    cancellationToken).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    var containerCount = result.Value?.Count() ?? 0;
                    DataGatewayServiceLog.SchemaDiscoveryCompleted(Logger, connectionName, containerCount);
                }
                else
                {
                    DataGatewayServiceLog.SchemaDiscoveryFailed(Logger, connectionName, result.Message);
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Schema discovery failed for connection {0}: {1}",
                    connectionName,
                    ex.Message);

                DataGatewayServiceLog.SchemaDiscoveryException(Logger, connectionName, ex);

                return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IDictionary<string, object>>> GetConnectionsInfo(CancellationToken cancellationToken = default)
    {
        using (LogContext.PushProperty("Operation", nameof(GetConnectionsInfo)))
        {
            DataGatewayServiceLog.RetrievingConnectionsInfo(Logger);

            try
            {
                var result = await _connectionProvider.GetConnectionsMetadata(cancellationToken).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    var connectionCount = result.Value?.Count ?? 0;
                    DataGatewayServiceLog.ConnectionsInfoRetrieved(Logger, connectionCount);
                }
                else
                {
                    DataGatewayServiceLog.ConnectionsInfoRetrievalFailed(Logger, result.Message);
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to retrieve connections information: {0}",
                    ex.Message);

                DataGatewayServiceLog.ConnectionsInfoRetrievalException(Logger, ex);

                return FdwResult<IDictionary<string, object>>.Failure(errorMessage);
            }
        }
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<T>> Execute<T>(DataCommandBase command)
    {
        return await Execute<T>(command, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the command before execution.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="command">The command to validate.</param>
    /// <returns>An error result if validation fails, null if validation passes.</returns>
    private static IFdwResult<T>? ValidateCommand<T>(DataCommandBase command)
    {
        if (command == null)
        {
            return FdwResult<T>.Failure("Command cannot be null");
        }

        if (string.IsNullOrWhiteSpace(command.ConnectionName))
        {
            return FdwResult<T>.Failure("Command must specify a valid connection name");
        }

        return null;
    }

    /// <summary>
    /// Creates a logging scope for command execution.
    /// </summary>
    /// <param name="command">The command being executed.</param>
    /// <returns>A dictionary for logging scope.</returns>
    private static Dictionary<string, object> CreateExecutionScope(DataCommandBase command)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = "ExecuteCommand",
            ["CommandType"] = command.GetType().Name,
            ["ConnectionName"] = command.ConnectionName ?? "(null)",
            ["CommandId"] = command.CommandId,
            ["CorrelationId"] = command.CorrelationId
        };
    }

    /// <summary>
    /// Executes the command with connection availability validation.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The execution result.</returns>
    private async Task<IFdwResult<T>> ExecuteCommandWithValidation<T>(DataCommandBase command, CancellationToken cancellationToken)
    {
        var connectionAvailable = await _connectionProvider.IsConnectionAvailable(
            command.ConnectionName ?? string.Empty, 
            cancellationToken).ConfigureAwait(false);

        if (!connectionAvailable.IsSuccess || !connectionAvailable.Value)
        {
            var errorMessage = string.Format(
                CultureInfo.InvariantCulture,
                "Connection {0} is not available",
                command.ConnectionName);

            DataGatewayServiceLog.ConnectionNotAvailable(Logger, command.ConnectionName, command.GetType().Name);

            return FdwResult<T>.Failure(errorMessage);
        }

        var result = await _connectionProvider.ExecuteCommand<T>(command, cancellationToken).ConfigureAwait(false);
        LogExecutionResult(command, result);
        return result;
    }

    /// <summary>
    /// Logs the result of command execution.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The executed command.</param>
    /// <param name="result">The execution result.</param>
    private void LogExecutionResult<T>(DataCommandBase command, IFdwResult<T> result)
    {
        if (result.IsSuccess)
        {
            DataGatewayServiceLog.CommandExecutedSuccessfully(Logger, command.GetType().Name, command.ConnectionName);
        }
        else
        {
            DataGatewayServiceLog.CommandExecutionFailed(Logger, command.GetType().Name, command.ConnectionName, result.Message);
        }
    }

    /// <summary>
    /// Handles exceptions that occur during command execution.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="command">The command that was being executed.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <returns>A failure result with the exception details.</returns>
    private IFdwResult<T> HandleExecutionException<T>(DataCommandBase command, Exception ex)
    {
        var errorMessage = string.Format(
            CultureInfo.InvariantCulture,
            "Command execution failed for {0} on connection {1}: {2}",
            command.GetType().Name,
            command.ConnectionName,
            ex.Message);

        DataGatewayServiceLog.CommandExecutionException(Logger, command.GetType().Name, command.ConnectionName, ex);

        return FdwResult<T>.Failure(errorMessage);
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(DataCommandBase command, CancellationToken cancellationToken)
    {
        var result = await Execute<object>(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? FdwResult.Success() : FdwResult.Failure(result.Message!);
    }
}
