using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Abstractions.Logging;

/// <summary>
/// High-performance source-generated logging for ServiceBase.
/// </summary>
public static partial class ServiceBaseLog
{
    /// <summary>
    /// Logs when a command type mismatch occurs during execution.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="expectedType">The expected command type name.</param>
    /// <param name="actualType">The actual command type name received.</param>
    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Error,
        Message = "Command type mismatch: expected {ExpectedType}, received {ActualType}")]
    public static partial void CommandTypeMismatch(ILogger logger, string expectedType, string actualType);
}
