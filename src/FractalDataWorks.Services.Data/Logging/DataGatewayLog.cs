using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Data.Logging;

/// <summary>
/// High-performance source-generated logging for DataGateway.
/// </summary>
public static partial class DataGatewayLog
{
    /// <summary>
    /// Logs when a data command is being routed to a connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command being routed.</param>
    /// <param name="connectionName">The name of the connection.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Routing data command {CommandType} to connection {ConnectionName}")]
    public static partial void RoutingCommand(ILogger logger, string commandType, string connectionName);

    /// <summary>
    /// Logs when retrieving a data connection fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionName">The name of the connection that failed.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to get data connection {ConnectionName}")]
    public static partial void ConnectionRetrievalFailed(ILogger logger, string connectionName);
}
