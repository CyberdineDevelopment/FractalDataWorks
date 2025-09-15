using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Web.RestEndpoints.Logging;

/// <summary>
/// High-performance logging methods for FractalEndpoint using source generators.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class FractalEndpointLog
{
    /// <summary>
    /// Logs when an error occurs in an endpoint.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="endpointType">The type name of the endpoint where the error occurred.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Error in endpoint {EndpointType}")]
    public static partial void EndpointError(ILogger logger, string endpointType, Exception exception);

    /// <summary>
    /// Logs when a command encounters an invalid operation.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type name of the command.</param>
    /// <param name="exception">The invalid operation exception.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Invalid operation in command {CommandType}")]
    public static partial void InvalidOperation(ILogger logger, string commandType, InvalidOperationException exception);

    /// <summary>
    /// Logs when a command receives an invalid argument.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type name of the command.</param>
    /// <param name="exception">The argument exception.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Invalid argument in command {CommandType}")]
    public static partial void InvalidArgument(ILogger logger, string commandType, ArgumentException exception);

    /// <summary>
    /// Logs when an endpoint execution begins.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="endpointType">The type name of the endpoint.</param>
    /// <param name="requestType">The type name of the request.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Executing endpoint {EndpointType} with request {RequestType}")]
    public static partial void ExecutingEndpoint(ILogger logger, string endpointType, string requestType);

    /// <summary>
    /// Logs when an endpoint execution completes successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="endpointType">The type name of the endpoint.</param>
    /// <param name="duration">The execution duration in milliseconds.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Endpoint {EndpointType} executed successfully in {Duration}ms")]
    public static partial void EndpointExecuted(ILogger logger, string endpointType, double duration);

    /// <summary>
    /// Logs when authorization fails for an endpoint.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="endpointType">The type name of the endpoint.</param>
    /// <param name="reason">The reason for authorization failure.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Authorization failed for endpoint {EndpointType}: {Reason}")]
    public static partial void AuthorizationFailed(ILogger logger, string endpointType, string reason);

    /// <summary>
    /// Logs when authorization succeeds for an endpoint.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="endpointType">The type name of the endpoint.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "Authorization succeeded for endpoint {EndpointType}")]
    public static partial void AuthorizationSucceeded(ILogger logger, string endpointType);

    /// <summary>
    /// Logs when command validation fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type name of the command.</param>
    /// <param name="validationErrors">The validation error messages.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Warning,
        Message = "Command validation failed for {CommandType}: {ValidationErrors}")]
    public static partial void CommandValidationFailed(ILogger logger, string commandType, string validationErrors);

    /// <summary>
    /// Logs when a command execution begins.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type name of the command.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Executing command {CommandType}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType);

    /// <summary>
    /// Logs when a command execution completes successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type name of the command.</param>
    /// <param name="duration">The execution duration in milliseconds.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Information,
        Message = "Command {CommandType} executed successfully in {Duration}ms")]
    public static partial void CommandExecuted(ILogger logger, string commandType, double duration);
}