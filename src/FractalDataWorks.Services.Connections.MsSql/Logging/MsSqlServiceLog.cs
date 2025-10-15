using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.MsSql.Logging;

/// <summary>
/// High-performance logging methods for MsSqlService using source generators.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class MsSqlServiceLog
{
    /// <summary>
    /// Logs when an MsSql connection is created successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionId">The ID of the connection that was created.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Created MsSql connection with ID: {ConnectionId}")]
    public static partial void ConnectionCreated(ILogger logger, string connectionId);

    /// <summary>
    /// Logs when MsSql connection creation fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred during connection creation.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to create MsSql connection")]
    public static partial void ConnectionCreationFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when an MsSql connection is removed successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionId">The ID of the connection that was removed.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Removed MsSql connection with ID: {ConnectionId}")]
    public static partial void ConnectionRemoved(ILogger logger, string connectionId);

    /// <summary>
    /// Logs when MsSql connection removal fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionId">The ID of the connection that failed to be removed.</param>
    /// <param name="exception">The exception that occurred during connection removal.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to remove MsSql connection {ConnectionId}")]
    public static partial void ConnectionRemovalFailed(ILogger logger, string connectionId, Exception exception);

    /// <summary>
    /// Logs when a health check fails for a connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionId">The ID of the connection that failed the health check.</param>
    /// <param name="exception">The exception that occurred during the health check.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Warning,
        Message = "Health check failed for connection {ConnectionId}")]
    public static partial void HealthCheckFailed(ILogger logger, string connectionId, Exception exception);

    /// <summary>
    /// Logs when an exception occurs while disposing a connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred while disposing the connection.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Exception occurred while disposing connection")]
    public static partial void ConnectionDisposeException(ILogger logger, Exception exception);
}