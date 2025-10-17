using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Authentication.Logging;

/// <summary>
/// High-performance logging methods for AuthenticationProvider using source generators.
/// </summary>
[ExcludeFromCodeCoverage]
public static partial class AuthenticationProviderLog
{
    /// <summary>
    /// Logs when getting an authentication service for a specific type.
    /// </summary>
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "Getting authentication service for type: {AuthenticationType}")]
    public static partial void GettingAuthenticationService(
        ILogger logger,
        string authenticationType);

    /// <summary>
    /// Logs when an unknown authentication type is encountered.
    /// </summary>
    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Unknown authentication type: {AuthenticationType}")]
    public static partial void UnknownAuthenticationType(
        ILogger logger,
        string authenticationType);

    /// <summary>
    /// Logs when no factory is registered for an authentication type.
    /// </summary>
    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "No factory registered for authentication type: {AuthenticationType}")]
    public static partial void NoFactoryRegistered(
        ILogger logger,
        string authenticationType);

    /// <summary>
    /// Logs when an authentication service is successfully created.
    /// </summary>
    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Debug,
        Message = "Successfully created authentication service for type: {AuthenticationType}")]
    public static partial void AuthenticationServiceCreated(
        ILogger logger,
        string authenticationType);

    /// <summary>
    /// Logs when authentication service creation fails.
    /// </summary>
    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Error,
        Message = "Failed to create authentication service for type: {AuthenticationType}. Error: {Error}")]
    public static partial void AuthenticationServiceCreationFailed(
        ILogger logger,
        string authenticationType,
        string error);

    /// <summary>
    /// Logs when authentication service creation throws an exception.
    /// </summary>
    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Error,
        Message = "Failed to create authentication service for type {AuthenticationType}")]
    public static partial void AuthenticationServiceCreationException(
        ILogger logger,
        Exception exception,
        string authenticationType);

    /// <summary>
    /// Logs when getting an authentication service by configuration name.
    /// </summary>
    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Debug,
        Message = "Getting authentication service by configuration name: {ConfigurationName}")]
    public static partial void GettingAuthenticationServiceByConfigurationName(
        ILogger logger,
        string configurationName);

    /// <summary>
    /// Logs when configuration section is not found.
    /// </summary>
    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Warning,
        Message = "Configuration section not found: Authentication:{ConfigurationName}")]
    public static partial void ConfigurationSectionNotFound(
        ILogger logger,
        string configurationName);

    /// <summary>
    /// Logs when AuthenticationType is not specified in configuration.
    /// </summary>
    [LoggerMessage(
        EventId = 2009,
        Level = LogLevel.Warning,
        Message = "AuthenticationType not specified in configuration section: {ConfigurationName}")]
    public static partial void AuthenticationTypeNotSpecified(
        ILogger logger,
        string configurationName);

    /// <summary>
    /// Logs when unknown authentication type is found in configuration.
    /// </summary>
    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Warning,
        Message = "Unknown authentication type in configuration: {AuthenticationType}")]
    public static partial void UnknownAuthenticationTypeInConfiguration(
        ILogger logger,
        string authenticationType);

    /// <summary>
    /// Logs when configuration binding fails.
    /// </summary>
    [LoggerMessage(
        EventId = 2011,
        Level = LogLevel.Error,
        Message = "Failed to bind configuration section to type: {ConfigurationType}")]
    public static partial void ConfigurationBindingFailed(
        ILogger logger,
        string? configurationType);

    /// <summary>
    /// Logs when getting authentication service by configuration name fails with exception.
    /// </summary>
    [LoggerMessage(
        EventId = 2012,
        Level = LogLevel.Error,
        Message = "Failed to get authentication service by configuration name: {ConfigurationName}")]
    public static partial void GetAuthenticationServiceByNameException(
        ILogger logger,
        Exception exception,
        string configurationName);
}