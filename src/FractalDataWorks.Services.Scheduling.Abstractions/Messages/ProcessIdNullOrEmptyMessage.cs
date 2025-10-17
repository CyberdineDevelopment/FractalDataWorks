using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Error message indicating that a process ID is null or empty.
/// </summary>
[Message("ProcessIdNullOrEmpty")]
public sealed class ProcessIdNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessIdNullOrEmptyMessage"/> class.
    /// </summary>
    public ProcessIdNullOrEmptyMessage()
        : base(2003, "ProcessIdNullOrEmpty", MessageSeverity.Error,
               "Process ID cannot be null or empty", "SCHED_PROCESS_ID_NULL") { }
}
