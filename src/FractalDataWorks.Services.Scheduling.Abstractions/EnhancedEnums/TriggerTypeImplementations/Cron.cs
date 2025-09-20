using System;
using Cronos;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums.TriggerTypeImplementations;

/// <summary>
/// Cron trigger type that uses cron expressions for time-based scheduling.
/// </summary>
/// <remarks>
/// <para>
/// The Cron trigger type enables complex time-based scheduling using standard cron expressions
/// with optional timezone support. It supports all standard cron formats including:
/// </para>
/// <list type="bullet">
///   <item><description>Second-precision cron expressions (6 fields)</description></item>
///   <item><description>Minute-precision cron expressions (5 fields)</description></item>
///   <item><description>Extended cron syntax with descriptors like @yearly, @monthly, @daily, @hourly</description></item>
/// </list>
/// <para>
/// The trigger validates cron expressions using the Cronos library and handles timezone
/// conversions for accurate scheduling across different time zones.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a cron trigger for daily execution at 9 AM EST
/// var cronConfig = new Dictionary&lt;string, object&gt;
/// {
///     { "CronExpression", "0 9 * * *" },
///     { "TimeZoneId", "America/New_York" }
/// };
/// 
/// // Validate and calculate next execution
/// var cronTrigger = TriggerTypes.Cron;
/// var validationResult = cronTrigger.ValidateTrigger(trigger);
/// var nextExecution = cronTrigger.CalculateNextExecution(trigger, DateTime.UtcNow);
/// </code>
/// </example>
[EnumOption("Cron")]
public sealed class Cron : TriggerTypeBase
{
    /// <summary>
    /// Configuration key for the cron expression.
    /// </summary>
    /// <remarks>
    /// The cron expression should be a valid cron format supported by the Cronos library.
    /// Examples: "0 9 * * MON-FRI", "0 0 1 * *", "@daily", "0 */15 * * * *"
    /// </remarks>
    public const string CronExpressionKey = "CronExpression";

    /// <summary>
    /// Configuration key for the timezone identifier.
    /// </summary>
    /// <remarks>
    /// Optional timezone identifier (e.g., "America/New_York", "Europe/London", "UTC").
    /// If not provided, UTC is used. The timezone affects when the cron expression executes
    /// and handles daylight saving time transitions automatically.
    /// </remarks>
    public const string TimeZoneIdKey = "TimeZoneId";

    /// <summary>
    /// Initializes a new instance of the <see cref="Cron"/> class.
    /// </summary>
    /// <remarks>
    /// Cron triggers require schedule persistence to track next execution times and
    /// do not execute immediately - they wait for their calculated schedule time.
    /// </remarks>
    public Cron() : base(1, "Cron", requiresSchedule: true, isImmediate: false)
    {
    }

    /// <summary>
    /// Calculates the next execution time based on the cron expression and timezone.
    /// </summary>
    /// <param name="trigger">The trigger configuration containing the cron expression and optional timezone.</param>
    /// <param name="lastExecution">The timestamp of the last execution, or null if never executed.</param>
    /// <returns>
    /// The next execution time in UTC, or null if the cron expression will never match again.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method parses the cron expression from the trigger configuration and calculates
    /// the next occurrence after the specified reference time. The calculation process:
    /// </para>
    /// <list type="number">
    ///   <item><description>Extract cron expression from trigger configuration</description></item>
    ///   <item><description>Parse timezone if provided, default to UTC</description></item>
    ///   <item><description>Use the later of lastExecution or current time as reference</description></item>
    ///   <item><description>Calculate next occurrence using Cronos library</description></item>
    ///   <item><description>Convert result back to UTC for consistent storage</description></item>
    /// </list>
    /// <para>
    /// The method handles daylight saving time transitions by using the configured timezone
    /// and ensures accurate scheduling across time zone changes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Daily at 9 AM Eastern Time
    /// var trigger = CreateTrigger("0 9 * * *", "America/New_York");
    /// var nextExecution = cronTrigger.CalculateNextExecution(trigger, DateTime.UtcNow);
    /// 
    /// // Every 15 minutes
    /// var trigger = CreateTrigger("0 */15 * * * *");
    /// var nextExecution = cronTrigger.CalculateNextExecution(trigger, lastRun);
    /// 
    /// // Using cron descriptors
    /// var trigger = CreateTrigger("@daily");
    /// var nextExecution = cronTrigger.CalculateNextExecution(trigger, null);
    /// </code>
    /// </example>
    public override DateTime? CalculateNextExecution(IFdwTrigger trigger, DateTime? lastExecution)
    {
        if (trigger?.Configuration == null)
        {
            return null;
        }

        // Extract cron expression
        if (!trigger.Configuration.TryGetValue(CronExpressionKey, out var cronExpressionObj) ||
            cronExpressionObj is not string cronExpression ||
            string.IsNullOrWhiteSpace(cronExpression))
        {
            return null;
        }

        try
        {
            // Parse cron expression
            var cronExpr = CronExpression.Parse(cronExpression);

            // Get timezone, default to UTC
            TimeZoneInfo timeZone = TimeZoneInfo.Utc;
            if (trigger.Configuration.TryGetValue(TimeZoneIdKey, out var timeZoneObj) &&
                timeZoneObj is string timeZoneId &&
                !string.IsNullOrWhiteSpace(timeZoneId))
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }

            // Use the later of lastExecution or current time as reference
            var referenceTime = DateTime.UtcNow;
            if (lastExecution.HasValue && lastExecution.Value > referenceTime)
            {
                referenceTime = lastExecution.Value;
            }

            // Convert reference time to the target timezone for calculation
            var referenceTimeInZone = TimeZoneInfo.ConvertTimeFromUtc(referenceTime, timeZone);

            // Calculate next occurrence in the target timezone
            var nextOccurrence = cronExpr.GetNextOccurrence(referenceTimeInZone, timeZone);

            // Convert back to UTC for storage
            return nextOccurrence?.ToUniversalTime();
        }
        catch (CronFormatException)
        {
            // Invalid cron expression format
            return null;
        }
        catch (TimeZoneNotFoundException)
        {
            // Invalid timezone - fall back to UTC calculation
            try
            {
                var cronExpr = CronExpression.Parse(cronExpression);
                var referenceTime = lastExecution ?? DateTime.UtcNow;
                return cronExpr.GetNextOccurrence(referenceTime, TimeZoneInfo.Utc);
            }
            catch
            {
                return null;
            }
        }
        catch (ArgumentException)
        {
            // Other parsing errors
            return null;
        }
    }

    /// <summary>
    /// Validates that the trigger configuration contains a valid cron expression and timezone.
    /// </summary>
    /// <param name="trigger">The trigger configuration to validate.</param>
    /// <returns>
    /// A success result if the trigger is valid, or an error result with validation messages if invalid.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs comprehensive validation of the cron trigger configuration:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><strong>Cron expression validation</strong>: Ensures the expression is valid Cronos format</description></item>
    ///   <item><description><strong>Timezone validation</strong>: Verifies timezone ID exists if provided</description></item>
    ///   <item><description><strong>Configuration completeness</strong>: Ensures required parameters are present</description></item>
    /// </list>
    /// <para>
    /// The validation uses the Cronos library to parse the cron expression, ensuring it will work
    /// correctly during execution. Invalid expressions or timezones result in detailed error messages.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid cron trigger
    /// var validTrigger = CreateTrigger("0 9 * * MON-FRI", "America/New_York");
    /// var result = cronTrigger.ValidateTrigger(validTrigger);
    /// // result.Success == true
    /// 
    /// // Invalid cron expression
    /// var invalidTrigger = CreateTrigger("invalid cron", "UTC");
    /// var result = cronTrigger.ValidateTrigger(invalidTrigger);
    /// // result.Error == true, result.Messages contains validation errors
    /// 
    /// // Invalid timezone
    /// var badTimezoneTrigger = CreateTrigger("0 9 * * *", "Invalid/Timezone");
    /// var result = cronTrigger.ValidateTrigger(badTimezoneTrigger);
    /// // result.Error == true with timezone validation error
    /// </code>
    /// </example>
    public override IFdwResult ValidateTrigger(IFdwTrigger trigger)
    {
        if (trigger?.Configuration == null)
        {
            return FdwResult.Failure("Trigger configuration is required for Cron trigger type");
        }

        // Validate cron expression is present
        if (!trigger.Configuration.TryGetValue(CronExpressionKey, out var cronExpressionObj) ||
            cronExpressionObj is not string cronExpression ||
            string.IsNullOrWhiteSpace(cronExpression))
        {
            return FdwResult.Failure($"Cron expression is required and must be provided in the '{CronExpressionKey}' configuration key");
        }

        // Validate cron expression format
        try
        {
            CronExpression.Parse(cronExpression);
        }
        catch (CronFormatException ex)
        {
            return FdwResult.Failure($"Invalid cron expression format: {ex.Message}. Expression: '{cronExpression}'");
        }
        catch (ArgumentException ex)
        {
            return FdwResult.Failure($"Invalid cron expression: {ex.Message}. Expression: '{cronExpression}'");
        }

        // Validate timezone if provided
        if (trigger.Configuration.TryGetValue(TimeZoneIdKey, out var timeZoneObj) &&
            timeZoneObj is string timeZoneId &&
            !string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                return FdwResult.Failure($"Invalid timezone identifier: '{timeZoneId}'. Use standard timezone IDs like 'UTC', 'America/New_York', or 'Europe/London'");
            }
            catch (InvalidTimeZoneException ex)
            {
                return FdwResult.Failure($"Invalid timezone configuration: {ex.Message}. Timezone: '{timeZoneId}'");
            }
        }

        // Validate that the cron expression will actually fire (not just syntactically valid)
        try
        {
            var cronExpr = CronExpression.Parse(cronExpression);
            var timeZone = TimeZoneInfo.Utc;
            
            if (trigger.Configuration.TryGetValue(TimeZoneIdKey, out var tzObj) &&
                tzObj is string tzId &&
                !string.IsNullOrWhiteSpace(tzId))
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            }

            var nextOccurrence = cronExpr.GetNextOccurrence(DateTime.UtcNow, timeZone);
            if (!nextOccurrence.HasValue)
            {
                return FdwResult.Failure($"Cron expression '{cronExpression}' will never execute. Verify the expression is not in the past or misconfigured");
            }
        }
        catch (Exception ex)
        {
            return FdwResult.Failure($"Cron expression validation failed: {ex.Message}. Expression: '{cronExpression}'");
        }

        return FdwResult.Success();
    }
}