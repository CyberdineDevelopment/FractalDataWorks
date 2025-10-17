using System;
using System.Diagnostics.CodeAnalysis;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums;

/// <summary>
/// Abstract base class for trigger types that define HOW a schedule determines WHEN to execute.
/// </summary>
/// <remarks>
/// <para>
/// Trigger types are enhanced enums that encapsulate the logic for different scheduling mechanisms:
/// </para>
/// <list type="bullet">
///   <item><description><strong>Cron triggers</strong>: Use cron expressions for complex time-based scheduling</description></item>
///   <item><description><strong>Interval triggers</strong>: Execute at regular intervals with optional delays</description></item>
///   <item><description><strong>Once triggers</strong>: Execute at a specific date/time, then never again</description></item>
///   <item><description><strong>Manual triggers</strong>: Execute only when manually triggered by user/system</description></item>
///   <item><description><strong>Event triggers</strong>: Execute in response to specific system events</description></item>
///   <item><description><strong>Dependent triggers</strong>: Execute based on completion of other jobs/processes</description></item>
/// </list>
/// <para>
/// Each trigger type validates trigger configurations and calculates next execution times based on
/// the trigger's specific parameters and scheduling rules.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example usage in a scheduling context
/// var cronTrigger = TriggerTypes.Cron;
/// var nextExecution = cronTrigger.CalculateNextExecution(myTrigger, lastExecution);
/// var validationResult = cronTrigger.ValidateTrigger(myTrigger);
/// 
/// if (cronTrigger.RequiresSchedule)
/// {
///     // This trigger type needs schedule persistence
/// }
/// 
/// if (cronTrigger.IsImmediate)
/// {
///     // This trigger executes immediately when created
/// }
/// </code>
/// </example>
public abstract class TriggerTypeBase : EnumOptionBase<TriggerTypeBase>, IEnumOption<TriggerTypeBase>, ITriggerType
{
    /// <summary>
    /// Gets a value indicating whether this trigger type requires schedule persistence.
    /// </summary>
    /// <value>
    /// <c>true</c> if the trigger type needs to store schedule state between executions;
    /// <c>false</c> if it's stateless or ephemeral.
    /// </value>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><strong>Requires schedule</strong>: Cron, Interval, Once (need to track next execution)</description></item>
    ///   <item><description><strong>No schedule needed</strong>: Manual, Event (triggered externally)</description></item>
    /// </list>
    /// </remarks>
    public bool RequiresSchedule { get; }

    /// <summary>
    /// Gets a value indicating whether this trigger type executes immediately upon creation.
    /// </summary>
    /// <value>
    /// <c>true</c> if the trigger should fire immediately when first created;
    /// <c>false</c> if it waits for its calculated next execution time.
    /// </value>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><strong>Immediate execution</strong>: Manual, Immediate, Event-driven triggers</description></item>
    ///   <item><description><strong>Scheduled execution</strong>: Cron, Interval, Once (wait for proper time)</description></item>
    /// </list>
    /// </remarks>
    public bool IsImmediate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TriggerTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this trigger type.</param>
    /// <param name="name">The name of this trigger type.</param>
    /// <param name="requiresSchedule">Whether this trigger type requires schedule persistence.</param>
    /// <param name="isImmediate">Whether this trigger type executes immediately upon creation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    /// <remarks>
    /// The constructor validates that the name is not null and initializes the trigger type
    /// characteristics that determine its scheduling behavior.
    /// </remarks>
    protected TriggerTypeBase(int id, string name, bool requiresSchedule, bool isImmediate) 
        : base(id, name)
    {
        RequiresSchedule = requiresSchedule;
        IsImmediate = isImmediate;
    }

    /// <summary>
    /// Calculates the next execution time for the specified trigger.
    /// </summary>
    /// <param name="trigger">The trigger configuration containing parameters for execution calculation.</param>
    /// <param name="lastExecution">The timestamp of the last execution, or null if never executed.</param>
    /// <returns>
    /// The next execution time in UTC, or null if the trigger will never execute again.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements the core scheduling logic for each trigger type:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><strong>Cron triggers</strong>: Parse cron expression and calculate next occurrence</description></item>
    ///   <item><description><strong>Interval triggers</strong>: Add interval to last execution (or start time)</description></item>
    ///   <item><description><strong>Once triggers</strong>: Return configured time if not yet executed, null otherwise</description></item>
    ///   <item><description><strong>Manual triggers</strong>: Always return null (no automatic scheduling)</description></item>
    /// </list>
    /// <para>
    /// The method should handle timezone conversions, daylight saving time transitions,
    /// and edge cases like invalid dates or expired schedules.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // For a cron trigger with "0 9 * * MON-FRI" (9 AM weekdays)
    /// var nextExecution = cronTrigger.CalculateNextExecution(trigger, DateTime.UtcNow);
    /// // Returns next weekday at 9 AM in the trigger's configured timezone
    /// 
    /// // For an interval trigger with 30-minute intervals
    /// var nextExecution = intervalTrigger.CalculateNextExecution(trigger, lastRun);
    /// // Returns lastRun + 30 minutes
    /// 
    /// // For a manual trigger
    /// var nextExecution = manualTrigger.CalculateNextExecution(trigger, lastRun);
    /// // Always returns null - manual triggers don't auto-schedule
    /// </code>
    /// </example>
    [ExcludeFromCodeCoverage] // Abstract method - implementation coverage tested in derived classes
    public abstract DateTime? CalculateNextExecution(IGenericTrigger trigger, DateTime? lastExecution);

    /// <summary>
    /// Validates that the trigger configuration is valid for this trigger type.
    /// </summary>
    /// <param name="trigger">The trigger configuration to validate.</param>
    /// <returns>
    /// A result indicating whether the trigger is valid. Success result if valid,
    /// error result with validation messages if invalid.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method validates trigger-type-specific configuration parameters:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><strong>Cron triggers</strong>: Validate cron expression syntax and timezone</description></item>
    ///   <item><description><strong>Interval triggers</strong>: Validate interval is positive, start delay is valid</description></item>
    ///   <item><description><strong>Once triggers</strong>: Validate execution time is in the future</description></item>
    ///   <item><description><strong>Manual triggers</strong>: Validate minimal configuration requirements</description></item>
    /// </list>
    /// <para>
    /// The validation should be comprehensive enough to catch configuration errors early,
    /// before the trigger is used in production scheduling.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate a cron trigger
    /// var result = cronTrigger.ValidateTrigger(trigger);
    /// if (result.Error)
    /// {
    ///     // Handle validation errors - invalid cron expression, bad timezone, etc.
    ///     Logger.LogError("Cron trigger validation failed: {Messages}", result.Messages);
    /// }
    /// 
    /// // Validate an interval trigger
    /// var result = intervalTrigger.ValidateTrigger(trigger);
    /// if (result.Error)
    /// {
    ///     // Handle validation errors - negative interval, invalid configuration format
    ///     Logger.LogError("Interval trigger validation failed: {Messages}", result.Messages);
    /// }
    /// </code>
    /// </example>
    [ExcludeFromCodeCoverage] // Abstract method - implementation coverage tested in derived classes
    public abstract IGenericResult ValidateTrigger(IGenericTrigger trigger);
}