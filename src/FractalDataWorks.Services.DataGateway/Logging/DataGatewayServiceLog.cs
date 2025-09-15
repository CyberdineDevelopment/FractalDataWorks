using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.DataGateway.Logging;

/// <summary>
/// High-performance logging methods for DataGatewayService using source generators.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class DataGatewayServiceLog
{
    /// <summary>
    /// Logs when routing a command to a connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command being routed.</param>
    /// <param name="connectionName">The name of the connection the command is being routed to.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Routing {CommandType} command to connection {ConnectionName}")]
    public static partial void RoutingCommand(ILogger logger, string commandType, string? connectionName);

    /// <summary>
    /// Logs when DiscoverSchema is called with null or empty connection name.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "DiscoverSchema called with null or empty connection name")]
    public static partial void DiscoverSchemaInvalidConnectionName(ILogger logger);

    /// <summary>
    /// Logs when starting schema discovery for a connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionName">The name of the connection for schema discovery.</param>
    /// <param name="startPath">The starting path for schema discovery.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Discovering schema for connection {ConnectionName} starting from path {StartPath}")]
    public static partial void DiscoveringSchema(ILogger logger, string connectionName, string startPath);

    /// <summary>
    /// Logs when schema discovery completes successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionName">The name of the connection for which schema was discovered.</param>
    /// <param name="containerCount">The number of containers found during discovery.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Schema discovery completed successfully for connection {ConnectionName}. Found {ContainerCount} containers")]
    public static partial void SchemaDiscoveryCompleted(ILogger logger, string connectionName, int containerCount);

    /// <summary>
    /// Logs when schema discovery fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionName">The name of the connection for which schema discovery failed.</param>
    /// <param name="errorMessage">The error message from the failed discovery.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Warning,
        Message = "Schema discovery failed for connection {ConnectionName}: {ErrorMessage}")]
    public static partial void SchemaDiscoveryFailed(ILogger logger, string connectionName, string? errorMessage);

    /// <summary>
    /// Logs when an exception occurs during schema discovery.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionName">The name of the connection for which the exception occurred.</param>
    /// <param name="exception">The exception that occurred during schema discovery.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "Exception occurred during schema discovery for connection {ConnectionName}")]
    public static partial void SchemaDiscoveryException(ILogger logger, string connectionName, Exception exception);

    /// <summary>
    /// Logs when retrieving connections information.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "Retrieving connections information")]
    public static partial void RetrievingConnectionsInfo(ILogger logger);

    /// <summary>
    /// Logs when connections information is successfully retrieved.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionCount">The number of connections for which information was retrieved.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Successfully retrieved information for {ConnectionCount} connections")]
    public static partial void ConnectionsInfoRetrieved(ILogger logger, int connectionCount);

    /// <summary>
    /// Logs when retrieving connections information fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="errorMessage">The error message from the failed retrieval.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Warning,
        Message = "Failed to retrieve connections information: {ErrorMessage}")]
    public static partial void ConnectionsInfoRetrievalFailed(ILogger logger, string? errorMessage);

    /// <summary>
    /// Logs when an exception occurs while retrieving connections information.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred during retrieval.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Error,
        Message = "Exception occurred while retrieving connections information")]
    public static partial void ConnectionsInfoRetrievalException(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when a connection is not available for command execution.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionName">The name of the unavailable connection.</param>
    /// <param name="commandType">The type of command that cannot be executed.</param>
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Error,
        Message = "Connection {ConnectionName} is not available for command {CommandType}")]
    public static partial void ConnectionNotAvailable(ILogger logger, string? connectionName, string commandType);

    /// <summary>
    /// Logs when a command is successfully executed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command that was executed.</param>
    /// <param name="connectionName">The name of the connection on which the command was executed.</param>
    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Debug,
        Message = "Successfully executed {CommandType} command on connection {ConnectionName}")]
    public static partial void CommandExecutedSuccessfully(ILogger logger, string commandType, string? connectionName);

    /// <summary>
    /// Logs when command execution fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command that failed.</param>
    /// <param name="connectionName">The name of the connection on which the command failed.</param>
    /// <param name="errorMessage">The error message from the failed execution.</param>
    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Warning,
        Message = "Command execution failed for {CommandType} on connection {ConnectionName}: {ErrorMessage}")]
    public static partial void CommandExecutionFailed(ILogger logger, string commandType, string? connectionName, string? errorMessage);

    /// <summary>
    /// Logs when an exception occurs during command execution.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command that caused the exception.</param>
    /// <param name="connectionName">The name of the connection on which the exception occurred.</param>
    /// <param name="exception">The exception that occurred during command execution.</param>
    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Error,
        Message = "Exception occurred during command execution for {CommandType} on connection {ConnectionName}")]
    public static partial void CommandExecutionException(ILogger logger, string commandType, string? connectionName, Exception exception);
}