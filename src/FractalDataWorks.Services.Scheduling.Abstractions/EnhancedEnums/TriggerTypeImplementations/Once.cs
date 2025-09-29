using System;
using System.Globalization;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums.TriggerTypeImplementations;

/// <summary>
/// Once trigger type that executes at a specific date and time only once.
/// </summary>
/// <remarks>
/// <para>
/// The Once trigger type enables one-time execution at a specified date and time
/// with optional timezone support. It supports:
/// </para>
/// <list type="bullet">
///   <item><description>Specific execution time specification</description></item>
///   <item><description>Timezone-aware scheduling for accurate execution</description></item>
///   <item><description>Automatic handling of daylight saving time</description></item>
///   <item><description>One-time execution guarantee (never executes twice)</description></item>
/// </list>
/// <para>
/// The trigger calculates the next execution as the specified start time only if
/// it has never executed before. Once executed, it returns null for all future
/// next execution calculations.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a once trigger for a specific time
/// var onceConfig = new Dictionary&lt;string, object&gt;
/// {
///     { "StartTime", new DateTime(2024, 12, 31, 23, 59, 0) }
/// };
/// 
/// // With timezone specification
/// var onceConfig = new Dictionary&lt;string, object&gt;
/// {
///     { "StartTime", new DateTime(2024, 1, 1, 9, 0, 0) },
///     { "TimeZoneId", "America/New_York" }
/// };
/// 
/// // Validate and calculate next execution
/// var onceTrigger = TriggerTypes.Once;
/// var validationResult = onceTrigger.ValidateTrigger(trigger);
/// var nextExecution = onceTrigger.CalculateNextExecution(trigger, null);
/// </code>
/// </example>
[EnumOption(typeof(TriggerTypes), "Once")]
public sealed class Once : TriggerTypeBase
{
    /// <summary>
    /// Configuration key for the start time when the trigger should execute.
    /// </summary>
    /// <remarks>
    /// The start time specifies when the one-time execution should occur.
    /// Should be a DateTime value. If not provided, the trigger executes immediately when created.
    /// </remarks>
    public const string StartTimeKey = "StartTime";

    /// <summary>
    /// Configuration key for the timezone identifier.
    /// </summary>
    /// <remarks>
    /// Optional timezone identifier (e.g., "America/New_York", "Europe/London", "UTC").
    /// If not provided, UTC is used. The timezone affects when the start time is interpreted
    /// and handles daylight saving time automatically.
    /// </remarks>
    public const string TimeZoneIdKey = "TimeZoneId";

    /// <summary>
    /// Initializes a new instance of the <see cref="Once"/> class.
    /// </summary>
    /// <remarks>
    /// Once triggers do not require schedule persistence as they execute only once and
    /// do not execute immediately - they wait for their specified execution time.
    /// </remarks>
    public Once() : base(3, "Once", requiresSchedule: false, isImmediate: false)
    {
    }

    /// <summary>
    /// Calculates the next execution time based on the start time and execution history.
    /// </summary>
    /// <param name="trigger">The trigger configuration containing the start time.</param>
    /// <param name="lastExecution">The timestamp of the last execution, or null if never executed.</param>
    /// <returns>
    /// The start time in UTC if never executed before, or null if already executed.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method calculates the next execution time using the following logic:
    /// </para>
    /// <list type="number">
    ///   <item><description>If lastExecution is not null, return null (already executed)</description></item>
    ///   <item><description>Extract start time from configuration or use current time</description></item>
    ///   <item><description>Handle timezone conversion if specified</description></item>
    ///   <item><description>Return start time in UTC, or null if time has already passed</description></item>
    /// </list>
    /// <para>
    /// The method ensures one-time execution by returning null once the trigger
    /// has been executed previously.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // First execution calculation (never executed before)
    /// var trigger = CreateTrigger(startTime: futureDateTime);
    /// var nextExecution = onceTrigger.CalculateNextExecution(trigger, null);
    /// // Returns futureDateTime in UTC
    /// 
    /// // After execution (already executed)
    /// var trigger = CreateTrigger(startTime: futureDateTime);
    /// var nextExecution = onceTrigger.CalculateNextExecution(trigger, pastDateTime);
    /// // Returns null - trigger only executes once
    /// 
    /// // No start time specified
    /// var trigger = CreateTrigger();
    /// var nextExecution = onceTrigger.CalculateNextExecution(trigger, null);
    /// // Returns current time (executes immediately)
    /// </code>
    /// </example>
    public override DateTime? CalculateNextExecution(IFdwTrigger trigger, DateTime? lastExecution)
    {
        if (trigger?.Configuration == null)
        {
            return null;
        }

        // If already executed, never execute again
        if (lastExecution.HasValue)
        {
            return null;
        }

        try
        {
            // Get timezone, default to UTC
            TimeZoneInfo timeZone = TimeZoneInfo.Utc;
            if (trigger.Configuration.TryGetValue(TimeZoneIdKey, out var timeZoneObj) &&
                timeZoneObj is string timeZoneId &&
                !string.IsNullOrWhiteSpace(timeZoneId))
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }

            // Get start time or use current time
            DateTime startTime;
            if (trigger.Configuration.TryGetValue(StartTimeKey, out var startTimeObj) &&
                TryConvertToDateTime(startTimeObj, out var configuredStartTime))
            {
                // Convert configured time to UTC if needed
                startTime = configuredStartTime.Kind == DateTimeKind.Utc 
                    ? configuredStartTime
                    : TimeZoneInfo.ConvertTimeToUtc(configuredStartTime, timeZone);
            }
            else
            {
                // No start time specified - execute immediately
                startTime = DateTime.UtcNow;
            }

            // Only return the start time if it's in the future
            return startTime > DateTime.UtcNow ? startTime : DateTime.UtcNow;
        }
        catch (TimeZoneNotFoundException)
        {
            // Invalid timezone - fall back to UTC calculation
            try
            {
                if (trigger.Configuration.TryGetValue(StartTimeKey, out var startTimeObj) &&
                    TryConvertToDateTime(startTimeObj, out var startTime))
                {
                    var utcStartTime = startTime.Kind == DateTimeKind.Utc ? startTime : startTime.ToUniversalTime();
                    return utcStartTime > DateTime.UtcNow ? utcStartTime : DateTime.UtcNow;
                }
                else
                {
                    return DateTime.UtcNow;
                }
            }
            catch
            {
                return null;
            }
        }
        catch (ArgumentException)
        {
            // Other timezone or datetime conversion errors
            return null;
        }
    }

    /// <summary>
    /// Validates that the trigger configuration contains valid parameters.
    /// </summary>
    /// <param name="trigger">The trigger configuration to validate.</param>
    /// <returns>
    /// A success result if the trigger is valid, or an error result with validation messages if invalid.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs comprehensive validation of the once trigger configuration:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><strong>Start time validation</strong>: Verifies start time format if provided</description></item>
    ///   <item><description><strong>Timezone validation</strong>: Verifies timezone ID exists if provided</description></item>
    ///   <item><description><strong>Future time validation</strong>: Warns if start time is in the past</description></item>
    /// </list>
    /// <para>
    /// The validation is lenient for once triggers since they may be created for immediate
    /// execution or future execution, and past times are converted to immediate execution.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid once trigger with future time
    /// var validTrigger = CreateTrigger(startTime: DateTime.UtcNow.AddHours(1));
    /// var result = onceTrigger.ValidateTrigger(validTrigger);
    /// // result.Success == true
    /// 
    /// // Valid trigger without start time (immediate execution)
    /// var immediateTrigger = CreateTrigger();
    /// var result = onceTrigger.ValidateTrigger(immediateTrigger);
    /// // result.Success == true
    /// 
    /// // Invalid timezone
    /// var badTimezoneTrigger = CreateTrigger(startTime: futureTime, timezone: "Invalid/Timezone");
    /// var result = onceTrigger.ValidateTrigger(badTimezoneTrigger);
    /// // result.Error == true with timezone validation error
    /// </code>
    /// </example>
    public override IFdwResult ValidateTrigger(IFdwTrigger trigger)
    {
        if (trigger?.Configuration == null)
        {
            return FdwResult.Failure("Trigger configuration is required for Once trigger type");
        }

        // Validate start time if provided
        if (trigger.Configuration.TryGetValue(StartTimeKey, out var startTimeObj) &&
            startTimeObj != null &&
            !TryConvertToDateTime(startTimeObj, out var startTime))
        {
            return FdwResult.Failure($"Start time must be a valid DateTime if provided in the '{StartTimeKey}' configuration key");
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

        // Additional validation: Check if start time is reasonable if provided
        if (trigger.Configuration.TryGetValue(StartTimeKey, out var startTimeCheck) &&
            TryConvertToDateTime(startTimeCheck, out var checkStartTime))
        {
            // Get timezone for accurate comparison
            TimeZoneInfo timeZone = TimeZoneInfo.Utc;
            if (trigger.Configuration.TryGetValue(TimeZoneIdKey, out var tzObj) &&
                tzObj is string tzId &&
                !string.IsNullOrWhiteSpace(tzId))
            {
                try
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                }
                catch
                {
                    // Use UTC if timezone is invalid (already reported above)
                }
            }

            try
            {
                var utcStartTime = checkStartTime.Kind == DateTimeKind.Utc 
                    ? checkStartTime
                    : TimeZoneInfo.ConvertTimeToUtc(checkStartTime, timeZone);

                // Check if start time is too far in the past (more than 1 day)
                if (utcStartTime < DateTime.UtcNow.AddDays(-1))
                {
                    return FdwResult.Failure($"Start time is more than 1 day in the past and may not execute as expected. Start time: {utcStartTime:yyyy-MM-dd HH:mm:ss} UTC");
                }
            }
            catch
            {
                // Ignore timezone conversion issues for validation - they're handled in execution
            }
        }

        return FdwResult.Success();
    }

    /// <summary>
    /// Attempts to convert an object to a DateTime.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted DateTime value.</param>
    /// <returns><c>true</c> if conversion succeeded; otherwise, <c>false</c>.</returns>
    private static bool TryConvertToDateTime(object? value, out DateTime result)
    {
        result = default;

        return value switch
        {
            DateTime dateTimeValue => TryAssignDateTime(dateTimeValue, out result),
            DateTimeOffset dateTimeOffsetValue => TryAssignDateTime(dateTimeOffsetValue.DateTime, out result),
            string stringValue => DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out result),
            _ => false
        };
    }

    /// <summary>
    /// Helper method to assign a DateTime value to the result parameter.
    /// </summary>
    /// <param name="value">The value to assign.</param>
    /// <param name="result">The result parameter to assign to.</param>
    /// <returns>Always returns <c>true</c>.</returns>
    private static bool TryAssignDateTime(DateTime value, out DateTime result)
    {
        result = value;
        return true;
    }
}