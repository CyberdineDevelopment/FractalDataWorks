using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Non-generic marker interface for scheduling service factories.
/// </summary>
public interface IGenericSchedulingServiceFactory : IServiceFactory
{
}

/// <summary>
/// Interface for scheduling service factories that create specific scheduling service implementations.
/// </summary>
/// <typeparam name="TSchedulingService">The scheduling service type to create.</typeparam>
public interface IGenericSchedulingServiceFactory<TSchedulingService> : IGenericSchedulingServiceFactory, IServiceFactory<TSchedulingService>
    where TSchedulingService : class, IGenericSchedulingService
{
}

/// <summary>
/// Interface for scheduling service factories that create scheduling services with configuration.
/// </summary>
/// <typeparam name="TSchedulingService">The scheduling service type to create.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the scheduling service.</typeparam>
public interface IGenericSchedulingServiceFactory<TSchedulingService, TConfiguration> : IGenericSchedulingServiceFactory<TSchedulingService>, IServiceFactory<TSchedulingService, TConfiguration>
    where TSchedulingService : class, IGenericSchedulingService
    where TConfiguration : class, ISchedulingConfiguration
{
}