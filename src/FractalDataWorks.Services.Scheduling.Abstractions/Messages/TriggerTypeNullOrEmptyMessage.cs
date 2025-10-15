using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that the trigger type is null or empty.
/// </summary>
[Message("TriggerTypeNullOrEmpty")]
public sealed class TriggerTypeNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TriggerTypeNullOrEmptyMessage"/> class.
    /// </summary>
    public TriggerTypeNullOrEmptyMessage()
        : base(1003, "TriggerTypeNullOrEmpty", MessageSeverity.Error,
               "Trigger type cannot be null or empty", "SCHED_TRIGGER_TYPE_NULL") { }
}
