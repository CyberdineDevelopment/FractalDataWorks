using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Messages;
using FractalDataWorks.Services.Logging;
using FastGenericNew;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Generic factory implementation that works for most services.
/// Uses FastGenericNew for high-performance instantiation and follows Railway-Oriented Programming.
/// </summary>
/// <typeparam name="TService">The service type to create.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the service.</typeparam>
public class GenericServiceFactory<TService, TConfiguration> : ServiceFactory<TService, TConfiguration>
    where TService : class, IGenericService
    where TConfiguration : class, IGenericConfiguration
{
    private readonly ILogger<GenericServiceFactory<TService, TConfiguration>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericServiceFactory{TService,TConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public GenericServiceFactory(ILogger<GenericServiceFactory<TService, TConfiguration>> logger)
        : base(logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericServiceFactory{TService,TConfiguration}"/> class with no logger.
    /// </summary>
    public GenericServiceFactory()
        : base(null)
    {
        _logger = NullLogger<GenericServiceFactory<TService, TConfiguration>>.Instance;
    }

    /// <summary>
    /// Creates a service instance using the provided configuration.
    /// Follows ROP pattern - returns Result instead of throwing exceptions.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the service instance or failure information.</returns>
    public override IGenericResult<TService> Create(TConfiguration configuration)
    {
        var serviceTypeName = typeof(TService).Name;
        
        // Log the creation attempt with source-generated logging
        ServiceFactoryLog.CreatingService(_logger, serviceTypeName, configuration.Name ?? "unnamed");

        // Configuration logging
        ServiceFactoryLog.ConfigurationValidationFailed(_logger, serviceTypeName, "Configuration accepted");

        // Try FastGenericNew first for performance - must pass logger as first parameter
        var serviceLogger = NullLogger<TService>.Instance; // TODO: Get proper logger for service
        if (FastNew.TryCreateInstance<TService, ILogger<TService>, TConfiguration>(serviceLogger, configuration, out var service))
        {
            ServiceFactoryLog.ServiceCreatedWithFastNew(_logger, serviceTypeName);
            return GenericResult<TService>.Success(service, $"Service created successfully: {serviceTypeName}");
        }


        // Fallback to Activator.CreateInstance for edge cases
        try
        {
            // Try with logger and configuration
            var fallbackServiceLogger = NullLogger<TService>.Instance; // TODO: Get proper logger for service
            var constructorParams = new object[] { fallbackServiceLogger, configuration };
            if (Activator.CreateInstance(typeof(TService), constructorParams) is TService activatorServiceWithLogger)
            {
                ServiceFactoryLog.ServiceCreatedWithActivator(_logger, serviceTypeName);
                return GenericResult<TService>.Success(activatorServiceWithLogger, $"Service created successfully: {serviceTypeName}");
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            // These are anticipated constructor failures, not exceptional
            ServiceFactoryLog.ServiceCreationFailed(_logger, serviceTypeName, ex.Message);
            return GenericResult<TService>.Failure(
                new ServiceCreationFailedMessage(serviceTypeName, ex.Message));
        }

        // If we get here, we couldn't create the service
        ServiceFactoryLog.ServiceCreationFailed(_logger, serviceTypeName, "No suitable constructor found");
        return GenericResult<TService>.Failure(
            new ServiceCreationFailedMessage(serviceTypeName, "No suitable constructor found"));
    }

}