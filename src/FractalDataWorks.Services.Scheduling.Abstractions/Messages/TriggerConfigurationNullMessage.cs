using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that the trigger configuration is null.
/// </summary>
[Message("TriggerConfigurationNull")]
public sealed class TriggerConfigurationNullMessage : SchedulingMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TriggerConfigurationNullMessage"/> class.
    /// </summary>
    public TriggerConfigurationNullMessage()
        : base(1004, "TriggerConfigurationNull", MessageSeverity.Error,
               "Trigger configuration cannot be null", "SCHED_CONFIG_NULL") { }
}
