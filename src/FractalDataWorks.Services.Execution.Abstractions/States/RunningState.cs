using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

namespace FractalDataWorks.Services.Execution.Abstractions.States;

/// <summary>
/// The process is currently executing.
/// </summary>
[TypeOption(typeof(ProcessStates), "Running")]
public sealed class RunningState : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RunningState"/> class.
    /// </summary>
    public RunningState()
        : base(
            id: 2,
            name: "Running",
            isTerminal: false,
            isError: false,
            isActive: true,
            isInitial: false)
    {
    }
}
