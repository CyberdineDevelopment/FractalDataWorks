using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums;

/// <summary>
/// Collection of all available trigger types in the scheduling system.
/// </summary>
/// <remarks>
/// <para>
/// The TriggerTypes collection provides static access to all trigger type implementations
/// that define how schedules determine when to execute. This collection is automatically
/// populated by the source generator with all implementations of <see cref="TriggerTypeBase"/>.
/// </para>
/// <para>
/// The collection automatically discovers and includes all trigger types:
/// </para>
/// <list type="bullet">
///   <item><description><strong>Cron triggers</strong>: Time-based scheduling using cron expressions</description></item>
///   <item><description><strong>Interval triggers</strong>: Regular interval-based execution with optional delays</description></item>
///   <item><description><strong>Once triggers</strong>: Single execution at a specific date/time</description></item>
///   <item><description><strong>Manual triggers</strong>: Execute only when manually triggered by user/system</description></item>
/// </list>
/// <para>
/// This collection enables consistent access patterns for trigger type discovery and validation
/// across the entire scheduling system.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Access specific trigger types by name
/// var cronTrigger = TriggerTypes.GetByName("Cron");
/// var intervalTrigger = TriggerTypes.GetByName("Interval");
/// 
/// // List all available trigger types
/// foreach (var triggerType in TriggerTypes.All())
/// {
///     Console.WriteLine($"Trigger Type: {triggerType.Name} (ID: {triggerType.Id})");
///     Console.WriteLine($"  Requires Schedule: {triggerType.RequiresSchedule}");
///     Console.WriteLine($"  Is Immediate: {triggerType.IsImmediate}");
/// }
/// 
/// // Safe access with validation
/// if (TriggerTypes.TryGetByName("CustomTrigger", out var customTrigger))
/// {
///     // Use the custom trigger type
///     var nextExecution = customTrigger.CalculateNextExecution(trigger, lastRun);
/// }
/// 
/// // Access by ID for persistence scenarios
/// var triggerById = TriggerTypes.GetById(1); // Returns Cron trigger type
/// 
/// // Validate trigger configurations
/// var allTriggerTypes = TriggerTypes.All();
/// foreach (var triggerType in allTriggerTypes)
/// {
///     var validationResult = triggerType.ValidateTrigger(myTrigger);
///     if (validationResult.Success)
///     {
///         // This trigger type can handle the configuration
///         break;
///     }
/// }
/// </code>
/// </example>
[StaticEnumCollection(CollectionName = "TriggerTypes", DefaultGenericReturnType = typeof(ITriggerType))]
public abstract class TriggerTypeCollectionBase : EnumCollectionBase<TriggerTypeBase>
{
}