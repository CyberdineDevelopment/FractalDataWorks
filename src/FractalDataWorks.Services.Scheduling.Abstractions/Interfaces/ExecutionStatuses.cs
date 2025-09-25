using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Collection of execution statuses.
/// </summary>
[TypeCollection(typeof(ExecutionStatus), typeof(IExecutionStatus), typeof(ExecutionStatuses))]
public sealed partial class ExecutionStatuses : TypeCollectionBase<ExecutionStatus, IExecutionStatus>
{
}