using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.Logging;

/// <summary>
/// High-performance logging methods for DefaultConnectionProvider using source generators.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage] // Source-generated logging class with no business logic
public static partial class DefaultConnectionProviderLog
{
    /// <summary>
    /// Logs when getting a connection for a specific type.
    /// </summary>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "Getting connection for type: {ConnectionType}")]
    public static partial void GettingConnection(
        ILogger logger,
        string connectionType);

    /// <summary>
    /// Logs when an unknown connection type is encountered.
    /// </summary>
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Unknown connection type: {ConnectionType}")]
    public static partial void UnknownConnectionType(
        ILogger logger,
        string connectionType);

    /// <summary>
    /// Logs when no factory is registered for a connection type.
    /// </summary>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "No factory registered for connection type: {ConnectionType}")]
    public static partial void NoFactoryRegistered(
        ILogger logger,
        string connectionType);

    /// <summary>
    /// Logs when a connection is successfully created.
    /// </summary>
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Debug,
        Message = "Successfully created connection for type: {ConnectionType}")]
    public static partial void ConnectionCreated(
        ILogger logger,
        string connectionType);

    /// <summary>
    /// Logs when connection creation fails.
    /// </summary>
    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "Failed to create connection for type: {ConnectionType}. Error: {Error}")]
    public static partial void ConnectionCreationFailed(
        ILogger logger,
        string connectionType,
        string error);

    /// <summary>
    /// Logs when connection creation throws an exception.
    /// </summary>
    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Error,
        Message = "Failed to create connection for type {ConnectionType}")]
    public static partial void ConnectionCreationException(
        ILogger logger,
        Exception exception,
        string connectionType);

    /// <summary>
    /// Logs when getting a connection by name.
    /// </summary>
    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Debug,
        Message = "Getting connection by name: {ConnectionName}")]
    public static partial void GettingConnectionByName(
        ILogger logger,
        string connectionName);

    /// <summary>
    /// Logs when connection configuration is not found.
    /// </summary>
    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Warning,
        Message = "Connection configuration not found: {ConnectionName}")]
    public static partial void ConnectionConfigurationNotFound(
        ILogger logger,
        string connectionName);

    /// <summary>
    /// Logs when GetConnection by ID is not implemented.
    /// </summary>
    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Warning,
        Message = "GetConnection by ID is not implemented: {ConfigurationId}")]
    public static partial void GetConnectionByIdNotImplemented(
        ILogger logger,
        int configurationId);

    /// <summary>
    /// Logs when getting a connection by configuration name.
    /// </summary>
    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Debug,
        Message = "Getting connection by configuration name: {ConfigurationName}")]
    public static partial void GettingConnectionByConfigurationName(
        ILogger logger,
        string configurationName);

    /// <summary>
    /// Logs when configuration section is not found.
    /// </summary>
    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Warning,
        Message = "Configuration section not found: Connections:{ConfigurationName}")]
    public static partial void ConfigurationSectionNotFound(
        ILogger logger,
        string configurationName);

    /// <summary>
    /// Logs when ConnectionType is not specified in configuration.
    /// </summary>
    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Warning,
        Message = "ConnectionType not specified in configuration section: {ConfigurationName}")]
    public static partial void ConnectionTypeNotSpecified(
        ILogger logger,
        string configurationName);

    /// <summary>
    /// Logs when unknown connection type is found in configuration.
    /// </summary>
    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Warning,
        Message = "Unknown connection type in configuration: {ConnectionType}")]
    public static partial void UnknownConnectionTypeInConfiguration(
        ILogger logger,
        string connectionType);

    /// <summary>
    /// Logs when configuration binding fails.
    /// </summary>
    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Error,
        Message = "Failed to bind configuration section to type: {ConfigurationType}")]
    public static partial void ConfigurationBindingFailed(
        ILogger logger,
        string? configurationType);

    /// <summary>
    /// Logs when getting connection by configuration name fails with exception.
    /// </summary>
    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Error,
        Message = "Failed to get connection by configuration name: {ConfigurationName}")]
    public static partial void GetConnectionByNameException(
        ILogger logger,
        Exception exception,
        string configurationName);
}