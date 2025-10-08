using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

namespace FractalDataWorks.Services.Execution.Abstractions.States;

/// <summary>
/// The process has been created but not yet started.
/// This is the initial state for new processes.
/// </summary>
[TypeOption(typeof(ProcessStates), "Created")]
public sealed class CreatedState : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatedState"/> class.
    /// </summary>
    public CreatedState()
        : base(
            id: 0,
            name: "Created",
            isTerminal: false,
            isError: false,
            isActive: false,
            isInitial: true)
    {
    }
}
