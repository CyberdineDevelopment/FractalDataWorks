using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Message indicating that a command is required.
/// </summary>
[Message("CommandRequired")]
public sealed class CommandRequiredMessage : DataCommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRequiredMessage"/> class.
    /// </summary>
    public CommandRequiredMessage()
        : base(
            id: 1,
            name: "CommandRequired",
            severity: MessageSeverity.Error,
            message: "Command is required",
            code: "DATACMD_001")
    {
    }
}
