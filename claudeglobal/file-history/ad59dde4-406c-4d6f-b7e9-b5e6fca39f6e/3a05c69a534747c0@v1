using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Interface for scheduler service types.
/// Defines the contract for scheduler service type implementations that integrate
/// with the service framework's dependency injection and configuration systems.
/// </summary>
/// <typeparam name="TService">The scheduler service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the scheduler service.</typeparam>
/// <typeparam name="TFactory">The factory type for creating scheduler service instances.</typeparam>
public interface ISchedulerType<TService, TConfiguration, TFactory> : IServiceType<TService, TFactory, TConfiguration>
    where TService : class, IGenericSchedulingService
    where TConfiguration : class, ISchedulingConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Gets the name of the underlying scheduling engine (e.g., "Quartz.NET", "Hangfire").
    /// </summary>
    string SchedulingEngine { get; }

    /// <summary>
    /// Gets the job executor type used by this scheduler.
    /// </summary>
    Type JobExecutorType { get; }

    /// <summary>
    /// Gets the trigger type used by this scheduler.
    /// </summary>
    Type TriggerType { get; }

    /// <summary>
    /// Gets whether this scheduler supports recurring jobs.
    /// </summary>
    bool SupportsRecurring { get; }

    /// <summary>
    /// Gets whether this scheduler supports delayed/scheduled jobs.
    /// </summary>
    bool SupportsDelayed { get; }

    /// <summary>
    /// Gets whether this scheduler supports cron expressions.
    /// </summary>
    bool SupportsCronExpressions { get; }

    /// <summary>
    /// Gets whether this scheduler supports interval-based scheduling.
    /// </summary>
    bool SupportsIntervalScheduling { get; }

    /// <summary>
    /// Gets whether this scheduler supports job persistence across restarts.
    /// </summary>
    bool SupportsJobPersistence { get; }

    /// <summary>
    /// Gets whether this scheduler supports clustering for high availability.
    /// </summary>
    bool SupportsClustering { get; }

    /// <summary>
    /// Gets whether this scheduler supports job queuing and prioritization.
    /// </summary>
    bool SupportsJobQueuing { get; }

    /// <summary>
    /// Gets the maximum number of concurrent jobs this scheduler can handle.
    /// </summary>
    int MaxConcurrentJobs { get; }
}

/// <summary>
/// Non-generic interface for scheduler service types.
/// Provides a common base for all scheduler types regardless of generic parameters.
/// </summary>
public interface ISchedulerType : IServiceType
{
    /// <summary>
    /// Gets the name of the underlying scheduling engine (e.g., "Quartz.NET", "Hangfire").
    /// </summary>
    string SchedulingEngine { get; }

    /// <summary>
    /// Gets the job executor type used by this scheduler.
    /// </summary>
    Type JobExecutorType { get; }

    /// <summary>
    /// Gets the trigger type used by this scheduler.
    /// </summary>
    Type TriggerType { get; }

    /// <summary>
    /// Gets whether this scheduler supports recurring jobs.
    /// </summary>
    bool SupportsRecurring { get; }

    /// <summary>
    /// Gets whether this scheduler supports delayed/scheduled jobs.
    /// </summary>
    bool SupportsDelayed { get; }

    /// <summary>
    /// Gets whether this scheduler supports cron expressions.
    /// </summary>
    bool SupportsCronExpressions { get; }

    /// <summary>
    /// Gets whether this scheduler supports interval-based scheduling.
    /// </summary>
    bool SupportsIntervalScheduling { get; }

    /// <summary>
    /// Gets whether this scheduler supports job persistence across restarts.
    /// </summary>
    bool SupportsJobPersistence { get; }

    /// <summary>
    /// Gets whether this scheduler supports clustering for high availability.
    /// </summary>
    bool SupportsClustering { get; }

    /// <summary>
    /// Gets whether this scheduler supports job queuing and prioritization.
    /// </summary>
    bool SupportsJobQueuing { get; }

    /// <summary>
    /// Gets the maximum number of concurrent jobs this scheduler can handle.
    /// </summary>
    int MaxConcurrentJobs { get; }
}