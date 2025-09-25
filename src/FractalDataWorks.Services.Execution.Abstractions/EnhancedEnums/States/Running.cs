using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums.States;

/// <summary>
/// State when a process is actively executing.
/// </summary>
[TypeOption(typeof(ProcessStates), "Running")]
public sealed class Running : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the Running state.
    /// </summary>
    public Running() : base(2, "Running", isTerminal: false, isError: false, isActive: true, isInitial: false)
    {
    }
}