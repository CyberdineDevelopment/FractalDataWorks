using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Interface for execution status.
/// </summary>
public interface IExecutionStatus : IEnumOption<ExecutionStatus>
{
}

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

/// <summary>
/// The execution was triggered but has not yet started.
/// </summary>
[EnumOption("Pending")]
public sealed class PendingExecutionStatus : ExecutionStatus
{
    public PendingExecutionStatus() : base(0, "Pending") { }
}

/// <summary>
/// The execution is currently running.
/// </summary>
[EnumOption("Running")]
public sealed class RunningExecutionStatus : ExecutionStatus
{
    public RunningExecutionStatus() : base(1, "Running") { }
}

/// <summary>
/// The execution completed successfully.
/// </summary>
[EnumOption("Completed")]
public sealed class CompletedExecutionStatus : ExecutionStatus
{
    public CompletedExecutionStatus() : base(2, "Completed") { }
}

/// <summary>
/// The execution failed with an error.
/// </summary>
[EnumOption("Failed")]
public sealed class FailedExecutionStatus : ExecutionStatus
{
    public FailedExecutionStatus() : base(3, "Failed") { }
}

/// <summary>
/// The execution was cancelled before completion.
/// </summary>
[EnumOption("Cancelled")]
public sealed class CancelledExecutionStatus : ExecutionStatus
{
    public CancelledExecutionStatus() : base(4, "Cancelled") { }
}

/// <summary>
/// The execution exceeded its timeout limit.
/// </summary>
[EnumOption("TimedOut")]
public sealed class TimedOutExecutionStatus : ExecutionStatus
{
    public TimedOutExecutionStatus() : base(5, "TimedOut") { }
}

/// <summary>
/// Collection of execution statuses.
/// </summary>
[EnumCollection(CollectionName = "ExecutionStatuses")]
public abstract class ExecutionStatusCollectionBase : EnumCollectionBase<ExecutionStatus>
{
}