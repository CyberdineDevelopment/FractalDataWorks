using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Logging;

/// <summary>
/// Source-generated logging for service registration operations.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage]
public static partial class ServiceRegistrationLog
{
    /// <summary>
    /// Logs when a service is being registered with the dependency injection container.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service being registered.</param>
    /// <param name="lifetime">The service lifetimeBase (Singleton, Scoped, Transient).</param>
    [LoggerMessage(
        EventId = 3100,
        Level = LogLevel.Information,
        Message = "Registering service '{ServiceType}' with lifetimeBase '{Lifetime}'")]
    public static partial void RegisteringService(ILogger logger, string serviceType, string lifetime);

    /// <summary>
    /// Logs when a service has been successfully registered with the dependency injection container.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service that was registered.</param>
    [LoggerMessage(
        EventId = 3101,
        Level = LogLevel.Information,
        Message = "Service '{ServiceType}' registered successfully")]
    public static partial void ServiceRegistered(ILogger logger, string serviceType);

    /// <summary>
    /// Logs when a service registration is skipped because the service is already registered.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service that was already registered.</param>
    [LoggerMessage(
        EventId = 3102,
        Level = LogLevel.Warning,
        Message = "Service '{ServiceType}' already registered, skipping")]
    public static partial void ServiceAlreadyRegistered(ILogger logger, string serviceType);

    /// <summary>
    /// Logs when service registration fails with an error.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="serviceType">The type of service that failed to register.</param>
    /// <param name="reason">The reason for the registration failure.</param>
    [LoggerMessage(
        EventId = 3103,
        Level = LogLevel.Error,
        Message = "Failed to register service '{ServiceType}': {Reason}")]
    public static partial void RegistrationFailed(ILogger logger, string serviceType, string reason);

    /// <summary>
    /// Logs when configuration begins for all services in a specific category.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="category">The category of services being configured.</param>
    [LoggerMessage(
        EventId = 3104,
        Level = LogLevel.Information,
        Message = "Configuring all services in category '{Category}'")]
    public static partial void ConfiguringAllServices(ILogger logger, string category);

    /// <summary>
    /// Logs the completion of service configuration with the total count of registered services.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="count">The number of services that were registered.</param>
    /// <param name="category">The category of services that were configured.</param>
    [LoggerMessage(
        EventId = 3105,
        Level = LogLevel.Information,
        Message = "Registered {Count} services in category '{Category}'")]
    public static partial void ServicesConfigured(ILogger logger, int count, string category);

    /// <summary>
    /// Logs when configuration is being bound from a specific configuration section.
    /// </summary>
    /// <param name="logger">The logger instance to write to.</param>
    /// <param name="configurationSection">The name of the configuration section being bound.</param>
    [LoggerMessage(
        EventId = 3106,
        Level = LogLevel.Debug,
        Message = "Binding configuration from section '{ConfigurationSection}'")]
    public static partial void BindingConfiguration(ILogger logger, string configurationSection);

}