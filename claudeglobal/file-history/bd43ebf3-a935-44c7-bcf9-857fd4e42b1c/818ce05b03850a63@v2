using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

namespace FractalDataWorks.Services.Execution.Abstractions.States;

/// <summary>
/// The process completed successfully.
/// This is a terminal state - no further transitions are possible.
/// </summary>
[TypeOption(typeof(ProcessStates), "Completed")]
public sealed class CompletedState : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompletedState"/> class.
    /// </summary>
    public CompletedState()
        : base(
            id: 3,
            name: "Completed",
            isTerminal: true,
            isError: false,
            isActive: false,
            isInitial: false)
    {
    }
}
