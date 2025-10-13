using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Logging;

/// <summary>
/// Source-generated logging for service factory operations.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage]
public static partial class ServiceFactoryLog
{
    /// <summary>
    /// Logs the initiation of service creation with specified configuration.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service being created.</param>
    /// <param name="configurationName">The name of the configuration being used.</param>
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "Creating service '{ServiceType}' with configuration '{ConfigurationName}'")]
    public static partial void CreatingService(ILogger logger, string serviceType, string configurationName);

    /// <summary>
    /// Logs configuration validation failures for a service.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service that failed validation.</param>
    /// <param name="validationErrors">The validation error details.</param>
    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Warning,
        Message = "Configuration validation failed for service '{ServiceType}': {ValidationErrors}")]
    public static partial void ConfigurationValidationFailed(ILogger logger, string serviceType, string validationErrors);

    /// <summary>
    /// Logs successful service creation using FastGenericNew optimization.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service that was created.</param>
    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Information,
        Message = "Service '{ServiceType}' created successfully using FastGenericNew")]
    public static partial void ServiceCreatedWithFastNew(ILogger logger, string serviceType);

    /// <summary>
    /// Logs successful service creation using Activator.CreateInstance fallback.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service that was created.</param>
    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Information,
        Message = "Service '{ServiceType}' created successfully using Activator.CreateInstance")]
    public static partial void ServiceCreatedWithActivator(ILogger logger, string serviceType);

    /// <summary>
    /// Logs service creation failures with error details.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service that failed to be created.</param>
    /// <param name="reason">The reason for the creation failure.</param>
    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Error,
        Message = "Failed to create service '{ServiceType}': {Reason}")]
    public static partial void ServiceCreationFailed(ILogger logger, string serviceType, string reason);

    /// <summary>
    /// Logs successful retrieval of a factory from the dependency injection container.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="factoryType">The type of factory that was retrieved.</param>
    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Debug,
        Message = "Factory '{FactoryType}' retrieved from DI container")]
    public static partial void FactoryRetrievedFromContainer(ILogger logger, string factoryType);

    /// <summary>
    /// Logs the creation of a new factory instance.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="factoryType">The type of factory being created.</param>
    [LoggerMessage(
        EventId = 3007,
        Level = LogLevel.Debug,
        Message = "Creating new factory instance '{FactoryType}'")]
    public static partial void CreatingFactoryInstance(ILogger logger, string factoryType);
}