using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// The execution failed with an error.
/// </summary>
[EnumOption("Failed")]
public sealed class FailedExecutionStatus : ExecutionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FailedExecutionStatus"/> class.
    /// </summary>
    public FailedExecutionStatus() : base(3, "Failed") { }
}