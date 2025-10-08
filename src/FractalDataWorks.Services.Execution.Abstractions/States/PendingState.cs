using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

namespace FractalDataWorks.Services.Execution.Abstractions.States;

/// <summary>
/// The process has been triggered but has not yet started execution.
/// </summary>
[TypeOption(typeof(ProcessStates), "Pending")]
public sealed class PendingState : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PendingState"/> class.
    /// </summary>
    public PendingState()
        : base(
            id: 1,
            name: "Pending",
            isTerminal: false,
            isError: false,
            isActive: false,
            isInitial: false)
    {
    }
}
