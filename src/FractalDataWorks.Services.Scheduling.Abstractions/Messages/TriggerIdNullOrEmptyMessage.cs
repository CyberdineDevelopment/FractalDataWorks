using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Message indicating that the trigger ID is null or empty.
/// </summary>
[Message("TriggerIdNullOrEmpty")]
public sealed class TriggerIdNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TriggerIdNullOrEmptyMessage"/> class.
    /// </summary>
    public TriggerIdNullOrEmptyMessage()
        : base(1001, "TriggerIdNullOrEmpty", MessageSeverity.Error,
               "Trigger ID cannot be null or empty", "SCHED_TRIGGER_ID_NULL") { }
}
