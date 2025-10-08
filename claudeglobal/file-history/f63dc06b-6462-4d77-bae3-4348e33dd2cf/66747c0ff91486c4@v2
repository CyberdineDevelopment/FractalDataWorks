using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

[Message("ProcessConfigurationNull")]
public sealed class ProcessConfigurationNullMessage : SchedulingMessage, IServiceMessage
{
    public ProcessConfigurationNullMessage()
        : base(2005, "ProcessConfigurationNull", MessageSeverity.Error,
               "Process configuration cannot be null", "SCHED_PROCESS_CONFIG_NULL") { }
}
