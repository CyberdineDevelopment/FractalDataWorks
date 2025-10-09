using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Error message indicating that a schedule's updated timestamp is earlier than its created timestamp.
/// </summary>
[Message("InvalidScheduleTimestamp")]
public sealed class InvalidScheduleTimestampMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidScheduleTimestampMessage"/> class.
    /// </summary>
    public InvalidScheduleTimestampMessage()
        : base(2007, "InvalidScheduleTimestamp", MessageSeverity.Error,
               "Updated timestamp cannot be earlier than created timestamp", "SCHED_INVALID_SCHEDULE_TIMESTAMP") { }
}
