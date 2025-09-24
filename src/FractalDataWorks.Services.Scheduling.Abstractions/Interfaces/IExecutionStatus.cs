using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Interface for execution status.
/// </summary>
public interface IExecutionStatus : IEnumOption<ExecutionStatus>
{
}