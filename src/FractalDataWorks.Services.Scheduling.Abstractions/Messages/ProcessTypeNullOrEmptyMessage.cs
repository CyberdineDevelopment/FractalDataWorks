using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Error message indicating that a process type is null or empty.
/// </summary>
[Message("ProcessTypeNullOrEmpty")]
public sealed class ProcessTypeNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessTypeNullOrEmptyMessage"/> class.
    /// </summary>
    public ProcessTypeNullOrEmptyMessage()
        : base(2004, "ProcessTypeNullOrEmpty", MessageSeverity.Error,
               "Process type cannot be null or empty", "SCHED_PROCESS_TYPE_NULL") { }
}
