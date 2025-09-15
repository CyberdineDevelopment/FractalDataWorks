using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.MsSql.Logging;

/// <summary>
/// Source-generated logging for MsSqlCommandTranslator.
/// </summary>
internal static partial class MsSqlCommandTranslatorLog
{
    /// <summary>
    /// Logs when command translation fails.
    /// </summary>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Command translation failed for type {CommandType} on connection {ConnectionName}")]
    public static partial void CommandTranslationFailed(ILogger logger, string commandType, string connectionName, Exception exception);

    /// <summary>
    /// Logs when SQL is successfully generated (debug level).
    /// </summary>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "SQL generated for command type {CommandType}: {Sql} with {ParameterCount} parameters")]
    public static partial void SqlGenerated(ILogger logger, string commandType, string sql, int parameterCount);

    /// <summary>
    /// Logs when translation starts for a command.
    /// </summary>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Starting translation for command type {CommandType} on entity {EntityName}")]
    public static partial void TranslationStarted(ILogger logger, string commandType, string entityName);

    /// <summary>
    /// Logs when translation completes successfully.
    /// </summary>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Translation completed for command type {CommandType} in {ElapsedMs}ms")]
    public static partial void TranslationCompleted(ILogger logger, string commandType, long elapsedMs);
}