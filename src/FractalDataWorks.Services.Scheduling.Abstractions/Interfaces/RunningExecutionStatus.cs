using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// The execution is currently running.
/// </summary>
[EnumOption("Running")]
public sealed class RunningExecutionStatus : ExecutionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RunningExecutionStatus"/> class.
    /// </summary>
    public RunningExecutionStatus() : base(1, "Running") { }
}