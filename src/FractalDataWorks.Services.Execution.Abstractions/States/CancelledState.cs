using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

namespace FractalDataWorks.Services.Execution.Abstractions.States;

/// <summary>
/// The process was cancelled before completion.
/// This is a terminal state - no further transitions are possible.
/// </summary>
[TypeOption(typeof(ProcessStates), "Cancelled")]
public sealed class CancelledState : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelledState"/> class.
    /// </summary>
    public CancelledState()
        : base(
            id: 5,
            name: "Cancelled",
            isTerminal: true,
            isError: false,
            isActive: false,
            isInitial: false)
    {
    }
}
