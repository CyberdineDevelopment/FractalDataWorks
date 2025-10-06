using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

[Message("ScheduleNameNullOrEmpty")]
public sealed class ScheduleNameNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    public ScheduleNameNullOrEmptyMessage()
        : base(2002, "ScheduleNameNullOrEmpty", MessageSeverity.Error,
               "Schedule name cannot be null or empty", "SCHED_SCHEDULE_NAME_NULL") { }
}
