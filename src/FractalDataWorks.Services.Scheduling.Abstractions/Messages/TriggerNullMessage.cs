using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Error message indicating that a trigger is null.
/// </summary>
[Message("TriggerNull")]
public sealed class TriggerNullMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TriggerNullMessage"/> class.
    /// </summary>
    public TriggerNullMessage()
        : base(2006, "TriggerNull", MessageSeverity.Error,
               "Trigger cannot be null", "SCHED_TRIGGER_NULL") { }
}
