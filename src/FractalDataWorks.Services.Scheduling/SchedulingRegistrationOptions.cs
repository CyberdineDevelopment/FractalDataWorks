using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Scheduling.Abstractions;

namespace FractalDataWorks.Services.Scheduling;

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