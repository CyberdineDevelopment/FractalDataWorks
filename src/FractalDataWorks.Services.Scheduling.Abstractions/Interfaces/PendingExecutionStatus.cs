using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// The execution was triggered but has not yet started.
/// </summary>
[TypeOption(typeof(ExecutionStatuses), "Pending")]
public sealed class PendingExecutionStatus : ExecutionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PendingExecutionStatus"/> class.
    /// </summary>
    public PendingExecutionStatus() : base(0, "Pending") { }
}