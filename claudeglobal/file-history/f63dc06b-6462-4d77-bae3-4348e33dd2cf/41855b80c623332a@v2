using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

[Message("ProcessTypeNullOrEmpty")]
public sealed class ProcessTypeNullOrEmptyMessage : SchedulingMessage, IServiceMessage
{
    public ProcessTypeNullOrEmptyMessage()
        : base(2004, "ProcessTypeNullOrEmpty", MessageSeverity.Error,
               "Process type cannot be null or empty", "SCHED_PROCESS_TYPE_NULL") { }
}
