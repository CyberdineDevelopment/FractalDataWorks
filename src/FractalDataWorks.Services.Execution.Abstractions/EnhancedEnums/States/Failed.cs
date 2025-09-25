using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums.States;

/// <summary>
/// Final state when a process has failed due to an error.
/// </summary>
[TypeOption(typeof(ProcessStates), "Failed")]
public sealed class Failed : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the Failed state.
    /// </summary>
    public Failed() : base(4, "Failed", isTerminal: true, isError: true, isActive: false, isInitial: false)
    {
    }
}