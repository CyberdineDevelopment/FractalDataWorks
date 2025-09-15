using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Scheduling.Abstractions.Interfaces;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Service type definition for Quartz.NET scheduler.
/// Provides robust enterprise-grade scheduling with persistence and clustering support.
/// </summary>
public sealed class QuartzNetSchedulerType : 
    SchedulerTypeBase<IFractalSchedulingService, ISchedulingConfiguration, ISchedulingServiceFactory<IFractalSchedulingService, ISchedulingConfiguration>>
{
    /// <summary>
    /// Gets the singleton instance of the Quartz.NET scheduler type.
    /// </summary>
    public static QuartzNetSchedulerType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="QuartzNetSchedulerType"/> class.
    /// </summary>
    private QuartzNetSchedulerType() : base(1, "QuartzNet", "Enterprise Scheduling")
    {
    }

    /// <inheritdoc/>
    public override string SchedulingEngine => "Quartz.NET";

    /// <inheritdoc/>
    public override Type JobExecutorType => typeof(IFractalScheduledExecutionHandler);

    /// <inheritdoc/>
    public override Type TriggerType => typeof(IFractalTrigger);

    /// <inheritdoc/>
    public override bool SupportsRecurring => true;

    /// <inheritdoc/>
    public override bool SupportsDelayed => true;

    /// <inheritdoc/>
    public override bool SupportsCronExpressions => true;

    /// <inheritdoc/>
    public override bool SupportsIntervalScheduling => true;

    /// <inheritdoc/>
    public override bool SupportsJobPersistence => true;

    /// <inheritdoc/>
    public override bool SupportsClustering => true;

    /// <inheritdoc/>
    public override bool SupportsJobQueuing => true;

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 100; // Configurable

    /// <inheritdoc/>
    public override Type FactoryType => typeof(ISchedulingServiceFactory<IFractalSchedulingService, ISchedulingConfiguration>);

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register Quartz.NET specific services
        // services.AddQuartz(q => {});
        // services.AddQuartzHostedService(options => {});
        // services.AddScoped<IQuartzSchedulingServiceFactory, QuartzSchedulingServiceFactory>();
        // services.AddScoped<QuartzSchedulingService>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // Quartz.NET specific configuration
        // This could configure job stores, clustering, thread pools, etc.
    }
}