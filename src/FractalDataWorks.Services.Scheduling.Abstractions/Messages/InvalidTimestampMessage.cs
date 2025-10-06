using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Message indicating that modified timestamp is earlier than created timestamp.
/// </summary>
[Message("InvalidTimestamp")]
public sealed class InvalidTimestampMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTimestampMessage"/> class.
    /// </summary>
    public InvalidTimestampMessage()
        : base(1005, "InvalidTimestamp", MessageSeverity.Error,
               "Modified timestamp cannot be earlier than created timestamp", "SCHED_INVALID_TIMESTAMP") { }
}
