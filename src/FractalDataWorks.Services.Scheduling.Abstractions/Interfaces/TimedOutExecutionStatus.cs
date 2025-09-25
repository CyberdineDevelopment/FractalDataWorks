using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// The execution exceeded its timeout limit.
/// </summary>
[TypeOption(typeof(ExecutionStatuses), "TimedOut")]
public sealed class TimedOutExecutionStatus : ExecutionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimedOutExecutionStatus"/> class.
    /// </summary>
    public TimedOutExecutionStatus() : base(5, "TimedOut") { }
}