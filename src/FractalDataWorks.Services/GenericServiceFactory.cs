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
public class GenericServiceFactory<TService, TConfiguration> : ServiceFactoryBase<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
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
    public override IFdwResult<TService> Create(TConfiguration configuration)
    {
        var serviceTypeName = typeof(TService).Name;
        
        // Log the creation attempt with source-generated logging
        ServiceFactoryLog.CreatingService(_logger, serviceTypeName, configuration.Name ?? "unnamed");

        // Validate configuration using base class method
        var validationResult = configuration.Validate();
        if (!validationResult.IsSuccess || validationResult.Value == null || !validationResult.Value.IsValid)
        {
            var errors = validationResult.Value != null && !validationResult.Value.IsValid
                ? string.Join("; ", validationResult.Value.Errors.Select(e => e.ErrorMessage))
                : validationResult.Message ?? "Configuration validation failed";
            ServiceFactoryLog.ConfigurationValidationFailed(_logger, serviceTypeName, errors);
            
            return FdwResult<TService>.Failure($"Validation failed: {errors}");
        }

        // Try FastGenericNew first for performance
        if (FastNew.TryCreateInstance<TService, TConfiguration>(configuration, out var service))
        {
            ServiceFactoryLog.ServiceCreatedWithFastNew(_logger, serviceTypeName);
            return FdwResult<TService>.Success(service, $"Service created successfully: {serviceTypeName}");
        }


        // Fallback to Activator.CreateInstance for edge cases
        try
        {
            // Try with just configuration
            var constructorParams = new object[] { configuration };
            if (Activator.CreateInstance(typeof(TService), constructorParams) is TService activatorServiceNoLogger)
            {
                ServiceFactoryLog.ServiceCreatedWithActivator(_logger, serviceTypeName);
                return FdwResult<TService>.Success(activatorServiceNoLogger, $"Service created successfully: {serviceTypeName}");
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            // These are anticipated constructor failures, not exceptional
            ServiceFactoryLog.ServiceCreationFailed(_logger, serviceTypeName, ex.Message);
            return FdwResult<TService>.Failure(
                new ServiceCreationFailedMessage(serviceTypeName, ex.Message));
        }

        // If we get here, we couldn't create the service
        ServiceFactoryLog.ServiceCreationFailed(_logger, serviceTypeName, "No suitable constructor found");
        return FdwResult<TService>.Failure(
            new ServiceCreationFailedMessage(serviceTypeName, "No suitable constructor found"));
    }

}