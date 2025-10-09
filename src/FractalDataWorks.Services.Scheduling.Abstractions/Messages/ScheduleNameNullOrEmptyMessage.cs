using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Error message indicating that a schedule name is null or empty.
/// </summary>
[Message("ScheduleNameNullOrEmpty")]
public sealed class ScheduleNameNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleNameNullOrEmptyMessage"/> class.
    /// </summary>
    public ScheduleNameNullOrEmptyMessage()
        : base(2002, "ScheduleNameNullOrEmpty", MessageSeverity.Error,
               "Schedule name cannot be null or empty", "SCHED_SCHEDULE_NAME_NULL") { }
}
