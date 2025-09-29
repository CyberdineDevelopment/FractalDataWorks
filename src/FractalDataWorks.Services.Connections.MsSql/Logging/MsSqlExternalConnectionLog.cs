using System;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.MsSql.Logging;

/// <summary>
/// Source-generated logging for MsSqlExternalConnection.
/// </summary>
internal static partial class MsSqlExternalConnectionLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "External connection created with ID: {ConnectionId}")]
    public static partial void GenericConnectionCreated(ILogger logger, string connectionId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Executing command type {CommandType} on connection {ConnectionId}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType, string connectionId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Transaction committed for command type {CommandType} on connection {ConnectionId}")]
    public static partial void TransactionCommitted(ILogger logger, string commandType, string connectionId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "Transaction rolled back for command type {CommandType} on connection {ConnectionId}: {Error}")]
    public static partial void TransactionRolledBack(ILogger logger, string commandType, string connectionId, string error);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Command execution failed for type {CommandType} on connection {ConnectionId}")]
    public static partial void CommandExecutionFailed(ILogger logger, string commandType, string connectionId, Exception exception);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "Discovering schema for connection {ConnectionId}")]
    public static partial void SchemaDiscoveryStarted(ILogger logger, string connectionId);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Schema discovery completed for connection {ConnectionId}: found {ContainerCount} containers")]
    public static partial void SchemaDiscoveryCompleted(ILogger logger, string connectionId, int containerCount);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "Schema discovery failed for connection {ConnectionId}")]
    public static partial void SchemaDiscoveryFailed(ILogger logger, string connectionId, Exception exception);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Testing connection {ConnectionId}")]
    public static partial void TestingConnection(ILogger logger, string connectionId);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Debug,
        Message = "Connection test successful for {ConnectionId}")]
    public static partial void ConnectionTestSuccessful(ILogger logger, string connectionId);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Warning,
        Message = "Connection test failed for {ConnectionId}")]
    public static partial void ConnectionTestFailed(ILogger logger, string connectionId, Exception exception);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Debug,
        Message = "Retrieving connection info for {ConnectionId}")]
    public static partial void RetrievingConnectionInfo(ILogger logger, string connectionId);

    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Error,
        Message = "Get connection info failed for {ConnectionId}")]
    public static partial void GetConnectionInfoFailed(ILogger logger, string connectionId, Exception exception);

    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Warning,
        Message = "Server info retrieval failed")]
    public static partial void ServerInfoRetrievalFailed(ILogger logger, Exception exception);
}