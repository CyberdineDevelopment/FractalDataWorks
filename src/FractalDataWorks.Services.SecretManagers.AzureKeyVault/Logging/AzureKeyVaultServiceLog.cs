using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault.Logging;

/// <summary>
/// High-performance logging methods for AzureKeyVaultService using source generators.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class AzureKeyVaultServiceLog
{
    /// <summary>
    /// Logs when a managementCommand execution begins.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of managementCommand being executed.</param>
    /// <param name="commandId">The ID of the managementCommand being executed.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Executing {CommandType} managementCommand with ID {CommandId}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType, string commandId);

    /// <summary>
    /// Logs when a managementCommand execution completes.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandId">The ID of the managementCommand that completed.</param>
    /// <param name="isSuccess">Whether the managementCommand succeeded.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "ManagementCommand {CommandId} completed with success: {IsSuccess}")]
    public static partial void CommandCompleted(ILogger logger, string commandId, bool isSuccess);

    /// <summary>
    /// Logs when an Azure Key Vault request fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandId">The ID of the managementCommand that caused the request failure.</param>
    /// <param name="errorCode">The Azure error code.</param>
    /// <param name="errorMessage">The error message from Azure.</param>
    /// <param name="exception">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Azure Key Vault request failed for managementCommand {CommandId}: {ErrorCode} - {ErrorMessage}")]
    public static partial void AzureRequestFailed(ILogger logger, string commandId, string? errorCode, string errorMessage, Exception exception);

    /// <summary>
    /// Logs when an unexpected error occurs during managementCommand execution.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandId">The ID of the managementCommand that caused the error.</param>
    /// <param name="exception">The unexpected exception.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Unexpected error executing managementCommand {CommandId}")]
    public static partial void UnexpectedError(ILogger logger, string commandId, Exception exception);

    /// <summary>
    /// Logs when a secret is successfully retrieved.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretName">The name of the secret that was retrieved.</param>
    /// <param name="version">The version of the secret that was retrieved.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Successfully retrieved secret {SecretName} with version {Version}")]
    public static partial void SecretRetrieved(ILogger logger, string secretName, string? version);

    /// <summary>
    /// Logs when a secret is not found.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretKey">The key of the secret that was not found.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Secret {SecretKey} not found")]
    public static partial void SecretNotFound(ILogger logger, string secretKey);

    /// <summary>
    /// Logs when a secret is successfully set.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretName">The name of the secret that was set.</param>
    /// <param name="version">The version of the secret that was set.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Successfully set secret {SecretName} with version {Version}")]
    public static partial void SecretSet(ILogger logger, string secretName, string? version);

    /// <summary>
    /// Logs when access is denied for setting a secret.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretKey">The key of the secret that couldn't be set.</param>
    /// <param name="errorMessage">The error message from Azure.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "Access denied setting secret {SecretKey}: {ErrorMessage}")]
    public static partial void SecretSetAccessDenied(ILogger logger, string secretKey, string errorMessage);

    /// <summary>
    /// Logs when a secret is successfully purged.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretName">The name of the secret that was purged.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Information,
        Message = "Successfully purged secret {SecretName}")]
    public static partial void SecretPurged(ILogger logger, string secretName);

    /// <summary>
    /// Logs when a secret is successfully deleted (soft delete).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretName">The name of the secret that was deleted.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Information,
        Message = "Successfully deleted secret {SecretName} (soft delete)")]
    public static partial void SecretDeleted(ILogger logger, string secretName);

    /// <summary>
    /// Logs when a secret is not found for deletion.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretKey">The key of the secret that was not found for deletion.</param>
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Warning,
        Message = "Secret {SecretKey} not found for deletion")]
    public static partial void SecretNotFoundForDeletion(ILogger logger, string secretKey);

    /// <summary>
    /// Logs when access is denied for deleting a secret.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretKey">The key of the secret that couldn't be deleted.</param>
    /// <param name="errorMessage">The error message from Azure.</param>
    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Error,
        Message = "Access denied deleting secret {SecretKey}: {ErrorMessage}")]
    public static partial void SecretDeleteAccessDenied(ILogger logger, string secretKey, string errorMessage);

    /// <summary>
    /// Logs when secrets are successfully retrieved during list operation.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of secrets retrieved.</param>
    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Debug,
        Message = "Retrieved {Count} secrets")]
    public static partial void SecretsListed(ILogger logger, int count);

    /// <summary>
    /// Logs when access is denied for listing secrets.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="errorMessage">The error message from Azure.</param>
    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Error,
        Message = "Access denied listing secrets: {ErrorMessage}")]
    public static partial void SecretsListAccessDenied(ILogger logger, string errorMessage);

    /// <summary>
    /// Logs when secret versions are successfully retrieved.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of versions retrieved.</param>
    /// <param name="secretName">The name of the secret whose versions were retrieved.</param>
    [LoggerMessage(
        EventId = 15,
        Level = LogLevel.Debug,
        Message = "Retrieved {Count} versions for secret {SecretName}")]
    public static partial void SecretVersionsRetrieved(ILogger logger, int count, string secretName);

    /// <summary>
    /// Logs when a secret is not found for version listing.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="secretKey">The key of the secret that was not found for version listing.</param>
    [LoggerMessage(
        EventId = 16,
        Level = LogLevel.Warning,
        Message = "Secret {SecretKey} not found for version listing")]
    public static partial void SecretNotFoundForVersionListing(ILogger logger, string secretKey);
}