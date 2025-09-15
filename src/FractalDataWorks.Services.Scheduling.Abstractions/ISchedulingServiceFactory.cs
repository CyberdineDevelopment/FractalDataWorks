using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Non-generic marker interface for scheduling service factories.
/// </summary>
public interface IFractalSchedulingServiceFactory : IServiceFactory
{
}

/// <summary>
/// Interface for scheduling service factories that create specific scheduling service implementations.
/// </summary>
/// <typeparam name="TSchedulingService">The scheduling service type to create.</typeparam>
public interface IFractalSchedulingServiceFactory<TSchedulingService> : IFractalSchedulingServiceFactory, IServiceFactory<TSchedulingService>
    where TSchedulingService : class, IFractalSchedulingService
{
}

/// <summary>
/// Interface for scheduling service factories that create scheduling services with configuration.
/// </summary>
/// <typeparam name="TSchedulingService">The scheduling service type to create.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the scheduling service.</typeparam>
public interface IFractalSchedulingServiceFactory<TSchedulingService, TConfiguration> : IFractalSchedulingServiceFactory<TSchedulingService>, IServiceFactory<TSchedulingService, TConfiguration>
    where TSchedulingService : class, IFractalSchedulingService
    where TConfiguration : class, ISchedulingConfiguration
{
}