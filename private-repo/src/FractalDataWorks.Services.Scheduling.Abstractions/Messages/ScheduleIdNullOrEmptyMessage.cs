using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Error message indicating that a schedule ID is null or empty.
/// </summary>
[Message("ScheduleIdNullOrEmpty")]
public sealed class ScheduleIdNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleIdNullOrEmptyMessage"/> class.
    /// </summary>
    public ScheduleIdNullOrEmptyMessage()
        : base(2001, "ScheduleIdNullOrEmpty", MessageSeverity.Error,
               "Schedule ID cannot be null or empty", "SCHED_SCHEDULE_ID_NULL") { }
}
