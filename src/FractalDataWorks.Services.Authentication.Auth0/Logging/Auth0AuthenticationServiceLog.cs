using System;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.Authentication.Auth0.Logging;

/// <summary>
/// Logging messages for Auth0 authentication service.
/// </summary>
[MessageCollection("Auth0AuthenticationServiceMessages")]
public static partial class Auth0AuthenticationServiceLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Starting Auth0 authentication for domain: {Domain}")]
    public static partial void LogAuthenticationStarted(
        this ILogger logger,
        string domain);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Auth0 authentication succeeded for domain: {Domain}")]
    public static partial void LogAuthenticationSucceeded(
        this ILogger logger,
        string domain);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Auth0 authentication failed for domain: {Domain}")]
    public static partial void LogAuthenticationFailed(
        this ILogger logger,
        string domain,
        Exception exception);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Starting token validation for domain: {Domain}")]
    public static partial void LogTokenValidationStarted(
        this ILogger logger,
        string domain);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Token validation succeeded for domain: {Domain}")]
    public static partial void LogTokenValidationSucceeded(
        this ILogger logger,
        string domain);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "Token validation failed for domain: {Domain}")]
    public static partial void LogTokenValidationFailed(
        this ILogger logger,
        string domain,
        Exception exception);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Starting token refresh for domain: {Domain}")]
    public static partial void LogTokenRefreshStarted(
        this ILogger logger,
        string domain);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Token refresh succeeded for domain: {Domain}")]
    public static partial void LogTokenRefreshSucceeded(
        this ILogger logger,
        string domain);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "Token refresh failed for domain: {Domain}")]
    public static partial void LogTokenRefreshFailed(
        this ILogger logger,
        string domain,
        Exception exception);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Information,
        Message = "Starting token revocation for domain: {Domain}")]
    public static partial void LogTokenRevocationStarted(
        this ILogger logger,
        string domain);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Information,
        Message = "Token revocation succeeded for domain: {Domain}")]
    public static partial void LogTokenRevocationSucceeded(
        this ILogger logger,
        string domain);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Error,
        Message = "Token revocation failed for domain: {Domain}")]
    public static partial void LogTokenRevocationFailed(
        this ILogger logger,
        string domain,
        Exception exception);
}
