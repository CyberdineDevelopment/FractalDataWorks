using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Data.Sql.Logging;

/// <summary>
/// High-performance structured logging for SQL translator operations.
/// </summary>
[ExcludeFromCodeCoverage]
public static partial class SqlTranslatorLog
{
    /// <summary>
    /// Logs the start of expression validation.
    /// </summary>
    [LoggerMessage(EventId = 3000, Level = LogLevel.Debug,
        Message = "Validating expression of type {ExpressionType}")]
    public static partial void ValidatingExpression(ILogger logger, string expressionType);

    /// <summary>
    /// Logs successful expression validation.
    /// </summary>
    [LoggerMessage(EventId = 3001, Level = LogLevel.Debug,
        Message = "Expression {ExpressionType} is valid for SQL translation")]
    public static partial void ExpressionValid(ILogger logger, string expressionType);

    /// <summary>
    /// Logs expression validation failure.
    /// </summary>
    [LoggerMessage(EventId = 3002, Level = LogLevel.Debug,
        Message = "Expression {ExpressionType} is invalid: {Reason}")]
    public static partial void ExpressionInvalid(ILogger logger, string expressionType, string reason);

    /// <summary>
    /// Logs the start of LINQ to SQL translation.
    /// </summary>
    [LoggerMessage(EventId = 3100, Level = LogLevel.Debug,
        Message = "Starting SQL translation for {ExpressionType}")]
    public static partial void TranslationStarted(ILogger logger, string expressionType);

    /// <summary>
    /// Logs successful SQL translation.
    /// </summary>
    [LoggerMessage(EventId = 3101, Level = LogLevel.Debug,
        Message = "SQL translation completed for {ExpressionType}: {SqlText}")]
    public static partial void TranslationCompleted(ILogger logger, string expressionType, string sqlText);

    /// <summary>
    /// Logs SQL translation failure.
    /// </summary>
    [LoggerMessage(EventId = 3102, Level = LogLevel.Warning,
        Message = "SQL translation failed for {ExpressionType}: {ErrorMessage}")]
    public static partial void TranslationFailed(ILogger logger, string expressionType, string errorMessage);

    /// <summary>
    /// Logs SQL translation error with exception.
    /// </summary>
    [LoggerMessage(EventId = 3103, Level = LogLevel.Error,
        Message = "SQL translation error for {ExpressionType}")]
    public static partial void TranslationError(ILogger logger, string expressionType, Exception exception);

    /// <summary>
    /// Logs the start of SQL optimization.
    /// </summary>
    [LoggerMessage(EventId = 3200, Level = LogLevel.Debug,
        Message = "Starting SQL optimization: {SqlText}")]
    public static partial void OptimizationStarted(ILogger logger, string sqlText);

    /// <summary>
    /// Logs successful SQL optimization.
    /// </summary>
    [LoggerMessage(EventId = 3201, Level = LogLevel.Debug,
        Message = "SQL optimization completed: {OptimizedSql}")]
    public static partial void OptimizationCompleted(ILogger logger, string optimizedSql);

    /// <summary>
    /// Logs SQL optimization failure.
    /// </summary>
    [LoggerMessage(EventId = 3202, Level = LogLevel.Warning,
        Message = "SQL optimization failed: {ErrorMessage}")]
    public static partial void OptimizationFailed(ILogger logger, string errorMessage);

    /// <summary>
    /// Logs SQL parameter mapping.
    /// </summary>
    [LoggerMessage(EventId = 3300, Level = LogLevel.Debug,
        Message = "Mapped parameter {ParameterName} = {ParameterValue}")]
    public static partial void ParameterMapped(ILogger logger, string parameterName, object? parameterValue);

    /// <summary>
    /// Logs SQL execution start.
    /// </summary>
    [LoggerMessage(EventId = 3400, Level = LogLevel.Information,
        Message = "Executing SQL command: {CommandType} with {ParameterCount} parameters")]
    public static partial void SqlExecutionStarted(ILogger logger, string commandType, int parameterCount);

    /// <summary>
    /// Logs SQL execution completion.
    /// </summary>
    [LoggerMessage(EventId = 3401, Level = LogLevel.Information,
        Message = "SQL command completed in {ElapsedMs}ms, affected {RowCount} rows")]
    public static partial void SqlExecutionCompleted(ILogger logger, long elapsedMs, int rowCount);

    /// <summary>
    /// Logs SQL execution failure.
    /// </summary>
    [LoggerMessage(EventId = 3402, Level = LogLevel.Error,
        Message = "SQL execution failed: {ErrorMessage}")]
    public static partial void SqlExecutionFailed(ILogger logger, string errorMessage, Exception exception);
}