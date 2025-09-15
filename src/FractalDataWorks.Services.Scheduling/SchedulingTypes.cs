using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Scheduling.Abstractions;

namespace FractalDataWorks.Services.Scheduling;

/// <summary>
/// ServiceType collection for all scheduling types.
/// The source generator will discover all SchedulingTypeBase implementations.
/// </summary>
[ServiceTypeCollection("ISchedulerType", "SchedulingTypes")]
public static partial class SchedulingTypes
{
    /// <summary>
    /// Registers all discovered scheduling types with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void Register(IServiceCollection services)
    {
        // Register each discovered scheduling type
        foreach (var schedulingType in All)
        {
            schedulingType.Register(services);
        }
    }

    /// <summary>
    /// Registers all discovered scheduling types with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Custom configuration action for special cases.</param>
    public static void Register(IServiceCollection services, Action<SchedulingRegistrationOptions> configure)
    {
        var options = new SchedulingRegistrationOptions();
        configure(options);

        // Register each discovered scheduling type with custom options
        foreach (var schedulingType in All)
        {
            schedulingType.Register(services);

            // Apply custom configuration if needed
            if (options.CustomConfigurations.TryGetValue(schedulingType.Name, out var customConfig))
            {
                customConfig(services, schedulingType);
            }
        }
    }
}

/// <summary>
/// Configuration options for scheduling registration.
/// </summary>
public class SchedulingRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific scheduling types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, ISchedulerType>> CustomConfigurations { get; } = new();

    /// <summary>
    /// Configure a specific scheduling type.
    /// </summary>
    /// <param name="schedulingTypeName">The name of the scheduling type (e.g., "Hangfire", "QuartzNet").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string schedulingTypeName, Action<IServiceCollection, ISchedulerType> configure)
    {
        CustomConfigurations[schedulingTypeName] = configure;
    }
}