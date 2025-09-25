using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// The execution completed successfully.
/// </summary>
[TypeOption(typeof(ExecutionStatuses), "Completed")]
public sealed class CompletedExecutionStatus : ExecutionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompletedExecutionStatus"/> class.
    /// </summary>
    public CompletedExecutionStatus() : base(2, "Completed") { }
}