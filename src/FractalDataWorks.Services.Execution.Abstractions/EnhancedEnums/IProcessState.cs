using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

/// <summary>
/// Interface defining the contract for process state enum options.
/// </summary>
public interface IProcessState : IEnumOption<IProcessState>
{
    /// <summary>
    /// Gets whether this is a terminal state (no further transitions possible).
    /// </summary>
    bool IsTerminal { get; }
    
    /// <summary>
    /// Gets whether this state represents an error condition.
    /// </summary>
    bool IsError { get; }
    
    /// <summary>
    /// Gets whether this is the initial state for new processes.
    /// </summary>
    bool IsInitial { get; }
    
    /// <summary>
    /// Gets whether the process is actively running in this state.
    /// </summary>
    bool IsActive { get; }
}