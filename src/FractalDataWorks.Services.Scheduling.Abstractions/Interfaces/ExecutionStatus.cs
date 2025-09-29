using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Base class for schedule execution status.
/// </summary>
public abstract class ExecutionStatus : EnumOptionBase<ExecutionStatus>, IExecutionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionStatus"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this execution status.</param>
    /// <param name="name">The name of this execution status.</param>
    protected ExecutionStatus(int id, string name) : base(id, name)
    {
    }
}

