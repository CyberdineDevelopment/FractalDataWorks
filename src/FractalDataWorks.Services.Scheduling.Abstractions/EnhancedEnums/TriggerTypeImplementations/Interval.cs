using System;
using System.Globalization;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Scheduling.Abstractions.Messages;

namespace FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums.TriggerTypeImplementations;

/// <summary>
/// Interval trigger type that executes at regular intervals.
/// </summary>
/// <remarks>
/// <para>
/// The Interval trigger type enables regular, recurring execution at specified intervals
/// with optional start time and timezone support. It supports:
/// </para>
/// <list type="bullet">
///   <item><description>Fixed intervals in minutes, hours, or days</description></item>
///   <item><description>Optional start time specification</description></item>
///   <item><description>Timezone-aware scheduling for consistent intervals</description></item>
///   <item><description>Automatic handling of daylight saving time transitions</description></item>
/// </list>
/// <para>
/// The trigger calculates the next execution by adding the specified interval to the
/// last execution time (or start time if never executed before).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create an interval trigger for every 30 minutes
/// var intervalConfig = new Dictionary&lt;string, object&gt;
/// {
///     { "IntervalMinutes", 30 }
/// };
/// 
/// // With start time and timezone
/// var intervalConfig = new Dictionary&lt;string, object&gt;
/// {
///     { "IntervalMinutes", 60 },
///     { "StartTime", DateTime.Today.AddHours(9) },
///     { "TimeZoneId", "America/New_York" }
/// };
/// 
/// // Validate and calculate next execution
/// var intervalTrigger = TriggerTypes.Interval;
/// var validationResult = intervalTrigger.ValidateTrigger(trigger);
/// var nextExecution = intervalTrigger.CalculateNextExecution(trigger, DateTime.UtcNow);
/// </code>
/// </example>
[EnumOption(typeof(TriggerTypes), "Interval")]
public sealed class Interval : TriggerTypeBase
{
    /// <summary>
    /// Configuration key for the interval in minutes.
    /// </summary>
    /// <remarks>
    /// The interval must be a positive integer representing the number of minutes
    /// between executions. Examples: 15 (every 15 minutes), 60 (hourly), 1440 (daily).
    /// </remarks>
    public const string IntervalMinutesKey = "IntervalMinutes";

    /// <summary>
    /// Configuration key for the optional start time.
    /// </summary>
    /// <remarks>
    /// Optional start time for the first execution. If not provided, the first execution
    /// will be one interval after the trigger is created. Should be a DateTime value.
    /// </remarks>
    public const string StartTimeKey = "StartTime";

    /// <summary>
    /// Configuration key for the timezone identifier.
    /// </summary>
    /// <remarks>
    /// Optional timezone identifier (e.g., "America/New_York", "Europe/London", "UTC").
    /// If not provided, UTC is used. The timezone affects when intervals are calculated
    /// and handles daylight saving time transitions automatically.
    /// </remarks>
    public const string TimeZoneIdKey = "TimeZoneId";

    /// <summary>
    /// Initializes a new instance of the <see cref="Interval"/> class.
    /// </summary>
    /// <remarks>
    /// Interval triggers require schedule persistence to track next execution times and
    /// do not execute immediately - they wait for their calculated schedule time.
    /// </remarks>
    public Interval() : base(2, "Interval", requiresSchedule: true, isImmediate: false)
    {
    }

    /// <summary>
    /// Calculates the next execution time based on the interval and last execution.
    /// </summary>
    /// <param name="trigger">The trigger configuration containing the interval and optional start time.</param>
    /// <param name="lastExecution">The timestamp of the last execution, or null if never executed.</param>
    /// <returns>
    /// The next execution time in UTC, calculated as (lastExecution ?? startTime ?? now) + interval.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method calculates the next execution time using the following logic:
    /// </para>
    /// <list type="number">
    ///   <item><description>Extract interval in minutes from trigger configuration</description></item>
    ///   <item><description>Determine reference time: lastExecution, startTime, or current time</description></item>
    ///   <item><description>Add interval to reference time, handling timezone if specified</description></item>
    ///   <item><description>Convert result back to UTC for consistent storage</description></item>
    /// </list>
    /// <para>
    /// The method handles timezone conversions and daylight saving time transitions
    /// by using the configured timezone for interval calculations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Every 30 minutes from now
    /// var trigger = CreateTrigger(intervalMinutes: 30);
    /// var nextExecution = intervalTrigger.CalculateNextExecution(trigger, null);
    /// // Returns current time + 30 minutes
    /// 
    /// // Every hour from last execution
    /// var trigger = CreateTrigger(intervalMinutes: 60);
    /// var nextExecution = intervalTrigger.CalculateNextExecution(trigger, lastRun);
    /// // Returns lastRun + 60 minutes
    /// 
    /// // Every 2 hours starting at 9 AM Eastern
    /// var trigger = CreateTrigger(intervalMinutes: 120, startTime: today9AM, timezone: "America/New_York");
    /// var nextExecution = intervalTrigger.CalculateNextExecution(trigger, null);
    /// // Returns 9 AM Eastern + 2 hours (first execution after start time)
    /// </code>
    /// </example>
    public override DateTime? CalculateNextExecution(IGenericTrigger trigger, DateTime? lastExecution)
    {
        if (trigger?.Configuration == null)
        {
            return null;
        }

        // Extract interval in minutes
        if (!trigger.Configuration.TryGetValue(IntervalMinutesKey, out var intervalObj) ||
            !TryConvertToInt(intervalObj, out var intervalMinutes) ||
            intervalMinutes <= 0)
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

            // Determine reference time: lastExecution, startTime, or current time
            DateTime referenceTime;
            if (lastExecution.HasValue)
            {
                referenceTime = lastExecution.Value;
            }
            else if (trigger.Configuration.TryGetValue(StartTimeKey, out var startTimeObj) &&
                     TryConvertToDateTime(startTimeObj, out var startTime))
            {
                referenceTime = startTime.Kind == DateTimeKind.Utc ? startTime : startTime.ToUniversalTime();
            }
            else
            {
                referenceTime = DateTime.UtcNow;
            }

            // Convert reference time to target timezone for calculation
            var referenceTimeInZone = TimeZoneInfo.ConvertTimeFromUtc(referenceTime, timeZone);

            // Add interval
            var nextExecutionInZone = referenceTimeInZone.AddMinutes(intervalMinutes);

            // Convert back to UTC for storage
            return TimeZoneInfo.ConvertTimeToUtc(nextExecutionInZone, timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            // Invalid timezone - fall back to UTC calculation
            try
            {
                DateTime referenceTime;
                if (lastExecution.HasValue)
                {
                    referenceTime = lastExecution.Value;
                }
                else if (trigger.Configuration.TryGetValue(StartTimeKey, out var startTimeObj) &&
                         TryConvertToDateTime(startTimeObj, out var startTime))
                {
                    referenceTime = startTime.Kind == DateTimeKind.Utc ? startTime : startTime.ToUniversalTime();
                }
                else
                {
                    referenceTime = DateTime.UtcNow;
                }

                return referenceTime.AddMinutes(intervalMinutes);
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
    /// Validates that the trigger configuration contains a valid interval and optional parameters.
    /// </summary>
    /// <param name="trigger">The trigger configuration to validate.</param>
    /// <returns>
    /// A success result if the trigger is valid, or an error result with validation messages if invalid.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs comprehensive validation of the interval trigger configuration:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><strong>Interval validation</strong>: Ensures interval is positive integer</description></item>
    ///   <item><description><strong>Start time validation</strong>: Verifies start time format if provided</description></item>
    ///   <item><description><strong>Timezone validation</strong>: Verifies timezone ID exists if provided</description></item>
    ///   <item><description><strong>Configuration completeness</strong>: Ensures required parameters are present</description></item>
    /// </list>
    /// <para>
    /// The validation ensures the interval trigger will work correctly during execution
    /// and provides detailed error messages for any configuration issues.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid interval trigger
    /// var validTrigger = CreateTrigger(intervalMinutes: 30, timezone: "UTC");
    /// var result = intervalTrigger.ValidateTrigger(validTrigger);
    /// // result.Success == true
    /// 
    /// // Invalid interval (negative)
    /// var invalidTrigger = CreateTrigger(intervalMinutes: -10);
    /// var result = intervalTrigger.ValidateTrigger(invalidTrigger);
    /// // result.Error == true, result.Messages contains validation errors
    /// 
    /// // Invalid timezone
    /// var badTimezoneTrigger = CreateTrigger(intervalMinutes: 60, timezone: "Invalid/Timezone");
    /// var result = intervalTrigger.ValidateTrigger(badTimezoneTrigger);
    /// // result.Error == true with timezone validation error
    /// </code>
    /// </example>
    public override IGenericResult ValidateTrigger(IGenericTrigger trigger)
    {
        if (trigger?.Configuration == null)
        {
            return GenericResult.Failure(SchedulingMessages.TriggerConfigurationNull());
        }

        // Validate interval is present and positive
        if (!trigger.Configuration.TryGetValue(IntervalMinutesKey, out var intervalObj) ||
            !TryConvertToInt(intervalObj, out var intervalMinutes))
        {
            return GenericResult.Failure($"Interval in minutes is required and must be provided in the '{IntervalMinutesKey}' configuration key as a positive integer");
        }

        if (intervalMinutes <= 0)
        {
            return GenericResult.Failure($"Interval must be greater than 0 minutes. Provided value: {intervalMinutes}");
        }

        // Validate start time if provided
        if (trigger.Configuration.TryGetValue(StartTimeKey, out var startTimeObj) &&
            startTimeObj != null &&
            !TryConvertToDateTime(startTimeObj, out var _))
        {
            return GenericResult.Failure($"Start time must be a valid DateTime if provided in the '{StartTimeKey}' configuration key");
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
                return GenericResult.Failure($"Invalid timezone identifier: '{timeZoneId}'. Use standard timezone IDs like 'UTC', 'America/New_York', or 'Europe/London'");
            }
            catch (InvalidTimeZoneException ex)
            {
                return GenericResult.Failure($"Invalid timezone configuration: {ex.Message}. Timezone: '{timeZoneId}'");
            }
        }

        return GenericResult.Success();
    }

    /// <summary>
    /// Attempts to convert an object to an integer.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted integer value.</param>
    /// <returns><c>true</c> if conversion succeeded; otherwise, <c>false</c>.</returns>
    private static bool TryConvertToInt(object? value, out int result)
    {
        result = 0;
        
        return value switch
        {
            int intValue => TryAssignInt(intValue, out result),
            long longValue when longValue >= int.MinValue && longValue <= int.MaxValue => TryAssignInt((int)longValue, out result),
            decimal decimalValue when decimalValue == Math.Truncate(decimalValue) && decimalValue >= int.MinValue && decimalValue <= int.MaxValue => TryAssignInt((int)decimalValue, out result),
            double doubleValue when doubleValue == Math.Truncate(doubleValue) && doubleValue >= int.MinValue && doubleValue <= int.MaxValue => TryAssignInt((int)doubleValue, out result),
            float floatValue when floatValue == Math.Truncate(floatValue) && floatValue >= int.MinValue && floatValue <= int.MaxValue => TryAssignInt((int)floatValue, out result),
            string stringValue => int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result),
            _ => false
        };
    }

    /// <summary>
    /// Helper method to assign an integer value to the result parameter.
    /// </summary>
    /// <param name="value">The value to assign.</param>
    /// <param name="result">The result parameter to assign to.</param>
    /// <returns>Always returns <c>true</c>.</returns>
    private static bool TryAssignInt(int value, out int result)
    {
        result = value;
        return true;
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