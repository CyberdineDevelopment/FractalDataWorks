using FractalDataWorks.Abstractions;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using System;using System.Threading;using System.Threading.Tasks;

namespace FractalDataWorks.Services.Abstractions;

/// <summary>
/// Defines the contract for service providers in the FractalDataWorks framework.
/// Follows Railway-Oriented Programming - all operations return Results.
/// </summary>
public interface IGenericServiceProvider
{
    /// <summary>
    /// Gets a service instance by configuration.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the service instance or failure information.</returns>
    IGenericResult<TService> Get<TService>(IGenericConfiguration configuration)
        where TService : IGenericService;

    /// <summary>
    /// Gets a service instance by configuration ID.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>A result containing the service instance or failure information.</returns>
    IGenericResult<TService> Get<TService>(int configurationId)
        where TService : IGenericService;
    
    /// <summary>
    /// Gets a service instance by service type name.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="serviceTypeName">The name of the service type.</param>
    /// <returns>A result containing the service instance or failure information.</returns>
    Task<IGenericResult<TService>> Get<TService>(string serviceTypeName)
        where TService : IGenericService;
}

/// <summary>
/// Defines the contract for typed service providers in the FractalDataWorks framework.
/// Follows Railway-Oriented Programming - all operations return Results.
/// </summary>
/// <typeparam name="TService">The type of service this provider manages.</typeparam>
public interface IGenericServiceProvider<TService>
    where TService : IGenericService
{
    /// <summary>
    /// Gets a service instance by configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the service instance or failure information.</returns>
    IGenericResult<TService> Get(IGenericConfiguration configuration);

    /// <summary>
    /// Gets a service instance by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>A result containing the service instance or failure information.</returns>
    IGenericResult<TService> Get(int configurationId);
    
    /// <summary>
    /// Gets a service instance by service type name.
    /// </summary>
    /// <param name="serviceTypeName">The name of the service type.</param>
    /// <returns>A result containing the service instance or failure information.</returns>
    Task<IGenericResult<TService>> Get(string serviceTypeName);
}

