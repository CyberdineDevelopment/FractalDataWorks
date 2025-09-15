using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums.States;

/// <summary>
/// Final state when a process has been cancelled before completion.
/// </summary>
[EnumOption("Cancelled")]
public sealed class Cancelled : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the Cancelled state.
    /// </summary>
    public Cancelled() : base(5, "Cancelled", isTerminal: true, isError: false, isActive: false, isInitial: false)
    {
    }
}