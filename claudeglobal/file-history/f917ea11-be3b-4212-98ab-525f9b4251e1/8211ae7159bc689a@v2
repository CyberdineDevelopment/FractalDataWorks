using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.DomainName.Logging;

/// <summary>
/// High-performance structured logging for DomainName services.
/// </summary>
/// <ExcludedFromCoverage>Source-generated logging methods are not testable.</ExcludedFromCoverage>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging methods cannot be unit tested")]
internal static partial class DomainNameServiceLog
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "DomainName service initialized with configuration: {ConfigurationName}")]
    public static partial void ServiceInitialized(ILogger logger, string configurationName);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "Executing command {CommandType} for DomainName service")]
    public static partial void CommandExecuting(ILogger logger, string commandType);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Command {CommandType} completed successfully in {Duration}ms")]
    public static partial void CommandCompleted(ILogger logger, string commandType, double duration);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Command {CommandType} execution took longer than expected: {Duration}ms")]
    public static partial void CommandSlowExecution(ILogger logger, string commandType, double duration);

    [LoggerMessage(
        EventId = 9000,
        Level = LogLevel.Error,
        Message = "Command {CommandType} failed with error: {ErrorMessage}")]
    public static partial void CommandFailed(ILogger logger, string commandType, string errorMessage, Exception exception);

    [LoggerMessage(
        EventId = 9001,
        Level = LogLevel.Error,
        Message = "Failed to create DomainName service: {ErrorMessage}")]
    public static partial void ServiceCreationFailed(ILogger logger, string errorMessage, Exception exception);

    [LoggerMessage(
        EventId = 9002,
        Level = LogLevel.Critical,
        Message = "Critical error in DomainName service: {ErrorMessage}")]
    public static partial void CriticalError(ILogger logger, string errorMessage, Exception exception);
}
