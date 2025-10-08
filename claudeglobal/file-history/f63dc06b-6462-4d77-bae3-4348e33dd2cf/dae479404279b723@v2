using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

[Message("InvalidScheduleTimestamp")]
public sealed class InvalidScheduleTimestampMessage : SchedulingMessage, IServiceMessage
{
    public InvalidScheduleTimestampMessage()
        : base(2007, "InvalidScheduleTimestamp", MessageSeverity.Error,
               "Updated timestamp cannot be earlier than created timestamp", "SCHED_INVALID_SCHEDULE_TIMESTAMP") { }
}
