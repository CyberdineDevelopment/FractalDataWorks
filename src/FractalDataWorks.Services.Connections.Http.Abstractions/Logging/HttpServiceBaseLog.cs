using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.Logging;

/// <summary>
/// High-performance logging methods for HttpServiceBase using source generators.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage] // Source-generated logging class with no business logic
public static partial class HttpServiceBaseLog
{
    /// <summary>
    /// Logs when opening HTTP connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Opening HTTP connection")]
    public static partial void OpeningConnection(ILogger logger);

    /// <summary>
    /// Logs when HTTP connection opened successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "HTTP connection opened successfully")]
    public static partial void ConnectionOpened(ILogger logger);

    /// <summary>
    /// Logs when failed to open HTTP connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Failed to open HTTP connection")]
    public static partial void OpenConnectionFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when closing HTTP connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Closing HTTP connection")]
    public static partial void ClosingConnection(ILogger logger);

    /// <summary>
    /// Logs when HTTP connection closed successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "HTTP connection closed successfully")]
    public static partial void ConnectionClosed(ILogger logger);

    /// <summary>
    /// Logs when failed to close HTTP connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "Failed to close HTTP connection")]
    public static partial void CloseConnectionFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when testing HTTP connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "Testing HTTP connection")]
    public static partial void TestingConnection(ILogger logger);

    /// <summary>
    /// Logs when HTTP connection test successful.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "HTTP connection test successful")]
    public static partial void ConnectionTestSuccessful(ILogger logger);

    /// <summary>
    /// Logs when HTTP connection test failed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "HTTP connection test failed")]
    public static partial void ConnectionTestFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when retrieving HTTP connection metadata.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Debug,
        Message = "Retrieving HTTP connection metadata")]
    public static partial void RetrievingMetadata(ILogger logger);

    /// <summary>
    /// Logs when HTTP connection metadata retrieved successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Debug,
        Message = "HTTP connection metadata retrieved successfully")]
    public static partial void MetadataRetrieved(ILogger logger);

    /// <summary>
    /// Logs when failed to retrieve HTTP connection metadata.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Error,
        Message = "Failed to retrieve HTTP connection metadata")]
    public static partial void MetadataRetrievalFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when HTTP service disposed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Debug,
        Message = "HTTP service disposed")]
    public static partial void ServiceDisposed(ILogger logger);

    /// <summary>
    /// Logs when executing HTTP command.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command being executed.</param>
    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Debug,
        Message = "Executing HTTP command: {CommandType}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType);

    /// <summary>
    /// Logs when HTTP command completed successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command that completed.</param>
    [LoggerMessage(
        EventId = 15,
        Level = LogLevel.Debug,
        Message = "HTTP command completed: {CommandType}")]
    public static partial void CommandCompleted(ILogger logger, string commandType);

    /// <summary>
    /// Logs when HTTP command failed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command that failed.</param>
    /// <param name="error">The error that occurred.</param>
    [LoggerMessage(
        EventId = 16,
        Level = LogLevel.Warning,
        Message = "HTTP command failed: {CommandType}, Error: {Error}")]
    public static partial void CommandFailed(ILogger logger, string commandType, string error);

    /// <summary>
    /// Logs when HTTP command threw exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command that threw exception.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 17,
        Level = LogLevel.Error,
        Message = "HTTP command exception: {CommandType}")]
    public static partial void CommandException(ILogger logger, string commandType, Exception exception);

    /// <summary>
    /// Logs when HTTP connection operation failed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 18,
        Level = LogLevel.Error,
        Message = "HTTP connection operation failed for service: {ServiceType}")]
    public static partial void ConnectionFailed(ILogger logger, string serviceType, Exception exception);
}