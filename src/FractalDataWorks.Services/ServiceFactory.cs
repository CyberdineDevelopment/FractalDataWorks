using System;
using System.Globalization;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FastGenericNew;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Messages;

namespace FractalDataWorks.Services;

/// <summary>
/// Base implementation of the service factory with comprehensive type-safe creation patterns.
/// Provides a complete foundation for service factories with automatic configuration validation,
/// type checking, and structured logging support.
/// </summary>
/// <typeparam name="TService">The type of service this factory creates.</typeparam>
/// <typeparam name="TConfiguration">The configuration type required by the service.</typeparam>
public abstract class ServiceFactory<TService, TConfiguration> : IServiceFactory<TService, TConfiguration> where TService : class
    where TConfiguration : class, IFdwConfiguration
{
    private readonly ILogger _logger;

    private static readonly Action<ILogger, string, Exception> LogCreateServiceError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, "CreateServiceError"),
            "Failed to create service of type {ServiceTypeBase}");

    /// <summary>
    /// Gets the logger instance for derived classes.
    /// </summary>
    protected ILogger Logger => _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceFactory{TService,TConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance. If null, uses Microsoft's NullLogger.</param>
    protected ServiceFactory(ILogger? logger)
    {
        // Use Microsoft's NullLogger for consistency with ILogger abstractions
        // This works seamlessly when Serilog is registered via services.AddSerilog()
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Creates a service instance for the specified configuration.
    /// Uses FastGeneric for high-performance instantiation.
    /// </summary>
    /// <param name="configuration">The configuration to use for service creation.</param>
    /// <returns>A result containing the created service or failure message.</returns>
    public virtual IFdwResult<TService> Create(TConfiguration configuration) 
    {
        var serviceTypeName = typeof(TService).Name;
        
        if (configuration == null)
        {
            Logging.ServiceBaseLog.InvalidConfigurationWarning(_logger, "Configuration cannot be null");
            return FdwResult<TService>.Failure(ServiceMessages.ConfigurationCannotBeNull());
        }

        // Log configuration
        Logging.ServiceBaseLog.ValidatingServiceConfiguration(_logger, serviceTypeName, configuration);

        // Must pass logger as first parameter per service constructor pattern
        var serviceLogger = NullLogger<TService>.Instance; // TODO: Get proper logger for service
        if (FastNew.TryCreateInstance<TService, ILogger<TService>, TConfiguration>(serviceLogger, configuration, out var service))
        {
            // Use structured logging for success
            Logging.ServiceBaseLog.FastGenericServiceCreated(_logger, serviceTypeName);
            Logging.ServiceBaseLog.ServiceStarted(_logger, serviceTypeName);
            
            // Use Enhanced Enum factory method with parameters
            return FdwResult<TService>.Success(service, $"Service created successfully: {serviceTypeName}");
        }
        
        // Use structured logging and Enhanced Enum factory method with parameters for failure
        Logging.ServiceBaseLog.FastGenericServiceCreationFailed(_logger, serviceTypeName);
        LogCreateServiceError(_logger, serviceTypeName, new InvalidOperationException("FastNew failed to create service"));
        
        return FdwResult<TService>.Failure($"Fast generic creation failed: {serviceTypeName}");
    }


    #region Configuration Validation

    /// <summary>
    /// Validates and casts a configuration to the expected type.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <param name="validConfiguration">The valid configuration if successful.</param>
    /// <returns>The validation result.</returns>
    protected IFdwResult<TConfiguration> ValidateConfiguration(
        IFdwConfiguration? configuration,
        out TConfiguration? validConfiguration)
    {
        if (configuration == null)
        {
            Logging.ServiceBaseLog.InvalidConfigurationWarning(_logger,
                "Configuration cannot be null");
            validConfiguration = null;
            return FdwResult<TConfiguration>.Failure(ServiceMessages.ConfigurationCannotBeNull());
        }

        if (configuration is TConfiguration config)
        {
            validConfiguration = config;
            return FdwResult<TConfiguration>.Success(config);
        }

        Logging.ServiceBaseLog.InvalidConfigurationWarning(_logger,
            string.Format(CultureInfo.InvariantCulture,
                "Invalid configuration type. Expected {0}, got {1}",
                typeof(TConfiguration).Name,
                configuration.GetType().Name));

        validConfiguration = null;
        return FdwResult<TConfiguration>.Failure(
            "Invalid configuration type");
    }

    #endregion

    #region IServiceFactory Implementation (Non-Generic)

    /// <summary>
    /// Creates a service instance of the specified type.
    /// This method checks if the requested type matches the factory's service type.
    /// </summary>
    /// <typeparam name="T">The type of service to create.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService
    {
        // Check if the requested type is assignable from our service type
        if (!typeof(T).IsAssignableFrom(typeof(TService)))
        {
            Logging.ServiceBaseLog.InvalidConfigurationWarning(_logger,
                string.Format(CultureInfo.InvariantCulture,
                    "Invalid service type. Expected {0} or compatible type, got {1}",
                    typeof(TService).Name,
                    typeof(T).Name));

            return FdwResult<T>.Failure(
                "Invalid service type");
        }

        // Validate configuration and create service
        var validationResult = ValidateConfiguration(configuration, out var validConfig);
        if (validationResult.Error || validConfig == null)
        {
            return FdwResult<T>.Failure(validationResult.Message ?? "Configuration validation failed");
        }

        var serviceResult = Create(validConfig);
        if (serviceResult.Error || serviceResult.Value == null)
        {
            return FdwResult<T>.Failure(serviceResult.Message ?? "Service creation failed");
        }

        if (serviceResult.Value is T typedService)
        {
            return FdwResult<T>.Success(typedService);
        }
        
        // Use structured logging and Enhanced Enum factory method with parameters
        var sourceTypeName = typeof(TService).Name;
        var targetTypeName = typeof(T).Name;
        Logging.ServiceBaseLog.ServiceTypeCastFailed(_logger, sourceTypeName, targetTypeName);
        
        return FdwResult<T>.Failure($"Service type cast failed from {sourceTypeName} to {targetTypeName}");
    }

    /// <summary>
    /// Creates a service instance and returns it as IFractalService.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<IFdwService> IServiceFactory.Create(IFdwConfiguration configuration)
    {
        // Validate configuration and create service
        var validationResult = ValidateConfiguration(configuration, out var validConfig);
        if (validationResult.Error || validConfig == null)
        {
            return FdwResult<IFdwService>.Failure(validationResult.Message ?? "Configuration validation failed");
        }

        var serviceResult = Create(validConfig);
        if (serviceResult.Error || serviceResult.Value == null)
        {
            return FdwResult<IFdwService>.Failure(serviceResult.Message ?? "Service creation failed");
        }

        if (serviceResult.Value is IFdwService recService)
        {
            return FdwResult<IFdwService>.Success(recService);
        }

        // Use structured logging and Enhanced Enum factory method with parameters
        var sourceTypeName = typeof(TService).Name;
        Logging.ServiceBaseLog.ServiceTypeCastFailed(_logger, sourceTypeName, nameof(IFdwService));
        
        return FdwResult<IFdwService>.Failure($"Service type cast failed from {sourceTypeName} to {nameof(IFdwService)}");
    }

    #endregion

    #region IServiceFactory<TService> Implementation

    /// <summary>
    /// Creates a service instance with configuration validation.
    /// This method validates that the configuration is of the correct type before creation.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<TService> IServiceFactory<TService>.Create(IFdwConfiguration configuration)
    {
        // Validate configuration and create service
        var validationResult = ValidateConfiguration(configuration, out var validConfig);
        if (validationResult.Error || validConfig == null)
        {
            return FdwResult<TService>.Failure(validationResult.Message ?? "Configuration validation failed");
        }

        return Create(validConfig);
    }

    #endregion

}
