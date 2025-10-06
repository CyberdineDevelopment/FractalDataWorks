using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

[Message("TriggerNull")]
public sealed class TriggerNullMessage : SchedulingMessage, IServiceMessage
{
    public TriggerNullMessage()
        : base(2006, "TriggerNull", MessageSeverity.Error,
               "Trigger cannot be null", "SCHED_TRIGGER_NULL") { }
}
