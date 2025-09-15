using System;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Interface for trigger type enhanced enums.
/// Defines the contract for scheduling trigger implementations.
/// </summary>
public interface ITriggerType : IEnumOption
{
    /// <summary>
    /// Gets the interval for periodic triggers.
    /// Returns null for non-periodic trigger types.
    /// </summary>
    TimeSpan? Interval { get; }
    
    /// <summary>
    /// Gets the cron expression for cron-based triggers.
    /// Returns null for non-cron trigger types.
    /// </summary>
    string? CronExpression { get; }
    
    /// <summary>
    /// Gets a value indicating whether this trigger type is recurring.
    /// </summary>
    bool IsRecurring { get; }
    
    /// <summary>
    /// Gets a value indicating whether this trigger type supports multiple schedules.
    /// </summary>
    bool SupportsMultipleSchedules { get; }
    
    /// <summary>
    /// Gets the priority level for this trigger type.
    /// Higher values indicate higher priority.
    /// </summary>
    int Priority { get; }
}