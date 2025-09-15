using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataGateway.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions.Commands;
using FractalDataWorks.Services.DataGateway.Abstractions.Models;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.DataGateway.Services;

/// <summary>
/// Concrete implementation of IExternalDataConnectionProvider that manages named data connections.
/// </summary>
/// <remarks>
/// This provider acts as a registry and router for external data connections, allowing
/// the system to manage multiple named connections and route commands to the appropriate
/// connection based on the command's ConnectionName property.
/// 
/// Key responsibilities:
/// - Maintain a registry of named connections
/// - Route data commands to appropriate connections
/// - Provide schema discovery across all connections
/// - Monitor connection health and availability
/// - Aggregate connection metadata
/// </remarks>
public sealed class ExternalDataConnectionProvider : IExternalDataConnectionProvider
{
    private readonly ConcurrentDictionary<string, object> _connections;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExternalDataConnectionProvider> _logger;

    // LoggerMessage.Define delegates for high-performance logging
    private static readonly Action<ILogger, Exception?> LogProviderInitialized =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, "ProviderInitialized"),
            "ExternalDataConnectionProvider initialized");

    private static readonly Action<ILogger, Exception?> LogNullCommandError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2, "NullCommandError"),
            "ExecuteCommand called with null command");

    private static readonly Action<ILogger, Exception?> LogEmptyConnectionNameError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(3, "EmptyConnectionNameError"),
            "ExecuteCommand called with null or empty connection name");

    private static readonly Action<ILogger, string, string, string, Exception?> LogCommandValidationFailed =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Warning,
            new EventId(4, "CommandValidationFailed"),
            "Command validation failed for {CommandType} on connection {ConnectionName}: {ErrorMessage}");

    private static readonly Action<ILogger, string, string, Exception?> LogExecutingCommand =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(5, "ExecutingCommand"),
            "Executing {CommandType} command on connection {ConnectionName}");

    private static readonly Action<ILogger, string, string, Exception?> LogConnectionNotFound =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(6, "ConnectionNotFound"),
            "Connection {ConnectionName} not found for command {CommandType}");

    private static readonly Action<ILogger, string, Exception?> LogConnectionIsNull =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(7, "ConnectionIsNull"),
            "Connection {ConnectionName} is null - likely due to interface cast failure during registration");

    private static readonly Action<ILogger, string, Exception?> LogConnectionInterfaceError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(8, "ConnectionInterfaceError"),
            "Connection {ConnectionName} does not implement IExternalDataConnection interface");

    private static readonly Action<ILogger, string, string, string, Exception?> LogConnectionNotAvailable =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(9, "ConnectionNotAvailable"),
            "Connection {ConnectionName} is not available for command {CommandType}: {ErrorMessage}");

    private static readonly Action<ILogger, string, string, Exception?> LogCommandExecutionSuccess =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(10, "CommandExecutionSuccess"),
            "Successfully executed {CommandType} command on connection {ConnectionName}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogCommandExecutionFailure =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Warning,
            new EventId(11, "CommandExecutionFailure"),
            "Command execution failed for {CommandType} on connection {ConnectionName}: {ErrorMessage}");

    private static readonly Action<ILogger, string, string, Exception> LogCommandExecutionException =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(12, "CommandExecutionException"),
            "Exception occurred during command execution for {CommandType} on connection {ConnectionName}");

    private static readonly Action<ILogger, Exception?> LogSchemaDiscoveryEmptyConnectionName =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(13, "SchemaDiscoveryEmptyConnectionName"),
            "DiscoverConnectionSchema called with null or empty connection name");

    private static readonly Action<ILogger, string, string, Exception?> LogSchemaDiscoveryStarted =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(14, "SchemaDiscoveryStarted"),
            "Discovering schema for connection {ConnectionName} starting from path {StartPath}");

    private static readonly Action<ILogger, string, Exception?> LogSchemaConnectionNotFound =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(15, "SchemaConnectionNotFound"),
            "Connection {ConnectionName} not found for schema discovery");

    private static readonly Action<ILogger, string, Exception?> LogSchemaConnectionInterfaceError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(16, "SchemaConnectionInterfaceError"),
            "Connection {ConnectionName} does not implement IExternalDataConnection interface");

    private static readonly Action<ILogger, string, int, Exception?> LogSchemaDiscoverySuccess =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(17, "SchemaDiscoverySuccess"),
            "Schema discovery completed successfully for connection {ConnectionName}. Found {ContainerCount} containers");

    private static readonly Action<ILogger, string, string, Exception?> LogSchemaDiscoveryFailure =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(18, "SchemaDiscoveryFailure"),
            "Schema discovery failed for connection {ConnectionName}: {ErrorMessage}");

    private static readonly Action<ILogger, string, Exception> LogSchemaDiscoveryException =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(19, "SchemaDiscoveryException"),
            "Exception occurred during schema discovery for connection {ConnectionName}");

    private static readonly Action<ILogger, Exception?> LogRetrievingMetadata =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(20, "RetrievingMetadata"),
            "Retrieving metadata for all connections");

    private static readonly Action<ILogger, string, Exception> LogMetadataRetrievalWarning =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(21, "MetadataRetrievalWarning"),
            "Failed to get metadata for connection {ConnectionName}");

    private static readonly Action<ILogger, int, Exception?> LogMetadataRetrievalSuccess =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(22, "MetadataRetrievalSuccess"),
            "Successfully retrieved metadata for {ConnectionCount} connections");

    private static readonly Action<ILogger, Exception> LogMetadataRetrievalException =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(23, "MetadataRetrievalException"),
            "Exception occurred while retrieving connections metadata");

    private static readonly Action<ILogger, Exception?> LogAvailabilityCheckEmptyConnectionName =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(24, "AvailabilityCheckEmptyConnectionName"),
            "IsConnectionAvailable called with null or empty connection name");

    private static readonly Action<ILogger, string, Exception?> LogCheckingAvailability =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(25, "CheckingAvailability"),
            "Checking availability for connection {ConnectionName}");

    private static readonly Action<ILogger, string, Exception?> LogConnectionNotInRegistry =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(26, "ConnectionNotInRegistry"),
            "Connection {ConnectionName} not found in registry");

    private static readonly Action<ILogger, string, Exception?> LogConnectionInterfaceNotImplemented =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(27, "ConnectionInterfaceNotImplemented"),
            "Connection {ConnectionName} does not implement required interface");

    private static readonly Action<ILogger, string, bool, Exception?> LogAvailabilityCheckCompleted =
        LoggerMessage.Define<string, bool>(
            LogLevel.Debug,
            new EventId(28, "AvailabilityCheckCompleted"),
            "Connection {ConnectionName} availability check completed: {IsAvailable}");

    private static readonly Action<ILogger, string, Exception> LogAvailabilityCheckException =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(29, "AvailabilityCheckException"),
            "Exception occurred while checking availability for connection {ConnectionName}");

    private static readonly Action<ILogger, string, Exception?> LogConnectionRegistered =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(30, "ConnectionRegistered"),
            "Successfully registered connection {ConnectionName}");

    private static readonly Action<ILogger, string, Exception?> LogConnectionRegistrationFailed =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(31, "ConnectionRegistrationFailed"),
            "Failed to register connection {ConnectionName} - name already exists");

    private static readonly Action<ILogger, string, Exception?> LogConnectionUnregistered =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(32, "ConnectionUnregistered"),
            "Successfully unregistered connection {ConnectionName}");

    private static readonly Action<ILogger, string, Exception?> LogConnectionUnregistrationFailed =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(33, "ConnectionUnregistrationFailed"),
            "Failed to unregister connection {ConnectionName} - not found");

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalDataConnectionProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection.</param>
    /// <param name="logger">Logger for this provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
    public ExternalDataConnectionProvider(
        IServiceProvider serviceProvider,
        ILogger<ExternalDataConnectionProvider> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connections = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);

        LogProviderInitialized(_logger, null);
    }

    /// <summary>
    /// Gets a specific named connection.
    /// </summary>
    /// <param name="connectionName">The name of the connection to retrieve.</param>
    /// <returns>The connection instance if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when connectionName is null or empty.</exception>
    private object? GetConnection(string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException("Connection name cannot be null or empty.", nameof(connectionName));

        return _connections.TryGetValue(connectionName, out var connection) ? connection : null;
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<T>> ExecuteCommand<T>(DataCommandBase command, CancellationToken cancellationToken = default)
    {
        if (command == null)
        {
            const string errorMessage = "Command cannot be null";
            LogNullCommandError(_logger, null);
            return FdwResult<T>.Failure(errorMessage);
        }

        if (string.IsNullOrWhiteSpace(command.ConnectionName))
        {
            const string errorMessage = "Command must specify a valid connection name";
            LogEmptyConnectionNameError(_logger, null);
            return FdwResult<T>.Failure(errorMessage);
        }

        // Validate the command first
        var validationResult = command.Validate();
        if (!validationResult.Value.IsValid)
        {
            var errorMessage = string.Format(
                CultureInfo.InvariantCulture,
                "Command validation failed: {0}",
                string.Join("; ", validationResult.Value.Errors.Select(e => e.ErrorMessage)));
            
            LogCommandValidationFailed(_logger,
                command.GetType().Name,
                command.ConnectionName,
                string.Join("; ", validationResult.Value.Errors.Select(e => e.ErrorMessage)),
                null);
            
            return FdwResult<T>.Failure(errorMessage);
        }

        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = nameof(ExecuteCommand),
            ["CommandType"] = command.GetType().Name,
            ["ConnectionName"] = command.ConnectionName,
            ["CommandId"] = command.CommandId,
            ["CorrelationId"] = command.CorrelationId
        }))
        {
            LogExecutingCommand(_logger,
                command.GetType().Name,
                command.ConnectionName,
                null);

            try
            {
                // Check if connection is registered
                if (!_connections.TryGetValue(command.ConnectionName!, out var connectionObj))
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' not found",
                        command.ConnectionName);

                    LogConnectionNotFound(_logger,
                        command.ConnectionName,
                        command.GetType().Name,
                        null);

                    return FdwResult<T>.Failure(errorMessage);
                }
                
                // Check if connection is null (interface cast failed during registration)
                if (connectionObj == null)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' does not implement the expected interface",
                        command.ConnectionName);

                    LogConnectionIsNull(_logger,
                        command.ConnectionName,
                        null);

                    return FdwResult<T>.Failure(errorMessage);
                }

                // Try to cast to the expected interface
                if (connectionObj is not IExternalDataConnection<IFdwConnectionConfiguration> connection)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' does not implement the expected interface",
                        command.ConnectionName);

                    LogConnectionInterfaceError(_logger,
                        command.ConnectionName,
                        null);

                    return FdwResult<T>.Failure(errorMessage);
                }

                // Test connection availability before executing
                var connectionTest = await connection.TestConnection(cancellationToken).ConfigureAwait(false);
                if (!connectionTest.IsSuccess || !connectionTest.Value)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' is not available: {1}",
                        command.ConnectionName,
                        connectionTest.Message ?? "Unknown reason");

                    LogConnectionNotAvailable(_logger,
                        command.ConnectionName,
                        command.GetType().Name,
                        connectionTest.Message ?? "Unknown reason",
                        null);

                    return FdwResult<T>.Failure(errorMessage);
                }

                var result = await connection.Execute<T>(command, cancellationToken).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    LogCommandExecutionSuccess(_logger,
                        command.GetType().Name,
                        command.ConnectionName,
                        null);
                }
                else
                {
                    LogCommandExecutionFailure(_logger,
                        command.GetType().Name,
                        command.ConnectionName,
                        result.Message ?? "Unknown error",
                        null);
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Command execution failed for {0} on connection {1}: {2}",
                    command.GetType().Name,
                    command.ConnectionName,
                    ex.Message);

                LogCommandExecutionException(_logger,
                    command.GetType().Name,
                    command.ConnectionName,
                    ex);

                return FdwResult<T>.Failure(errorMessage);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IEnumerable<DataContainer>>> DiscoverConnectionSchema(
        string connectionName, 
        DataPath? startPath = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
        {
            const string errorMessage = "Connection name cannot be null or empty";
            LogSchemaDiscoveryEmptyConnectionName(_logger, null);
            return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
        }

        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = "DiscoverSchema",
            ["ConnectionName"] = connectionName,
            ["StartPath"] = startPath?.ToString() ?? "null"
        }))
        {
            LogSchemaDiscoveryStarted(_logger,
                connectionName,
                startPath?.ToString() ?? "root",
                null);

            try
            {
                var connectionObj = GetConnection(connectionName);
                if (connectionObj == null)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' not found",
                        connectionName);

                    LogSchemaConnectionNotFound(_logger,
                        connectionName,
                        null);

                    return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
                }

                // Try to cast to the expected interface
                if (connectionObj is not IExternalDataConnection<IFdwConnectionConfiguration> connection)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' does not implement the expected interface",
                        connectionName);

                    LogSchemaConnectionInterfaceError(_logger,
                        connectionName,
                        null);

                    return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
                }

                var result = await connection.DiscoverSchema(startPath, cancellationToken).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    var containerCount = result.Value?.Count() ?? 0;
                    LogSchemaDiscoverySuccess(_logger,
                        connectionName,
                        containerCount,
                        null);
                }
                else
                {
                    LogSchemaDiscoveryFailure(_logger,
                        connectionName,
                        result.Message ?? "Unknown error",
                        null);
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

                LogSchemaDiscoveryException(_logger,
                    connectionName,
                    ex);

                return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IDictionary<string, object>>> GetConnectionsMetadata(CancellationToken cancellationToken = default)
    {
        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = nameof(GetConnectionsMetadata)
        }))
        {
            LogRetrievingMetadata(_logger, null);

            try
            {
                var metadata = new Dictionary<string, object>(StringComparer.Ordinal);
                var connectionTasks = new List<Task>();

                foreach (var kvp in _connections)
                {
                    var connectionName = kvp.Key;
                    var connection = kvp.Value;

                    connectionTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // Try to cast to the expected interface
                            if (connection is IExternalDataConnection<IFdwConnectionConfiguration> dataConnection)
                            {
                                var connectionInfo = await dataConnection.GetConnectionInfo(cancellationToken).ConfigureAwait(false);
                                if (connectionInfo.IsSuccess)
                                {
                                    lock (metadata)
                                    {
                                        metadata[connectionName] = connectionInfo.Value!;
                                    }
                                }
                                else
                                {
                                    lock (metadata)
                                    {
                                        metadata[connectionName] = new Dictionary<string, object>(StringComparer.Ordinal)
                                        {
                                            ["Error"] = connectionInfo.Message ?? "Unknown error",
                                            ["Available"] = false
                                        };
                                    }
                                }
                            }
                            else
                            {
                                lock (metadata)
                                {
                                    metadata[connectionName] = new Dictionary<string, object>(StringComparer.Ordinal)
                                    {
                                        ["Error"] = "Connection does not implement required interface",
                                        ["Available"] = false
                                    };
                                }
                            }
                        }
                        catch (Exception ex) when (ex is not OutOfMemoryException)
                        {
                            LogMetadataRetrievalWarning(_logger,
                                connectionName,
                                ex);

                            lock (metadata)
                            {
                                metadata[connectionName] = new Dictionary<string, object>(StringComparer.Ordinal)
                                {
                                    ["Error"] = ex.Message,
                                    ["Available"] = false
                                };
                            }
                        }
                    }, cancellationToken));
                }

                await Task.WhenAll(connectionTasks).ConfigureAwait(false);

                LogMetadataRetrievalSuccess(_logger,
                    metadata.Count,
                    null);

                return FdwResult<IDictionary<string, object>>.Success(metadata);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to retrieve connections metadata: {0}",
                    ex.Message);

                LogMetadataRetrievalException(_logger, ex);

                return FdwResult<IDictionary<string, object>>.Failure(errorMessage);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<bool>> IsConnectionAvailable(string connectionName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
        {
            const string errorMessage = "Connection name cannot be null or empty";
            LogAvailabilityCheckEmptyConnectionName(_logger, null);
            return FdwResult<bool>.Failure(errorMessage);
        }

        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = nameof(IsConnectionAvailable),
            ["ConnectionName"] = connectionName
        }))
        {
            LogCheckingAvailability(_logger, connectionName, null);

            try
            {
                var connectionObj = GetConnection(connectionName);
                if (connectionObj == null)
                {
                    LogConnectionNotInRegistry(_logger,
                        connectionName,
                        null);

                    return FdwResult<bool>.Success(false);
                }

                // Try to cast to the expected interface
                if (connectionObj is not IExternalDataConnection<IFdwConnectionConfiguration> connection)
                {
                    LogConnectionInterfaceNotImplemented(_logger,
                        connectionName,
                        null);

                    return FdwResult<bool>.Success(false);
                }

                var result = await connection.TestConnection(cancellationToken).ConfigureAwait(false);

                var isAvailable = result.IsSuccess && result.Value;
                
                LogAvailabilityCheckCompleted(_logger,
                    connectionName,
                    isAvailable,
                    null);

                return FdwResult<bool>.Success(isAvailable);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to check availability for connection {0}: {1}",
                    connectionName,
                    ex.Message);

                LogAvailabilityCheckException(_logger,
                    connectionName,
                    ex);

                return FdwResult<bool>.Failure(errorMessage);
            }
        }
    }

    /// <summary>
    /// Registers a new named connection with the provider.
    /// </summary>
    /// <param name="name">The unique name for the connection.</param>
    /// <param name="connection">The connection instance to register.</param>
    /// <returns>True if the connection was registered successfully; false if a connection with the same name already exists.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public bool RegisterConnection<TConfiguration>(string name, IExternalDataConnection<TConfiguration> connection)
        where TConfiguration : IFdwConnectionConfiguration
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connection name cannot be null or empty.", nameof(name));
        
        var registered = _connections.TryAdd(name, connection);
        
        if (registered)
        {
            LogConnectionRegistered(_logger,
                name,
                null);
        }
        else
        {
            LogConnectionRegistrationFailed(_logger,
                name,
                null);
        }

        return registered;
    }

    /// <summary>
    /// Unregisters a connection from the provider.
    /// </summary>
    /// <param name="name">The name of the connection to unregister.</param>
    /// <returns>True if the connection was unregistered successfully; false if the connection was not found.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public bool UnregisterConnection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connection name cannot be null or empty.", nameof(name));

        var removed = _connections.TryRemove(name, out _);
        
        if (removed)
        {
            LogConnectionUnregistered(_logger,
                name,
                null);
        }
        else
        {
            LogConnectionUnregistrationFailed(_logger,
                name,
                null);
        }

        return removed;
    }

    /// <summary>
    /// Gets the names of all registered connections.
    /// </summary>
    /// <returns>A collection of all registered connection names.</returns>
    public ICollection<string> GetConnectionNames()
    {
        return _connections.Keys.ToList();
    }

    /// <summary>
    /// Gets the count of registered connections.
    /// </summary>
    public int ConnectionCount => _connections.Count;
}
