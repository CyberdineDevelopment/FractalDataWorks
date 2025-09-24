using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Collection of execution statuses.
/// </summary>
[EnumCollection(CollectionName = "ExecutionStatuses")]
public abstract class ExecutionStatusCollectionBase : EnumCollectionBase<ExecutionStatus>
{
}