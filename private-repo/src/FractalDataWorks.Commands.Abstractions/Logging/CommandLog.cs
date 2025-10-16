using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Commands.Abstractions.Logging;

/// <summary>
/// High-performance structured logging for command operations.
/// </summary>
/// <remarks>
/// Uses source-generated logging methods for optimal performance
/// and structured logging support with compile-time validation.
/// </remarks>
[ExcludeFromCodeCoverage]
public static partial class CommandLog
{
    /// <summary>
    /// Logs the start of command execution.
    /// </summary>
    [LoggerMessage(EventId = 1000, Level = LogLevel.Debug,
        Message = "Executing command {CommandType} with ID {CommandId}")]
    public static partial void CommandExecutionStarted(ILogger logger, string commandType, Guid commandId);

    /// <summary>
    /// Logs successful command completion.
    /// </summary>
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Command {CommandType} completed successfully in {ElapsedMs}ms")]
    public static partial void CommandExecutionCompleted(ILogger logger, string commandType, long elapsedMs);

    /// <summary>
    /// Logs command execution failure.
    /// </summary>
    [LoggerMessage(EventId = 1002, Level = LogLevel.Error,
        Message = "Command {CommandType} failed: {ErrorMessage}")]
    public static partial void CommandExecutionFailed(ILogger logger, string commandType, string errorMessage, Exception exception);

    /// <summary>
    /// Logs command validation started.
    /// </summary>
    [LoggerMessage(EventId = 1100, Level = LogLevel.Debug,
        Message = "Validating command {CommandType}")]
    public static partial void CommandValidationStarted(ILogger logger, string commandType);

    /// <summary>
    /// Logs command validation failure.
    /// </summary>
    [LoggerMessage(EventId = 1101, Level = LogLevel.Warning,
        Message = "Command validation failed for {CommandType}: {ValidationError}")]
    public static partial void CommandValidationFailed(ILogger logger, string commandType, string validationError);

    /// <summary>
    /// Logs the start of command translation.
    /// </summary>
    [LoggerMessage(EventId = 2000, Level = LogLevel.Debug,
        Message = "Translating command from {SourceFormat} to {TargetFormat}")]
    public static partial void TranslationStarted(ILogger logger, string sourceFormat, string targetFormat);

    /// <summary>
    /// Logs successful command translation.
    /// </summary>
    [LoggerMessage(EventId = 2001, Level = LogLevel.Debug,
        Message = "Translation completed from {SourceFormat} to {TargetFormat} in {ElapsedMs}ms")]
    public static partial void TranslationCompleted(ILogger logger, string sourceFormat, string targetFormat, long elapsedMs);

    /// <summary>
    /// Logs command translation failure.
    /// </summary>
    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
        Message = "Translation failed from {SourceFormat} to {TargetFormat}: {Reason}")]
    public static partial void TranslationFailed(ILogger logger, string sourceFormat, string targetFormat, string reason);

    /// <summary>
    /// Logs translator selection.
    /// </summary>
    [LoggerMessage(EventId = 2100, Level = LogLevel.Debug,
        Message = "Selected translator {TranslatorType} for {SourceFormat} to {TargetFormat}")]
    public static partial void TranslatorSelected(ILogger logger, string translatorType, string sourceFormat, string targetFormat);

    /// <summary>
    /// Logs when no translator is found.
    /// </summary>
    [LoggerMessage(EventId = 2101, Level = LogLevel.Warning,
        Message = "No translator found for {SourceFormat} to {TargetFormat}")]
    public static partial void TranslatorNotFound(ILogger logger, string sourceFormat, string targetFormat);

    /// <summary>
    /// Logs bulk command processing started.
    /// </summary>
    [LoggerMessage(EventId = 3000, Level = LogLevel.Information,
        Message = "Starting bulk command processing with {ItemCount} items in batches of {BatchSize}")]
    public static partial void BulkProcessingStarted(ILogger logger, int itemCount, int batchSize);

    /// <summary>
    /// Logs bulk batch completion.
    /// </summary>
    [LoggerMessage(EventId = 3001, Level = LogLevel.Debug,
        Message = "Completed batch {BatchNumber} of {TotalBatches} ({ProcessedCount}/{TotalCount} items)")]
    public static partial void BulkBatchCompleted(ILogger logger, int batchNumber, int totalBatches, int processedCount, int totalCount);

    /// <summary>
    /// Logs bulk command processing completion.
    /// </summary>
    [LoggerMessage(EventId = 3002, Level = LogLevel.Information,
        Message = "Bulk command processing completed: {SuccessCount} succeeded, {FailureCount} failed in {ElapsedMs}ms")]
    public static partial void BulkProcessingCompleted(ILogger logger, int successCount, int failureCount, long elapsedMs);

    /// <summary>
    /// Logs command caching.
    /// </summary>
    [LoggerMessage(EventId = 4000, Level = LogLevel.Debug,
        Message = "Caching command result for {CommandType} with key {CacheKey} for {DurationSeconds}s")]
    public static partial void CommandCached(ILogger logger, string commandType, string cacheKey, int durationSeconds);

    /// <summary>
    /// Logs cache hit.
    /// </summary>
    [LoggerMessage(EventId = 4001, Level = LogLevel.Debug,
        Message = "Cache hit for command {CommandType} with key {CacheKey}")]
    public static partial void CacheHit(ILogger logger, string commandType, string cacheKey);

    /// <summary>
    /// Logs cache miss.
    /// </summary>
    [LoggerMessage(EventId = 4002, Level = LogLevel.Debug,
        Message = "Cache miss for command {CommandType} with key {CacheKey}")]
    public static partial void CacheMiss(ILogger logger, string commandType, string cacheKey);
}