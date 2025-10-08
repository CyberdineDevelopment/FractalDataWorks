using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

namespace FractalDataWorks.Services.Execution.Abstractions.States;

/// <summary>
/// The process failed with an error.
/// This is a terminal error state - no further transitions are possible.
/// </summary>
[TypeOption(typeof(ProcessStates), "Failed")]
public sealed class FailedState : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FailedState"/> class.
    /// </summary>
    public FailedState()
        : base(
            id: 4,
            name: "Failed",
            isTerminal: true,
            isError: true,
            isActive: false,
            isInitial: false)
    {
    }
}
