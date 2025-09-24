using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services;

public interface IServiceFactory<TService, TConfiguration> 
    where TService : class 
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Creates a service instance for the specified configuration.
    /// Uses FastGeneric for high-performance instantiation.
    /// </summary>
    /// <param name="configuration">The configuration to use for service creation.</param>
    /// <returns>A result containing the created service or failure message.</returns>
    IFdwResult<TService> Create(TConfiguration configuration);

    /// <summary>
    /// Creates a service instance of the specified type.
    /// This method checks if the requested type matches the factory's service type.
    /// </summary>
    /// <typeparam name="T">The type of service to create.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService;
}