using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums.States;

/// <summary>
/// The process has been triggered but has not yet started execution.
/// </summary>
[TypeOption(typeof(ProcessStates), "Pending")]
public sealed class Pending : ProcessStateBase
{
    /// <summary>
    /// Initializes a new instance of the Pending state.
    /// </summary>
    public Pending()
        : base(
            id: 6,
            name: "Pending",
            isTerminal: false,
            isError: false,
            isActive: false,
            isInitial: false)
    {
    }
}
