using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

[Message("ScheduleIdNullOrEmpty")]
public sealed class ScheduleIdNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    public ScheduleIdNullOrEmptyMessage()
        : base(2001, "ScheduleIdNullOrEmpty", MessageSeverity.Error,
               "Schedule ID cannot be null or empty", "SCHED_SCHEDULE_ID_NULL") { }
}
