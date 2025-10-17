using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.Rest.Logging;

/// <summary>
/// High-performance logging methods for RestService using source generators.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage] // Source-generated logging class with no business logic
public static partial class RestServiceLog
{
    /// <summary>
    /// Logs when executing REST command.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of REST command being executed.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Executing REST command: {CommandType}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType);

    /// <summary>
    /// Logs when REST command executed successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "REST command executed successfully")]
    public static partial void CommandSuccess(ILogger logger);

    /// <summary>
    /// Logs when HTTP request failed during REST command execution.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "HTTP request failed during REST command execution")]
    public static partial void HttpRequestFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when REST request timeout during command execution.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "REST request timeout during command execution")]
    public static partial void RequestTimeout(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when unexpected error during REST command execution.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Unexpected error during REST command execution")]
    public static partial void UnexpectedError(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when REST service disposed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Debug,
        Message = "REST service disposed")]
    public static partial void ServiceDisposed(ILogger logger);

    /// <summary>
    /// Logs when creating REST service instance.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="baseAddress">The base address for the REST service.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "Creating REST service instance with base address: {BaseAddress}")]
    public static partial void CreatingService(ILogger logger, string? baseAddress);

    /// <summary>
    /// Logs when configuring HTTP client for REST service.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="baseAddress">The base address being configured.</param>
    /// <param name="timeout">The timeout value in seconds.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "Configuring HTTP client with base address: {BaseAddress}, timeout: {Timeout}s")]
    public static partial void ConfiguringHttpClient(ILogger logger, string? baseAddress, int timeout);

    /// <summary>
    /// Logs when using query translator.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="translatorType">The type of translator being used.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Using query translator: {TranslatorType}")]
    public static partial void UsingTranslator(ILogger logger, string translatorType);

    /// <summary>
    /// Logs when using result mapper.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="mapperType">The type of mapper being used.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Debug,
        Message = "Using result mapper: {MapperType}")]
    public static partial void UsingMapper(ILogger logger, string mapperType);

    /// <summary>
    /// Logs when processing DataQuery command.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandId">The command identifier.</param>
    /// <param name="dataSetName">The name of the dataset being queried.</param>
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Debug,
        Message = "Processing DataQuery command {CommandId} for dataset: {DataSetName}")]
    public static partial void ProcessingDataQuery(ILogger logger, string commandId, string dataSetName);

    /// <summary>
    /// Logs when DataQuery command executed successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandId">The command identifier.</param>
    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Debug,
        Message = "DataQuery command {CommandId} executed successfully")]
    public static partial void DataQuerySuccess(ILogger logger, string commandId);

    /// <summary>
    /// Logs when DataQuery command execution failed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandId">The command identifier.</param>
    /// <param name="error">The error message.</param>
    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Error,
        Message = "DataQuery command {CommandId} execution failed: {Error}")]
    public static partial void DataQueryFailed(ILogger logger, string commandId, string error);
}