using System;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.SecretManager;

/// <summary>
/// High-performance logging methods for SecretProviderBase using source generators.
/// </summary>
public static partial class SecretProviderBaseLog
{
    /// <summary>
    /// Logs when an empty managementCommand list is provided.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "ManagementCommand list cannot be empty")]
    public static partial void EmptyCommandList(ILogger logger);

    /// <summary>
    /// Logs when batch execution fails with an exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exceptionMessage">The exception message.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Batch execution failed: {ExceptionMessage}")]
    public static partial void BatchExecutionFailed(ILogger logger, string exceptionMessage);

    /// <summary>
    /// Logs when a provider doesn't support a managementCommand type.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="providerName">The provider name.</param>
    /// <param name="commandType">The unsupported managementCommand type.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Provider '{ProviderName}' does not support managementCommand type '{CommandType}'")]
    public static partial void UnsupportedCommandType(ILogger logger, string providerName, string commandType);

    /// <summary>
    /// Logs when a health check fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="errorMessage">The health check error message.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Health check failed: {ErrorMessage}")]
    public static partial void HealthCheckFailed(ILogger logger, string errorMessage);
}