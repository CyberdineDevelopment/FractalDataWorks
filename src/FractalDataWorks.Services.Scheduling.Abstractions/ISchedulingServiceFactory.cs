using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Non-generic marker interface for scheduling service factories.
/// </summary>
public interface IFdwSchedulingServiceFactory : IServiceFactory
{
}

/// <summary>
/// Interface for scheduling service factories that create specific scheduling service implementations.
/// </summary>
/// <typeparam name="TSchedulingService">The scheduling service type to create.</typeparam>
public interface IFdwSchedulingServiceFactory<TSchedulingService> : IFdwSchedulingServiceFactory, IServiceFactory<TSchedulingService>
    where TSchedulingService : class, IFdwSchedulingService
{
}

/// <summary>
/// Interface for scheduling service factories that create scheduling services with configuration.
/// </summary>
/// <typeparam name="TSchedulingService">The scheduling service type to create.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the scheduling service.</typeparam>
public interface IFdwSchedulingServiceFactory<TSchedulingService, TConfiguration> : IFdwSchedulingServiceFactory<TSchedulingService>, IServiceFactory<TSchedulingService, TConfiguration>
    where TSchedulingService : class, IFdwSchedulingService
    where TConfiguration : class, ISchedulingConfiguration
{
}