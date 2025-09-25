using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// The execution was cancelled before completion.
/// </summary>
[TypeOption(typeof(ExecutionStatuses), "Cancelled")]
public sealed class CancelledExecutionStatus : ExecutionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelledExecutionStatus"/> class.
    /// </summary>
    public CancelledExecutionStatus() : base(4, "Cancelled") { }
}