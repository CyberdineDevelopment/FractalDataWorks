using System;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Base class for scheduler service type definitions that inherit from ServiceTypeBase.
/// Provides scheduler-specific metadata and capabilities.
/// </summary>
/// <typeparam name="TService">The scheduler service type.</typeparam>
/// <typeparam name="TConfiguration">The scheduler configuration type.</typeparam>
/// <typeparam name="TFactory">The factory type for creating scheduler service instances.</typeparam>
/// <remarks>
/// This class replaces the old EnhancedEnum-based SchedulingServiceType.
/// Scheduler types should inherit from this class and provide metadata only - 
/// no instantiation logic should be included (that belongs in factories).
/// </remarks>
public abstract class SchedulerTypeBase<TService, TConfiguration, TFactory> : 
    ServiceTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IGenericSchedulingService
    where TConfiguration : class, ISchedulingConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Gets the scheduling engine used by this service type.
    /// </summary>
    /// <remarks>
    /// Identifies the underlying scheduling framework or implementation
    /// (e.g., "Quartz.NET", "Hangfire", "FluentScheduler", "TaskScheduler").
    /// </remarks>
    public string SchedulingEngine { get; }

    /// <summary>
    /// Gets the job executor type for this scheduler.
    /// </summary>
    /// <remarks>
    /// The type responsible for executing scheduled jobs. This typically
    /// implements job-specific logic and integrates with the scheduling engine.
    /// </remarks>
    public Type JobExecutorType { get; }

    /// <summary>
    /// Gets the trigger type used by this scheduler.
    /// </summary>
    /// <remarks>
    /// The type that defines when and how often jobs should execute.
    /// Different schedulers may use different trigger implementations
    /// (e.g., CronTrigger, SimpleTrigger, custom triggers).
    /// </remarks>
    public Type TriggerType { get; }

    /// <summary>
    /// Gets a value indicating whether this scheduler supports recurring jobs.
    /// </summary>
    /// <remarks>
    /// Recurring jobs execute multiple times based on a schedule (cron, interval, etc.).
    /// This is the most common scheduling pattern for maintenance tasks,
    /// data processing, and periodic operations.
    /// </remarks>
    public bool SupportsRecurring { get; }

    /// <summary>
    /// Gets a value indicating whether this scheduler supports delayed jobs.
    /// </summary>
    /// <remarks>
    /// Delayed jobs execute once at a specific future time.
    /// Common for notifications, cleanup tasks, and workflow orchestration.
    /// </remarks>
    public bool SupportsDelayed { get; }

    /// <summary>
    /// Gets a value indicating whether this scheduler supports cron expressions.
    /// </summary>
    /// <remarks>
    /// Cron expressions provide flexible scheduling patterns using a standard format.
    /// Enables complex scheduling scenarios like "every Monday at 3 AM".
    /// </remarks>
    public virtual bool SupportsCronExpressions => true;

    /// <summary>
    /// Gets a value indicating whether this scheduler supports interval-based scheduling.
    /// </summary>
    /// <remarks>
    /// Interval scheduling executes jobs at fixed time intervals
    /// (e.g., every 5 minutes, hourly, daily).
    /// Simpler than cron but less flexible.
    /// </remarks>
    public virtual bool SupportsIntervalScheduling => true;

    /// <summary>
    /// Gets a value indicating whether this scheduler supports job persistence.
    /// </summary>
    /// <remarks>
    /// Persistent jobs survive application restarts and are stored in durable storage.
    /// Critical for long-running applications and high-availability scenarios.
    /// </remarks>
    public virtual bool SupportsJobPersistence => false;

    /// <summary>
    /// Gets a value indicating whether this scheduler supports clustering.
    /// </summary>
    /// <remarks>
    /// Clustering allows multiple application instances to share job execution,
    /// providing load balancing and failover capabilities.
    /// Requires coordination mechanisms and shared storage.
    /// </remarks>
    public virtual bool SupportsClustering => false;

    /// <summary>
    /// Gets a value indicating whether this scheduler supports job queuing.
    /// </summary>
    /// <remarks>
    /// Job queuing allows scheduling multiple instances of the same job
    /// and managing execution order and concurrency limits.
    /// </remarks>
    public virtual bool SupportsJobQueuing => false;

    /// <summary>
    /// Gets the maximum number of concurrent jobs this scheduler can handle.
    /// </summary>
    /// <remarks>
    /// Defines the concurrency limit for job execution. -1 indicates no limit.
    /// Important for resource management and system stability.
    /// </remarks>
    public virtual int MaxConcurrentJobs => -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerTypeBase{TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the scheduler type.</param>
    /// <param name="name">The name of the scheduler type.</param>
    /// <param name="schedulingEngine">The scheduling engine used by this service type.</param>
    /// <param name="jobExecutorType">The job executor type for this scheduler.</param>
    /// <param name="triggerType">The trigger type used by this scheduler.</param>
    /// <param name="supportsRecurring">Indicates whether this scheduler supports recurring jobs.</param>
    /// <param name="supportsDelayed">Indicates whether this scheduler supports delayed jobs.</param>
    /// <param name="category">The category for this scheduler type (defaults to "Scheduling").</param>
    protected SchedulerTypeBase(
        int id,
        string name,
        string schedulingEngine,
        Type jobExecutorType,
        Type triggerType,
        bool supportsRecurring,
        bool supportsDelayed,
        string? category = null)
        : base(id, name, $"Services:Scheduling:{name}", $"{name} Scheduling Service", $"Scheduling service using {schedulingEngine} engine", category ?? "Scheduling")
    {
        SchedulingEngine = schedulingEngine ?? throw new ArgumentNullException(nameof(schedulingEngine));
        JobExecutorType = jobExecutorType ?? throw new ArgumentNullException(nameof(jobExecutorType));
        TriggerType = triggerType ?? throw new ArgumentNullException(nameof(triggerType));
        SupportsRecurring = supportsRecurring;
        SupportsDelayed = supportsDelayed;
    }
}