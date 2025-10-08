using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

[Message("ProcessIdNullOrEmpty")]
public sealed class ProcessIdNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    public ProcessIdNullOrEmptyMessage()
        : base(2003, "ProcessIdNullOrEmpty", MessageSeverity.Error,
               "Process ID cannot be null or empty", "SCHED_PROCESS_ID_NULL") { }
}
