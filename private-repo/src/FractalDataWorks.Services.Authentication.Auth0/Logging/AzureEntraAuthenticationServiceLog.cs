using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Authentication.AzureEntra.Logging;

/// <summary>
/// High-performance logging methods for EntraAuthenticationService using source generators.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class EntraAuthenticationServiceLog
{
    /// <summary>
    /// Logs when authentication fails for Azure Entra.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that caused the authentication failure.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Authentication failed for Azure Entra")]
    public static partial void AuthenticationFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when token validation fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that caused the validation failure.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Token validation failed")]
    public static partial void TokenValidationFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when token refresh fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that caused the refresh failure.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Token refresh failed")]
    public static partial void TokenRefreshFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when token revocation is requested.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Token revocation requested for Azure Entra")]
    public static partial void TokenRevocationRequested(ILogger logger);

    /// <summary>
    /// Logs when token revocation fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that caused the revocation failure.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Token revocation failed")]
    public static partial void TokenRevocationFailed(ILogger logger, Exception exception);
}