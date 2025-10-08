using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

namespace FractalDataWorks.Services.Execution.Abstractions.States;

/// <summary>
/// The process exceeded its execution timeout limit.
/// This is a terminal error state - no further transitions are possible.
/// </summary>
[TypeOption(typeof(ProcessStates), "TimedOut")]
public sealed class TimedOutState : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimedOutState"/> class.
    /// </summary>
    public TimedOutState()
        : base(
            id: 6,
            name: "TimedOut",
            isTerminal: true,
            isError: true,
            isActive: false,
            isInitial: false)
    {
    }
}
