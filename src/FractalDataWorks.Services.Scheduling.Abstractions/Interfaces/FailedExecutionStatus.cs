using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// The execution failed with an error.
/// </summary>
[TypeOption(typeof(ExecutionStatuses), "Failed")]
public sealed class FailedExecutionStatus : ExecutionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FailedExecutionStatus"/> class.
    /// </summary>
    public FailedExecutionStatus() : base(3, "Failed") { }
}