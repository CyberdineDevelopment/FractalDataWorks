using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Scheduling.Abstractions.Interfaces;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Service type definition for Hangfire scheduler.
/// Provides simple background job processing with web-based monitoring dashboard.
/// </summary>
public sealed class HangfireSchedulerType : 
    SchedulerTypeBase<IFractalSchedulingService, ISchedulingConfiguration, IServiceFactory<IFractalSchedulingService, ISchedulingConfiguration>>
{
    /// <summary>
    /// Gets the singleton instance of the Hangfire scheduler type.
    /// </summary>
    public static HangfireSchedulerType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HangfireSchedulerType"/> class.
    /// </summary>
    private HangfireSchedulerType() : base(2, "Hangfire", "Background Jobs")
    {
    }

    /// <inheritdoc/>
    public override string SchedulingEngine => "Hangfire";

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
    public override bool SupportsIntervalScheduling => false; // Hangfire uses cron-like expressions

    /// <inheritdoc/>
    public override bool SupportsJobPersistence => true;

    /// <inheritdoc/>
    public override bool SupportsClustering => true; // With Redis/SQL Server storage

    /// <inheritdoc/>
    public override bool SupportsJobQueuing => true;

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 20; // Default worker count

    /// <inheritdoc/>
    public override Type FactoryType => typeof(IServiceFactory<IFractalSchedulingService, ISchedulingConfiguration>);

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register Hangfire specific services
        // services.AddHangfire(configuration => {});
        // services.AddHangfireServer();
        // services.AddScoped<IHangfireSchedulingServiceFactory, HangfireSchedulingServiceFactory>();
        // services.AddScoped<HangfireSchedulingService>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // Hangfire specific configuration
        // This could configure storage providers, dashboard settings, retry policies, etc.
    }
}