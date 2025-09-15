using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums.States;

/// <summary>
/// Final state when a process has completed successfully.
/// </summary>
[EnumOption("Completed")]
public sealed class Completed : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the Completed state.
    /// </summary>
    public Completed() : base(3, "Completed", isTerminal: true, isError: false, isActive: false, isInitial: false)
    {
    }
}